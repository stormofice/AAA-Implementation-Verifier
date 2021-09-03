using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AAARunCheck.Config;

namespace AAARunCheck
{
    /// <summary>
    /// This class is responsible for executing a given file by a given language configuration.
    /// </summary>
    public class ExecutionEngine
    {
        // Magic number to return as exit code if execution fails and running further steps needs to be stopped
        private readonly int EXECUTION_FATAL_RESULT = 97;

        private readonly string _outputPath;
        private readonly string _outputPathWithoutSeparator;

        // This event gets invoked before an implementation gets executed
        public event EventHandler<ImplementationExecutionStartEventArgs> ImplementationExecutionStart;

        // This event gets invoked after an implementation got executed
        public event EventHandler<ImplementationExecutionStopEventArgs> ImplementationExecutionStop;

        public ExecutionEngine(string outputPath)
        {
            _outputPath = outputPath.EndsWith(Path.DirectorySeparatorChar)
                ? outputPath
                : outputPath + Path.DirectorySeparatorChar;
            _outputPathWithoutSeparator = outputPath.TrimEnd(Path.DirectorySeparatorChar);

            if (!Directory.Exists(outputPath))
            {
                Logger.LogInfo("Creating output directory");
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

            // Check if language should even be tested right now (depending on the internal configuration)
            if (!Program.Instance.ConfigManager.IntConfig.FileExtensions.Contains("all"))
            {
                if (!Program.Instance.ConfigManager.IntConfig.FileExtensions.Contains(fileExtension))
                {
                    Logger.LogDebug("Skipping {0} as it is not present in the internal configuration", fileExtension);
                    return true;
                }
            }

            // If there is no language specification (yet), skip the file
            if (!Program.Instance.ConfigManager.LanguageConfigs.TryGetValue(fileExtension, out var languageConfig))
            {
                Logger.LogDebug("Could not find language config for {0}", fileExtension);
                return true;
            }

            ImplementationExecutionStart(this, new ImplementationExecutionStartEventArgs
            {
                ImplLanguageConfig = languageConfig,
                Filename = filename
            });

            var stepCounter = 1;

            // Each LanguageConfig includes {1..n} StepConfigs, which are used to run a set of commands
            foreach (var step in languageConfig.steps)
            {
                var stepResult = RunStep(step, filename);

                Logger.LogDebug("Result for step: {0} was exitCode: {1}", step, stepResult);

                if (stepResult != 0)
                {
                    Logger.LogDebug("Failed execution of: {0} at step: {1} with exit code: {2}", filename, step,
                        stepResult);
                    Logger.LogWarn("Failed execution of: {0}", filename);
                    // Invoke EventHandler for non succeeded execution 
                    ImplementationExecutionStop(this, new ImplementationExecutionStopEventArgs
                    {
                        ImplLanguageConfig = languageConfig,
                        Filename = filename,
                        Succeeded = false,
                        CurrentStepIndex = stepCounter,
                        CurrentStep = step,
                        ExitCode = stepResult
                    });

                    // This will also invoke the logging infrastructure, which will output all results until this point
                    if (stepResult == EXECUTION_FATAL_RESULT)
                    {
                        Program.Instance.StatisticsCollector.StopMeasuring();
                        Program.Instance.StatisticsCollector.OutputStats();
                        Environment.Exit(1);
                    }
                        
                    return false;
                }

                stepCounter++;
            }

            Logger.LogInfo("Verified execution of: {0}", filename);
            // Invoke EventHandler for succeeded execution 
            ImplementationExecutionStop(this, new ImplementationExecutionStopEventArgs
            {
                ImplLanguageConfig = languageConfig,
                Filename = filename,
                Succeeded = true,
                CurrentStepIndex = stepCounter,
                ExitCode = 0
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
            string completeArgs;
            try
            {
                completeArgs = DemagifyString(filename, String.Format(config.command, config.args));
            }
            catch (FormatException)
            {
                Logger.LogError("Could not resolve command: filename=[{0}] config.command=[{1}] config.args=[{2}]",
                    filename, config.command,
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
                return EXECUTION_FATAL_RESULT;
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

    public class ImplementationExecutionStartEventArgs : EventArgs
    {
        public LanguageConfig ImplLanguageConfig { get; set; }
        public string Filename { get; set; }
    }

    public class ImplementationExecutionStopEventArgs : EventArgs
    {
        public LanguageConfig ImplLanguageConfig { get; set; }
        public bool Succeeded { get; set; }
        public string Filename { get; set; }

        public int CurrentStepIndex { get; set; }
        public StepConfig CurrentStep { get; set; }
        public int ExitCode { get; set; }
    }
}