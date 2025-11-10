using System.Text.Json;

namespace Cocoar.Json.Mutable;

public sealed class MutableJsonString : MutableJsonNode
{
    private byte[] _utf8Value;
    
    public override JsonValueKind Kind => JsonValueKind.String;
    
    public MutableJsonString(byte[] utf8Value)
    {
        _utf8Value = utf8Value;
    }
    
    public MutableJsonString(string value)
    {
        _utf8Value = System.Text.Encoding.UTF8.GetBytes(value);
    }
    
    public static MutableJsonString FromOwned(byte[] utf8Value)
    {
        return new MutableJsonString(utf8Value);
    }
    
    public static MutableJsonString FromCopy(ReadOnlySpan<byte> utf8Value)
    {
        return new MutableJsonString(utf8Value.ToArray());
    }
    
    public ReadOnlySpan<byte> ValueUtf8 => _utf8Value;
    
    public void Replace(byte[] newUtf8)
    {
        _utf8Value = newUtf8;
    }
    
    public override void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStringValue(_utf8Value);
    }
    
    internal override MutableJsonNode CloneCore()
    {
        var copy = new byte[_utf8Value.Length];
        _utf8Value.AsSpan().CopyTo(copy);
        return new MutableJsonString(copy);
    }
}
