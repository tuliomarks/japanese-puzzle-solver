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

        private List<Cell> Grid { get; set; }
        private int[][] LinesClues { get; set; }
        private int[][] ColumnsClues { get; set; }

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

                Grid = new List<Cell>();
                LinesClues = new int[aux0][];
                ColumnsClues = new int[aux1][];

                var cntLines = 0;
                var cntColumns = 0;
                while (!sr.EndOfStream)
                {
                    ln = sr.ReadLine();
                    if (string.IsNullOrEmpty(ln) || (string.IsNullOrEmpty(ln.Replace(" ", string.Empty))))
                        split = new string[0];
                    else
                        split = ln.Split(' ');

                    if (cntLines < LinesClues.Length)
                    {
                        cntLines++;
                        LinesClues[cntLines - 1] = new int[split.Count(x => (!string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(x.Replace(" ", string.Empty))))];

                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i]) || string.IsNullOrEmpty(split[i].Replace(" ", string.Empty))) continue;
                            LinesClues[cntLines - 1][i] = Convert.ToInt16(split[i]);
                        }
                    }
                    else if (cntColumns < ColumnsClues.Length)
                    {
                        cntColumns++;
                        ColumnsClues[cntColumns - 1] = new int[split.Count(x => (!string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(x.Replace(" ", string.Empty))))];

                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i]) || string.IsNullOrEmpty(split[i].Replace(" ", string.Empty))) continue;
                            ColumnsClues[cntColumns - 1][i] = Convert.ToInt16(split[i]);
                        }
                    }
                }

                for (int i = 0; i < aux0; i++)
                {
                    for (int j = 0; j < aux1; j++)
                    {
                        Grid.Add(new Cell(i, j, 0));
                    }
                }

            }
        }

        public void DoStep()
        {

            if (NextStep == 0)
            {
                #region Verifica se tem alguma linha ou coluna com tamanho cheio ou proximo
                for (int i = 0; i < LinesClues.Length; i++)
                {
                    var checks = LinesClues[i];
                    if (checks.Length == 1)
                    {
                        var middle = LineSize / 2;
                        if (checks[0] <= middle) continue;

                        var start = LineSize - checks[0];
                        for (int j = start; j < LineSize - start; j++)
                        {
                            SetValue(i, j, 1);
                        }
                    }
                }

                for (int j = 0; j < ColumnsClues.Length; j++)
                {
                    var checks = ColumnsClues[j];
                    if (checks.Length == 1)
                    {
                        var middle = ColumnSize / 2;
                        if ((checks.Length != 1) || (checks[0] <= middle)) continue;

                        var start = ColumnSize - checks[0];
                        for (int i = start; i < ColumnSize - start; i++)
                        {
                            SetValue(i, j, 1);
                        }
                    }
                }
                #endregion
                NextStep = 1;
            }
            else if (NextStep == 1)
            {
                #region Verifica se tem linha e colunas que ficam completas, entao marca tudo com X

                for (int i = 0; i < LinesClues.Length; i++)
                {
                    var sumLine = Grid.Where(x => x.Line == i).Sum(x => x.Value);
                    if (sumLine == 0) continue;
                    else if (sumLine == LinesClues[i].Sum()) // se já encontrou todos da linha marca com X
                    {
                        for (int j = 0; j < LineSize; j++)
                        {
                            if (GetValue(i, j) == 0)
                                SetValue(i, j, -1);
                        }
                    }
                }

                for (int j = 0; j < ColumnsClues.Length; j++)
                {
                    var sumColumn = Grid.Where(x => x.Column == j).Sum(x => x.Value);
                    if (sumColumn == 0) continue;
                    else if (sumColumn == ColumnsClues[j].Sum()) // se já encontrou todos da linha marca com X
                    {
                        for (int i = 0; i < ColumnSize; i++)
                        {
                            if (GetValue(i, j) == 0)
                                SetValue(i, j, -1);
                        }
                    }
                }
                #endregion
                NextStep = 2;
            }
            else if (NextStep == 2)
            {
                #region Verifica as linha com uma Clue e um valor preenchido e então estima a pintura e restante fica com X
                for (int i = 0; i < LinesClues.Length; i++)
                {

                    if (LinesClues[i].Length > 1) continue;
                    var primaryClue = LinesClues[i][0];
                    foreach (var cell in Grid.Where(x => x.Line == i && x.Value == 1))
                    {
                        // aqui vai testar esquerda e direita da posicao que esta marcada e vai estimar os proximos

                        //se esta no inicio e as posicoes a esquerda necessarias estao livres entao marca elas
                        if (cell.Column == 0)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Line == i && x.Column < primaryClue && x.Value == 1);
                            if (cells.Any())
                                foreach (var cell1 in cells)
                                    cell1.Value = -1;
                            else
                                foreach (var cell1 in cells)
                                    cell1.Value = 1;
                        }
                        // se esta no final faz o mesmo teste para as colunas da esquerda
                        else if (cell.Column == LineSize)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Line == i && x.Column > LineSize - primaryClue && x.Value == 1);
                            if (cells.Any())
                                foreach (var cell1 in cells)
                                    cell1.Value = -1;
                            else
                                foreach (var cell1 in cells)
                                    cell1.Value = 1;
                        }
                        // se nao esta no inicio nem no fim, obrigatoriamente esta no meio
                        else
                        {
                            var startLeft = cell.Column - primaryClue;
                            var finishRight = cell.Column + primaryClue;

                            foreach (var cell1 in Grid.Where(x => x.Line == i && x.Column <= startLeft && x.Value == 0))
                                cell1.Value = -1;

                            foreach (var cell1 in Grid.Where(x => x.Line == i && x.Column >= finishRight && x.Value == 0))
                                cell1.Value = -1;

                        }

                    }
                }

                for (int i = 0; i < ColumnsClues.Length; i++)
                {

                    if (ColumnsClues[i].Length > 1) continue;
                    var primaryClue = ColumnsClues[i][0];
                    foreach (var cell in Grid.Where(x => x.Column == i && x.Value == 1))
                    {
                        // aqui vai testar esquerda e direita da posicao que esta marcada e vai estimar os proximos

                        //se esta no inicio e as posicoes a esquerda necessarias estao livres entao marca elas
                        if (cell.Line == 0)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Column == i && x.Line < primaryClue && x.Value == 1);
                            if (cells.Any())
                                foreach (var cell1 in cells)
                                    cell1.Value = -1;
                            else
                                foreach (var cell1 in cells)
                                    cell1.Value = 1;
                        }
                        // se esta no final faz o mesmo teste para as colunas da esquerda
                        else if (cell.Line == ColumnSize)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Column == i && x.Line > ColumnSize - primaryClue && x.Value == 1);
                            if (cells.Any())
                                foreach (var cell1 in cells)
                                    cell1.Value = -1;
                            else
                                foreach (var cell1 in cells)
                                    cell1.Value = 1;
                        }
                        // se nao esta no inicio nem no fim, obrigatoriamente esta no meio
                        else
                        {
                            var startTop = cell.Line - primaryClue;
                            var finishBottom = cell.Line + primaryClue;

                            foreach (var cell1 in Grid.Where(x => x.Column == i && x.Line <= startTop && x.Value == 0))
                                cell1.Value = -1;

                            foreach (var cell1 in Grid.Where(x => x.Column == i && x.Line >= finishBottom && x.Value == 0))
                                cell1.Value = -1;
                        }

                    }
                }
                #endregion

            }
            //TODO: procura por espaços que nao é possivel inserir nenhum pintado e marca com X
            else if (NextStep == 3)
            {

                //valida linhas
                foreach (var cell in Grid.Where(x => x.Value == 1).OrderBy(x => x.Line))
                {

                }

            }
        }

        private int GetValue(int line, int column)
        {
            return Grid.First(x => x.Line == line && x.Column == column).Value;
        }

        private void SetValue(int line, int column, int value)
        {
            Grid.First(x => x.Line == line && x.Column == column).Value = value;
        }

        public string Debug()
        {

            var ret = string.Empty;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("   ");
            for (int j = 0; j < ColumnsClues.Length; j++)
            {
                Console.Write("{0}", j.ToString().PadLeft(3));
            }
            Console.ResetColor();
            Console.Write("\n");

            for (int i = 0; i < LinesClues.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0}", i.ToString().PadLeft(3));
                Console.ResetColor();
                for (int j = 0; j < ColumnsClues.Length; j++)
                {
                    if (GetValue(i, j) == 0)
                        Console.Write("   ");
                    else if (GetValue(i, j) == 1)
                        Console.Write("  #");
                    else if (GetValue(i, j) == -1)
                        Console.Write("  X");
                }
                Console.Write("\n");
            }
            return ret;
        }
    }
}
