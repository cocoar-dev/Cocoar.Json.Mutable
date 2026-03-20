# UTF-8 API & Memory Management

This library stores all string and number data as UTF-8 `byte[]`. This page explains the memory model and how to choose between the string API and the UTF-8 API.

## Why UTF-8?

JSON is typically transmitted and stored as UTF-8. Most .NET JSON libraries decode these bytes into .NET `string` (UTF-16) on parse, then re-encode to UTF-8 on serialization. This double conversion is unnecessary when the goal is merging — the merged result goes back to UTF-8 anyway.

`Cocoar.Json.Mutable` skips the conversion entirely. Property names and string values stay as `byte[]` from parse to serialize.

## The Two APIs

### String API

Convenient for application code where allocations aren't a concern:

```csharp
obj.Set("name", new MutableJsonString("MyApp"));
var value = obj.Get("name");
obj.Remove("name");
```

Each call to `Set(string, ...)` or `Get(string)` encodes the key to UTF-8 internally. This is fine for most code — the encoding is fast and the allocation is small.

### UTF-8 API

For hot paths where you want zero allocations on the key lookup:

```csharp
obj.Set("name"u8, new MutableJsonString("MyApp"u8));
var value = obj.Get("name"u8);
obj.Remove("name"u8);
```

The `u8` suffix creates a `ReadOnlySpan<byte>` at compile time. No runtime encoding or allocation.

## Memory Ownership

### Constructors

`MutableJsonString` and `MutableJsonNumber` offer two ways to provide data:

```csharp
// Constructor: always copies the input
var s = new MutableJsonString("hello"u8.ToArray());

// FromOwned: takes ownership, no copy
byte[] buffer = GetBuffer();
var s2 = MutableJsonString.FromOwned(buffer);

// FromCopy: copies the span into a new array
ReadOnlySpan<byte> span = GetSpan();
var s3 = MutableJsonString.FromCopy(span);
```

| Method | Copies? | Use When |
|---|---|---|
| Constructor (`byte[]`) | No | You're passing a dedicated array |
| Constructor (`string`) | Yes (encodes) | You have a .NET string |
| `FromOwned(byte[])` | No | You have a byte array you won't reuse |
| `FromCopy(ReadOnlySpan<byte>)` | Yes | The source buffer might change |

### Reading Values

`ValueUtf8` returns a `ReadOnlySpan<byte>` — a view into the internal array without copying:

```csharp
var str = new MutableJsonString("hello");
ReadOnlySpan<byte> bytes = str.ValueUtf8; // No allocation
```

::: warning
The span is only valid while the node exists and hasn't been replaced. Don't store it across async boundaries — copy it to a `byte[]` if you need to keep it.
:::

## Parsing and Memory

When `MutableJsonDocument.Parse` processes JSON, it copies relevant byte ranges from the input into new `byte[]` for each string and number value. The input buffer is not retained — you can free or reuse it after parsing.

For stream parsing, `ParseFromStream` uses `ArrayPool<byte>` to rent a read buffer. The buffer is returned to the pool after parsing completes — individual node values are copied into their own arrays.

## When NOT to Use UTF-8 API

The UTF-8 API is not always the right choice:

- **Property names are dynamic** (user input, config keys) — use the string API to avoid manual encoding
- **You need the value as a .NET string** — just use `new MutableJsonString(myString)` and let the library encode once
- **Allocations don't matter** — if you're merging a few configs at startup, the string API is simpler and equally fast

The UTF-8 API is most valuable in tight loops or high-throughput scenarios where you're merging many documents per second.
