# Node Types

All JSON values are represented as subclasses of `MutableJsonNode`. The type hierarchy mirrors the JSON spec: objects, arrays, strings, numbers, booleans, and null.

## MutableJsonNode

The abstract base class. Every node exposes:

```csharp
public abstract JsonValueKind Kind { get; }
public abstract void WriteTo(Utf8JsonWriter writer);
```

`Kind` returns the `System.Text.Json.JsonValueKind` for the node — useful for type checks without casting.

## MutableJsonObject

A JSON object with named properties. Properties are stored in insertion order. When `Set` overwrites an existing property, the value is replaced at its original position — the property order does not change.

```csharp
var obj = new MutableJsonObject();

// Set properties
obj.Set("name", new MutableJsonString("MyApp"));
obj.Set("port", new MutableJsonNumber(8080));

// Get properties (returns null if not found)
var name = obj.Get("name") as MutableJsonString;

// Remove properties
obj.Remove("port");

// Iterate all properties
foreach (var prop in obj.Properties)
{
    Console.WriteLine($"{prop.Name} = {prop.Value.Kind}");
}
```

### UTF-8 API

Every `Get`, `Set`, and `Remove` method has a `ReadOnlySpan<byte>` overload:

```csharp
obj.Set("host"u8, new MutableJsonString("localhost"u8));
var host = obj.Get("host"u8);
obj.Remove("host"u8);
```

### Automatic Indexing

For objects with few properties, lookups scan the property list linearly. When the property count reaches a threshold (default: 12), a dictionary index is built automatically. You can control this:

```csharp
// Custom threshold for this object
var largeObj = new MutableJsonObject(indexThreshold: 5);

// Change the default for all new objects
MutableJsonObject.DefaultIndexThreshold = 20;
```

### Property Struct

Each property is a `readonly struct` with:

| Member | Type | Description |
|---|---|---|
| `NameUtf8` | `ReadOnlyMemory<byte>` | Property name as UTF-8 bytes |
| `Name` | `string` | Property name decoded to string |
| `Value` | `MutableJsonNode` | The property value |

## MutableJsonArray

A JSON array of nodes. The API is intentionally minimal — `Add` and read-only indexing. There are no `RemoveAt`, `Insert`, or `Clear` methods. If you need a different set of items, build a new array.

This matches the library's focus: merge operations replace arrays wholesale (see [Merging](/guide/merging)), so fine-grained array manipulation is rarely needed.

```csharp
var arr = new MutableJsonArray();
arr.Add(new MutableJsonString("first"));
arr.Add(new MutableJsonNumber(42));
arr.Add(MutableJsonNull.Instance);

// Access by index
var first = arr[0] as MutableJsonString;

// Count
int count = arr.Items.Count;

// Iterate
foreach (var item in arr.Items)
{
    Console.WriteLine(item.Kind);
}
```

## MutableJsonString

A JSON string stored as UTF-8 bytes.

```csharp
// From a .NET string (encodes to a new byte[] internally)
var str = new MutableJsonString("hello");

// From a byte[] (stores the reference directly — no copy)
byte[] raw = "hello"u8.ToArray();
var str2 = new MutableJsonString(raw); // str2 and raw share the same array

// Access the raw bytes
ReadOnlySpan<byte> utf8 = str.ValueUtf8;

// Replace the value in place
str.Replace("world"u8.ToArray());
```

### Constructors and Factory Methods

| Method | Copies? | Description |
|---|---|---|
| `MutableJsonString(byte[])` | No | Stores the array reference directly |
| `MutableJsonString(string)` | Yes (encodes) | Encodes the string to a new UTF-8 byte array |
| `FromOwned(byte[])` | No | Same as the byte[] constructor — explicit ownership intent |
| `FromCopy(ReadOnlySpan<byte>)` | Yes | Copies the data into a new array |

Use the `byte[]` constructor or `FromOwned` when you have a byte array you own and want to keep a reference to (e.g., for zeroing later). Use `FromCopy` when the source data might change or be reused.

## MutableJsonNumber

A JSON number stored as raw UTF-8 digits. The number is never parsed to `int` or `double` — it stays in its original representation until serialized.

```csharp
// From .NET numeric types
var fromInt = new MutableJsonNumber(42);
var fromLong = new MutableJsonNumber(9_999_999_999L);
var fromDouble = new MutableJsonNumber(3.14);

// From raw UTF-8 bytes
var fromBytes = new MutableJsonNumber("42"u8);

// Access the raw representation
ReadOnlySpan<byte> raw = fromInt.ValueUtf8; // "42" as UTF-8 bytes
```

### Factory Methods

| Method | Behavior |
|---|---|
| `FromOwned(byte[])` | Takes ownership — no copy |
| `FromCopy(ReadOnlySpan<byte>)` | Copies the data |

## MutableJsonBool

A JSON boolean.

```csharp
var t = new MutableJsonBool(true);
var f = new MutableJsonBool(false);

// Kind tells you the value
bool isTrue = t.Kind == JsonValueKind.True;
```

## MutableJsonNull

A JSON null. Singleton — there's only one instance:

```csharp
var n = MutableJsonNull.Instance;
// n.Kind == JsonValueKind.Null
```

Use `MutableJsonNull.Instance` wherever you need a null value. The private constructor ensures no unnecessary allocations.
