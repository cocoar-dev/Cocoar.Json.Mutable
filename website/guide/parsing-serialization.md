# Parsing & Serialization

All parsing and serialization goes through the `MutableJsonDocument` static class. It wraps `System.Text.Json`'s `Utf8JsonReader` and `Utf8JsonWriter` internally.

## Parsing

### From UTF-8 Bytes

The primary parse path. Works with byte arrays, spans, and memory:

```csharp
// From a UTF-8 literal (ReadOnlySpan<byte>)
var node = MutableJsonDocument.Parse("{\"port\": 8080}"u8);

// From a byte array
byte[] data = File.ReadAllBytes("config.json");
var node2 = MutableJsonDocument.Parse(data);

// From ReadOnlyMemory<byte> — ideal for provider scenarios
ReadOnlyMemory<byte> memory = GetDataFromProvider();
var node3 = MutableJsonDocument.Parse(memory);
```

All three overloads return a `MutableJsonNode`. For JSON objects (the most common case), cast the result:

```csharp
var obj = (MutableJsonObject)MutableJsonDocument.Parse(jsonBytes);
```

All `Parse` methods throw `JsonException` for malformed input — the same exception type and error messages as `System.Text.Json`. There is no `TryParse` variant.

### From a Stream

For large files or network streams, use `ParseFromStream` to avoid loading the entire payload into memory at once:

```csharp
using var stream = File.OpenRead("large-config.json");
var node = MutableJsonDocument.ParseFromStream(stream);
```

The parser uses `ArrayPool<byte>` internally with a 64 KB initial buffer and grows it exponentially as needed. The buffer is returned to the pool after parsing — individual node values are copied into their own dedicated `byte[]` arrays.

## Serialization

### To Byte Array

```csharp
byte[] utf8Json = MutableJsonDocument.ToUtf8Bytes(node);
```

Returns compact (unindented) JSON as a UTF-8 byte array.

### To Stream

Write directly to a stream without intermediate allocation:

```csharp
// Default options (compact)
using var stream = File.Create("output.json");
MutableJsonDocument.WriteTo(node, stream);

// With custom options (e.g. indented)
var options = new JsonWriterOptions { Indented = true };
MutableJsonDocument.WriteTo(node, stream, options);
```

### Async Writing

For non-blocking I/O:

```csharp
await using var stream = File.Create("output.json");
await MutableJsonDocument.WriteToAsync(node, stream);

// With options
var options = new JsonWriterOptions { Indented = true };
await MutableJsonDocument.WriteToAsync(node, stream, options);
```

## Round-Trip Example

Parse, modify, serialize:

```csharp
// Parse
var config = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "port": 8080 }, "debug": false }
    """u8);

// Modify
config.Set("debug", new MutableJsonBool(true));
var server = config.Get("server") as MutableJsonObject;
server?.Set("host", new MutableJsonString("localhost"));

// Serialize
byte[] result = MutableJsonDocument.ToUtf8Bytes(config);
// {"server":{"port":8080,"host":"localhost"},"debug":true}
```
