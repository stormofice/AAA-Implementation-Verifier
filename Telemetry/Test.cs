namespace AAARunCheck.Telemetry
{
    public class Test
    {
        public string title { get; set; }
        public string fullTitle { get; set; }
        public string file { get; set; }
        public long duration { get; set; }

        // Not yet supported
        public int currentRetry { get; set; }
        public string speed { get; set; }

        public Error err { get; set; }
    }
}