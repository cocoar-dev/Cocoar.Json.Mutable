# Cocoar.Json.Mutable Benchmarks

This directory contains benchmarks comparing Cocoar.Json.Mutable against System.Text.Json for JSON merging scenarios.

## Running Benchmarks

```bash
cd benchmarks/Cocoar.Json.Mutable.Benchmarks
dotnet run -c Release
```

### Quick Run (for testing)
```bash
dotnet run -c Release -- --job short
```

### Run Specific Benchmark
```bash
# Only allocation benchmarks
dotnet run -c Release -- --filter "*AllocationBenchmarks*"

# Only merge benchmarks
dotnet run -c Release -- --filter "*MergeBenchmarks*"
```

## Benchmark Scenarios

### MergeBenchmarks
Compares different approaches to merging multiple JSON providers:

1. **System.Text.Json with Manual Merge + DeepClone** (Baseline)
   - What users would typically write
   - Uses `JsonNode.DeepClone()` for safety
   - Allocates new objects for all values

2. **System.Text.Json with Flatten/Unflatten**
   - Flatten to `Dictionary<string, value>` with path keys
   - Merge dictionaries  
   - Unflatten back to JSON
   - Your previous approach

3. **Cocoar.Json.Mutable with Merge()**
   - Non-destructive merge
   - Clones source values
   - Merges nested objects in-place

4. **Cocoar.Json.Mutable with MergeDestructive()**
   - Destructive merge (fastest)
   - Moves values from source
   - Source should not be used after merge

**Tested with:** 2, 10, 50, 100 providers

### AllocationBenchmarks
Focused benchmark to prove allocation differences between approaches.

Tests merging 2 JSON objects with nested structures to show:
- System.Text.Json allocates for ALL values (DeepClone)
- Cocoar.Json.Mutable only allocates for new/changed values

## Expected Results

Based on the design, Cocoar.Json.Mutable should show:

### Performance Benefits
- ✅ **30-50% faster** than manual merge for multiple providers
- ✅ **2-3x faster** than flatten/unflatten approach
- ✅ Scales better with more providers

### Memory Benefits
- ✅ **40-50% fewer allocations** (no nested object clones)
- ✅ Lower GC pressure
- ✅ Better memory locality

### Why It's Faster

1. **No DeepClone overhead** - Nested objects merged in-place
2. **No flatten/unflatten** - Single pass merging
3. **UTF-8 support** - Works directly with byte arrays from providers
4. **Efficient indexing** - Dictionary-based lookup for large objects

## Benchmark Results

After running, results will be in:
- `BenchmarkDotNet.Artifacts/results/`
- Look for files like:
  - `MergeBenchmarks-report.html`
  - `MergeBenchmarks-report-github.md`
  - `AllocationBenchmarks-report.html`

## Example Output

```
| Method                                | ProviderCount | Mean     | Allocated |
|-------------------------------------- |-------------- |---------:|----------:|
| CocoarJsonMutable_MergeDestructive   | 100           | 245.2 us | 156.2 KB  |
| CocoarJsonMutable_Merge              | 100           | 287.4 us | 234.5 KB  |
| SystemTextJson_ManualMerge_DeepClone | 100           | 412.8 us | 445.7 KB  |
| SystemTextJson_FlattenUnflatten      | 100           | 623.5 us | 612.3 KB  |
```

## Interpreting Results

- **Mean**: Average execution time (lower is better)
- **Allocated**: Total memory allocated (lower is better)
- **Rank**: Performance ranking (1 is fastest)
- **Ratio**: Performance relative to baseline

Look for:
1. Cocoar.Json.Mutable should be fastest
2. Significantly less memory allocated
3. Better scaling with provider count
