// SpreadsheetLogic.cs
//using AndroidX.Annotations;
//using Android.Credentials;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyExcelMauiLab1
{
    public enum DisplayMode
    {
        Value,
        Expression
    }

    public class SpreadsheetLogic
    {
        private List<List<Cell>> _cellData;
        private const int LETTERS_IN_ENG = 26;

        public int RowCount => _cellData.Count;
        public int ColumnCount => _cellData.Count > 0 ? _cellData[0].Count : 0;

        public event Action? OnDisplayModeChanged;
        public DisplayMode CurrentDisplayMode { get; private set; } = DisplayMode.Value;


        public SpreadsheetLogic(int initialRows, int initialCols)
        {
            _cellData = new List<List<Cell>>();
            InitializeData(initialRows, initialCols);
        }

        private void InitializeData(int rows, int cols)
        {
            for (int i = 0; i < rows; i++)
            {
                AddRow(cols);
            }
        }


        public void AddRow(int columnCount)
        {
            var newRowData = new List<Cell>();
            for (int i = 0; i < columnCount; i++)
            {
                newRowData.Add(new Cell(""));
            }
            _cellData.Add(newRowData);
        }

        public void AddColumn()
        {
            foreach (var rowData in _cellData)
            {
                rowData.Add(new Cell(""));
            }
        }

        public async Task<string> SaveFile()
        {
            var option = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            string projectPath = FileSystem.AppDataDirectory;
            string folderName = "Tables";
            string folderPath = Path.Combine(projectPath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = "";
            string fullFilePath = "";

            int i = 0;
            while (true)
            {
                string candidateName = $"Table{i}.json";
                string candidatePath = Path.Combine(folderPath, candidateName);

                if (!File.Exists(candidatePath))
                {
                    fileName = candidateName;
                    fullFilePath = candidatePath;
                    break;
                }
                i++;
            }

            using (FileStream fstream = new FileStream(fullFilePath, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fstream, _cellData, option);
            }

            return fileName;
        }

        public List<string> GetSavedFiles()
        {
            string projectPath = FileSystem.AppDataDirectory;
            string folderPath = Path.Combine(projectPath, "Tables");

            if (Directory.Exists(folderPath))
            {
                return Directory.GetFiles(folderPath, "*.json").Select(Path.GetFileName).ToList();
            }
            return new List<string>();
        }

        public async Task LoadFile(string fileName)
        {
            string projectPath = FileSystem.AppDataDirectory;
            string fullPath = Path.Combine(projectPath, "Tables", fileName);

            if (!File.Exists(fullPath)) return;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            using (FileStream openStream = File.OpenRead(fullPath))
            {
                var loadedData = await JsonSerializer.DeserializeAsync<List<List<Cell>>>(openStream, options);

                if (loadedData != null && loadedData.Count > 0)
                {
                    _cellData = loadedData;

                    RestoreState();
                }
            }
        }

        private void RestoreState()
        {
            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColumnCount; c++)
                {
                    var cell = _cellData[r][c];
                    ProcessCellUpdate(r, c, cell.Expression);
                }
            }
        }

        public void DeleteLastRow()
        {
            if (_cellData.Count > 1)
            {
                _cellData.RemoveAt(_cellData.Count - 1);
            }
        }

        public void DeleteLastColumn()
        {
            if (_cellData.Count > 0 && _cellData[0].Count > 1)
            {
                foreach (var rowData in _cellData)
                {
                    rowData.RemoveAt(rowData.Count - 1);
                }
            }
        }

        public int? GetCellValueByName(string name) // for interpreter
        {
            string letters = "";
            string rowStr = "";

            int i = 0;
            for (; i < name.Length && char.IsLetter(name[i]); i++)
                letters += name[i];

            for (; i < name.Length && char.IsDigit(name[i]); i++)
                rowStr += name[i];

            int column_0_based = 0;
            foreach (char c in letters.ToUpper())
            {
                column_0_based = column_0_based * 26 + (c - 'A' + 1);
            }
            column_0_based--;

            if (!int.TryParse(rowStr, out int row_1_based))
            {
                throw new ArgumentException($"Не вдалося розпізнати номер рядка: {rowStr}");
            }
            int row_0_based = row_1_based - 1;

            if (row_0_based >= 0 && row_0_based < this.RowCount &&
                column_0_based >= 0 && column_0_based < this.ColumnCount)
            {
                return _cellData[row_0_based][column_0_based].Value;
            }
            else
            {
                throw new IndexOutOfRangeException($"Посилання на комірку '{name}' виходить за межі таблиці.");
            }
        }

        public string GetColumnName(int colIndex)
        {
            int dividend = colIndex;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % LETTERS_IN_ENG;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                dividend = (dividend - modulo) / LETTERS_IN_ENG;
            }
            return columnName;
        }

        public void ProcessCellUpdate(int row, int col, string content)
        {
            var cell = _cellData[row][col];
            int.TryParse(content, out int val);
            if (CurrentDisplayMode == DisplayMode.Expression && cell.Expression == content
                || CurrentDisplayMode == DisplayMode.Value && cell.Value == val)
            {
                return;
            }
            cell.Expression = content;

            if (string.IsNullOrWhiteSpace(content) || int.TryParse(content, out _))
            {
                cell.SetReferences(new List<string>());
            }
            else
            {
                try
                {
                    Lexer lexer = new Lexer(content);
                    Parser parser = new Parser(lexer);
                    parser.Parse();
                    cell.SetReferences(parser.SeenCellRefs.Distinct().ToList());
                }
                catch
                {
                    cell.SetReferences(new List<string>());
                }
            }

            RefreshAll();
        }

        public void RefreshAll()
        {
            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColumnCount; c++)
                {
                    _cellData[r][c].ErrorMessage = "";
                }
            }

            for (int r = 0; r < RowCount; r++)
            {
                for (int c = 0; c < ColumnCount; c++)
                {
                    var cell = _cellData[r][c];
                    if (CheckIfCellInCycle(r, c, r, c, new HashSet<string>()))
                    {
                        cell.ErrorMessage = "Cycle Detected!";
                        cell.Value = null;
                    }
                }
            }

            int maxPasses = Math.Max(RowCount, ColumnCount) + 1;
            for (int pass = 0; pass < maxPasses; pass++)
            {
                for (int r = 0; r < RowCount; r++)
                {
                    for (int c = 0; c < ColumnCount; c++)
                    {
                        if (string.IsNullOrEmpty(_cellData[r][c].ErrorMessage))
                        {
                            CalculateCell(_cellData[r][c]);
                        }
                    }
                }
            }
        }

        private bool CheckIfCellInCycle(int currentRow, int currentCol, int targetRow, int targetCol, HashSet<string> visited)
        {
            string currentKey = $"{currentRow},{currentCol}";

            if (visited.Contains(currentKey)) return false;
            visited.Add(currentKey);

            var cell = _cellData[currentRow][currentCol];

            foreach (var refName in cell.ReferencedCellNames)
            {
                if (GetCoordinates(refName, out int nextR, out int nextC))
                {
                    if (nextR == targetRow && nextC == targetCol) return true;

                    if (CheckIfCellInCycle(nextR, nextC, targetRow, targetCol, visited))
                        return true;
                }
            }

            visited.Remove(currentKey);
            return false;
        }

        private void CalculateCell(Cell cell)
        {
            if (string.IsNullOrWhiteSpace(cell.Expression))
            {
                cell.Value = null;
                return;
            }

            if (int.TryParse(cell.Expression, out int val))
            {
                cell.Value = val;
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(cell.ErrorMessage))
                {
                    Lexer lexer = new Lexer(cell.Expression);
                    Parser parser = new Parser(lexer);
                    Interpreter interpreter = new Interpreter(parser, this);
                    cell.Value = interpreter.Interpret();
                }
            }
            catch
            {
                cell.ErrorMessage = "Error";
                cell.Value = null;
            }
        }

        public bool GetCoordinates(string cellName, out int row, out int col)
        {
            row = -1; col = -1;
            string letters = "";
            string numbers = "";

            foreach (char c in cellName)
            {
                if (char.IsLetter(c)) letters += c;
                else if (char.IsDigit(c)) numbers += c;
            }

            if (string.IsNullOrEmpty(letters) || string.IsNullOrEmpty(numbers)) return false;

            int column_0_based = 0;
            foreach (char c in letters.ToUpper())
            {
                column_0_based = column_0_based * 26 + (c - 'A' + 1);
            }
            column_0_based--;

            if (int.TryParse(numbers, out int row_1_based))
            {
                int row_0_based = row_1_based - 1;
                if (row_0_based >= 0 && row_0_based < RowCount &&
                    column_0_based >= 0 && column_0_based < ColumnCount)
                {
                    row = row_0_based;
                    col = column_0_based;
                    return true;
                }
            }
            return false;
        }

        public void ToggleDisplayMode()
        {
            CurrentDisplayMode = (CurrentDisplayMode == DisplayMode.Value)
                ? DisplayMode.Expression
                : DisplayMode.Value;

            OnDisplayModeChanged?.Invoke();
        }

        public string GetCellDisplayString(int row, int col)
        {
            if (row >= 0 && row < RowCount
                && col >= 0 && col < ColumnCount)
            {
                var cell = _cellData[row][col];
                if (cell.ErrorMessage != null && cell.ErrorMessage != "")
                {
                    return cell.ErrorMessage;
                }
                else if (CurrentDisplayMode == DisplayMode.Value)
                {
                    return cell.Value.HasValue
                        ? cell.Value.Value.ToString()
                        : string.Empty;
                }
                else
                {
                    return cell.Expression;
                }
            }
            return string.Empty;
        }

        public string GetCellExpression(int row, int col)
        {
            if (row >= 0 && row < RowCount
                && col >= 0 && col < ColumnCount)
            {
                return _cellData[row][col].Expression;
            }
            return string.Empty;
        }

        public string? GetCellValue(int row, int col)
        {
            if (row >= 0 && row < RowCount
                && col >= 0 && col < ColumnCount
                && _cellData[row][col].Value != null)
            {
                return _cellData[row][col].Value.ToString();
            }
            return string.Empty;
        }
    }
}
