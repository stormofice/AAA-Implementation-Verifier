using System;
using AAARunCheck.Config;
using AAARunCheck.Telemetry;

namespace AAARunCheck
{
    public class OutputValidator
    {
        // Validate the output of a process given the current chapter
        public Error CheckProcessOutput(ChapterConfig config, string processOutput)
        {
            // Split line by line
            var outputLines = processOutput.Split(Environment.NewLine);

            // Count how many actual numerical lines there are
            var currentIndex = 0;
            foreach (var output in outputLines)
            {
                var cleanedOutput = output.Trim();
                // Skip empty or verbose human readable lines
                if (cleanedOutput.StartsWith("[#]") || cleanedOutput.Length == 0)
                    continue;

                // Bail out if there are too many output values to prevent IOOB
                if (currentIndex >= config.ExpectedValues.Length)
                {
                    Logger.LogWarn("Too much output");
                    return new Error
                    {
                        message = "There were too many outputted values",
                        expected = $"{config.ExpectedValues.Length} values",
                        actual = $"Additional value: {cleanedOutput}",
                        code = "ERR_TOO_MANY_VALUES",
                        generatedMessage = true
                    };
                }

                // Try to get the numerical value
                if (!decimal.TryParse(cleanedOutput, out var numericalOutput))
                {
                    Logger.LogWarn("Could not parse output {0} to a decimal value", cleanedOutput);
                    return new Error
                    {
                        message = "Output value could not be parsed",
                        expected = $"{config.ExpectedValues[currentIndex]}",
                        actual = $"{numericalOutput}",
                        code = "ERR_NUMBER_PARSE",
                        generatedMessage = true
                    };
                }

                // Compare values
                if (!IsApprox(config.ExpectedValues[currentIndex], numericalOutput, config.Delta))
                {
                    Logger.LogWarn("Output is not the same: {0} != {1} with delta {2}",
                        config.ExpectedValues[currentIndex], numericalOutput, config.Delta);
                    return new Error
                    {
                        message = "Output value does not match expected value",
                        expected = $"{config.ExpectedValues[currentIndex]}",
                        actual = $"{numericalOutput}",
                        code = $"ERR_INCORRECT_RESULT",
                        generatedMessage = true
                    };
                }

                currentIndex++;
            }

            if (currentIndex + 1 < config.ExpectedValues.Length)
            {
                Logger.LogWarn("Not enough output");
                return new Error
                {
                    message = "There were not enough values outputted",
                    expected = $"{config.ExpectedValues.Length}",
                    actual = $"{currentIndex + 1}",
                    code = $"ERR_TOO_FEW_VALUES",
                    generatedMessage = true
                };
            }

            // null == valid result in this case
            return null;
        }

        // Checks if two decimal numbers are approximately the same
        private bool IsApprox(decimal expected, decimal actual, decimal delta)
        {
            // Logger.LogDebug("Comparing: Is {0} ~= {1} with delta {2}", actual, expected, delta);
            return Math.Abs(expected - actual) <= delta;
        }
    }
}