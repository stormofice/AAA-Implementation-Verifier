using System;
using System.IO;

namespace AAARunCheck
{
    /// <summary>
    /// This class is responsible for walking through the AAA's contents directory and executing code examples if found
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// This method recursively goes through every directory starting from the first call's currentPath.
        /// It uses the callback function to determine whether to stop (false will be returned on error).
        /// </summary>
        /// <param name="currentPath">the path to handle in the current call</param>
        /// <param name="callback">the callback method to execute</param>
        /// <param name="pred">a predicate to apply before trying to execute a file</param>
        /// <returns></returns>
        public bool SearchAndUtilizeFiles(string currentPath, Func<string, bool> callback, Predicate<string> pred)
        {
            foreach (var currentEntry in Directory.GetFileSystemEntries(currentPath))
            {
                if (Directory.Exists(currentEntry))
                {
                    if (!SearchAndUtilizeFiles(currentEntry, callback, pred))
                        return false;
                }
                else
                {
                    if (pred.Invoke(currentEntry))
                    {
                        var cbr = callback(currentEntry);
                        if (Program.Instance.ConfigManager.IntConfig.StopOnExecutionError && !cbr)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}