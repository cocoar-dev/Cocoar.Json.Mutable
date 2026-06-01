namespace Cocoar.Json.Mutable;

public sealed class MutableJsonMergeOptions
{
    public bool PropertyNameCaseInsensitive { get; init; }
}

public static class MutableJsonMerge
{
    public static MutableJsonObject Merge(
        MutableJsonObject target,
        MutableJsonObject source,
        MutableJsonMergeOptions? options = null)
    {
        return MergeCore(target, source, cloneSourceValues: true, options);
    }

    public static MutableJsonObject MergeDestructive(
        MutableJsonObject target,
        MutableJsonObject source,
        MutableJsonMergeOptions? options = null)
    {
        return MergeCore(target, source, cloneSourceValues: false, options);
    }

    private static MutableJsonObject MergeCore(
        MutableJsonObject target,
        MutableJsonObject source,
        bool cloneSourceValues,
        MutableJsonMergeOptions? options)
    {
        var propertyIndex = options?.PropertyNameCaseInsensitive == true
            ? BuildCaseInsensitiveIndex(target)
            : null;

        foreach (var sourceProp in source.Properties)
        {
            var existingIndex = propertyIndex is not null
                ? FindCaseInsensitiveIndex(propertyIndex, sourceProp.Name)
                : target.FindPropertyIndex(sourceProp.NameUtf8.Span);

            var existingNode = existingIndex >= 0
                ? target.Properties[existingIndex].Value
                : null;
            
            if (existingNode is MutableJsonObject existingObj && sourceProp.Value is MutableJsonObject sourceObj)
            {
                MergeCore(existingObj, sourceObj, cloneSourceValues, options);
            }
            else
            {
                var value = cloneSourceValues ? Clone(sourceProp.Value) : sourceProp.Value;
                if (existingIndex >= 0)
                {
                    target.SetValueAt(existingIndex, value);
                }
                else
                {
                    target.Set(sourceProp.NameUtf8.Span, value);
                    propertyIndex?.Add(sourceProp.Name, target.Properties.Count - 1);
                }
            }
        }
        
        return target;
    }

    private static Dictionary<string, int> BuildCaseInsensitiveIndex(MutableJsonObject target)
    {
        var propertyIndex = new Dictionary<string, int>(
            target.Properties.Count,
            StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < target.Properties.Count; i++)
        {
            propertyIndex[target.Properties[i].Name] = i;
        }

        return propertyIndex;
    }

    private static int FindCaseInsensitiveIndex(Dictionary<string, int> propertyIndex, string propertyName)
    {
        return propertyIndex.TryGetValue(propertyName, out var index) ? index : -1;
    }
    
    public static MutableJsonNode Clone(MutableJsonNode node)
    {
        return node switch
        {
            MutableJsonObject obj => CloneObject(obj),
            MutableJsonArray arr => CloneArray(arr),
            MutableJsonString str => CloneString(str),
            MutableJsonNumber num => CloneNumber(num),
            MutableJsonBool b => CloneBool(b),
            MutableJsonNull => MutableJsonNull.Instance,
            _ => throw new InvalidOperationException($"Unknown node type: {node.GetType()}")
        };
    }

    private static MutableJsonObject CloneObject(MutableJsonObject obj)
    {
        var clone = new MutableJsonObject();
        foreach (var prop in obj.Properties)
        {
            var clonedValue = Clone(prop.Value);
            clone.Set(prop.NameUtf8.Span, clonedValue);
        }
        return clone;
    }

    private static MutableJsonArray CloneArray(MutableJsonArray arr)
    {
        var clone = new MutableJsonArray();
        foreach (var item in arr.Items)
        {
            var clonedItem = Clone(item);
            clone.Add(clonedItem);
        }
        return clone;
    }

    private static MutableJsonString CloneString(MutableJsonString str)
    {
        var bytes = new byte[str.ValueUtf8.Length];
        str.ValueUtf8.CopyTo(bytes);
        return new MutableJsonString(bytes);
    }
    
    private static MutableJsonNumber CloneNumber(MutableJsonNumber num)
    {
        var raw = num.ValueUtf8;
        var copy = new byte[raw.Length];
        raw.CopyTo(copy);
        return MutableJsonNumber.FromOwned(copy);
    }
    
    private static MutableJsonBool CloneBool(MutableJsonBool b)
    {
        return new MutableJsonBool(b.Kind == System.Text.Json.JsonValueKind.True);
    }
}
