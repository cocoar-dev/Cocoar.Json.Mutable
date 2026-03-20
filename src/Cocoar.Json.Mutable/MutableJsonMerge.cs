namespace Cocoar.Json.Mutable;

public static class MutableJsonMerge
{
    public static MutableJsonObject Merge(MutableJsonObject target, MutableJsonObject source)
    {
        foreach (var sourceProp in source.Properties)
        {
            var existingNode = target.Get(sourceProp.NameUtf8.Span);
            
            if (existingNode is MutableJsonObject existingObj && sourceProp.Value is MutableJsonObject sourceObj)
            {
                Merge(existingObj, sourceObj);
            }
            else
            {
                var cloned = Clone(sourceProp.Value);
                target.Set(sourceProp.NameUtf8.Span, cloned);
            }
        }
        
        return target;
    }
    
    public static MutableJsonObject MergeDestructive(MutableJsonObject target, MutableJsonObject source)
    {
        foreach (var sourceProp in source.Properties)
        {
            var existingNode = target.Get(sourceProp.NameUtf8.Span);
            if (existingNode is MutableJsonObject existingObj && sourceProp.Value is MutableJsonObject sourceObj)
            {
                MergeDestructive(existingObj, sourceObj);
            }
            else
            {
                target.Set(sourceProp.NameUtf8.Span, sourceProp.Value);
            }
        }
        return target;
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
