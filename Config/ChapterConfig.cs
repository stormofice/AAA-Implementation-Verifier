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
        public ValueRow[] ExpectedValues { get; set; }

        public override string ToString()
        {
            return $"{nameof(Description)}: {Description}, {nameof(Delta)}: {Delta}, {nameof(OutputValues)}: {OutputValues}, {nameof(ExpectedValues)}: {ExpectedValues}";
        }
    }
}