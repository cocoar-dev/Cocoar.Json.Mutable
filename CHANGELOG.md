# Changelog

All notable changes to the Cocoar.Json.Mutable project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
