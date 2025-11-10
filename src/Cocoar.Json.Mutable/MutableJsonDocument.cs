using System.Buffers;
using System.Text.Json;

namespace Cocoar.Json.Mutable;

public static class MutableJsonDocument
{
    /// <summary>
    /// Parses UTF-8 JSON bytes into a mutable JSON node.
    /// </summary>
    public static MutableJsonNode Parse(byte[] utf8Json)
    {
        return MutableJsonParser.Parse(utf8Json);
    }
    
    /// <summary>
    /// Parses UTF-8 JSON bytes into a mutable JSON node.
    /// </summary>
    public static MutableJsonNode Parse(ReadOnlySpan<byte> utf8Json)
    {
        return MutableJsonParser.Parse(utf8Json);
    }
    
    /// <summary>
    /// Parses UTF-8 JSON bytes into a mutable JSON node from providers.
    /// </summary>
    public static MutableJsonNode Parse(ReadOnlyMemory<byte> utf8Json)
    {
        return MutableJsonParser.Parse(utf8Json.Span);
    }
    
    public static MutableJsonNode ParseFromStream(Stream stream)
    {
        return MutableJsonParser.ParseFromStream(stream);
    }
    
    public static byte[] ToUtf8Bytes(MutableJsonNode node)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            node.WriteTo(writer);
            writer.Flush();
        }
        return stream.ToArray();
    }
    
    public static void WriteTo(MutableJsonNode node, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var writer = new Utf8JsonWriter(stream);
        node.WriteTo(writer);
        writer.Flush();
    }
    
    public static void WriteTo(MutableJsonNode node, Stream stream, JsonWriterOptions options)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var writer = new Utf8JsonWriter(stream, options);
        node.WriteTo(writer);
        writer.Flush();
    }
    
    public static async Task WriteToAsync(MutableJsonNode node, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await using var writer = new Utf8JsonWriter(stream);
        node.WriteTo(writer);
        await writer.FlushAsync(cancellationToken);
    }
    
    public static async Task WriteToAsync(MutableJsonNode node, Stream stream, JsonWriterOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        await using var writer = new Utf8JsonWriter(stream, options);
        node.WriteTo(writer);
        await writer.FlushAsync(cancellationToken);
    }
}
