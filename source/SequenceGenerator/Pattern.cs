using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceGenerator
{
    public class Pattern
    {
        // Reference to the pattern data and the sequence
        public int PatternDataIndex;

        public int Interval;
        public int StartPosition;
        public int Duration;

        public Pattern() { }

        public Pattern(int patternDataIndex, int duration, int interval, int startPosition = 0)
        {
            this.PatternDataIndex = patternDataIndex;
            this.Interval = interval;
            this.StartPosition = startPosition;
            this.Duration = duration;
        }

        // Copy constructor
        public Pattern(Pattern pat)
        {
            PatternDataIndex = pat.PatternDataIndex;
            Interval = pat.Interval;
            StartPosition = pat.StartPosition;
            Duration = pat.Duration;
        }
    }
}
