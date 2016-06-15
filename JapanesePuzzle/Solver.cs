using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanesePuzzle
{
    public class Solver
    {

        private int[,] Grid { get; set; }
        private int[][] Lines { get; set; }
        private int[][] Columns { get; set; }
        private Stack<int> StackLines { get; set; }
        private Stack<int> StackColumns { get; set; }

        private int NextStep = 0;
        private int LineSize = 0;
        private int ColumnSize = 0;


        public Solver(string arquivo)
        {
            if (!File.Exists(arquivo)) throw new FileNotFoundException();

            using (var sr = new StreamReader(new FileStream(arquivo, FileMode.Open)))
            {
                var ln = sr.ReadLine();
                var split = ln.Split(' ');
                if (split.Length < 2) throw new ArgumentException("Qtde de linhas e colunas nao informado corretamente. Usar 10 20");

                var aux0 = Convert.ToInt16(split[0]);
                var aux1 = Convert.ToInt16(split[1]);

                LineSize = aux1;
                ColumnSize = aux0;

                Grid = new int[aux0, aux1];
                Lines = new int[aux0][];
                Columns = new int[aux1][];
                StackColumns = new Stack<int>();
                StackLines = new Stack<int>();

                var cntLines = 0;
                var cntColumns = 0;
                while (!sr.EndOfStream)
                {
                    ln = sr.ReadLine();
                    if (string.IsNullOrEmpty(ln) || (string.IsNullOrEmpty(ln.Replace(" ", string.Empty))))
                        split = new string[0];
                    else
                        split = ln.Split(' ');

                    if (cntLines < Lines.Length)
                    {
                        cntLines++;
                        Lines[cntLines - 1] = new int[split.Count(x => (!string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(x.Replace(" ", string.Empty))))];

                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i]) || string.IsNullOrEmpty(split[i].Replace(" ", string.Empty))) continue;
                            Lines[cntLines - 1][i] = Convert.ToInt16(split[i]);
                        }
                    }
                    else if (cntColumns < Columns.Length)
                    {
                        cntColumns++;
                        Columns[cntColumns - 1] = new int[split.Count(x => (!string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(x.Replace(" ", string.Empty))))];

                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i]) || string.IsNullOrEmpty(split[i].Replace(" ", string.Empty))) continue;
                            Columns[cntColumns - 1][i] = Convert.ToInt16(split[i]);
                        }
                    }
                }
            }
        }

        public void DoStep()
        {

            // primeira etapa procura se tem alguma linha ou coluna com tamanho cheio ou proximo
            if (NextStep == 0)
            {
                for (int i = 0; i < Lines.Length; i++)
                {
                    var checks = Lines[i];
                    if (checks.Length == 1)
                    {
                        var middle = LineSize / 2;
                        if (checks[0] <= middle) continue;

                        var start = LineSize - checks[0];
                        for (int j = start; j < LineSize - start; j++)
                        {
                            Grid[i, j] = 1;
                        }
                    }
                }

                for (int j = 0; j < Columns.Length; j++)
                {
                    var checks = Columns[j];
                    if (checks.Length == 1)
                    {
                        var middle = ColumnSize / 2;
                        if ((checks.Length != 1) || (checks[0] <= middle)) continue;

                        var start = ColumnSize - checks[0];
                        for (int i = start; i < ColumnSize - start; i++)
                        {
                            Grid[i, j] = 1;
                        }
                    }
                }

                NextStep = 1;
            }
            // verifica nas linhas se existe alguma que pode ter um X 
            else if (NextStep == 1)
            {
                for (int i = 0; i < Lines.Length; i++)
                {
                    var sumLine = SumLine(i);
                    if (sumLine == 0) continue;
                    else if (sumLine == Lines[i].Sum()) // se já encontrou todos da linha marca com X
                    {
                        for (int j = 0; j < LineSize; j++)
                        {
                            if (Grid[i, j] == 0)
                                Grid[i, j] = -1;
                        }
                    }
                }

                for (int j = 0; j < Columns.Length; j++)
                {
                    var sumColumn = SumColumn(j);
                    if (sumColumn == 0) continue;
                    else if (sumColumn == Columns[j].Sum()) // se já encontrou todos da linha marca com X
                    {
                        for (int i = 0; i < ColumnSize; i++)
                        {
                            if (Grid[i, j] == 0)
                                Grid[i, j] = -1;
                        }
                    }
                }

                NextStep = 2;
            }            
            //TODO: procura por espaços que nao é possivel inserir nenhum pintado e marca com X
            else if (NextStep == 2)
            {

            }
        }

        private int SumLine(int i)
        {
            var sum = 0;
            for (int j = 0; j < Columns.Length; j++)
            {
                if (Grid[i, j] > 0)
                    sum += Grid[i, j];
            }
            return sum;
        }

        private int SumColumn(int j)
        {
            var sum = 0;
            for (int i = 0; i < Columns.Length; i++)
            {
                if (Grid[i, j] > 0)
                    sum += Grid[i, j];
            }
            return sum;
        }

        private int CountEmptyLine(int i)
        {
            var sum = 0;
            for (int j = 0; j < Columns.Length; j++)
            {
                if (Grid[i, j] == 0)
                    sum += Grid[i, j];
            }
            return sum;
        }

        private int CountEmptyColumn(int j)
        {
            var sum = 0;
            for (int i = 0; i < Columns.Length; i++)
            {
                if (Grid[i, j] == 0)
                    sum += Grid[i, j];
            }
            return sum;
        }

        public string Debug()
        {

            var ret = string.Empty;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("   ");
            for (int j = 0; j < Columns.Length; j++)
            {
                Console.Write("{0}", j.ToString().PadLeft(3));
            }
            Console.ResetColor();
            Console.Write("\n");

            for (int i = 0; i < Lines.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0}", i.ToString().PadLeft(3));
                Console.ResetColor();
                for (int j = 0; j < Columns.Length; j++)
                {
                    if (Grid[i, j] == 0)
                        Console.Write("   ");
                    else if (Grid[i, j] == 1)
                        Console.Write("  #");
                    else if (Grid[i, j] == -1)
                        Console.Write("  X");
                }
                Console.Write("\n");
            }
            return ret;
        }
    }
}
