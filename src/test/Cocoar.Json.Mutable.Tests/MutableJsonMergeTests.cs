using Xunit;

namespace Cocoar.Json.Mutable.Tests;

public class MutableJsonMergeTests
{
    [Fact]
    public void Merge_SimpleMerge_Works()
    {
        var target = new MutableJsonObject();
        target.Set("a"u8, new MutableJsonString("value1"u8.ToArray()));
        
        var source = new MutableJsonObject();
        source.Set("b"u8, new MutableJsonString("value2"u8.ToArray()));
        
        MutableJsonMerge.Merge(target, source);
        
        Assert.NotNull(target.Get("a"u8));
        Assert.NotNull(target.Get("b"u8));
    }
    
    [Fact]
    public void Merge_NestedObjects_MergesRecursively()
    {
        var target = new MutableJsonObject();
        var targetNested = new MutableJsonObject();
        targetNested.Set("port"u8, new MutableJsonNumber("8080"u8));
        target.Set("server"u8, targetNested);
        
        var source = new MutableJsonObject();
        var sourceNested = new MutableJsonObject();
        sourceNested.Set("host"u8, new MutableJsonString("localhost"u8.ToArray()));
        source.Set("server"u8, sourceNested);
        source.Set("debug"u8, new MutableJsonBool(true));
        
        MutableJsonMerge.Merge(target, source);
        
        var serverNode = target.Get("server"u8) as MutableJsonObject;
        Assert.NotNull(serverNode);
        Assert.NotNull(serverNode.Get("port"u8));
        Assert.NotNull(serverNode.Get("host"u8));
        Assert.NotNull(target.Get("debug"u8));
    }

    [Fact]
    public void Merge_ByDefault_TreatsDifferentCasingAsDifferentProperties()
    {
        var target = new MutableJsonObject();
        target.Set("property"u8, new MutableJsonString("target"u8.ToArray()));

        var source = new MutableJsonObject();
        source.Set("Property"u8, new MutableJsonString("source"u8.ToArray()));

        MutableJsonMerge.Merge(target, source);

        Assert.Equal(2, target.Properties.Count);
        Assert.Equal("target", GetStringValue(target.Get("property"u8)));
        Assert.Equal("source", GetStringValue(target.Get("Property"u8)));
    }

    [Fact]
    public void Merge_WithCaseInsensitivePropertyNames_ReplacesExistingValueAndKeepsTargetName()
    {
        var target = new MutableJsonObject();
        target.Set("property"u8, new MutableJsonString("target"u8.ToArray()));

        var source = new MutableJsonObject();
        source.Set("Property"u8, new MutableJsonString("source"u8.ToArray()));

        MutableJsonMerge.Merge(
            target,
            source,
            new MutableJsonMergeOptions { PropertyNameCaseInsensitive = true });

        Assert.Single(target.Properties);
        Assert.Equal("property", target.Properties[0].Name);
        Assert.Equal("source", GetStringValue(target.Get("property"u8)));
        Assert.Null(target.Get("Property"u8));
    }

    [Fact]
    public void Merge_WithCaseInsensitivePropertyNames_MergesNestedObjectsAndKeepsTargetNames()
    {
        var target = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "server": { "host": "0.0.0.0", "port": 8080 } }"""u8);
        var source = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "Server": { "Host": "localhost" } }"""u8);

        MutableJsonMerge.Merge(
            target,
            source,
            new MutableJsonMergeOptions { PropertyNameCaseInsensitive = true });

        Assert.Single(target.Properties);
        Assert.Equal("server", target.Properties[0].Name);

        var server = Assert.IsType<MutableJsonObject>(target.Get("server"u8));
        Assert.Equal(2, server.Properties.Count);
        Assert.Equal("host", server.Properties[0].Name);
        Assert.Equal("localhost", GetStringValue(server.Get("host"u8)));
        Assert.NotNull(server.Get("port"u8));
    }

    [Fact]
    public void MergeDestructive_WithCaseInsensitivePropertyNames_MovesSourceValueAndKeepsTargetName()
    {
        var target = new MutableJsonObject();
        target.Set("property"u8, new MutableJsonString("target"u8.ToArray()));

        var sourceValue = new MutableJsonString("source"u8.ToArray());
        var source = new MutableJsonObject();
        source.Set("Property"u8, sourceValue);

        MutableJsonMerge.MergeDestructive(
            target,
            source,
            new MutableJsonMergeOptions { PropertyNameCaseInsensitive = true });

        Assert.Single(target.Properties);
        Assert.Equal("property", target.Properties[0].Name);
        Assert.Same(sourceValue, target.Get("property"u8));
        Assert.Null(target.Get("Property"u8));
    }
    
    [Fact]
    public void Merge_FromParsedJson_Works()
    {
        var doc = new MutableJsonObject();
        
        var config1 = MutableJsonDocument.Parse("{\"server\": {\"port\": 8080}}"u8);
        var config2 = MutableJsonDocument.Parse("{\"server\": {\"host\": \"localhost\"}, \"debug\": true}"u8);
        
        MutableJsonMerge.Merge(doc, (MutableJsonObject)config1);
        MutableJsonMerge.Merge(doc, (MutableJsonObject)config2);
        
        var json = MutableJsonDocument.ToUtf8Bytes(doc);
        var jsonString = System.Text.Encoding.UTF8.GetString(json);
        
        Assert.Contains("\"port\"", jsonString);
        Assert.Contains("\"host\"", jsonString);
        Assert.Contains("\"debug\"", jsonString);
    }
    
    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new MutableJsonObject();
        original.Set("name"u8, new MutableJsonString("test"u8.ToArray()));
        
        var cloned = MutableJsonMerge.Clone(original) as MutableJsonObject;
        
        Assert.NotNull(cloned);
        original.Set("name"u8, new MutableJsonString("modified"u8.ToArray()));
        
        var clonedName = (cloned.Get("name"u8) as MutableJsonString)!;
        var originalName = (original.Get("name"u8) as MutableJsonString)!;
        
        Assert.NotEqual(
            System.Text.Encoding.UTF8.GetString(originalName.ValueUtf8),
            System.Text.Encoding.UTF8.GetString(clonedName.ValueUtf8)
        );
    }
    
    [Fact]
    public void Parse_WithReadOnlyMemory_Works()
    {
        // Simulate provider giving us ReadOnlyMemory<byte>
        byte[] jsonBytes = "{\"key\": \"value\", \"number\": 42}"u8.ToArray();
        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(jsonBytes);
        
        var node = MutableJsonDocument.Parse(memory);
        
        Assert.NotNull(node);
        var obj = Assert.IsType<MutableJsonObject>(node);
        Assert.NotNull(obj.Get("key"u8));
        Assert.NotNull(obj.Get("number"u8));
    }
    
    [Fact]
    public void Parse_WithReadOnlyMemory_CanMerge()
    {
        // Simulate getting JSON from a provider as ReadOnlyMemory<byte>
        byte[] json1 = "{\"server\": {\"port\": 8080}}"u8.ToArray();
        byte[] json2 = "{\"server\": {\"host\": \"localhost\"}}"u8.ToArray();
        
        ReadOnlyMemory<byte> memory1 = new ReadOnlyMemory<byte>(json1);
        ReadOnlyMemory<byte> memory2 = new ReadOnlyMemory<byte>(json2);
        
        var config = new MutableJsonObject();
        MutableJsonMerge.Merge(config, (MutableJsonObject)MutableJsonDocument.Parse(memory1));
        MutableJsonMerge.Merge(config, (MutableJsonObject)MutableJsonDocument.Parse(memory2));
        
        var serverObj = config.Get("server"u8) as MutableJsonObject;
        Assert.NotNull(serverObj);
        Assert.NotNull(serverObj.Get("port"u8));
        Assert.NotNull(serverObj.Get("host"u8));
    }

    private static string GetStringValue(MutableJsonNode? node)
    {
        var value = Assert.IsType<MutableJsonString>(node);
        return System.Text.Encoding.UTF8.GetString(value.ValueUtf8);
    }
}
