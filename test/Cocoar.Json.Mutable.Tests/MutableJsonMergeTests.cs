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
}
