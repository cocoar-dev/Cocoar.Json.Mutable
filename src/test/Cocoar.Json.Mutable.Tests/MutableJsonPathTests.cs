using Xunit;

namespace Cocoar.Json.Mutable.Tests;

public class MutableJsonPathTests
{
    [Fact]
    public void GetAtPath_ReturnsNestedValue()
    {
        var root = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "server": { "host": "localhost" } }"""u8);

        var host = root.GetAtPath(["server", "host"]);

        Assert.Equal("localhost", GetStringValue(host));
    }

    [Fact]
    public void GetAtPath_SupportsPropertyNamesContainingDots()
    {
        var root = new MutableJsonObject();
        var nested = new MutableJsonObject();
        nested.Set("port", new MutableJsonNumber(8080));
        root.Set("server.host", nested);

        var port = root.GetAtPath(["server.host", "port"]);

        var number = Assert.IsType<MutableJsonNumber>(port);
        Assert.Equal("8080", System.Text.Encoding.UTF8.GetString(number.ValueUtf8));
    }

    [Fact]
    public void GetAtPath_WithCaseInsensitiveMatching_FindsNestedValue()
    {
        var root = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "Server": { "Host": "localhost" } }"""u8);

        var host = root.GetAtPath(
            ["server", "host"],
            new MutableJsonPathOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal("localhost", GetStringValue(host));
    }

    [Fact]
    public void SetAtPath_CreatesMissingIntermediateObjects()
    {
        var root = new MutableJsonObject();

        root.SetAtPath(["server", "host"], new MutableJsonString("localhost"));

        var server = Assert.IsType<MutableJsonObject>(root.Get("server"));
        Assert.Equal("localhost", GetStringValue(server.Get("host")));
    }

    [Fact]
    public void SetAtPath_WithCaseInsensitiveMatching_KeepsExistingTargetCasing()
    {
        var root = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "server": { "host": "old" } }"""u8);

        root.SetAtPath(
            ["Server", "Host"],
            new MutableJsonString("new"),
            new MutableJsonPathOptions { PropertyNameCaseInsensitive = true });

        Assert.Single(root.Properties);
        Assert.Equal("server", root.Properties[0].Name);

        var server = Assert.IsType<MutableJsonObject>(root.Get("server"));
        Assert.Single(server.Properties);
        Assert.Equal("host", server.Properties[0].Name);
        Assert.Equal("new", GetStringValue(server.Get("host")));
        Assert.Null(root.Get("Server"));
    }

    [Fact]
    public void SetAtPath_ThrowsWhenIntermediateNodeIsNotAnObject()
    {
        var root = new MutableJsonObject();
        root.Set("server", new MutableJsonString("localhost"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            root.SetAtPath(["server", "host"], new MutableJsonString("new")));

        Assert.Contains("server", exception.Message);
    }

    [Fact]
    public void RemoveAtPath_RemovesLeafProperty()
    {
        var root = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "server": { "host": "localhost", "port": 8080 } }"""u8);

        var removed = root.RemoveAtPath(["server", "host"]);

        Assert.True(removed);
        var server = Assert.IsType<MutableJsonObject>(root.Get("server"));
        Assert.Null(server.Get("host"));
        Assert.NotNull(server.Get("port"));
    }

    [Fact]
    public void RemoveAtPath_WithPruneEmptyAncestors_RemovesEmptyParents()
    {
        var root = (MutableJsonObject)MutableJsonDocument.Parse(
            """{ "server": { "ssl": { "enabled": true } } }"""u8);

        var removed = root.RemoveAtPath(
            ["server", "ssl", "enabled"],
            new MutableJsonRemovePathOptions { PruneEmptyAncestors = true });

        Assert.True(removed);
        Assert.Empty(root.Properties);
    }

    [Fact]
    public void RemoveAtPath_WithCaseInsensitiveMatching_RemovesLastMatchingProperty()
    {
        var root = new MutableJsonObject();
        root.Set("Property", new MutableJsonString("first"));
        root.Set("propertyX", new MutableJsonString("keep"));
        root.Set("property", new MutableJsonString("last"));

        var removed = root.RemoveAtPath(
            ["PROPERTY"],
            new MutableJsonRemovePathOptions { PropertyNameCaseInsensitive = true });

        Assert.True(removed);
        Assert.Equal(2, root.Properties.Count);
        Assert.Equal("Property", root.Properties[0].Name);
        Assert.Equal("keep", GetStringValue(root.Get("propertyX")));
    }

    [Fact]
    public void PathOperations_RejectEmptyPaths()
    {
        var root = new MutableJsonObject();
        var pathSegments = Array.Empty<string>();

        Assert.Throws<ArgumentException>(() => root.GetAtPath(pathSegments));
        Assert.Throws<ArgumentException>(() => root.SetAtPath(pathSegments, new MutableJsonBool(true)));
        Assert.Throws<ArgumentException>(() => root.RemoveAtPath(pathSegments));
    }

    private static string GetStringValue(MutableJsonNode? node)
    {
        var value = Assert.IsType<MutableJsonString>(node);
        return System.Text.Encoding.UTF8.GetString(value.ValueUtf8);
    }
}
