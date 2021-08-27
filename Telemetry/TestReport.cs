using System.Collections.Generic;

namespace AAARunCheck.Telemetry
{
    public class TestReport
    {
        
        public BasicStats stats { get; set; }
        public List<Test> tests { get; set; }
        public List<Test> pending { get; set; }
        public List<Test> failures { get; set; }
        public List<Test> passes { get; set; }

    }
}