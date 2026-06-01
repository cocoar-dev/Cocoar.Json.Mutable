namespace Cocoar.Json.Mutable;

public class MutableJsonPathOptions
{
    public bool PropertyNameCaseInsensitive { get; init; }
}

public sealed class MutableJsonRemovePathOptions : MutableJsonPathOptions
{
    public bool PruneEmptyAncestors { get; init; }
}

public static class MutableJsonPath
{
    public static MutableJsonNode? GetAtPath(
        this MutableJsonObject root,
        string[] pathSegments,
        MutableJsonPathOptions? options = null)
    {
        return GetAtPath(root, (IReadOnlyList<string>)pathSegments, options);
    }

    public static MutableJsonNode? GetAtPath(
        this MutableJsonObject root,
        IReadOnlyList<string> pathSegments,
        MutableJsonPathOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ValidatePathSegments(pathSegments);

        MutableJsonNode current = root;
        var propertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive == true;

        for (var i = 0; i < pathSegments.Count; i++)
        {
            if (current is not MutableJsonObject currentObject)
                return null;

            var propertyIndex = currentObject.FindPropertyIndex(pathSegments[i], propertyNameCaseInsensitive);
            if (propertyIndex < 0)
                return null;

            current = currentObject.Properties[propertyIndex].Value;
        }

        return current;
    }

    public static void SetAtPath(
        this MutableJsonObject root,
        string[] pathSegments,
        MutableJsonNode value,
        MutableJsonPathOptions? options = null)
    {
        SetAtPath(root, (IReadOnlyList<string>)pathSegments, value, options);
    }

    public static void SetAtPath(
        this MutableJsonObject root,
        IReadOnlyList<string> pathSegments,
        MutableJsonNode value,
        MutableJsonPathOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(value);
        ValidatePathSegments(pathSegments);

        var current = root;
        var propertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive == true;

        for (var i = 0; i < pathSegments.Count - 1; i++)
        {
            var segment = pathSegments[i];
            var propertyIndex = current.FindPropertyIndex(segment, propertyNameCaseInsensitive);

            if (propertyIndex < 0)
            {
                var createdObject = new MutableJsonObject();
                current.Set(segment, createdObject);
                current = createdObject;
                continue;
            }

            if (current.Properties[propertyIndex].Value is not MutableJsonObject nextObject)
            {
                throw new InvalidOperationException(
                    $"Cannot traverse path segment '{segment}' because the existing value is not an object.");
            }

            current = nextObject;
        }

        var leafSegment = pathSegments[^1];
        var existingLeafIndex = current.FindPropertyIndex(leafSegment, propertyNameCaseInsensitive);
        if (existingLeafIndex >= 0)
        {
            current.SetValueAt(existingLeafIndex, value);
            return;
        }

        current.Set(leafSegment, value);
    }

    public static bool RemoveAtPath(
        this MutableJsonObject root,
        string[] pathSegments,
        MutableJsonRemovePathOptions? options = null)
    {
        return RemoveAtPath(root, (IReadOnlyList<string>)pathSegments, options);
    }

    public static bool RemoveAtPath(
        this MutableJsonObject root,
        IReadOnlyList<string> pathSegments,
        MutableJsonRemovePathOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ValidatePathSegments(pathSegments);

        var propertyNameCaseInsensitive = options?.PropertyNameCaseInsensitive == true;
        var pruneEmptyAncestors = options?.PruneEmptyAncestors == true;

        var parentChain = new List<(MutableJsonObject Parent, int PropertyIndex)>(pathSegments.Count - 1);
        var current = root;

        for (var i = 0; i < pathSegments.Count - 1; i++)
        {
            var propertyIndex = current.FindPropertyIndex(pathSegments[i], propertyNameCaseInsensitive);
            if (propertyIndex < 0)
                return false;

            if (current.Properties[propertyIndex].Value is not MutableJsonObject nextObject)
                return false;

            parentChain.Add((current, propertyIndex));
            current = nextObject;
        }

        var leafIndex = current.FindPropertyIndex(pathSegments[^1], propertyNameCaseInsensitive);
        if (leafIndex < 0)
            return false;

        current.RemoveAt(leafIndex);

        if (!pruneEmptyAncestors)
            return true;

        for (var i = parentChain.Count - 1; i >= 0; i--)
        {
            var (parent, propertyIndex) = parentChain[i];
            if (parent.Properties[propertyIndex].Value is not MutableJsonObject childObject)
                break;

            if (childObject.Properties.Count > 0)
                break;

            parent.RemoveAt(propertyIndex);
        }

        return true;
    }

    private static void ValidatePathSegments(IReadOnlyList<string> pathSegments)
    {
        ArgumentNullException.ThrowIfNull(pathSegments);

        if (pathSegments.Count == 0)
            throw new ArgumentException("Path must contain at least one segment.", nameof(pathSegments));

        for (var i = 0; i < pathSegments.Count; i++)
        {
            if (string.IsNullOrEmpty(pathSegments[i]))
            {
                throw new ArgumentException(
                    "Path segments must not be null or empty.",
                    nameof(pathSegments));
            }
        }
    }
}
