using BenchmarkDotNet.Running;
using Buildenator.Benchmarks;

var summary = BenchmarkRunner.Run<GenerationTests>();
