using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace JapanesePuzzle
{
    public class Solver
    {

        private List<Cell> Grid { get; set; }
        private int[,] BruteGrid;
        private int[] BruteLineClueSum;
        private int[] BruteColumnClueSum;

        private int[][] LinesClues;
        private int[][] ColumnsClues;

        private int NextStep = 0;
        private int LineSize = 0;
        private int ColumnSize = 0;

        private bool HasChanges;
        private int NoChanges = 0;

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

        public bool Solve()
        {
            while (NoChanges <= 15)
            {
                DoStep();
            }
            return IsSolved();
        }

        private DateTime TimeStartDebug;
        private DateTime TimeStart;

        public void PrepareBruteForce()
        {
            BruteGrid = new int[ColumnSize, LineSize];
            foreach (var cell in Grid)
            {
                BruteGrid[cell.Line, cell.Column] = cell.Value;
            }

            BruteLineClueSum = new int[ColumnSize];
            for (int i = 0; i < ColumnSize; i++)
            {
                BruteLineClueSum[i] = LinesClues[i].Sum(x => x);
            }

            BruteColumnClueSum = new int[LineSize];
            for (int i = 0; i < LineSize; i++)
            {
                BruteColumnClueSum[i] = ColumnsClues[i].Sum(x => x);
            }
            TimeStart = DateTime.Now;
            TimeStartDebug = DateTime.Now;
        }

        public bool SolveBruteForce(int i, int j)
        {
            if (j >= ColumnsClues.Length)
            {
                // faz a validação da linha se a mesma esta com q soma das pistas erradas entao ja ignora toda a arvore abaixo.
                //var sumClues = LinesClues[i].Sum(x => x);
                //var sumLine = Grid.Where(x => x.Line == i && x.Value == 1).Sum(x => x.Value);
                //if (sumLine != sumClues)
                //    return false;

                //var sumLine = Grid.Where(x => x.Line == i && x.Value == 1).Sum(x => x.Value);
                var sumLine = 0;
                for (int k = 0; k < ColumnsClues.Length; k++)
                {
                    if (BruteGrid[i, k] > 0)
                        sumLine += BruteGrid[i, k];
                }
                if (sumLine != BruteLineClueSum[i])
                    return false;

                i++;
                j = 0;
            }

            if (i >= LinesClues.Length)
            {
                /*if ((DateTime.Now - TimeStartDebug).TotalSeconds > 1)
                {
                    TimeStartDebug = DateTime.Now;
                    Console.Clear();
                    DebugBruteForce();
                    //Console.Write(DateTime.Now.ToString());
                    //Console.ReadLine();    
                }*/

                var ret = IsSolvedBruteForce();
                return ret;
            }

            //nao percorre o que esta preenchido
            //if (GetValue(i, j) == 0)
            if (BruteGrid[i, j] == 0)
            {

                if (i > BruteColumnClueSum[j])


                //var cell = Grid.First(x => x.Line == i && x.Column == j);
                BruteGrid[i, j] = -1;
                //cell.Value = -1;
                var solved = SolveBruteForce(i, j + 1);
                if (!solved)
                {
                    //cell.Value = 1;
                    BruteGrid[i, j] = 1;
                    solved = SolveBruteForce(i, j + 1);
                }

                if (!solved)
                {
                    //cell.Value = 0;
                    BruteGrid[i, j] = 0;
                }

                return solved;
            }

            return SolveBruteForce(i, j + 1);
        }

        public void DoStep()
        {
            HasChanges = false;

            if (NextStep == 0)
            {
                #region Verifica se tem alguma linha ou coluna com tamanho cheio ou proximo
                for (int i = 0; i < LinesClues.Length; i++)
                {
                    var checks = LinesClues[i];
                    if (checks.Length == 1)
                    {
                        // nao passa pelas linhas ja preenchidas
                        if (!Grid.Any(x => x.Line == i && x.Value == 0)) continue;

                        var minStart = Grid.Where(x => x.Line == i && (x.Value == 0 || x.Value == 1)).Min(x => x.Column);
                        var maxEnd = Grid.Where(x => x.Line == i && (x.Value == 0 || x.Value == 1)).Max(x => x.Column);

                        if (Grid.Any(x => x.Line == i && x.Value == -1 && x.Column >= minStart && x.Column <= maxEnd)) continue;

                        var length = (maxEnd - minStart) + 1;
                        var middle = length / 2;
                        if (checks[0] <= middle || middle <= 1) continue;

                        var start = minStart + (length - checks[0]);
                        var end = maxEnd - (length - checks[0]) + 1;
                        for (int j = start; j < end; j++)
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
                        // nao passa pelas colunas ja preenchidas
                        if (!Grid.Any(x => x.Column == j && x.Value == 0)) continue;

                        var minStart = Grid.Where(x => x.Column == j && (x.Value == 0 || x.Value == 1)).Min(x => x.Line);
                        var maxEnd = Grid.Where(x => x.Column == j && (x.Value == 0 || x.Value == 1)).Max(x => x.Line);

                        if (Grid.Any(x => x.Column == j && x.Value == -1 && x.Line >= minStart && x.Line <= maxEnd)) continue;

                        var length = (maxEnd - minStart) + 1;
                        var middle = length / 2;
                        if (checks[0] <= middle || middle <= 1) continue;

                        var start = minStart + (length - checks[0]);
                        var end = maxEnd - (length - checks[0]) + 1;
                        for (int i = start; i < end; i++)
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
                    var sumLine = Grid.Count(x => x.Line == i && x.Value == 1);
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
                    var sumColumn = Grid.Count(x => x.Column == j && x.Value == 1);
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
                    // nao passa pelas linhas ja preenchidas
                    if (!Grid.Any(x => x.Line == i && x.Value == 0)) continue;

                    var paintedCells = Grid.Where(x => x.Line == i && x.Value == 1).ToArray();
                    var groups = 0;
                    var dif = 0;
                    for (int j = 0; j < paintedCells.Count(); j++)
                    {
                        if (j == 0)
                        {
                            groups++;
                            continue;
                        }
                        dif = paintedCells[j].Column - paintedCells[j - 1].Column;
                        if (dif > 1 && !LinesClues[i].Any(x => x >= dif + 1))
                            groups++;
                    }
                    if (LinesClues[i].Length != groups) continue;

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
                                SetValue(cell1.Line, cell1.Column, 1);

                            removedCells.AddRange(Grid.Where(x => x.Line == i && x.Column < clue && x.Value == 1));
                        }
                        // se esta no final faz o mesmo teste para as colunas da esquerda
                        else if (cell.Column == LineSize - 1)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Line == i && x.Column < LineSize - clue && x.Column >= lastFinishRight && x.Value == 0);
                            foreach (var cell1 in cells)
                                SetValue(cell1.Line, cell1.Column, -1);

                            cells = Grid.Where(x => x.Line == i && x.Column >= LineSize - clue && x.Value == 0);
                            foreach (var cell1 in cells)
                                SetValue(cell1.Line, cell1.Column, 1);

                            removedCells.AddRange(Grid.Where(x => x.Line == i && x.Column >= LineSize - clue && x.Value == 1));
                        }
                        // se nao esta no inicio nem no fim, obrigatoriamente esta no meio
                        else
                        {
                            var startLeft = cell.Column - clue;
                            var finishRight = cell.Column + clue;

                            // remove as celulas ja pintadas de dentro da faixa que pode chegar
                            removedCells.AddRange(Grid.Where(x => x.Line == i && x.Column > startLeft && x.Column < finishRight && x.Value == 1));

                            foreach (var cell1 in Grid.Where(x => x.Line == i && x.Column <= startLeft && x.Column >= lastFinishRight && x.Value == 0))
                                SetValue(cell1.Line, cell1.Column, -1);

                            if (LinesClues[i].Length == 1 || currentClue == LinesClues[i].Length - 1) // só completa pra direita se não tem nenhum a direita
                                foreach (var cell1 in Grid.Where(x => x.Line == i && x.Column > finishRight && x.Value == 0))
                                    SetValue(cell1.Line, cell1.Column, -1);

                            var cells = Grid.Where(x => x.Line == i && x.Column > startLeft && x.Column < finishRight && (x.Value == 0 || x.Value == 1));
                            if (cells.Count() == clue)
                                foreach (var cell1 in cells.Where(x => x.Value == 0))
                                    SetValue(cell1.Line, cell1.Column, 1);

                            lastFinishRight = finishRight;

                        }
                        currentClue++;
                    }

                }

                for (int i = 0; i < ColumnsClues.Length; i++)
                {

                    // nao passa pelas colunas ja preenchidas
                    if (!Grid.Any(x => x.Column == i && x.Value == 0)) continue;

                    var paintedCells = Grid.Where(x => x.Column == i && x.Value == 1).ToArray();
                    var groups = 0;
                    var dif = 0;
                    for (int j = 0; j < paintedCells.Count(); j++)
                    {
                        if (j == 0)
                        {
                            groups++;
                            continue;
                        }
                        dif = paintedCells[j].Line - paintedCells[j - 1].Line;
                        if (dif > 1 && !ColumnsClues[i].Any(x => x >= dif + 1))
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
                            var cells = Grid.Where(x => x.Column == i && x.Line < clue && x.Value == 0).ToList();
                            foreach (var cell1 in cells)
                                SetValue(cell1.Line, cell1.Column, 1);

                            removedCells.AddRange(Grid.Where(x => x.Column == i && x.Line < clue && x.Value == 1));

                        }
                        // se esta no final faz o mesmo teste para as colunas da cima
                        else if (cell.Line == ColumnSize - 1)
                        {
                            // se achou algum pintado entao nao eh ali que deve estar
                            var cells = Grid.Where(x => x.Column == i && x.Line < ColumnSize - clue && x.Line >= lastFinishBottom && x.Value == 0);
                            foreach (var cell1 in cells)
                                SetValue(cell1.Line, cell1.Column, -1);

                            cells = Grid.Where(x => x.Column == i && x.Line >= ColumnSize - clue && x.Value == 0).ToList();
                            foreach (var cell1 in cells)
                                SetValue(cell1.Line, cell1.Column, 1);

                            removedCells.AddRange(Grid.Where(x => x.Column == i && x.Line >= ColumnSize - clue && x.Value == 1));
                        }
                        // se nao esta no inicio nem no fim, obrigatoriamente esta no meio
                        else
                        {
                            var startTop = cell.Line - clue;
                            var finishBottom = cell.Line + clue;

                            // remove as celulas ja pintadas de dentro da faixa que pode chegar
                            removedCells.AddRange(Grid.Where(x => x.Column == i && x.Line > startTop && x.Line < finishBottom && x.Value == 1));

                            foreach (var cell1 in Grid.Where(x => x.Column == i && x.Line <= startTop && x.Line >= lastFinishBottom && x.Value == 0))
                                SetValue(cell1.Line, cell1.Column, -1);

                            if (ColumnsClues[i].Length == 1 || currentClue == ColumnsClues[i].Length - 1) // só completa pra direita se não tem nenhum a direita
                                foreach (var cell1 in Grid.Where(x => x.Column == i && x.Line > finishBottom && x.Value == 0))
                                    SetValue(cell1.Line, cell1.Column, -1);

                            // se o que esta entre o inicio e o fim é exato o que esta na cola entao marca todos como pintados
                            var cells = Grid.Where(x => x.Column == i && x.Line > startTop && x.Line < finishBottom && (x.Value == 0 || x.Value == 1));
                            if (cells.Count() == clue)
                                foreach (var cell1 in cells.Where(x => x.Value == 0))
                                    SetValue(cell1.Line, cell1.Column, 1);

                            lastFinishBottom = finishBottom;

                        }
                        currentClue++;
                    }
                }
                #endregion
                NextStep = 3;
            }

            else if (NextStep == 3)
            {
                #region Verifica os preenchidos nas bordas e preenche se possivel

                // borda esquerda
                var paintedCells = Grid.Where(x => x.Column == 0 && x.Value == 1);
                foreach (var cell in paintedCells)
                {

                    if (!Grid.Any(x => x.Line == cell.Line && x.Value == 0)) continue;

                    var clue = LinesClues[cell.Line];
                    var valueClue = clue[0];

                    var found = false;
                    foreach (var cell1 in Grid.Where(x => x.Line == cell.Line && x.Column < valueClue && (x.Value == 0 || x.Value == 1)))
                    {
                        found = true;
                        SetValue(cell1.Line, cell1.Column, 1);
                    }

                    // a proxima é obrigatoriamente um intervalo
                    if (found)
                        SetValue(cell.Line, cell.Column + valueClue, -1);
                }

                // borda superior
                paintedCells = Grid.Where(x => x.Line == 0 && x.Value == 1);
                foreach (var cell in paintedCells)
                {
                    if (!Grid.Any(x => x.Column == cell.Column && x.Value == 0)) continue;

                    var clue = ColumnsClues[cell.Column];
                    var valueClue = clue[0];

                    var found = false;
                    foreach (var cell1 in Grid.Where(x => x.Column == cell.Column && x.Line < valueClue && (x.Value == 0 || x.Value == 1)))
                    {
                        found = true;
                        SetValue(cell1.Line, cell1.Column, 1);
                    }

                    // a proxima é obrigatoriamente um intervalo
                    if (found)
                        SetValue(cell.Line + valueClue, cell.Column, -1);
                }

                // borda direita
                paintedCells = Grid.Where(x => x.Column == LineSize - 1 && x.Value == 1);
                foreach (var cell in paintedCells)
                {
                    if (!Grid.Any(x => x.Line == cell.Line && x.Value == 0)) continue;

                    var clue = LinesClues[cell.Line];
                    var valueClue = clue[clue.Length - 1];

                    var found = false;
                    foreach (var cell1 in Grid.Where(x => x.Line == cell.Line && x.Column >= LineSize - valueClue && (x.Value == 0 || x.Value == 1)))
                    {
                        found = true;
                        SetValue(cell1.Line, cell1.Column, 1);
                    }

                    // a proxima é obrigatoriamente um intervalo
                    if (found)
                        SetValue(cell.Line, cell.Column - valueClue, -1);
                }

                // borda superior
                paintedCells = Grid.Where(x => x.Line == ColumnSize - 1 && x.Value == 1);
                foreach (var cell in paintedCells)
                {

                    if (!Grid.Any(x => x.Column == cell.Column && x.Value == 0)) continue;

                    var clue = ColumnsClues[cell.Column];
                    var valueClue = clue[clue.Length - 1];

                    var found = false;
                    foreach (var cell1 in Grid.Where(x => x.Column == cell.Column && x.Line >= ColumnSize - valueClue && (x.Value == 0 || x.Value == 1)))
                    {
                        found = true;
                        SetValue(cell1.Line, cell1.Column, 1);
                    }

                    // a proxima é obrigatoriamente um intervalo
                    if (found)
                        SetValue(cell.Line - valueClue, cell.Column, -1);
                }

                #endregion
                NextStep = 4;
            }
            else if (NextStep == 4)
            {
                #region Procura por espaços que nao é possivel inserir nenhum pintado e marca com X
                for (int i = 0; i < LinesClues.Length; i++)
                {
                    // nao passa pelas linhas ja preenchidas
                    if (!Grid.Any(x => x.Line == i && x.Value == 0)) continue;

                    var maxClue = LinesClues[i].Max(x => x);
                    var minClue = LinesClues[i].Min(x => x);
                    if (maxClue == 1) continue;

                    // busca os espaços em branco
                    var pointer = 0;
                    var cluePointer = 0;
                    var start = 0;
                    var hasOne = false;
                    var last = false;

                    while (!last)
                    {
                        var value = 0;
                        if (pointer < ColumnsClues.Length)
                        {
                            value = GetValue(i, pointer);
                        }
                        else
                        {
                            last = true;
                        }

                        if ((value == 0 || value == 1) && !last)
                        {
                            if (value == 1)
                                hasOne = true;
                            pointer++;
                        }
                        else
                        {
                            if (!hasOne)
                            {
                                // nenhuma cola cabe no espaco
                                if (pointer - start > 0 && pointer - start < maxClue && pointer - start < minClue)
                                {
                                    for (int k = start; k < pointer; k++)
                                        SetValue(i, k, -1);
                                }

                                if (cluePointer < LinesClues[i].Length)
                                {
                                    // se aquele espaço pode preencher uma cola na sequencia e só cabe nesse ponto 
                                    if (pointer - start >= LinesClues[i][cluePointer] &&
                                        // se a soma das posicoes não permite que essa cola e as demais caibam em outra posicao                                       
                                        Grid.Count(x => x.Line == i && x.Column > pointer && (x.Value == 0 || x.Value == 1)) <
                                        LinesClues[i].Where(x => x >= cluePointer).Sum(x => x) + LinesClues[i].Count(x => x >= cluePointer) - 1 &&
                                        // se a soma das posicoes anteriores não permite que essa cola e as demais caibam em outra posicao
                                        Grid.Count(x => x.Line == i && x.Column < start && (x.Value == 0 || x.Value == 1)) <
                                        LinesClues[i].Where(x => x <= cluePointer).Sum(x => x) + LinesClues[i].Count(x => x >= cluePointer) - 1)
                                    {

                                        var startPaint = 0;
                                        var endPaint = 0;
                                        var length = (pointer - start);
                                        if (length == LinesClues[i][cluePointer])
                                        {
                                            startPaint = start;
                                            endPaint = startPaint + length;
                                        }
                                        else
                                        {
                                            var middle = length / 2;
                                            if (LinesClues[i][cluePointer] <= middle || middle <= 1)
                                            {
                                                startPaint = -1;
                                            }
                                            else
                                            {
                                                startPaint = length + start - LinesClues[i][cluePointer];
                                                endPaint = length - startPaint;
                                            }

                                        }

                                        if (startPaint >= 0)
                                            for (int k = startPaint; k < endPaint; k++)
                                                SetValue(i, k, 1);

                                        cluePointer++;
                                    }
                                }
                            }
                            hasOne = false;
                            pointer++;
                            start = pointer;
                        }
                    }
                }

                for (int j = 0; j < ColumnsClues.Length; j++)
                {
                    // nao passa pelas linhas ja preenchidas
                    if (!Grid.Any(x => x.Column == j && x.Value == 0)) continue;

                    var maxClue = ColumnsClues[j].Max(x => x);
                    var minClue = ColumnsClues[j].Min(x => x);
                    if (maxClue == 1) continue;

                    // busca os espaços em branco
                    var pointer = 0;
                    var cluePointer = 0;
                    var start = 0;
                    var hasOne = false;
                    var last = false;

                    while (!last)
                    {
                        var value = 0;
                        if (pointer < LinesClues.Length)
                        {
                            value = GetValue(pointer, j);
                        }
                        else
                        {
                            last = true;
                        }

                        if ((value == 0 || value == 1) && !last)
                        {
                            if (value == 1)
                                hasOne = true;
                            pointer++;
                        }
                        else
                        {
                            if (!hasOne)
                            {
                                if (pointer - start > 0 && pointer - start < maxClue && pointer - start < minClue)
                                {
                                    for (int k = start; k < pointer; k++)
                                        SetValue(k, j, -1);
                                }

                                if (cluePointer < ColumnsClues[j].Length)
                                {
                                    // se aquele espaço pode preencher uma cola na sequencia e só cabe nesse ponto 
                                    if (pointer - start >= ColumnsClues[j][cluePointer] &&
                                        // se a soma das posicoes posteriores não permite que essa cola e as demais caibam em outra posicao
                                        Grid.Count(x => x.Column == j && x.Line > pointer && (x.Value == 0 || x.Value == 1)) <
                                        ColumnsClues[j].Where(x => x >= cluePointer).Sum(x => x) + ColumnsClues[j].Count(x => x >= cluePointer) - 1 &&
                                        // se a soma das posicoes anteriores não permite que essa cola e as demais caibam em outra posicao
                                        Grid.Count(x => x.Column == j && x.Line < start && (x.Value == 0 || x.Value == 1)) <
                                        ColumnsClues[j].Where(x => x <= cluePointer).Sum(x => x) + ColumnsClues[j].Count(x => x >= cluePointer) - 1)
                                    {

                                        var startPaint = 0;
                                        var endPaint = 0;
                                        var length = (pointer - start);
                                        if (length == ColumnsClues[j][cluePointer])
                                        {
                                            startPaint = start;
                                            endPaint = startPaint + length;
                                        }
                                        else
                                        {
                                            var middle = length / 2;
                                            if (ColumnsClues[j][cluePointer] <= middle || middle <= 1)
                                            {
                                                startPaint = -1;
                                            }
                                            else
                                            {
                                                startPaint = length + start - ColumnsClues[j][cluePointer];
                                                endPaint = length - startPaint;
                                            }
                                        }

                                        if (startPaint >= 0)
                                            for (int k = startPaint; k < endPaint; k++)
                                                SetValue(k, j, 1);

                                        cluePointer++;
                                    }
                                }

                            }
                            hasOne = false;
                            pointer++;
                            start = pointer;
                        }
                    }
                }
                #endregion
                NextStep = 0;
            }

            if (HasChanges)
                NoChanges = 0;
            else
                NoChanges++;

        }

        public bool IsSolved()
        {
            for (int i = 0; i < LinesClues.Length; i++)
            {
                if (Grid.Any(x => x.Line == i && x.Value == 0))
                    return false;

                var cluePointer = 0;
                var pointer = -1;
                while (cluePointer < LinesClues[i].Length)
                {

                    var clueValue = LinesClues[i][cluePointer];

                    var next = Grid.FirstOrDefault(x => x.Line == i && x.Column > pointer && x.Value == 1);

                    var aux = 0;
                    if (next == null && clueValue != 0) return false;
                    else if (next != null) aux = next.Column;
                    else aux = 0;

                    var nextWhiteSpace = Grid.FirstOrDefault(x => x.Line == i && x.Column > aux && x.Value == -1);
                    var aux2 = 0;
                    if (nextWhiteSpace != null) aux2 = nextWhiteSpace.Column;
                    else aux2 = LineSize;

                    var count = Grid.Count(x => x.Line == i && x.Column >= aux && x.Column < aux2 && x.Value == 1);

                    pointer = aux2;

                    if (count != clueValue) return false;

                    cluePointer++;
                }

            }

            for (int j = 0; j < ColumnsClues.Length; j++)
            {
                if (Grid.Any(x => x.Column == j && x.Value == 0))
                    return false;

                var cluePointer = 0;
                var pointer = -1;
                while (cluePointer < ColumnsClues[j].Length)
                {

                    var clueValue = ColumnsClues[j][cluePointer];
                    var next = Grid.FirstOrDefault(x => x.Column == j && x.Line > pointer && x.Value == 1);

                    var aux = 0;
                    if (next == null && clueValue != 0) return false;
                    else if (next != null) aux = next.Line;
                    else aux = 0;

                    var nextWhiteSpace = Grid.FirstOrDefault(x => x.Column == j && x.Line > aux && x.Value == -1);
                    var aux2 = 0;
                    if (nextWhiteSpace != null) aux2 = nextWhiteSpace.Line;
                    else aux2 = ColumnSize;

                    var count = Grid.Count(x => x.Column == j && x.Line >= aux && x.Line < aux2 && x.Value == 1);
                    pointer = aux2;

                    if (count != clueValue) return false;

                    cluePointer++;
                }
            }

            return true;
        }

        public bool IsSolvedBruteForce()
        {
            for (int i = 0; i < LinesClues.Length; i++)
            {
                for (int j = 0; j < ColumnsClues.Length; j++)
                    if (BruteGrid[i, j] == 0) return false;

                var cluePointer = 0;
                var pointer = -1;
                while (cluePointer < LinesClues[i].Length)
                {

                    var clueValue = LinesClues[i][cluePointer];
                    var next = -1;

                    for (int j = pointer + 1; j < ColumnsClues.Length; j++)
                    {
                        if (BruteGrid[i, j] == 1)
                        {
                            next = j;
                            break;
                        }
                    }
                    if (next == -1 && clueValue != 0) return false;

                    var nextWhiteSpace = -1;
                    for (int j = next + 1; j < ColumnsClues.Length; j++)
                    {
                        if (BruteGrid[i, j] == -1)
                        {
                            nextWhiteSpace = j;
                            break;
                        }
                    }

                    if (nextWhiteSpace == -1) nextWhiteSpace = LineSize;

                    var count = 0;
                    for (int j = next; j < nextWhiteSpace ; j++)
                    {
                        if (BruteGrid[i, j] == 1)
                            count++;
                    }

                    pointer = nextWhiteSpace;

                    if (count != clueValue) return false;

                    cluePointer++;
                }

            }

            for (int j = 0; j < ColumnsClues.Length; j++)
            {
                for (int i = 0; i < LinesClues.Length; i++)
                    if (BruteGrid[i, j] == 0) return false;

                var cluePointer = 0;
                var pointer = -1;
                while (cluePointer < ColumnsClues[j].Length)
                {

                    var clueValue = ColumnsClues[j][cluePointer];
                    var next = -1;

                    for (int i = pointer + 1; i < LinesClues.Length; i++)
                    {
                        if (BruteGrid[i, j] == 1)
                        {
                            next = i;
                            break;
                        }
                    }
                    if (next == -1 && clueValue != 0) return false;

                    var nextWhiteSpace = -1;
                    for (int i = next + 1; i < LinesClues.Length; i++)
                    {
                        if (BruteGrid[i, j] == -1)
                        {
                            nextWhiteSpace = i;
                            break;
                        }
                    }

                    if (nextWhiteSpace == -1) nextWhiteSpace = ColumnSize;

                    var count = 0;
                    for (int i = next; i < nextWhiteSpace; i++)
                    {
                        if (BruteGrid[i, j] == 1)
                            count++;
                    }

                    pointer = nextWhiteSpace;

                    if (count != clueValue) return false;

                    cluePointer++;
                }
            }

            return true;
        }

        private int GetValue(int line, int column)
        {
            return Grid.First(x => x.Line == line && x.Column == column).Value;
        }

        private void SetValue(int line, int column, int value)
        {
            var cell = Grid.First(x => x.Line == line && x.Column == column);
            if (cell.Value == 0)
                HasChanges = true;
            cell.Value = value;
        }

        public string Debug()
        {
            var e = Encoding.GetEncoding("iso-8859-1");
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
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("  #");
                        Console.ResetColor();
                    }
                    else if (GetValue(i, j) == -1)
                        Console.Write("  .");
                }
                Console.Write("\n");
            }
            return ret;
        }

        public string DebugBruteForce()
        {
            var e = Encoding.GetEncoding("iso-8859-1");
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
                    if (BruteGrid[i, j] == 0)
                        Console.Write("   ");
                    else if (BruteGrid[i, j] == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("  #");
                        Console.ResetColor();
                    }
                    else if (BruteGrid[i, j] == -1)
                        Console.Write("  .");
                }
                Console.Write("\n");
            }
            Console.WriteLine((DateTime.Now - TimeStart).ToString());

            return ret;
        }
    }
}
