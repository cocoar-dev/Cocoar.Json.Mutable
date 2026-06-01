# Path Operations

`MutableJsonObject.Get(...)`, `Set(...)`, and `Remove(...)` only work on direct properties of the current object. If you need to traverse nested objects, use the path APIs in `MutableJsonPath`.

## Why a Separate Path API?

This distinction is intentional:

```csharp
var obj = new MutableJsonObject();
obj.Set("server.host", new MutableJsonString("literal property"));

obj.Get("server.host");           // direct property lookup
obj.GetAtPath(["server", "host"]); // nested traversal
```

Using segment arrays keeps path traversal unambiguous. A property name like `"server.host"` is treated as one real property when you pass it as one segment:

```csharp
var port = obj.GetAtPath(["server.host", "port"]);
```

## Getting a Nested Value

```csharp
var config = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "host": "localhost", "port": 8080 } }
    """u8);

var host = config.GetAtPath(["server", "host"]);
```

If any segment is missing, or traversal hits a non-object before the final segment, `GetAtPath` returns `null`.

## Setting a Nested Value

```csharp
var config = new MutableJsonObject();

config.SetAtPath(
    ["server", "host"],
    new MutableJsonString("localhost"));
```

Missing intermediate objects are created automatically. The example above produces:

```json
{ "server": { "host": "localhost" } }
```

If traversal hits an existing non-object before the final segment, `SetAtPath` throws `InvalidOperationException` instead of silently replacing that value.

## Removing a Nested Value

```csharp
var config = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "ssl": { "enabled": true } } }
    """u8);

config.RemoveAtPath(
    ["server", "ssl", "enabled"],
    new MutableJsonRemovePathOptions { PruneEmptyAncestors = true });
```

With `PruneEmptyAncestors = true`, empty parent objects created only to hold the removed value are pruned recursively.

## Case-Insensitive Matching

Path operations are case-sensitive by default, just like direct object operations. Enable case-insensitive matching explicitly:

```csharp
var config = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "host": "old" } }
    """u8);

config.SetAtPath(
    ["Server", "Host"],
    new MutableJsonString("new"),
    new MutableJsonPathOptions { PropertyNameCaseInsensitive = true });
```

When a matching target property already exists, the target casing is preserved. In the example above, the result still uses `"server"` and `"host"`.

## API Summary

```csharp
MutableJsonNode? GetAtPath(string[] pathSegments, MutableJsonPathOptions? options = null)
void SetAtPath(string[] pathSegments, MutableJsonNode value, MutableJsonPathOptions? options = null)
bool RemoveAtPath(string[] pathSegments, MutableJsonRemovePathOptions? options = null)
```

## Current Scope

The current path APIs operate on object segments only.

- No dotted-string path parsing
- No array index syntax
- No automatic replacement of non-object intermediates

That keeps the API predictable and avoids ambiguity around property names containing `.`.
