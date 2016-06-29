using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanesePuzzle
{
    public class Cell
    {
        //public int Line { get; set; }
        //public int Column { get; set; }
        //public int Value { get; set; }

        public int Line;
        public int Column;
        public int Value;

        public Cell(int line, int column, int value)
        {
            Line = line;
            Column = column;
            Value = value;
        }

    }
}
