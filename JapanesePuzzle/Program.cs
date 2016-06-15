using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanesePuzzle
{
    class Program
    {
        static void Main(string[] args)
        {
            var solver = new Solver("solve1_small.txt");
            //var solver = new Solver("solve2_small.txt");

            var read = "";
            while (string.IsNullOrEmpty(read))
            {
                solver.DoStep();
                solver.Debug();
                read =Console.ReadLine();    
                Console.Clear();
            }
        }
    }
}
