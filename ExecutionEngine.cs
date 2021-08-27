﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AAARunCheck
{
    /// <summary>
    /// This class is responsible for executing a given file by a given language configuration.
    /// </summary>
    public class ExecutionEngine
    {
        private readonly string _outputPath;
        private readonly string _outputPathWithoutSeparator;

        // This event gets invoked after an execution completed
        public event EventHandler<ImplementationExecutionEventArgs> ImplementationExecution;

        public ExecutionEngine(string outputPath)
        {
            _outputPath = outputPath.EndsWith(Path.DirectorySeparatorChar)
                ? outputPath
                : outputPath + Path.DirectorySeparatorChar;
            _outputPathWithoutSeparator = outputPath.TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
        }

        // Cleans the output folder by deleting its contents (these are temporary results of compilation or program output)
        public void CleanOutput()
        {
            foreach (var file in Directory.EnumerateFileSystemEntries(_outputPath))
            {
                if (Directory.Exists(file))
                {
                    Directory.Delete(file, true);
                }
                else
                {
                    File.Delete(file);
                }

                Logger.LogDebug("Removed old output entry: {0}", file);
            }
        }

        /// <summary>
        /// Callback method which gets provided to FileManager.SearchAndUtilizeFiles
        /// </summary>
        /// <param name="filename">the path to the current code file to execute</param>
        /// <returns>whether the execution was successful</returns>
        public bool ConfirmFileExecution(string filename)
        {
            var fileExtension = Path.GetExtension(filename).TrimStart('.');

            // Check if language should even be right now (depending on the internal configuration)
            if (!Program.Instance.ConfigManager.IntConfig.FileExtensions.Contains("all"))
            {
                if (!Program.Instance.ConfigManager.IntConfig.FileExtensions.Contains(fileExtension))
                {
                    Logger.LogDebug("Skipping {0} as it is not present in the internal configuration");
                    return true;
                }
            }

            // If there is no language specification (yet), skip the file
            if (!Program.Instance.ConfigManager.LanguageConfigs.TryGetValue(fileExtension, out var languageConfig))
            {
                Logger.LogDebug("Could not find language config for {0}", fileExtension);
                return true;
            }

            // Each LanguageConfig includes {1..n} StepConfigs, which are used to run a set of commands
            foreach (var step in languageConfig.steps)
            {
                var stepResult = RunStep(step, filename);

                Logger.LogDebug("Result for step: {0} was exitCode: {1}", step, stepResult);

                if (stepResult != 0)
                {
                    Logger.LogDebug("Failed execution of: {0} at step: {1} with exit code: {2}", filename, step,
                        stepResult);
                    Logger.LogInfo("Failed execution of: {0}", filename);
                    // Invoke EventHandler for non succeeded execution 
                    ImplementationExecution(this, new ImplementationExecutionEventArgs
                    {
                        ImplLanguageConfig = languageConfig,
                        Succeeded = false
                    });
                    return false;
                }
            }

            Logger.LogInfo("Verified execution of: {0}", filename);
            // Invoke EventHandler for succeeded execution 
            ImplementationExecution(this, new ImplementationExecutionEventArgs
            {
                ImplLanguageConfig = languageConfig,
                Succeeded = true
            });
            return true;
        }

        /// <summary>
        /// Runs a single step out of a language config
        /// </summary>
        /// <param name="config">the current step to execute</param>
        /// <param name="filename">the file path of the current file</param>
        /// <returns></returns>
        private int RunStep(StepConfig config, string filename)
        {
            // This resolves the configuration string to the actual command
            var completeArgs = "";
            try
            {
                completeArgs = DemagifyString(filename, String.Format(config.command, config.args));
            }
            catch (FormatException)
            {
                Logger.LogError("Could not resolve command: filename=[{0}] config.command=[{1}] config.args=[{2}]", filename, config.command,
                    String.Join(",", config.args));
                return 1;
            }

            // Create the properties of the new process
            var psi = new ProcessStartInfo
            {
                // *Some* compilers do not respect this, which is why WORKING_DIR_FULL needs to exist
                // Most of the time this contains the compilation output to the specified directory
                WorkingDirectory = _outputPath,
                FileName = DemagifyString(filename, config.runtime),
                Arguments = completeArgs,
                // Negating here, as redirecting means *not* showing the output
                RedirectStandardOutput = !Program.Instance.ConfigManager.IntConfig.ShowExecutionStandardOutput,
                RedirectStandardError = !Program.Instance.ConfigManager.IntConfig.ShowExecutionErrorOutput
            };
            return RunProcessAndWaitForTermination(psi);
        }

        /// <summary>
        /// Starts a process specified by the parameter
        /// </summary>
        /// <returns>the exit code of the process</returns>
        private int RunProcessAndWaitForTermination(ProcessStartInfo psi)
        {
            try
            {
                using var process = Process.Start(psi);
                Debug.Assert(process != null, nameof(process) + " != null");
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Win32Exception)
            {
                Logger.LogError("Could not execute process: {0} {1}", psi.FileName, psi.Arguments);
                Environment.Exit(1);
                return 1;
            }
        }

        // See more in ConfigManager
        private string DemagifyString(string filename, string withMagic)
        {
            var fileBuilder = new StringBuilder();
            if (withMagic.Contains("ALL_FILES_IN_DIR"))
            {
                foreach (var fileInfo in Directory.GetParent(filename).GetFiles())
                {
                    fileBuilder.Append(fileInfo.FullName).Append(' ');
                }
            }

            return withMagic.Replace("ALL_FILES_IN_DIR", fileBuilder.ToString())
                .Replace("FILE_NAME_WEX", Path.ChangeExtension(Path.GetFileName(filename), null))
                .Replace("FILE_PATH", filename)
                .Replace("WORKING_DIR_FULL", Path.GetFullPath(_outputPathWithoutSeparator))
                .Replace("WORKING_DIR", _outputPathWithoutSeparator);
        }
    }

    public class ImplementationExecutionEventArgs : EventArgs
    {
        public LanguageConfig ImplLanguageConfig { get; set; }
        public bool Succeeded { get; set; }
    }
}