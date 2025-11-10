using BenchmarkDotNet.Running;
using Cocoar.Json.Mutable.Benchmarks;

Console.WriteLine("Cocoar.Json.Mutable Benchmarks");
Console.WriteLine("================================");
Console.WriteLine();
Console.WriteLine("This benchmark compares:");
Console.WriteLine("1. System.Text.Json with manual merge + DeepClone (baseline)");
Console.WriteLine("2. System.Text.Json with flatten/unflatten approach");
Console.WriteLine("3. Cocoar.Json.Mutable with Merge()");
Console.WriteLine("4. Cocoar.Json.Mutable with MergeDestructive()");
Console.WriteLine();
Console.WriteLine("Scenarios: 2, 10, 50, 100 providers");
Console.WriteLine();

BenchmarkRunner.Run<MergeBenchmarks>();
