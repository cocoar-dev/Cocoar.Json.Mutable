using System.Text.Json;

namespace Cocoar.Json.Mutable;

public sealed class MutableJsonNumber : MutableJsonNode
{
    private readonly byte[] _rawUtf8;
    
    public override JsonValueKind Kind => JsonValueKind.Number;
    
    public MutableJsonNumber(ReadOnlySpan<byte> rawUtf8)
    {
        _rawUtf8 = rawUtf8.ToArray();
    }
    
    public MutableJsonNumber(int value)
    {
        _rawUtf8 = System.Text.Encoding.UTF8.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
    
    public MutableJsonNumber(long value)
    {
        _rawUtf8 = System.Text.Encoding.UTF8.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
    
    public MutableJsonNumber(double value)
    {
        _rawUtf8 = System.Text.Encoding.UTF8.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
    
    private MutableJsonNumber(byte[] rawUtf8, bool owned)
    {
        _rawUtf8 = rawUtf8;
    }
    
    public static MutableJsonNumber FromCopy(ReadOnlySpan<byte> rawUtf8)
    {
        return new MutableJsonNumber(rawUtf8);
    }
    
    public static MutableJsonNumber FromOwned(byte[] rawUtf8)
    {
        return new MutableJsonNumber(rawUtf8, owned: true);
    }
    
    public ReadOnlySpan<byte> ValueUtf8 => _rawUtf8;
    
    public override void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteRawValue(_rawUtf8, skipInputValidation: true);
    }
    
    internal override MutableJsonNode CloneCore()
    {
        var copy = new byte[_rawUtf8.Length];
        _rawUtf8.AsSpan().CopyTo(copy);
        return new MutableJsonNumber(copy, owned: true);
    }
}

public sealed class MutableJsonBool : MutableJsonNode
{
    private readonly bool _value;
    
    public override JsonValueKind Kind => _value ? JsonValueKind.True : JsonValueKind.False;
    
    public MutableJsonBool(bool value)
    {
        _value = value;
    }
    
    public override void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteBooleanValue(_value);
    }
    
    internal override MutableJsonNode CloneCore()
    {
        return new MutableJsonBool(_value);
    }
}

public sealed class MutableJsonNull : MutableJsonNode
{
    public static readonly MutableJsonNull Instance = new();
    
    private MutableJsonNull() { }
    
    public override JsonValueKind Kind => JsonValueKind.Null;
    
    public override void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteNullValue();
    }
    
    internal override MutableJsonNode CloneCore()
    {
        return Instance;
    }
}
