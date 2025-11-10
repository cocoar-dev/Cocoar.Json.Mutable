using System.Diagnostics;
using System.Text.Json;

namespace Cocoar.Json.Mutable;

[DebuggerDisplay("{ToDebugString(),nq}")]
public abstract class MutableJsonNode
{
    public abstract JsonValueKind Kind { get; }
    
    public abstract void WriteTo(Utf8JsonWriter writer);
    
    public override string ToString()
    {
        return $"[{GetType().Name} Kind={Kind}]";
    }
    
    public virtual string ToDebugString()
    {
        return this switch
        {
            MutableJsonString str => $"[String: \"{GetPreview(str.ValueUtf8, 16)}\"]",
            MutableJsonNumber num => $"[Number: {GetPreview(num.ValueUtf8, 16)}]",
            MutableJsonObject obj => $"[Object: {obj.Properties.Count} properties]",
            MutableJsonArray arr => $"[Array: {arr.Items.Count} items]",
            MutableJsonBool b => $"[Bool: {(b.Kind == JsonValueKind.True ? "true" : "false")}]",
            MutableJsonNull => "[Null]",
            _ => ToString()
        };
    }
    
    private static string GetPreview(ReadOnlySpan<byte> utf8, int maxLength)
    {
        if (utf8.Length == 0)
            return "(empty)";
        
        if (utf8.Length <= maxLength)
            return System.Text.Encoding.UTF8.GetString(utf8);
        
        var preview = System.Text.Encoding.UTF8.GetString(utf8.Slice(0, maxLength));
        return $"{preview}... ({utf8.Length} bytes total)";
    }
    
    internal abstract MutableJsonNode CloneCore();
}
