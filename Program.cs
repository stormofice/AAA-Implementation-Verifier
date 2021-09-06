using System;
using System.Diagnostics;
using AAARunCheck.Config;

namespace AAARunCheck
{
    /// <summary>
    /// Entry point for the project.
    /// Executing it works as follows:
    /// dotnet run OR NameOfExecutable <path to the contents directory of aaa> <path to the folder with the language configs> <path for temporary output files>
    /// </summary>
    class Program
    {
        public static readonly Program Instance = new Program();

        public ConfigManager ConfigManager;
        public ExecutionEngine ExecutionEngine;
        public StatisticsCollector StatisticsCollector;

        private FileManager _fileManager;
        public OutputValidator OutputValidator;

        private static int Main(string[] args)
        {
            return Instance.Init(args);
        }

        private int Init(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine(
                    "Usage: ./aaaruncheck <contents or chapter path> <config directory> <working directory>");
                return 1;
            }

            var repoDirectory = args[0];
            var configDirectory = args[1];
            var workingDirectory = args[2];

            Logger.LogDebug("RepoDir: {0}, ConfigDir: {1}, WorkingDir: {2}", repoDirectory, configDirectory,
                workingDirectory);

            ConfigManager = new ConfigManager(configDirectory);

            _fileManager = new FileManager();
            OutputValidator = new OutputValidator();
            
            ExecutionEngine = new ExecutionEngine(workingDirectory);
            StatisticsCollector = new StatisticsCollector();

            StatisticsCollector.StartMeasuring();
            _fileManager.SearchAndUtilizeFiles(repoDirectory, ExecutionEngine.ConfirmFileExecution,
                str => str.Contains("code/"));
            StatisticsCollector.StopMeasuring();

            ExecutionEngine.CleanOutput();

            StatisticsCollector.OutputStats();

            return 0;
        }
    }
}