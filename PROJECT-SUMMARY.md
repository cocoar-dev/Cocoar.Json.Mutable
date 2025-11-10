# Cocoar.Json.Mutable - Project Summary

## Overview
**Cocoar.Json.Mutable** is a new library created to address the need for efficient JSON merging without the security overhead of Cocoar.Json.Zero. It focuses purely on providing a mutable JSON Document Object Model (DOM) with excellent merge performance.

## Why We Created This

### Original Problem
- **Cocoar.Json.Zero** was designed for maximum security (zero-string guarantee, deterministic memory cleanup)
- However, the security features (memory zeroing, secure hashing) introduced performance overhead
- Many use cases need JSON merging but NOT the security features

### Solution
- Created **Cocoar.Json.Mutable** as a focused library with:
  - **Only** the mutable JSON DOM functionality
  - **Only** the merge capabilities
  - **No** security overhead (no memory zeroing, no secure hashing)
  - Much simpler and faster for non-security-critical use cases

## Key Features

### ✅ What's Included
1. **Mutable JSON Nodes**: `MutableJsonObject`, `MutableJsonArray`, `MutableJsonString`, `MutableJsonNumber`, `MutableJsonBool`, `MutableJsonNull`
2. **Merge Functionality**: Deep merging with `MutableJsonMerge.Merge()`
3. **Destructive Merge**: Fast merge without cloning via `MutableJsonMerge.MergeDestructive()`
4. **Clone Support**: Deep cloning of JSON structures with `MutableJsonMerge.Clone()`
5. **UTF-8 Native**: All operations work with `ReadOnlySpan<byte>` for zero-copy performance
6. **Parse & Serialize**: Full JSON parsing and serialization support

### ❌ What's NOT Included (vs Cocoar.Json.Zero)
- No memory zeroing (no `CryptographicOperations.ZeroMemory`)
- No secure random hashing
- No `JsonZeroOptions` configuration
- No `Dispose()` patterns for memory cleanup
- No `SecureArrayPool`
- Uses simple `Dictionary<string, int>` instead of secure hash-based indexing

## Architecture

### Core Classes
```
MutableJsonNode (abstract base)
├── MutableJsonObject       - JSON object with properties
├── MutableJsonArray        - JSON array with items  
├── MutableJsonString       - UTF-8 string value
├── MutableJsonNumber       - UTF-8 number value
├── MutableJsonBool         - Boolean value
└── MutableJsonNull         - Null value (singleton)

MutableJsonMerge (static)   - Merge operations
MutableJsonDocument (static)- Parse/serialize
MutableJsonParser (internal)- UTF-8 JSON parser
MutableJsonWriter (internal)- UTF-8 JSON writer
```

### Key Differences from Cocoar.Json.Zero

| Feature | Cocoar.Json.Zero | Cocoar.Json.Mutable |
|---------|------------------|---------------------|
| **Purpose** | Secure JSON handling | Fast JSON merging |
| **Memory Cleanup** | Triple-zero with random overwrite | Standard GC |
| **Hash Tables** | Cryptographically secure | Standard Dictionary |
| **Options** | MemoryCleanupMode | None |
| **IDisposable** | Yes (zeros memory) | No |
| **Parser** | Custom secure parser | Utf8JsonReader |
| **Performance** | Slower (security overhead) | Faster (no overhead) |
| **Use Case** | Secrets, PII, compliance | Configuration, general JSON |

## Usage Examples

### Basic Merge
```csharp
var config = new MutableJsonObject();
var base = MutableJsonDocument.Parse("{\"port\": 8080}"u8);
var override = MutableJsonDocument.Parse("{\"host\": \"localhost\"}"u8);

MutableJsonMerge.Merge(config, (MutableJsonObject)base);
MutableJsonMerge.Merge(config, (MutableJsonObject)override);

// Result: {"port": 8080, "host": "localhost"}
```

### Deep Merge
```csharp
var config1 = MutableJsonDocument.Parse(@"{
    ""server"": {""port"": 8080, ""timeout"": 30}
}"u8);

var config2 = MutableJsonDocument.Parse(@"{
    ""server"": {""host"": ""localhost"", ""port"": 9000}
}"u8);

var merged = new MutableJsonObject();
MutableJsonMerge.Merge(merged, (MutableJsonObject)config1);
MutableJsonMerge.Merge(merged, (MutableJsonObject)config2);

// Result: {"server": {"port": 9000, "timeout": 30, "host": "localhost"}}
```

## Use Cases

### ✅ Perfect For
- Configuration merging (base → environment → user overrides)
- API response aggregation
- JSON document transformations
- Building complex JSON programmatically
- Non-sensitive data processing

### ❌ NOT Suitable For
- Handling API keys, passwords, tokens
- PII data that must be wiped from memory
- Security-sensitive applications
- Compliance requirements (PCI-DSS, GDPR data handling)

## Project Structure
```
cocoar.json.mutable/
├── src/
│   └── Cocoar.Json.Mutable/
│       ├── MutableJsonNode.cs
│       ├── MutableJsonObject.cs
│       ├── MutableJsonArray.cs
│       ├── MutableJsonString.cs
│       ├── MutableJsonPrimitives.cs
│       ├── MutableJsonMerge.cs
│       ├── MutableJsonDocument.cs
│       ├── MutableJsonParser.cs
│       ├── MutableJsonWriter.cs
│       ├── PooledBufferWriter.cs
│       └── JsonStringUnescaper.cs
├── test/
│   └── Cocoar.Json.Mutable.Tests/
│       └── MutableJsonMergeTests.cs
├── examples/
│   └── BasicMergeExample.cs
├── README.md
├── CHANGELOG.md
└── PROJECT-SUMMARY.md
```

## Build & Test

### Build
```bash
cd cocoar.json.mutable
dotnet build src/Cocoar.Json.Mutable.slnx
```

### Test
```bash
dotnet test src/Cocoar.Json.Mutable.slnx
```

### Package
```bash
dotnet pack src/Cocoar.Json.Mutable/Cocoar.Json.Mutable.csproj -c Release -o ./artifacts
```

## Current Status

✅ **Completed**
- Core mutable JSON nodes (Object, Array, String, Number, Bool, Null)
- Merge functionality (both regular and destructive)
- Clone functionality
- Parse and serialize (UTF-8)
- Unit tests (4 tests, all passing)
- NuGet package generation
- Documentation (README, examples)
- Git repository initialized

## What's Inherited from Cocoar.Json.Zero

The following files were copied and adapted:
- Build configuration (.editorconfig, Directory.Build.props, Directory.Packages.props)
- GitHub workflows (CI/CD pipelines)
- License, contributing guidelines, security policy
- General project infrastructure

## Next Steps (Optional)

1. **More Tests**: Add comprehensive test coverage
2. **Benchmarks**: Create performance benchmarks
3. **Documentation**: Add API reference docs
4. **Examples**: More usage examples
5. **Publishing**: Publish to NuGet.org

## Summary

**Cocoar.Json.Mutable** is a streamlined, performance-focused library for JSON merging that eliminates the security overhead of Cocoar.Json.Zero. It's perfect for configuration management and general JSON manipulation where security is not a concern.
