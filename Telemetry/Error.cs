namespace AAARunCheck.Telemetry
{
    // Not all entries are supported currently
    public class Error
    {
        public string stack { get; set; }
        public string message { get; set; }
        public bool generatedMessage { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public int timeout { get; set; }
        public string actual { get; set; }
        public string expected { get; set; }
        
        // FIXME: Not the same as in mocha as operator is a reserved keyword
        public string operator_ { get; set; }
        
        // Seems redundant, but is in spec
        public string file { get; set; }
    }
}