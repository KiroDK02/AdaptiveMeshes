// See https://aka.ms/new-console-template for more information

using System;
using BenchmarkDotNet.Running;
using ViewModels.BenchmarkTests.MaterialTests;

BenchmarkRunner.Run<MaterialCompilationBenchmark>();
