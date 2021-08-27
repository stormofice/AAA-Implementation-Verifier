using System;
using System.Diagnostics;

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
        
        private FileManager _fileManager;
        private StatisticsCollector _statisticsCollector;
        
        private static int Main(string[] args)
        {
            return Instance.Init(args);
        }

        private int Init(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: ./aaaruncheck <contents path> <config directory> <working directory>");
                return 1;
            }

            var repoDirectory = args[0];
            var configDirectory = args[1];
            var workingDirectory = args[2];

            ConfigManager = new ConfigManager(configDirectory);
            Logger.CurrentLogLevel = ConfigManager.IntConfig.LogLevel;
            
            _fileManager = new FileManager();
            ExecutionEngine = new ExecutionEngine(workingDirectory);
            _statisticsCollector = new StatisticsCollector();

            _statisticsCollector.StartMeasuring();
            _fileManager.SearchAndUtilizeFiles(repoDirectory, ExecutionEngine.ConfirmFileExecution, str => str.Contains("code/"));
            _statisticsCollector.StopMeasuring();
            
            ExecutionEngine.CleanOutput();
            
            _statisticsCollector.OutputStats();
            
            return 0;
        }
        
    }
}