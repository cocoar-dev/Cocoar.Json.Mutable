using System.Buffers;
using System.Text.Json;

namespace Cocoar.Json.Mutable;

internal static class MutableJsonParser
{
    public static MutableJsonNode Parse(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json);
        
        if (!reader.Read())
            throw new JsonException("Empty JSON document");
        
        return ReadNode(ref reader);
    }

    public static MutableJsonNode ParseFromStream(Stream stream, int chunkSize = 64 * 1024)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead) throw new ArgumentException("Stream is not readable", nameof(stream));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(chunkSize);
        int bufferSize = buffer.Length;
        int totalBytes = 0;

        try
        {
            while (true)
            {
                if (totalBytes >= bufferSize - 1)
                {
                    int newSize = bufferSize * 2;
                    byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
                    buffer.AsSpan(0, totalBytes).CopyTo(newBuffer);
                    ArrayPool<byte>.Shared.Return(buffer);
                    buffer = newBuffer;
                    bufferSize = newBuffer.Length;
                }

                int bytesRead = stream.Read(buffer, totalBytes, bufferSize - totalBytes);
                if (bytesRead == 0)
                    break;

                totalBytes += bytesRead;
            }

            var result = Parse(buffer.AsSpan(0, totalBytes));
            return result;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static MutableJsonNode ReadNode(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartObject => ReadObject(ref reader),
            JsonTokenType.StartArray => ReadArray(ref reader),
            JsonTokenType.String => MutableJsonString.FromOwned(ReadUnescaped(ref reader)),
            JsonTokenType.Number => MutableJsonNumber.FromOwned(reader.ValueSpan.ToArray()),
            JsonTokenType.True => new MutableJsonBool(true),
            JsonTokenType.False => new MutableJsonBool(false),
            JsonTokenType.Null => MutableJsonNull.Instance,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    // Strings and property names are held as their VALUE in UTF-8, never in the escaped form of the source
    // text: WriteTo escapes on the way out, so keeping an escape sequence as written (a unicode escape, a
    // backslash-n, an escaped quote) would escape it twice and hand the consumer a different string than
    // the document contained.
    private static byte[] ReadUnescaped(ref Utf8JsonReader reader)
    {
        if (!reader.ValueIsEscaped)
            return reader.ValueSpan.ToArray();

        // An unescaped value is never longer than its escaped form.
        var escapedLength = reader.ValueSpan.Length;
        byte[]? rented = null;
        Span<byte> buffer = escapedLength <= 256
            ? stackalloc byte[256]
            : (rented = ArrayPool<byte>.Shared.Rent(escapedLength));
        try
        {
            var written = reader.CopyString(buffer);
            return buffer[..written].ToArray();
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static MutableJsonObject ReadObject(ref Utf8JsonReader reader)
    {
        var obj = new MutableJsonObject();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return obj;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Expected property name, got {reader.TokenType}");

            var propertyName = ReadUnescaped(ref reader);

            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON");

            var value = ReadNode(ref reader);
            obj.SetOwned(propertyName, value);
        }

        throw new JsonException("Unterminated object");
    }

    private static MutableJsonArray ReadArray(ref Utf8JsonReader reader)
    {
        var arr = new MutableJsonArray();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return arr;

            var value = ReadNode(ref reader);
            arr.Add(value);
        }

        throw new JsonException("Unterminated array");
    }
}
