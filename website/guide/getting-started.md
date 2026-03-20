# Getting Started

## Install

```shell
dotnet add package Cocoar.Json.Mutable
```

No additional dependencies — the library is built on `System.Text.Json` which ships with .NET.

## Parse and Merge Two JSON Objects

The most common use case: merge multiple JSON documents into one.

```csharp
using Cocoar.Json.Mutable;

var baseConfig = MutableJsonDocument.Parse("""
    { "server": { "port": 8080, "host": "0.0.0.0" }, "debug": false }
    """u8);

var overrides = MutableJsonDocument.Parse("""
    { "server": { "host": "localhost" }, "debug": true }
    """u8);

var result = new MutableJsonObject();
MutableJsonMerge.Merge(result, (MutableJsonObject)baseConfig);
MutableJsonMerge.Merge(result, (MutableJsonObject)overrides);

// Result: { "server": { "port": 8080, "host": "localhost" }, "debug": true }
byte[] json = MutableJsonDocument.ToUtf8Bytes(result);
```

Properties merge recursively. The second call to `Merge` overwrites `host` and `debug` but leaves `port` intact.

## Build a JSON Object from Scratch

You don't have to start from parsed JSON. Build structures programmatically using the string API:

```csharp
var doc = new MutableJsonObject();
doc.Set("name", new MutableJsonString("MyApp"));
doc.Set("version", new MutableJsonNumber(2));
doc.Set("enabled", new MutableJsonBool(true));

var tags = new MutableJsonArray();
tags.Add(new MutableJsonString("production"));
tags.Add(new MutableJsonString("v2"));
doc.Set("tags", tags);

byte[] json = MutableJsonDocument.ToUtf8Bytes(doc);
// {"name":"MyApp","version":2,"enabled":true,"tags":["production","v2"]}
```

## Read Values Back

Access properties by name and cast to the expected node type:

```csharp
var config = (MutableJsonObject)MutableJsonDocument.Parse("""
    { "server": { "port": 8080 }, "name": "MyApp" }
    """u8);

// String API
var server = config.Get("server") as MutableJsonObject;
var port = server?.Get("port") as MutableJsonNumber;

// UTF-8 API (zero allocation)
var name = config.Get("name"u8) as MutableJsonString;
```

Both APIs can be mixed freely within the same codebase.

## Next Steps

- [Why Cocoar.Json.Mutable?](/guide/why) — How it compares to raw System.Text.Json
- [Node Types](/guide/node-types) — All available node types and their API
- [Parsing & Serialization](/guide/parsing-serialization) — Reading and writing JSON
- [Merging](/guide/merging) — Deep merge strategies and cloning
