using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanesePuzzle
{
    class Program
    {
        static void Main(string[] args)
        {
            var read = "";
            while (string.IsNullOrEmpty(read))
            {
                Console.WriteLine("Informe o nome do arquivo de teste:");
                var arq = Console.ReadLine();

                if (!File.Exists(arq)) throw new Exception("arquivo nao existe");

                Console.Clear();

                var solver = new Solver(arq);                
                var res = solver.Solve();
                if (!res)
                {
                    solver.Debug();
                    Console.WriteLine("As heuristicas nao resolveram completamente, deseja finalizar com o Backtracking? (S/N)");
                    var bruteForce = Console.ReadLine();

                    if (bruteForce == "S" || bruteForce == "s")
                    {
                        solver.PrepareBruteForce();
                        res = solver.SolveBruteForce(solver.BruteGrid, 0);

                        Console.Clear();
                        solver.DebugBruteForce(solver.BruteGrid);
                    }
                        
                }
                else
                {
                    Console.Clear();
                    solver.Debug();
                }

                if (res)
                    Console.WriteLine("Resolvido!!!");
                else
                    Console.WriteLine("Não foi possivel resolver...");

                read = Console.ReadLine();
                Console.Clear();
            }
        }
    }
}
