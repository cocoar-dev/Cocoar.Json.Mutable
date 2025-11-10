using System.Buffers;
using System.Text.Json;

namespace Cocoar.Json.Mutable;

public static class MutableJsonDocument
{
    public static MutableJsonNode Parse(ReadOnlySpan<byte> utf8Json)
    {
        return MutableJsonParser.Parse(utf8Json);
    }
    
    public static MutableJsonNode ParseFromStream(Stream stream)
    {
        return MutableJsonParser.ParseFromStream(stream);
    }
    
    public static byte[] ToUtf8Bytes(MutableJsonNode node)
    {
        using var buffer = new PooledBufferWriter();
        var writer = new MutableJsonWriter(buffer);
        node.WriteToMutable(writer);
        var result = new byte[buffer.WrittenCount];
        buffer.WrittenSpan.CopyTo(result);
        return result;
    }

    public static (byte[] RentedArray, int Length) ToUtf8BytesPooled(MutableJsonNode node)
    {
        using var buffer = new PooledBufferWriter();
        var writer = new MutableJsonWriter(buffer);
        node.WriteToMutable(writer);
        
        var length = buffer.WrittenCount;
        var rented = ArrayPool<byte>.Shared.Rent(length);
        buffer.WrittenSpan.CopyTo(rented);
        
        return (rented, length);
    }

    public static int TryWriteToSpan(MutableJsonNode node, Span<byte> destination)
    {
        using var buffer = new PooledBufferWriter();
        var writer = new MutableJsonWriter(buffer);
        node.WriteToMutable(writer);
        
        if (buffer.WrittenCount > destination.Length)
            return -1;
        
        buffer.WrittenSpan.CopyTo(destination);
        return buffer.WrittenCount;
    }
    
    public static void WriteTo(MutableJsonNode node, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        
        using var buffer = new PooledBufferWriter();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            node.WriteTo(writer);
            writer.Flush();
        }
        
        stream.Write(buffer.WrittenSpan);
    }
    
    public static void WriteTo(MutableJsonNode node, Stream stream, JsonWriterOptions options)
    {
        ArgumentNullException.ThrowIfNull(stream);
        
        using var buffer = new PooledBufferWriter();
        using (var writer = new Utf8JsonWriter(buffer, options))
        {
            node.WriteTo(writer);
            writer.Flush();
        }
        
        stream.Write(buffer.WrittenSpan);
    }
    
    public static async Task WriteToAsync(MutableJsonNode node, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        
        using var buffer = new PooledBufferWriter();
        await using (var writer = new Utf8JsonWriter(buffer))
        {
            node.WriteTo(writer);
            await writer.FlushAsync(cancellationToken);
        }
        
        await stream.WriteAsync(buffer.WrittenSpan.ToArray(), cancellationToken);
    }
    
    public static async Task WriteToAsync(MutableJsonNode node, Stream stream, JsonWriterOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        
        using var buffer = new PooledBufferWriter();
        await using (var writer = new Utf8JsonWriter(buffer, options))
        {
            node.WriteTo(writer);
            await writer.FlushAsync(cancellationToken);
        }
        
        await stream.WriteAsync(buffer.WrittenSpan.ToArray(), cancellationToken);
    }
}
