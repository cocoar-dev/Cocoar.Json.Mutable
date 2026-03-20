# API Overview

All types are in the `Cocoar.Json.Mutable` namespace. The library targets .NET 8.0.

## Thread Safety

This library is **not thread-safe**. No type is safe for concurrent reads and writes from multiple threads. If you need to share a `MutableJsonObject` across threads, use external synchronization (e.g., a lock).

After serialization, the resulting `byte[]` is an independent copy and can be safely shared across threads.

## MutableJsonNode

Abstract base class for all JSON node types.

| Member | Type | Description |
|---|---|---|
| `Kind` | `JsonValueKind` | The JSON value kind (Object, Array, String, Number, True, False, Null) |
| `WriteTo(Utf8JsonWriter)` | `void` | Serialize this node to a writer |
| `ToDebugString()` | `string` | Human-readable debug representation |

## MutableJsonObject

Sealed. A JSON object with ordered, named properties.

### Constructor

```csharp
MutableJsonObject(int indexThreshold = -1)
```

Creates an empty object. When `indexThreshold` is -1, uses `DefaultIndexThreshold` (default: 12).

### Static Properties

| Member | Type | Description |
|---|---|---|
| `DefaultIndexThreshold` | `int` | Property count at which a dictionary index is built. Default: 12 |

### Properties

| Member | Type | Description |
|---|---|---|
| `Properties` | `IReadOnlyList<Property>` | All properties in insertion order |
| `Kind` | `JsonValueKind` | Always `JsonValueKind.Object` |

### Methods

| Method | Return | Description |
|---|---|---|
| `Get(ReadOnlySpan<byte>)` | `MutableJsonNode?` | Get by UTF-8 property name |
| `Get(string)` | `MutableJsonNode?` | Get by string property name |
| `Set(ReadOnlySpan<byte>, MutableJsonNode)` | `void` | Set or overwrite by UTF-8 name. Overwrites preserve position |
| `Set(string, MutableJsonNode)` | `void` | Set or overwrite by string name. Overwrites preserve position |
| `Remove(ReadOnlySpan<byte>)` | `bool` | Remove by UTF-8 name. Returns `true` if found |
| `Remove(string)` | `bool` | Remove by string name. Returns `true` if found |

### Property Struct

`MutableJsonObject.Property` is a `readonly struct`:

| Member | Type | Description |
|---|---|---|
| `NameUtf8` | `ReadOnlyMemory<byte>` | Property name as UTF-8 bytes |
| `Name` | `string` | Property name as string (decoded on access) |
| `Value` | `MutableJsonNode` | The property value |

## MutableJsonArray

Sealed. A JSON array of nodes. Intentionally minimal — supports append and read. For removals or reordering, build a new array.

### Properties

| Member | Type | Description |
|---|---|---|
| `Items` | `IReadOnlyList<MutableJsonNode>` | All items in order |
| `Kind` | `JsonValueKind` | Always `JsonValueKind.Array` |

### Methods

| Method | Return | Description |
|---|---|---|
| `Add(MutableJsonNode)` | `void` | Append an item |
| `this[int]` | `MutableJsonNode` | Read-only indexer |

## MutableJsonString

Sealed. A JSON string stored as UTF-8 bytes.

### Constructors

| Constructor | Copies? | Description |
|---|---|---|
| `MutableJsonString(byte[])` | No | Stores the array reference directly — caller retains access to the internal buffer |
| `MutableJsonString(string)` | Yes | Encodes to a new UTF-8 byte array |

### Static Factory Methods

| Method | Description |
|---|---|
| `FromOwned(byte[])` | Takes ownership of the array — no copy |
| `FromCopy(ReadOnlySpan<byte>)` | Copies the bytes into a new array |

### Properties & Methods

| Member | Type | Description |
|---|---|---|
| `ValueUtf8` | `ReadOnlySpan<byte>` | The raw UTF-8 content |
| `Kind` | `JsonValueKind` | Always `JsonValueKind.String` |
| `Replace(byte[])` | `void` | Replace the value in place |

## MutableJsonNumber

Sealed. A JSON number stored as raw UTF-8 digits.

### Constructors

| Constructor | Description |
|---|---|
| `MutableJsonNumber(ReadOnlySpan<byte>)` | From raw UTF-8 digit bytes |
| `MutableJsonNumber(int)` | From int |
| `MutableJsonNumber(long)` | From long |
| `MutableJsonNumber(double)` | From double |

### Static Factory Methods

| Method | Description |
|---|---|
| `FromOwned(byte[])` | Takes ownership — no copy |
| `FromCopy(ReadOnlySpan<byte>)` | Copies the bytes |

### Properties

| Member | Type | Description |
|---|---|---|
| `ValueUtf8` | `ReadOnlySpan<byte>` | The raw UTF-8 representation |
| `Kind` | `JsonValueKind` | Always `JsonValueKind.Number` |

## MutableJsonBool

Sealed. A JSON boolean.

### Constructor

```csharp
MutableJsonBool(bool value)
```

### Properties

| Member | Type | Description |
|---|---|---|
| `Kind` | `JsonValueKind` | `JsonValueKind.True` or `JsonValueKind.False` |

## MutableJsonNull

Sealed. A JSON null. Singleton pattern.

| Member | Type | Description |
|---|---|---|
| `Instance` | `MutableJsonNull` | The single null instance |
| `Kind` | `JsonValueKind` | Always `JsonValueKind.Null` |

## MutableJsonDocument

Static class. Entry point for parsing and serialization.

### Parsing

| Method | Return | Description |
|---|---|---|
| `Parse(byte[])` | `MutableJsonNode` | Parse from byte array |
| `Parse(ReadOnlySpan<byte>)` | `MutableJsonNode` | Parse from span |
| `Parse(ReadOnlyMemory<byte>)` | `MutableJsonNode` | Parse from memory (provider-friendly) |
| `ParseFromStream(Stream)` | `MutableJsonNode` | Parse from stream (64 KB initial buffer, grows exponentially) |

All `Parse` methods throw `JsonException` for malformed input.

### Serialization

| Method | Return | Description |
|---|---|---|
| `ToUtf8Bytes(MutableJsonNode)` | `byte[]` | Serialize to compact UTF-8 JSON |
| `WriteTo(MutableJsonNode, Stream)` | `void` | Write to stream (compact) |
| `WriteTo(MutableJsonNode, Stream, JsonWriterOptions)` | `void` | Write to stream with options |
| `WriteToAsync(MutableJsonNode, Stream, CancellationToken)` | `Task` | Async write to stream |
| `WriteToAsync(MutableJsonNode, Stream, JsonWriterOptions, CancellationToken)` | `Task` | Async write with options |

## MutableJsonMerge

Static class. Merging and cloning operations.

| Method | Return | Description |
|---|---|---|
| `Merge(MutableJsonObject, MutableJsonObject)` | `MutableJsonObject` | Non-destructive merge — clones source values |
| `MergeDestructive(MutableJsonObject, MutableJsonObject)` | `MutableJsonObject` | Destructive merge — moves source values |
| `Clone(MutableJsonNode)` | `MutableJsonNode` | Deep clone any node |

Both merge methods return the target object for chaining.
