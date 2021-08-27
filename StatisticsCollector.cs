using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AAARunCheck
{
    /// <summary>
    /// This class is responsible for collecting statistics on the execution of implementations.
    /// It does so by subscribing to the provided Event in the ExecutionEngine.
    /// </summary>
    public class StatisticsCollector
    {
        // Basically a mutable version of Tuple<T1, T2>
        private class Pair<T1, T2>
        {
            public Pair(T1 first, T2 second)
            {
                First = first;
                Second = second;
            }

            public T1 First { get; set; }
            public T2 Second { get; set; }
        }

        private readonly Dictionary<LanguageConfig, Pair<uint, uint>> _runStats;
        private int _successCount;
        private int _failCount;
        private Stopwatch _stopwatch;

        public StatisticsCollector()
        {
            _runStats = new Dictionary<LanguageConfig, Pair<uint, uint>>();
            Program.Instance.ExecutionEngine.ImplementationExecution += ExecutionEngineOnImplementationExecution;
        }

        public void StartMeasuring()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void StopMeasuring()
        {
            _stopwatch.Stop();
        }

        public void OutputStats()
        {
            Console.WriteLine("Test result overview:");
            Console.WriteLine(
                $"{_successCount} out of {_successCount + _failCount} tests passed in {_stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine("Language stats:");
            foreach (var (languageConfig, languageResult) in _runStats)
            {
                Console.WriteLine(
                    $"{languageConfig.language}: {languageResult.First}/{languageResult.First + languageResult.Second}");
            }
        }

        private void ExecutionEngineOnImplementationExecution(object? sender, ImplementationExecutionEventArgs e)
        {
            if (!_runStats.TryGetValue(e.ImplLanguageConfig, out var statsTuple))
            {
                statsTuple = new Pair<uint, uint>(0u, 0u);
                _runStats.Add(e.ImplLanguageConfig, statsTuple);
            }

            if (e.Succeeded)
            {
                statsTuple.First++;
                _successCount++;
            }
            else
            {
                statsTuple.Second++;
                _failCount++;
            }
        }
    }
}