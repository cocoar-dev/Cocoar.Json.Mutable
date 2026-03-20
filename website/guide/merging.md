# Merging

Merging is the core operation of `Cocoar.Json.Mutable`. The `MutableJsonMerge` class provides two merge strategies and a deep clone.

## Non-Destructive Merge

`Merge` copies values from the source into the target. The source remains unchanged:

```csharp
var target = new MutableJsonObject();
var source = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "port": 8080 } }
    """u8);

MutableJsonMerge.Merge(target, source);

// target now has { "server": { "port": 8080 } }
// source is unchanged — its values were cloned
```

Use this when you need the source object after merging (e.g., to merge it into multiple targets).

## Destructive Merge

`MergeDestructive` moves values from the source into the target. The source should not be used after the call:

```csharp
var target = new MutableJsonObject();
var source = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "port": 8080 } }
    """u8);

MutableJsonMerge.MergeDestructive(target, source);

// target now has { "server": { "port": 8080 } }
// source still has its property list, but values are shared with target — do not use source
```

Destructive merge is faster because it avoids cloning. Use it when you parse a document only to merge it and discard the source.

## Merge Behavior

Both merge strategies follow the same rules:

| Source Value | Target Has Key? | Existing Target Value | Result |
|---|---|---|---|
| Object | Yes | Object | Recursive merge into existing object |
| Object | Yes | Non-object | Source replaces target value |
| Object | No | — | Source added to target |
| Non-object | Yes | Any | Source replaces target value |
| Non-object | No | — | Source added to target |

The key distinction: when both source and target have the same property and both are objects, the merge recurses into the nested object rather than replacing it.

### Example: Recursive Merge

```csharp
var target = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "port": 8080, "host": "0.0.0.0" }, "debug": false }
    """u8);

var source = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "host": "localhost" }, "version": "2.0" }
    """u8);

MutableJsonMerge.Merge(target, source);

// Result:
// {
//   "server": { "port": 8080, "host": "localhost" },
//   "debug": false,
//   "version": "2.0"
// }
```

- `server.host` was overwritten (`"localhost"` replaces `"0.0.0.0"`)
- `server.port` was preserved (source didn't set it)
- `debug` was preserved (source didn't set it)
- `version` was added (new property)

## Multi-Source Merging

Merge multiple sources in sequence — last write wins:

```csharp
var result = new MutableJsonObject();

var sources = new[]
{
    """{ "log": "info",  "port": 3000 }""",
    """{ "port": 8080 }""",
    """{ "log": "debug", "host": "localhost" }""",
};

foreach (var json in sources)
{
    var source = (MutableJsonObject)MutableJsonDocument.Parse(
        System.Text.Encoding.UTF8.GetBytes(json));
    MutableJsonMerge.MergeDestructive(result, source);
}

// Result: { "log": "debug", "port": 8080, "host": "localhost" }
```

This pattern is ideal for layered configuration: base settings, environment overrides, user preferences.

## Deep Clone

Clone any node to create a fully independent copy:

```csharp
var original = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "items": [1, 2, 3], "name": "test" }
    """u8);

var copy = (MutableJsonObject)MutableJsonMerge.Clone(original);

// Modify the copy — original is unaffected
copy.Set("name", new MutableJsonString("modified"));
```

Clone works recursively on all node types: objects, arrays, strings, numbers, booleans, and null.

## Choosing a Strategy

| Scenario | Strategy | Why |
|---|---|---|
| Merge parsed config, discard source | `MergeDestructive` | Faster — no cloning overhead |
| Merge source into multiple targets | `Merge` | Source stays valid for reuse |
| Keep a backup before merging | `Clone` + `MergeDestructive` | Clone the target first, then merge fast |
