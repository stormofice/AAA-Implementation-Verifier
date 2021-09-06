using System.Text.Json.Serialization;

namespace AAARunCheck.Config
{
    public class ChapterConfig
    {
        public string Description { get; set; }
        public decimal Delta { get; set; }
        
        public string[] OutputValues { get; set; }
        
        // Gets set manually, as the values need to be parsed manually (no JSON support for decimal values)
        [JsonIgnore]
        public decimal[] ExpectedValues { get; set; }
        
    }
}