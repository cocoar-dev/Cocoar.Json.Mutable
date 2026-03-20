# Changelog

All notable changes to the Cocoar.Json.Mutable project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [1.0.1] - 2026-03-20

### Fixed
- Eliminate double allocation when parsing JSON numbers (`MutableJsonNumber` constructor copied bytes that were already copied by the parser)
- Eliminate double allocation when cloning `MutableJsonNumber` nodes
- Eliminate double allocation of property name bytes during parsing (new internal `SetOwned` path takes ownership of the byte array directly)

### Added
- VitePress documentation site
- Documentation deployment workflow and auto-deploy on stable release
- Path filters on PR validation and develop build workflows to skip builds on documentation-only changes

## [1.0.0] - 2025-11-10

### Added
- Initial release of Cocoar.Json.Mutable
- Mutable JSON document object model (DOM) with UTF-8 byte storage
- Efficient JSON object merging (Merge and MergeDestructive methods)
- Clone functionality for deep copying JSON nodes
- MutableJsonObject, MutableJsonArray, and primitive node types (String, Number, Bool, Null)
- Parse and serialize functionality using System.Text.Json
- Developer-friendly string API alongside UTF-8 byte API
- Support for `ReadOnlyMemory<byte>` from data providers
- Comprehensive unit tests (15 tests)
