using System.Collections.Generic;
using System.Linq;
using SequenceGenerator;

namespace MicrocontrollerSimulation
{
    public class VirtualLEDProcessor
    {
        // This method recreates the PWM signal timeline for a specific LED
        public List<double> ReconstructTimeline(JsonContent content, int ledIndex)
        {
            List<double> timeline = new List<double>();
            var sequence = content.Sequences.FirstOrDefault(s => s.LedIndex == ledIndex);

            if (sequence == null) return timeline;

            foreach (var pattern in sequence.Patterns)
            {
                var patternData = content.PatternDatas[pattern.PatternDataIndex].Data;

                // hardware logic: repeat the data for the specified Duration
                // Since your steps are 10ms, we loop accordingly
                int elapsed = 0;
                while (elapsed < pattern.Duration)
                {
                    foreach (int pwmValue in patternData)
                    {
                        // Normalize to 0.0 - 1.0 range for the graph
                        timeline.Add(pwmValue / 65535.0);
                        elapsed += 10;
                        if (elapsed >= pattern.Duration) break;
                    }
                }

                // If Interval is longer than Duration, the gap is filled with baseline (0.0)
                int gap = pattern.Interval - pattern.Duration;
                for (int i = 0; i < gap; i += 10)
                {
                    timeline.Add(0.0);
                }
            }
            return timeline;
        }
    }
}