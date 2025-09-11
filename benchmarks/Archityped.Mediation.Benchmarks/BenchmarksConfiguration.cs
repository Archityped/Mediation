using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.NativeAot;

namespace Archityped.Mediation.Benchmarks;

public class BenchmarksConfiguration : ManualConfig
{
    public BenchmarksConfiguration()
    {
        // Base columns
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddColumn(StatisticColumn.Median, StatisticColumn.Min, StatisticColumn.Max, StatisticColumn.P95);
        AddColumn(RankColumn.Stars);

        // Grouping
        AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByMethod);

        // Exporters / Loggers (limit to those referenced in project)
        AddLogger(ConsoleLogger.Default);
        AddExporter(MarkdownExporter.GitHub, HtmlExporter.Default, CsvExporter.Default);

        // Diagnostics
        AddDiagnoser(MemoryDiagnoser.Default);

        // Summary style
        SummaryStyle = SummaryStyle.Default
            .WithRatioStyle(RatioStyle.Trend)
            .WithMaxParameterColumnWidth(40);

        //AddJitJobs();
        AddAotJobs();
    }

    public virtual void AddJitJobs()
    {
        AddJob(Job.Dry
               .WithId("JIT-ColdStart")
               .WithRuntime(CoreRuntime.Core90)
               .WithPlatform(Platform.AnyCpu)
               .WithJit(Jit.Default)
               .WithWarmupCount(0)
               .WithIterationCount(1)
               .WithLaunchCount(10)
           );

        AddJob(Job.Default
            .WithId("JIT-WarmStart")
            .WithRuntime(CoreRuntime.Core90)
            .WithPlatform(Platform.AnyCpu)
            .WithJit(Jit.Default)
            .WithWarmupCount(6)
            .WithIterationCount(15)
            .WithLaunchCount(1)
        );
    }

    public void AddAotJobs()
    {
        //// NativeAOT job (steady-state)
        //AddJob(Job.Default
        //    .WithId("AOT-ColdStart")
        //    .WithRuntime(NativeAotRuntime.Net90)
        //    .WithToolchain(NativeAotToolchain.Net90) 
        //    .WithWarmupCount(9)
        //    .WithIterationCount(1)
        //    .WithLaunchCount(10)
        //);

        AddJob(Job.Default
            .WithId("AOT-WarmStart")
            .WithRuntime(NativeAotRuntime.Net90)
            .WithToolchain(NativeAotToolchain.Net90)
            .WithWarmupCount(6)
            .WithIterationCount(15)
            .WithLaunchCount(1)
        );
    }
}

