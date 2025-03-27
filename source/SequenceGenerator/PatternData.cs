using System.Collections.Generic;

namespace SequenceGenerator
{
    public class PatternData
    {
        public string Name { get; set; }

        public List<int> Data;

        public PatternData()
        {
            Data = new List<int>();
            Name = "";
        }
    }
}
