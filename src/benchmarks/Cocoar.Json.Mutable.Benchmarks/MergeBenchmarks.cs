using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Cocoar.Json.Mutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Cocoar.Json.Mutable.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MergeBenchmarks
{
    private byte[][] _providerJsons = null!;
    private string[] _providerJsonStrings = null!;
    
    [Params(2, 10, 50, 100)]
    public int ProviderCount { get; set; }
    
    [GlobalSetup]
    public void Setup()
    {
        // Create realistic provider JSONs
        _providerJsons = new byte[ProviderCount][];
        _providerJsonStrings = new string[ProviderCount];
        
        for (int i = 0; i < ProviderCount; i++)
        {
            var json = $$"""
            {
                "server": {
                    "host": "provider-{{i}}.example.com",
                    "port": {{8080 + i}},
                    "timeout": 30,
                    "maxConnections": 100
                },
                "database": {
                    "host": "db-{{i}}.example.com",
                    "port": 5432,
                    "username": "user{{i}}",
                    "poolSize": 20
                },
                "features": {
                    "feature1": {{(i % 2 == 0 ? "true" : "false")}},
                    "feature2": {{(i % 3 == 0 ? "true" : "false")}},
                    "feature3": {{(i % 5 == 0 ? "true" : "false")}}
                },
                "logging": {
                    "level": "{{(i % 2 == 0 ? "info" : "debug")}}",
                    "verbose": {{(i % 2 == 0 ? "true" : "false")}},
                    "format": "json"
                },
                "metadata": {
                    "provider": "provider-{{i}}",
                    "version": "1.{{i}}.0",
                    "timestamp": "2024-01-{{i + 1:D2}}T00:00:00Z"
                }
            }
            """;
            
            _providerJsonStrings[i] = json;
            _providerJsons[i] = System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
    
    [Benchmark(Baseline = true)]
    public string SystemTextJson_ManualMerge_DeepClone()
    {
        JsonObject? result = null;
        
        for (int i = 0; i < ProviderCount; i++)
        {
            var parsed = JsonNode.Parse(_providerJsonStrings[i])!.AsObject();
            
            if (result == null)
            {
                result = parsed;
            }
            else
            {
                MergeJsonObjects(result, parsed);
            }
        }
        
        return result!.ToJsonString();
    }
    
    [Benchmark]
    public byte[] CocoarJsonMutable_Merge()
    {
        var result = new MutableJsonObject();
        
        for (int i = 0; i < ProviderCount; i++)
        {
            var parsed = MutableJsonDocument.Parse(_providerJsons[i]);
            MutableJsonMerge.Merge(result, (MutableJsonObject)parsed);
        }
        
        return MutableJsonDocument.ToUtf8Bytes(result);
    }
    
    [Benchmark]
    public byte[] CocoarJsonMutable_MergeDestructive()
    {
        var result = new MutableJsonObject();
        
        for (int i = 0; i < ProviderCount; i++)
        {
            var parsed = MutableJsonDocument.Parse(_providerJsons[i]);
            MutableJsonMerge.MergeDestructive(result, (MutableJsonObject)parsed);
        }
        
        return MutableJsonDocument.ToUtf8Bytes(result);
    }
    
    [Benchmark]
    public string SystemTextJson_FlattenUnflatten()
    {
        var flatDict = new Dictionary<string, JsonNode?>();
        
        for (int i = 0; i < ProviderCount; i++)
        {
            var parsed = JsonNode.Parse(_providerJsonStrings[i])!.AsObject();
            FlattenJson(parsed, "", flatDict);
        }
        
        var result = UnflattenJson(flatDict);
        return result.ToJsonString();
    }
    
    // Helper: Manual merge with DeepClone (what users would write)
    private static void MergeJsonObjects(JsonObject target, JsonObject source)
    {
        foreach (var prop in source)
        {
            if (target[prop.Key] is JsonObject targetObj && 
                prop.Value is JsonObject sourceObj)
            {
                MergeJsonObjects(targetObj, sourceObj);
            }
            else
            {
                target[prop.Key] = prop.Value?.DeepClone();
            }
        }
    }
    
    // Helper: Flatten JSON to paths
    private static void FlattenJson(JsonObject obj, string prefix, Dictionary<string, JsonNode?> result)
    {
        foreach (var prop in obj)
        {
            var key = string.IsNullOrEmpty(prefix) ? prop.Key : $"{prefix}:{prop.Key}";
            
            if (prop.Value is JsonObject childObj)
            {
                FlattenJson(childObj, key, result);
            }
            else
            {
                result[key] = prop.Value?.DeepClone();
            }
        }
    }
    
    // Helper: Unflatten paths back to JSON
    private static JsonObject UnflattenJson(Dictionary<string, JsonNode?> flat)
    {
        var result = new JsonObject();
        
        foreach (var kvp in flat)
        {
            var parts = kvp.Key.Split(':');
            JsonObject current = result;
            
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (current[parts[i]] is not JsonObject childObj)
                {
                    childObj = new JsonObject();
                    current[parts[i]] = childObj;
                }
                current = (JsonObject)current[parts[i]]!;
            }
            
            current[parts[^1]] = kvp.Value?.DeepClone();
        }
        
        return result;
    }
}
