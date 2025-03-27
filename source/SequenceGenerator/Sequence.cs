using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceGenerator
{
    public class Sequence
    {
        public int LedIndex;

        public List<Pattern> Patterns;

        public Sequence()
        {
            Patterns = new List<Pattern>();
        }
    }
}
