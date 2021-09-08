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
            if (config == null)
                return null;


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

                var separatedOutput = cleanedOutput.Split('\t',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (separatedOutput.Length != config.ExpectedValues[currentIndex].Values.Length)
                {
                    Logger.LogWarn("Incorrect number of values in row {0}", output);
                    return new Error
                    {
                        message = "Incorrect number of values in a row",
                        expected = $"{config.ExpectedValues[currentIndex].Values.Length}",
                        actual = $"{separatedOutput.Length}",
                        code = "ERR_INCORRECT_VALUE_COUNT_LINE",
                        generatedMessage = true
                    };
                }

                for (var k = 0; k < separatedOutput.Length; k++)
                {
                    var currentValueWrapper = new ValueWrapper(separatedOutput[k]);

                    if (!config.ExpectedValues[currentIndex].Values[k].VEquals(currentValueWrapper, config.Delta))
                    {
                        return new Error
                        {
                            message = "Output value does not match expected value",
                            expected = $"{config.ExpectedValues[currentIndex].Values[k]}",
                            actual = $"{separatedOutput[k]}",
                            code = $"ERR_INCORRECT_RESULT",
                            generatedMessage = true
                        };
                    }
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
    }
}