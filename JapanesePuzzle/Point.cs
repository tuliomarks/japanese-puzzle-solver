using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanesePuzzle
{
    public class Point
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public int Value { get; set; }

        public Point(int line, int column, int value)
        {
            Line = line;
            Column = column;
            Value = value;
        }

    }
}
