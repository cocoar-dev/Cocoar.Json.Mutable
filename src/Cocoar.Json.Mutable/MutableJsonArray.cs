using System.Text.Json;

namespace Cocoar.Json.Mutable;

public sealed class MutableJsonArray : MutableJsonNode
{
    private readonly List<MutableJsonNode> _items = new();
    
    public override JsonValueKind Kind => JsonValueKind.Array;
    
    public IReadOnlyList<MutableJsonNode> Items => _items;
    
    public void Add(MutableJsonNode item)
    {
        _items.Add(item);
    }
    
    public MutableJsonNode this[int index]
    {
        get => _items[index];
    }
    
    public override void WriteTo(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();
        foreach (var item in _items)
        {
            item.WriteTo(writer);
        }
        writer.WriteEndArray();
    }
    
    internal override MutableJsonNode CloneCore()
    {
        var clone = new MutableJsonArray();
        
        foreach (var item in _items)
        {
            var itemClone = item.CloneCore();
            clone._items.Add(itemClone);
        }
        
        return clone;
    }
}
