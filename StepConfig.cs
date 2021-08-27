using System;

namespace AAARunCheck
{
    public class StepConfig
    {
        public string runtime { get; set; }
        public string command { get; set; }
        public string[] args { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(runtime)}: {runtime}, {nameof(command)}: {command}, {nameof(args)}: [{String.Join(",", args)}]";
        }
    }
}