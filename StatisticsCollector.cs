using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AAARunCheck.Config;
using AAARunCheck.Telemetry;

namespace AAARunCheck
{
    /// <summary>
    /// This class is responsible for collecting statistics on the execution of implementations.
    /// It does so by subscribing to the provided Event in the ExecutionEngine.
    /// </summary>
    public class StatisticsCollector
    {
        private Stopwatch _stopwatch;

        private TestReport _report;
        private Test _currentTest;

        private Stopwatch _currentTestTimer;


        public StatisticsCollector()
        {
            Program.Instance.ExecutionEngine.ImplementationExecutionStart +=
                ExecutionEngineOnImplementationExecutionStart;
            Program.Instance.ExecutionEngine.ImplementationExecutionStop +=
                ExecutionEngineOnImplementationExecutionStop;

            _report = new TestReport
            {
                stats = new BasicStats
                {
                    suites = 1
                },
                tests = new List<Test>(),
                passes = new List<Test>(),
                pending = new List<Test>(),
                failures = new List<Test>()
            };
        }

        private void ExecutionEngineOnImplementationExecutionStart(object? sender,
            ImplementationExecutionStartEventArgs e)
        {
            Debug.Assert(_report != null, nameof(_report) + " != null");
            // Make sure that we are at a new test
            Debug.Assert(_currentTest == null, nameof(_currentTest) + " == null");
            Debug.Assert(_currentTestTimer == null, nameof(_currentTestTimer) + " == null");


            _currentTest = new Test();
            _currentTest.file = e.Filename;

            // This value is fixed for now
            _currentTest.currentRetry = 1;

            _currentTestTimer = Stopwatch.StartNew();
        }

        private void ExecutionEngineOnImplementationExecutionStop(object? sender,
            ImplementationExecutionStopEventArgs e)
        {
            _currentTestTimer.Stop();

            _currentTest.duration = _currentTestTimer.ElapsedMilliseconds;
            _currentTest.file = e.Filename;
            // This value is fixed for now
            _currentTest.speed = "Standard";

            _report.stats.tests++;

            if (e.Succeeded)
            {
                _currentTest.title = $"{e.ImplLanguageConfig.language}: Passed test";
                _currentTest.fullTitle =
                    $"{e.ImplLanguageConfig.language}: Passed test with exit code: {e.ExitCode} after {e.CurrentStepIndex} steps";

                _report.stats.passes++;
                _report.passes.Add(_currentTest);
            }
            else
            {
                _currentTest.title = $"{e.ImplLanguageConfig.language}: Failed test";
                _currentTest.fullTitle =
                    $"{e.ImplLanguageConfig.language}: Failed test with exit code: {e.ExitCode} at step {e.CurrentStepIndex} -> {e.CurrentStep}";

                _report.stats.failures++;
                _report.failures.Add(_currentTest);
            }


            _report.tests.Add(_currentTest);

            _currentTest = null;
            _currentTestTimer = null;
        }


        public void StartMeasuring()
        {
            Logger.LogInfo("Starting tests");
            _stopwatch = Stopwatch.StartNew();
            _report.stats.start = String.Format("{0:O}", DateTime.UtcNow);
        }

        public void StopMeasuring()
        {
            _stopwatch.Stop();
            _report.stats.end = String.Format("{0:O}", DateTime.UtcNow);
            _report.stats.duration = _stopwatch.ElapsedMilliseconds;
            Logger.LogInfo("Finished testing after " + _stopwatch.ElapsedMilliseconds + "ms");
        }

        public void OutputStats()
        {
            var jsonOutput = JsonSerializer.Serialize(_report, new JsonSerializerOptions
            {
                WriteIndented = true,
                // This is dangerous, as it does not escape HTML chars and does not offer XSS Protection
                // It is needed to render + correctly though
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            if (Program.Instance.ConfigManager.IntConfig.RedirectJsonToFile != null)
            {
                File.WriteAllText(Program.Instance.ConfigManager.IntConfig.RedirectJsonToFile, jsonOutput);
            }
            else
            {
                Console.WriteLine(jsonOutput);
            }
        }
    }
}