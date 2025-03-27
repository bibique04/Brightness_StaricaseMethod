using System.Collections.Generic;
using System.Linq;

namespace SequenceGenerator
{
    public class JsonContent
    {
        public List<PatternData> PatternDatas;
        public List<Sequence> Sequences;

        public JsonContent()
        {
            PatternDatas = new List<PatternData>();
            Sequences = new List<Sequence>();
        }

        public PatternData GetPatternDataAtIndex(int index)
        {
            if (index < PatternDatas.Count && index >= 0)
                return PatternDatas.ElementAt(index);

            return null;
        }
    }
}
