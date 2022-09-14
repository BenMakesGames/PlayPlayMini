// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using BenMakesGames.PlayPlayMini.Performance;

BenchmarkRunner.Run<GameStateBenchmarks>();