using Xunit;

namespace Cocoar.Json.Mutable.Tests;

public class StringApiTests
{
    [Fact]
    public void Get_WithString_Works()
    {
        var obj = new MutableJsonObject();
        obj.Set("name", new MutableJsonString("John"));
        obj.Set("age", new MutableJsonNumber(30));
        
        var name = obj.Get("name") as MutableJsonString;
        var age = obj.Get("age") as MutableJsonNumber;
        
        Assert.NotNull(name);
        Assert.NotNull(age);
        Assert.Equal("John", System.Text.Encoding.UTF8.GetString(name.ValueUtf8));
        Assert.Equal("30", System.Text.Encoding.UTF8.GetString(age.ValueUtf8));
    }
    
    [Fact]
    public void Set_WithString_Works()
    {
        var obj = new MutableJsonObject();
        
        obj.Set("title", new MutableJsonString("Hello World"));
        obj.Set("count", new MutableJsonNumber(42));
        obj.Set("active", new MutableJsonBool(true));
        
        Assert.NotNull(obj.Get("title"));
        Assert.NotNull(obj.Get("count"));
        Assert.NotNull(obj.Get("active"));
    }
    
    [Fact]
    public void Remove_WithString_Works()
    {
        var obj = new MutableJsonObject();
        obj.Set("keep", new MutableJsonString("value"));
        obj.Set("remove", new MutableJsonString("value"));
        
        Assert.True(obj.Remove("remove"));
        Assert.NotNull(obj.Get("keep"));
        Assert.Null(obj.Get("remove"));
    }
    
    [Fact]
    public void MutableJsonString_FromString_Works()
    {
        var str = new MutableJsonString("Hello, World!");
        
        var value = System.Text.Encoding.UTF8.GetString(str.ValueUtf8);
        Assert.Equal("Hello, World!", value);
    }
    
    [Fact]
    public void MutableJsonNumber_FromInt_Works()
    {
        var num = new MutableJsonNumber(42);
        
        var value = System.Text.Encoding.UTF8.GetString(num.ValueUtf8);
        Assert.Equal("42", value);
    }
    
    [Fact]
    public void MutableJsonNumber_FromLong_Works()
    {
        var num = new MutableJsonNumber(9223372036854775807L);
        
        var value = System.Text.Encoding.UTF8.GetString(num.ValueUtf8);
        Assert.Equal("9223372036854775807", value);
    }
    
    [Fact]
    public void MutableJsonNumber_FromDouble_Works()
    {
        var num = new MutableJsonNumber(3.14159);
        
        var value = System.Text.Encoding.UTF8.GetString(num.ValueUtf8);
        Assert.Contains("3.14", value);
    }
    
    [Fact]
    public void RealWorld_StringApi_Example()
    {
        // Create config using string API
        var config = new MutableJsonObject();
        
        var server = new MutableJsonObject();
        server.Set("host", new MutableJsonString("localhost"));
        server.Set("port", new MutableJsonNumber(8080));
        server.Set("ssl", new MutableJsonBool(true));
        
        config.Set("server", server);
        config.Set("appName", new MutableJsonString("MyApp"));
        config.Set("version", new MutableJsonNumber(1.5));
        
        // Verify
        var serverNode = config.Get("server") as MutableJsonObject;
        Assert.NotNull(serverNode);
        
        var host = serverNode.Get("host") as MutableJsonString;
        Assert.NotNull(host);
        Assert.Equal("localhost", System.Text.Encoding.UTF8.GetString(host.ValueUtf8));
        
        var port = serverNode.Get("port") as MutableJsonNumber;
        Assert.NotNull(port);
        Assert.Equal("8080", System.Text.Encoding.UTF8.GetString(port.ValueUtf8));
    }
    
    [Fact]
    public void MixedApi_BytesAndStrings_Works()
    {
        var obj = new MutableJsonObject();
        
        // Mix byte-based and string-based API
        obj.Set("key1"u8, new MutableJsonString("value1"u8.ToArray()));
        obj.Set("key2", new MutableJsonString("value2"));
        
        // Both should work for retrieval
        Assert.NotNull(obj.Get("key1"u8));
        Assert.NotNull(obj.Get("key1"));
        Assert.NotNull(obj.Get("key2"u8));
        Assert.NotNull(obj.Get("key2"));
    }
}
