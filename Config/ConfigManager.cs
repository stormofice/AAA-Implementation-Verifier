using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;

namespace AAARunCheck.Config
{
    /// <summary>
    /// This class is responsible for loading internal configuration (such as the log level)
    /// and language specific configurations (on how to handle code from a given language).
    /// Configurations are basic json files.
    ///
    /// Example of how a language configuration works:
    ///
    /// {
    /// "language": "asm x86_64",   # Human friendly name of the language
    /// "extension": "s",           # The file extension corresponding to the language
    /// "description": "Uses -no-pie to disable position independent execution",    # If there are things you want to explain, here is the place
    /// "steps": [ # Array of Steps to execute
    ///         { # This step compiles the assembly into a known location
    ///           # (as this process is started in the output directory, the output will also land there)
    ///             "runtime": "gcc",       # Runtime is the actual command being executed, needs to be in PATH or the full path must be given
    ///             "command": " -no-pie -o {0} {1} -lm", # Everything which comes after the path to the executable,
    ///                                                     the placeholders will be replaced by the args below
    ///             "args": [
    ///                 "FILE_NAME_WEX", # Magic value to put into {0}
    ///                 "ALL_FILES_IN_DIR" # ^
    ///              ]
    ///         },
    ///         { # Second
    ///             "runtime": "WORKING_DIR/FILE_NAME_WEX", # This is the same as calling the program in your terminal
    ///             "command": "", # No flags are needed as this is something like ./verlet
    ///             "args": [] # ^
    ///         }
    ///     ]
    /// }
    ///
    /// Magic values:
    /// There are a few magic values available, to majorly simplify hardcoding.
    /// These are available currently [all of these are in context of the current implementation]:
    /// ALL_FILES_IN_DIR - This gets replaced by every file in the directory of the code file
    /// FILE_NAME_WEX - The chapterPath of the implementation without its extension
    /// FILE_PATH - The *full* path to the code file
    /// WORKING_DIR_FULL - The *full* path to the working directory
    /// WORKING_DIR - The path to the working directory as given by user input
    /// </summary>
    public class ConfigManager
    {
        public readonly Dictionary<string, LanguageConfig> LanguageConfigs;
        public readonly InternalConfig IntConfig;

        private readonly string _configPath;

        public ConfigManager(string configPath)
        {
            _configPath = configPath;

            var internalConfigContents = File.ReadAllText(Path.Combine(configPath, "internal.json"));
            IntConfig = JsonSerializer.Deserialize<InternalConfig>(
                internalConfigContents);

            Logger.LogDebug("Loaded internal config: " + Environment.NewLine + "{0}", internalConfigContents);
            Logger.CurrentLogLevel = IntConfig.LogLevel;

            LanguageConfigs = new Dictionary<string, LanguageConfig>();
            LoadLanguageConfigs();
        }

        private void LoadLanguageConfigs()
        {
            foreach (var config in Directory.EnumerateFiles(_configPath))
            {
                // Filter out non json files to be on the safe side and exclude the internal config file
                if (!config.EndsWith(".json") || config.Contains("internal.json"))
                {
                    Logger.LogDebug("Skipping file {0} due to naming", config);
                    continue;
                }

                var languageConfigContents = File.ReadAllText(config);
                var current = JsonSerializer.Deserialize<LanguageConfig>(languageConfigContents);
                Debug.Assert(current != null, nameof(current) + " != null");
                Logger.LogDebug("Loaded config for {0}", current.language);
                Logger.LogDebug("{0}", current);

                LanguageConfigs.Add(current.extension, current);
            }

            Logger.LogInfo("Loaded " + LanguageConfigs.Count + " language configs");
        }

        // Loads the expected values for the given chapter
        public ChapterConfig LoadChapterConfig(string chapterPath)
        {
            var expectedValuesPath = chapterPath + Path.DirectorySeparatorChar + "expected.json";
            if (!File.Exists(expectedValuesPath) &&
                Program.Instance.ConfigManager.IntConfig.IgnoreMissingExpectedValues)
                return null;
            
            var configContents = File.ReadAllText(expectedValuesPath);
            var current = JsonSerializer.Deserialize<ChapterConfig>(configContents);
            Debug.Assert(current != null, nameof(current) + " != null");
            Debug.Assert(current.OutputValues != null, nameof(current) + " != null");

            current.ExpectedValues = new ValueRow[current.OutputValues.Length];

            for (var i = 0; i < current.ExpectedValues.Length; i++)
            {
                var separatedValues = current.OutputValues[i].Split('\t',
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                current.ExpectedValues[i] = new ValueRow(separatedValues.Length);

                for (var k = 0; k < separatedValues.Length; k++)
                {
                    current.ExpectedValues[i].Values[k] = new ValueWrapper(separatedValues[k]);
                }
            }

            return current;
        }
    }
}