using System.Text;
using System.Text.Json;
using Xunit;

namespace Cocoar.Json.Mutable.Tests;

/// <summary>
/// A string is held as its value, not as the escaped text it was written with: what goes in through a
/// parse comes out of a write as the same string, however the source chose to escape it.
/// </summary>
public class StringEscapeTests
{
    private static string RoundTrip(string value)
    {
        // System.Text.Json's default encoder escapes quotes, backslashes, control characters, non-ASCII
        // and the HTML-sensitive characters — the form every serialised provider value arrives in.
        var json = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string> { ["v"] = value });
        var node = MutableJsonDocument.Parse(json);
        var written = MutableJsonDocument.ToUtf8Bytes(node);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(written)!["v"];
    }

    [Theory]
    [InlineData("plain")]
    [InlineData("Größe 'ß' €")]
    [InlineData("pa\"ss\\word")]
    [InlineData("line one\nline two\ttab")]
    [InlineData("a+b&c<d>e`f")]
    [InlineData("{\"type\":\"service_account\",\"private_key\":\"-----BEGIN-----\\nabc\\n-----END-----\\n\"}")]
    [InlineData("emoji 😀 and surrogate pair")]
    [InlineData("")]
    public void A_string_survives_parse_and_write(string value)
    {
        Assert.Equal(value, RoundTrip(value));
    }

    [Fact]
    public void A_parsed_string_exposes_its_value_not_the_escaped_source()
    {
        var node = (MutableJsonObject)MutableJsonDocument.Parse("""{ "v": "a\nb ö \"q\" back\\slash" }"""u8);

        var text = (MutableJsonString)node.Get("v")!;

        Assert.Equal("a\nb ö \"q\" back\\slash", Encoding.UTF8.GetString(text.ValueUtf8));
    }

    [Fact]
    public void An_escaped_property_name_is_the_same_property_as_its_plain_form()
    {
        var node = (MutableJsonObject)MutableJsonDocument.Parse("""{ "Größe": 1 }"""u8);

        Assert.NotNull(node.Get("Größe"));
        var written = JsonSerializer.Deserialize<Dictionary<string, int>>(MutableJsonDocument.ToUtf8Bytes(node))!;
        Assert.Equal(1, written["Größe"]);
    }

    [Fact]
    public void A_long_escaped_string_survives()
    {
        var value = string.Concat(Enumerable.Repeat("ö\"\\\n+", 2_000));

        Assert.Equal(value, RoundTrip(value));
    }

    [Fact]
    public void A_merge_keeps_the_value_of_an_escaped_string()
    {
        var target = (MutableJsonObject)MutableJsonDocument.Parse("""{ "a": "x" }"""u8);
        var source = (MutableJsonObject)MutableJsonDocument.Parse("""{ "a": "Köln \"am\" Rhein", "b": "1\n2" }"""u8);

        MutableJsonMerge.Merge(target, source);

        var written = JsonSerializer.Deserialize<Dictionary<string, string>>(MutableJsonDocument.ToUtf8Bytes(target))!;
        Assert.Equal("Köln \"am\" Rhein", written["a"]);
        Assert.Equal("1\n2", written["b"]);
    }
}
