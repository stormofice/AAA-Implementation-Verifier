using System;

namespace AAARunCheck.Config
{
    public class StepConfig
    {
        public string Runtime { get; set; }
        public string Command { get; set; }
        public string[] Args { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Runtime)}: {Runtime}, {nameof(Command)}: {Command}, {nameof(Args)}: [{String.Join(",", Args)}]";
        }
    }
}