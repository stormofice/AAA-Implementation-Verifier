namespace AAARunCheck.Config
{
    public class LanguageConfig
    {
        public string language { get; set; }
        public string extension { get; set; }
        public string description { get; set; }

        public StepConfig[] steps { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(language)}: {language}, {nameof(extension)}: {extension}, {nameof(description)}: {description}, {nameof(steps)}: {steps}";
        }
        
    }
}