namespace AAARunCheck.Config
{
    public class LanguageConfig
    {
        public string Language { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }

        public StepConfig[] Steps { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Language)}: {Language}, {nameof(Extension)}: {Extension}, {nameof(Description)}: {Description}, {nameof(Steps)}: {Steps}";
        }
        
    }
}