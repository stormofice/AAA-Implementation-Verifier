namespace AAARunCheck.Telemetry
{
    public class BasicStats
    {
        public int suites { get; set; }
        public int tests { get; set; }
        public int passes { get; set; }
        public int pending { get; set; }
        public int failures { get; set; }

        // As date strings
        public string start { get; set; }
        public string end { get; set; }

        // In milliseconds
        public long duration { get; set; }

    }
}