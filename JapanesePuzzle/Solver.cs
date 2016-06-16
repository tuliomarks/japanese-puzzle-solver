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
                // tambem verifica se é o restante e pinta

                for (int i = 0; i < LinesClues.Length; i++)
                {

                    var paintedCells = Grid.Where(x => x.Line == i && x.Value == 1).ToArray();
                    var groups = 0;
                    for (int j = 0; j < paintedCells.Count(); j++)
                    {
                        if (j == 0)
                        {
                            groups++;
                            continue;
                        }
                        if (paintedCells[j].Column - paintedCells[j-1].Column > 1)
                            groups++;
                    }
                    if (LinesClues[i].Length != paintedCells.Count()) continue;

                    var currentClue = 0;
                    var lastFinishRight = 0;
                    var removedCells = new List<Cell>();
                    foreach (var cell in paintedCells)
                    {                        
                        if (removedCells.Contains(cell)) continue;

                        var clue = LinesClues[i][currentClue];
                        // aqui vai testar esquerda e direita da posicao que esta marcada e vai estimar os proximos

                        //se esta no inicio e as posicoes a esquerda necessarias estao livres entao marca elas
                        if (cell.Column == 0)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Line == i && x.Column < clue && x.Value == 0);
                            foreach (var cell1 in cells)
                                cell1.Value = 1;                           
                        }
                        // se esta no final faz o mesmo teste para as colunas da esquerda
                        else if (cell.Column == LineSize - 1)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Line == i && x.Column < LineSize - clue && x.Column >= lastFinishRight && x.Value == 0);
                            foreach (var cell1 in cells)
                                cell1.Value = -1;

                            cells = Grid.Where(x => x.Line == i && x.Column >= LineSize - clue && x.Value == 0);
                            foreach (var cell1 in cells)
                                cell1.Value = 1;
                        }
                        // se nao esta no inicio nem no fim, obrigatoriamente esta no meio
                        else
                        {
                            var startLeft = cell.Column - clue;
                            var finishRight = cell.Column + clue;

                            // remove as celulas ja pintadas de dentro da faixa que pode chegar
                            removedCells.AddRange(Grid.Where(x => x.Line == i && x.Column > startLeft && x.Column < finishRight && x.Value == 1));                            

                            foreach (var cell1 in Grid.Where(x => x.Line == i && x.Column <= startLeft && x.Column >= lastFinishRight && x.Value == 0))
                                cell1.Value = -1;

                            if (LinesClues[i].Length == 1 || currentClue == LinesClues[i].Length - 1) // só completa pra direita se não tem nenhum a direita
                                foreach (var cell1 in Grid.Where(x => x.Line == i && x.Column >= finishRight && x.Value == 0))
                                    cell1.Value = -1;

                            var cells = Grid.Where(x => x.Line == i && x.Column > startLeft && x.Column < finishRight && (x.Value == 0 || x.Value == 1));
                            if (cells.Count() == clue)
                                foreach (var cell2 in cells.Where(x => x.Value == 0))
                                    cell2.Value = 1;

                            lastFinishRight = finishRight;

                        }
                        currentClue++;
                    }

                }

                for (int i = 0; i < ColumnsClues.Length; i++)
                {

                    var paintedCells = Grid.Where(x => x.Column == i && x.Value == 1).ToArray();
                    var groups = 0;
                    for (int j = 0; j < paintedCells.Count(); j++)
                    {
                        if (j == 0)
                        {
                            groups++;
                            continue;
                        }
                        if (paintedCells[j].Line - paintedCells[j - 1].Line > 1)
                            groups++;
                    }
                    if (ColumnsClues[i].Length != groups) continue;

                    var currentClue = 0;
                    var lastFinishBottom = 0;
                    var removedCells = new List<Cell>();
                    foreach (var cell in paintedCells)
                    {
                        if (removedCells.Contains(cell)) continue;

                        var clue = ColumnsClues[i][currentClue];
                        // aqui vai testar esquerda e direita da posicao que esta marcada e vai estimar os proximos

                        //se esta no inicio e as posicoes a esquerda necessarias estao livres entao marca elas
                        if (cell.Line == 0)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Column == i && x.Line < clue && x.Value == 0);
                            foreach (var cell1 in cells)
                                cell1.Value = 1;
                        }
                        // se esta no final faz o mesmo teste para as colunas da esquerda
                        else if (cell.Line == ColumnSize - 1)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Column == i && x.Line < ColumnSize - clue && x.Line >= lastFinishBottom && x.Value == 0);
                            foreach (var cell1 in cells)
                                cell1.Value = -1;

                            cells = Grid.Where(x => x.Column == i && x.Line >= ColumnSize - clue && x.Value == 0);
                            foreach (var cell1 in cells)
                                cell1.Value = 1;
                        }
                        // se nao esta no inicio nem no fim, obrigatoriamente esta no meio
                        else
                        {
                            var startTop = cell.Line - clue;
                            var finishBottom = cell.Line + clue;

                            // remove as celulas ja pintadas de dentro da faixa que pode chegar
                            removedCells.AddRange(Grid.Where(x => x.Column == i && x.Line > startTop && x.Line < finishBottom && x.Value == 1));                            

                            foreach (var cell1 in Grid.Where(x => x.Column == i && x.Line <= startTop && x.Line >= lastFinishBottom && x.Value == 0))
                                cell1.Value = -1;

                            if (ColumnsClues[i].Length == 1 || currentClue == ColumnsClues[i].Length - 1) // só completa pra direita se não tem nenhum a direita
                                foreach (var cell1 in Grid.Where(x => x.Column == i && x.Line >= finishBottom && x.Value == 0))
                                    cell1.Value = -1;

                            // se o que esta entre o inicio e o fim é exato o que esta na cola entao marca todos como pintados
                            var cells = Grid.Where(x => x.Column == i && x.Line > startTop && x.Line < finishBottom && (x.Value == 0 || x.Value == 1));
                            if (cells.Count() == clue)
                                foreach (var cell2 in cells.Where(x => x.Value == 0))
                                    cell2.Value = 1;

                            lastFinishBottom = finishBottom;

                        }
                        currentClue++;
                    }
                }
                #endregion

                NextStep = 0;
            }
            //TODO: procura por espaços que nao é possivel inserir nenhum pintado e marca com X
            else if (NextStep == 3)
            {

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
