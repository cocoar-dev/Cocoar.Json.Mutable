using System.Text.Json;

namespace Cocoar.Json.Mutable;

public sealed class MutableJsonObject : MutableJsonNode
{
    public static int DefaultIndexThreshold { get; set; } = 12;
    
    private readonly List<Property> _properties = new();
    private Dictionary<string, int>? _index;
    private readonly int _indexThreshold;
    
    public override JsonValueKind Kind => JsonValueKind.Object;
    
    public MutableJsonObject(int indexThreshold = -1)
    {
        _indexThreshold = indexThreshold >= 0 ? indexThreshold : DefaultIndexThreshold;
    }
    
    public IReadOnlyList<Property> Properties => _properties;
    
    public MutableJsonNode? Get(ReadOnlySpan<byte> nameUtf8)
    {
        if (_index is not null)
        {
            var key = System.Text.Encoding.UTF8.GetString(nameUtf8);
            if (_index.TryGetValue(key, out var idx))
                return _properties[idx].Value;
            return null;
        }
        
        foreach (var prop in _properties)
        {
            if (prop.NameUtf8.Span.SequenceEqual(nameUtf8))
                return prop.Value;
        }
        return null;
    }

    public bool Remove(ReadOnlySpan<byte> nameUtf8)
    {
        int idx = FindPropertyIndex(nameUtf8);
        if (idx < 0)
            return false;
        
        _properties.RemoveAt(idx);
        if (_index is not null)
            RebuildIndex();
        return true;
    }
    
    public void Set(ReadOnlySpan<byte> nameUtf8, MutableJsonNode value)
    {
        int existingIndex = FindPropertyIndex(nameUtf8);
        if (existingIndex >= 0)
        {
            _properties[existingIndex] = new Property(nameUtf8.ToArray(), value);
            return;
        }
        
        _properties.Add(new Property(nameUtf8.ToArray(), value));
        
        if (_properties.Count >= _indexThreshold && _index is null)
        {
            BuildIndex();
        }
        else if (_index is not null)
        {
            var key = System.Text.Encoding.UTF8.GetString(nameUtf8);
            _index[key] = _properties.Count - 1;
        }
    }
    
    private int FindPropertyIndex(ReadOnlySpan<byte> nameUtf8)
    {
        if (_index is not null)
        {
            var key = System.Text.Encoding.UTF8.GetString(nameUtf8);
            if (_index.TryGetValue(key, out var idx))
                return idx;
            return -1;
        }
        
        for (int i = 0; i < _properties.Count; i++)
        {
            if (_properties[i].NameUtf8.Span.SequenceEqual(nameUtf8))
                return i;
        }
        return -1;
    }
    
    private void BuildIndex()
    {
        _index = new Dictionary<string, int>(_properties.Count);
        for (int i = 0; i < _properties.Count; i++)
        {
            var key = System.Text.Encoding.UTF8.GetString(_properties[i].NameUtf8.Span);
            _index[key] = i;
        }
    }
    
    private void RebuildIndex()
    {
        if (_index is null) return;
        _index.Clear();
        for (int i = 0; i < _properties.Count; i++)
        {
            var key = System.Text.Encoding.UTF8.GetString(_properties[i].NameUtf8.Span);
            _index[key] = i;
        }
    }
    
    public override void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        foreach (var prop in _properties)
        {
            writer.WritePropertyName(prop.NameUtf8.Span);
            prop.Value.WriteTo(writer);
        }
        writer.WriteEndObject();
    }
    
    internal override void WriteToMutable(MutableJsonWriter writer)
    {
        writer.WriteStartObject();
        foreach (var prop in _properties)
        {
            writer.WritePropertyName(prop.NameUtf8.Span);
            prop.Value.WriteToMutable(writer);
        }
        writer.WriteEndObject();
    }
    
    internal override MutableJsonNode CloneCore()
    {
        var clone = new MutableJsonObject();
        foreach (var prop in _properties)
        {
            var clonedValue = prop.Value.CloneCore();
            clone.Set(prop.NameUtf8.Span, clonedValue);
        }
        return clone;
    }
    
    public readonly struct Property
    {
        private readonly byte[] _nameUtf8;
        private readonly MutableJsonNode _value;
        
        internal Property(byte[] nameUtf8, MutableJsonNode value)
        {
            _nameUtf8 = nameUtf8;
            _value = value;
        }
        
        public ReadOnlyMemory<byte> NameUtf8 => _nameUtf8;
        
        public MutableJsonNode Value => _value;
        
        public string Name => System.Text.Encoding.UTF8.GetString(_nameUtf8);
    }
}
