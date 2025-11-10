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

// Set values directly
doc.Set("version"u8, new MutableJsonString("1.0.0"u8.ToArray()));

// Get values
var serverNode = doc.Get("server"u8);

// Serialize to JSON
var jsonBytes = MutableJsonDocument.ToUtf8Bytes(doc);
```

## Merge Strategies

```csharp
// Non-destructive merge (clones source values)
MutableJsonMerge.Merge(target, source);

// Destructive merge (moves source values, faster)
MutableJsonMerge.MergeDestructive(target, source);

// Clone nodes when needed
var cloned = MutableJsonMerge.Clone(original);
```

## Use Cases

- Configuration merging (base config + environment overrides + user settings)
- API response aggregation
- JSON document transformations
- Building complex JSON structures programmatically

## When NOT to Use This

- If you need immutable JSON structures (use System.Text.Json.JsonDocument)
- If you're handling sensitive data that must be securely wiped from memory (use Cocoar.Json.Zero)
- If you only need to read JSON once without modifications (use Utf8JsonReader directly)

## License

See [LICENSE](LICENSE) file for details.
