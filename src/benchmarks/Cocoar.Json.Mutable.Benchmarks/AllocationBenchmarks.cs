using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Cocoar.Json.Mutable;
using System.Text.Json.Nodes;

namespace Cocoar.Json.Mutable.Benchmarks;

/// <summary>
/// Focused benchmark to prove allocation differences
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AllocationBenchmarks
{
    private byte[] _json1 = null!;
    private byte[] _json2 = null!;
    private string _json1Str = null!;
    private string _json2Str = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _json1Str = """
        {
            "server": {
                "host": "server1.example.com",
                "port": 8080,
                "timeout": 30
            },
            "database": {
                "host": "db1.example.com",
                "port": 5432
            }
        }
        """;
        
        _json2Str = """
        {
            "server": {
                "host": "server2.example.com",
                "maxConnections": 100
            },
            "database": {
                "username": "admin"
            },
            "features": {
                "enabled": true
            }
        }
        """;
        
        _json1 = System.Text.Encoding.UTF8.GetBytes(_json1Str);
        _json2 = System.Text.Encoding.UTF8.GetBytes(_json2Str);
    }
    
    [Benchmark(Baseline = true)]
    public string SystemTextJson_DeepClone()
    {
        var target = JsonNode.Parse(_json1Str)!.AsObject();
        var source = JsonNode.Parse(_json2Str)!.AsObject();
        
        // This allocates new objects for EVERYTHING
        foreach (var prop in source)
        {
            if (target[prop.Key] is JsonObject targetObj && 
                prop.Value is JsonObject sourceObj)
            {
                MergeWithDeepClone(targetObj, sourceObj);
            }
            else
            {
                target[prop.Key] = prop.Value?.DeepClone(); // Allocation!
            }
        }
        
        return target.ToJsonString();
    }
    
    [Benchmark]
    public byte[] CocoarJsonMutable_InPlaceMerge()
    {
        var target = MutableJsonDocument.Parse(_json1);
        var source = MutableJsonDocument.Parse(_json2);
        
        // This only allocates for NEW/CHANGED values, nested objects are merged in-place
        MutableJsonMerge.Merge((MutableJsonObject)target, (MutableJsonObject)source);
        
        return MutableJsonDocument.ToUtf8Bytes(target);
    }
    
    private static void MergeWithDeepClone(JsonObject target, JsonObject source)
    {
        foreach (var prop in source)
        {
            if (target[prop.Key] is JsonObject targetObj && 
                prop.Value is JsonObject sourceObj)
            {
                MergeWithDeepClone(targetObj, sourceObj);
            }
            else
            {
                target[prop.Key] = prop.Value?.DeepClone();
            }
        }
    }
}
