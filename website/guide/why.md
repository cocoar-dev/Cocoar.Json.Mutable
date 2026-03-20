# Why Cocoar.Json.Mutable?

## The Problem with System.Text.Json

.NET provides two built-in ways to work with JSON at the DOM level. Neither is designed for merging.

### JsonDocument — Immutable

`System.Text.Json.JsonDocument` is fast for reading but immutable. You cannot change a property, add an element, or merge two documents without serializing and re-parsing:

```csharp
// Reading is fine
using var doc = JsonDocument.Parse(json);
var port = doc.RootElement.GetProperty("server").GetProperty("port").GetInt32();

// But merging two documents? No built-in support.
// You'd have to manually walk both trees and write to a Utf8JsonWriter.
```

### JsonNode — Mutable but String-Based

`System.Text.Json.Nodes.JsonNode` is mutable, but it stores all values as managed .NET objects and uses string-based property names:

```csharp
var node = JsonNode.Parse(json);
node["server"]["port"] = 9090; // Works, but every property name is a string allocation

// Merging requires manual recursion + DeepClone for every value
var clone = source.DeepClone(); // Allocates the entire subtree
```

Merging `JsonNode` trees is manual, allocation-heavy, and gets complex with nested objects.

## The Cocoar Approach

`Cocoar.Json.Mutable` is purpose-built for the merge use case:

```csharp
var target = new MutableJsonObject();

// Merge any number of sources — each call is one line
MutableJsonMerge.Merge(target, (MutableJsonObject)MutableJsonDocument.Parse(baseJson));
MutableJsonMerge.Merge(target, (MutableJsonObject)MutableJsonDocument.Parse(overrideJson));
MutableJsonMerge.Merge(target, (MutableJsonObject)MutableJsonDocument.Parse(envJson));

byte[] result = MutableJsonDocument.ToUtf8Bytes(target);
```

### Comparison

| Capability | JsonDocument | JsonNode | Cocoar.Json.Mutable |
|---|---|---|---|
| Mutable | No | Yes | Yes |
| Deep merge | Not supported | Manual recursion | `Merge()` / `MergeDestructive()` |
| String storage | UTF-8 (readonly) | .NET strings | UTF-8 byte arrays |
| Property access | Span-based | String-based | Both (string + UTF-8) |
| Memory model | Pooled, disposable | GC-managed | GC-managed, no zeroing |
| Deep clone | Not applicable | `DeepClone()` | `Clone()` |
| Provider input | `ReadOnlyMemory<byte>` | String | `byte[]`, `ReadOnlySpan<byte>`, `ReadOnlyMemory<byte>` |

## Memory Ownership — Why It Matters for Secrets

Beyond merging, there is a second reason this library exists: **byte array ownership**.

When a configuration system handles secrets (API keys, connection strings, encryption keys), the secret data must be zeroable after use. .NET strings are immutable and managed by the garbage collector — once a secret lands in a `string`, it cannot be erased from memory. It stays until the GC collects it, and you have no control over when or whether the memory is actually overwritten.

The built-in JSON APIs make this problem worse:

| API | What happens to secret values | Can you zero them? |
|---|---|---|
| `JsonDocument` | Internal buffers are pooled via `ArrayPool`. Secret bytes go back into the pool and may be handed to unrelated code. | No — you don't own the buffers |
| `JsonNode` | Values are converted to .NET `string`. | No — strings are immutable |
| `Utf8JsonReader` | `ValueSpan` points into the input buffer. For escaped strings, the reader copies into an internal buffer you don't control. | Partially — you own the input buffer, but not the reader's internal copy |

`Cocoar.Json.Mutable` solves this by design:

```csharp
// Parser always calls .ToArray() — every value gets its own byte[]
JsonTokenType.String => new MutableJsonString(reader.ValueSpan.ToArray())
```

Every parsed value is copied into a dedicated `byte[]` that the caller fully owns. No shared pools, no .NET strings, no internal buffers. When you're done with a secret, you can zero the array:

```csharp
var secret = obj.Get("apiKey") as MutableJsonString;
ReadOnlySpan<byte> bytes = secret.ValueUtf8;

// Use the secret...

// Zero the owned byte array when done
Array.Clear(secret.ValueUtf8.ToArray()); // or via the reference you kept
```

This is exactly how [Cocoar.Configuration](https://github.com/cocoar-dev/Cocoar.Configuration) handles secrets: `Secret<T>` wraps the owned byte arrays, provides lease-based access, and calls `Array.Clear()` on dispose — ensuring secret material never touches a .NET string and is wiped from memory as soon as the lease ends.

::: info
The library itself does not perform automatic zeroing — it provides the **mechanism** (byte array ownership). The **policy** (when and how to zero) is implemented by the consuming code, such as `Secret<T>` in Cocoar.Configuration.
:::

### Design Principles

**Merge-first.** The core operation is combining multiple JSON sources into one object. Everything else — parsing, serialization, node types — supports that use case.

**Byte ownership.** Every value is stored in a dedicated `byte[]` that the caller owns. No shared pools, no immutable strings. This enables downstream code to zero sensitive data after use.

**UTF-8 throughout.** Property names and string values are stored as `byte[]`. No encoding conversion until you explicitly need a .NET string. Numbers are stored as raw UTF-8 digits — no parsing to `double` until you need it.

**Dual API.** The string API (`Get("name")`, `Set("key", value)`) makes code readable. The UTF-8 API (`Get("name"u8)`) avoids allocations in hot paths. Both work on the same data.
