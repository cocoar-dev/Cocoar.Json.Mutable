using System.Buffers;
using System.Text;

namespace Cocoar.Json.Mutable;

internal sealed class MutableJsonWriter
{
    private readonly IBufferWriter<byte> _buffer;
    private bool _firstProperty = true;
    private readonly Stack<bool> _containerStack = new();

    public MutableJsonWriter(IBufferWriter<byte> buffer)
    {
        _buffer = buffer;
    }

    public void WriteStartObject()
    {
        WriteByte((byte)'{');
        _containerStack.Push(_firstProperty);
        _firstProperty = true;
    }

    public void WriteEndObject()
    {
        WriteByte((byte)'}');
        if (_containerStack.Count > 0)
            _firstProperty = _containerStack.Pop();
    }

    public void WriteStartArray()
    {
        WriteByte((byte)'[');
        _containerStack.Push(_firstProperty);
        _firstProperty = true;
    }

    public void WriteEndArray()
    {
        WriteByte((byte)']');
        if (_containerStack.Count > 0)
            _firstProperty = _containerStack.Pop();
    }

    public void WritePropertyName(ReadOnlySpan<byte> utf8Name)
    {
        WriteCommaIfNeeded();
        WriteByte((byte)'"');
        WriteBytes(utf8Name);
        WriteByte((byte)'"');
        WriteByte((byte)':');
        // After property name, the next value should not have a comma before it
        _firstProperty = true;
    }

    public void WriteStringValue(ReadOnlySpan<byte> utf8Value)
    {
        WriteCommaIfNeeded();
        WriteByte((byte)'"');
        WriteEscapedString(utf8Value);
        WriteByte((byte)'"');
    }

    public void WriteNumberValue(ReadOnlySpan<byte> utf8Number)
    {
        WriteCommaIfNeeded();
        WriteBytes(utf8Number);
    }

    public void WriteBooleanValue(bool value)
    {
        WriteCommaIfNeeded();
        if (value)
            WriteBytes("true"u8);
        else
            WriteBytes("false"u8);
    }

    public void WriteNullValue()
    {
        WriteCommaIfNeeded();
        WriteBytes("null"u8);
    }

    private void WriteCommaIfNeeded()
    {
        if (!_firstProperty)
        {
            WriteByte((byte)',');
        }
        _firstProperty = false;
    }

    private void WriteByte(byte b)
    {
        var span = _buffer.GetSpan(1);
        span[0] = b;
        _buffer.Advance(1);
    }

    private void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        var span = _buffer.GetSpan(bytes.Length);
        bytes.CopyTo(span);
        _buffer.Advance(bytes.Length);
    }

    private void WriteEscapedString(ReadOnlySpan<byte> utf8Value)
    {
        // Fast path: no escaping needed
        if (!NeedsEscaping(utf8Value))
        {
            WriteBytes(utf8Value);
            return;
        }

        // Slow path: escape special characters
        Span<byte> hex = stackalloc byte[6]; // Move out of loop
        for (int i = 0; i < utf8Value.Length; i++)
        {
            byte b = utf8Value[i];
            switch (b)
            {
                case (byte)'"':
                    WriteBytes("\\\""u8);
                    break;
                case (byte)'\\':
                    WriteBytes("\\\\"u8);
                    break;
                case (byte)'\n':
                    WriteBytes("\\n"u8);
                    break;
                case (byte)'\r':
                    WriteBytes("\\r"u8);
                    break;
                case (byte)'\t':
                    WriteBytes("\\t"u8);
                    break;
                case (byte)'\b':
                    WriteBytes("\\b"u8);
                    break;
                case (byte)'\f':
                    WriteBytes("\\f"u8);
                    break;
                case < 0x20:
                    // Control characters: write as \u00XX
                    hex[0] = (byte)'\\';
                    hex[1] = (byte)'u';
                    hex[2] = (byte)'0';
                    hex[3] = (byte)'0';
                    hex[4] = HexChar((b >> 4) & 0xF);
                    hex[5] = HexChar(b & 0xF);
                    WriteBytes(hex);
                    break;
                default:
                    WriteByte(b);
                    break;
            }
        }
    }

    private static bool NeedsEscaping(ReadOnlySpan<byte> utf8Value)
    {
        for (int i = 0; i < utf8Value.Length; i++)
        {
            byte b = utf8Value[i];
            if (b == '"' || b == '\\' || b < 0x20)
                return true;
        }
        return false;
    }

    private static byte HexChar(int value) => value < 10 
        ? (byte)('0' + value) 
        : (byte)('a' + (value - 10));
}
