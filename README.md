# Cocoar.Json.Mutable

**Fast. Mutable. Merge-Focused.**

A high-performance mutable JSON document object model (DOM) optimized for merging multiple JSON objects efficiently. Built on UTF-8 byte arrays for zero-overhead string handling.

## Why Use This?

**Problem:** Most JSON libraries either:
- Offer immutable structures (hard to modify/merge)
- Use managed strings (overhead for large documents)
- Lack efficient deep-merge capabilities

**Solution:** `Cocoar.Json.Mutable` provides a mutable JSON DOM that:
- Stores content as UTF-8 `byte[]` for performance
- Supports efficient deep merging of JSON objects
- Allows in-place modifications
- No security overhead from memory zeroing

## Key Features

✅ **Mutable JSON structure** - Create, modify, and merge JSON documents in-memory  
✅ **UTF-8 native** - Works directly with UTF-8 bytes using `ReadOnlySpan<byte>` and `ReadOnlyMemory<byte>`  
✅ **Efficient deep merge** - Merge multiple JSON objects with recursive merging  
✅ **High performance** - No unnecessary memory zeroing or security overhead  
✅ **Built on System.Text.Json** - Uses `Utf8JsonReader` and `Utf8JsonWriter`  
✅ **Provider-friendly** - Accepts `ReadOnlyMemory<byte>` from data providers

## Documentation

Full documentation is available at [docs.cocoar.dev/json-mutable](https://docs.cocoar.dev/json-mutable/).

## Quick Start

```csharp
using Cocoar.Json.Mutable;

// Create an empty document
var doc = new MutableJsonObject();

// Parse from ReadOnlySpan<byte> or ReadOnlyMemory<byte>
var config1 = MutableJsonDocument.Parse("{\"server\": {\"port\": 8080}}"u8);
var config2 = MutableJsonDocument.Parse("{\"server\": {\"host\": \"localhost\"}, \"debug\": true}"u8);

// Or from ReadOnlyMemory<byte> (perfect for providers!)
ReadOnlyMemory<byte> memoryFromProvider = GetJsonFromProvider();
var config3 = MutableJsonDocument.Parse(memoryFromProvider);

// Merge them together
MutableJsonMerge.Merge(doc, (MutableJsonObject)config1);
MutableJsonMerge.Merge(doc, (MutableJsonObject)config2);

// Result: {"server": {"port": 8080, "host": "localhost"}, "debug": true}

// Developer-friendly string API
doc.Set("version", new MutableJsonString("1.0.0"));
doc.Set("maxConnections", new MutableJsonNumber(100));
doc.Set("enabled", new MutableJsonBool(true));

// Get values using strings (much easier!)
var serverNode = doc.Get("server") as MutableJsonObject;
var port = serverNode?.Get("port") as MutableJsonNumber;

// Traverse nested objects with explicit path segments
var nestedPort = doc.GetAtPath(["server", "port"]) as MutableJsonNumber;

// Or use UTF-8 bytes for zero allocations
var versionNode = doc.Get("version"u8);

// Serialize to JSON
var jsonBytes = MutableJsonDocument.ToUtf8Bytes(doc);
```

## Merge Strategies

```csharp
// Non-destructive merge (clones source values)
MutableJsonMerge.Merge(target, source);

// Destructive merge (moves source values, faster)
MutableJsonMerge.MergeDestructive(target, source);

// Optional: match property names case-insensitively during merge.
// The target property's casing is preserved.
MutableJsonMerge.Merge(
    target,
    source,
    new MutableJsonMergeOptions { PropertyNameCaseInsensitive = true });

// Clone nodes when needed
var cloned = MutableJsonMerge.Clone(original);
```

## Path Operations

```csharp
// Get a nested value
var host = config.GetAtPath(["server", "host"]);

// Set a nested value and create missing intermediate objects
config.SetAtPath(["server", "ssl", "enabled"], new MutableJsonBool(true));

// Remove a nested value and prune empty parents
config.RemoveAtPath(
    ["server", "ssl", "enabled"],
    new MutableJsonRemovePathOptions { PruneEmptyAncestors = true });
```

Path APIs use explicit segments, not dotted strings, so property names like `"server.host"` remain unambiguous.

## Use Cases

- Configuration merging (base config + environment overrides + user settings)
- API response aggregation
- JSON document transformations
- Building complex JSON structures programmatically

## When NOT to Use This

- If you need immutable JSON structures (use System.Text.Json.JsonDocument)
- If you're handling sensitive data that must be securely erased from memory (this library does NOT provide memory zeroing)
- If you only need to read JSON once without modifications (use Utf8JsonReader directly)
- If you need thread-safe concurrent access (this library is NOT thread-safe - use external synchronization if sharing instances across threads)

## License

See [LICENSE](LICENSE) file for details.
