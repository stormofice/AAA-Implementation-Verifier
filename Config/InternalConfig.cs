namespace AAARunCheck.Config
{
    public class InternalConfig
    {
        public Logger.LogLevel LogLevel { get; set; }
        public string[] FileExtensions { get; set; }

        public bool ShowExecutionErrorOutput { get; set; }
        public bool ShowExecutionStandardOutput { get; set; }
        public bool StopOnExecutionError { get; set; }
        
        public string RedirectJsonToFile { get; set; }
        
        public bool IgnoreMissingExpectedValues { get; set; }
    }
}