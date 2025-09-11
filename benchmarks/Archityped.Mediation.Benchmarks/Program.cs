using Archityped.Mediation.Benchmarks;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run(typeof(Benchmarks), new BenchmarksConfiguration(), args);