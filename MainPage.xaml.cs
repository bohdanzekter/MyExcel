// MainPage.xaml.cs
using System;
//using MediaPlayer;
using Microsoft.Maui.Controls;

namespace MyExcelMauiLab1
{
    public partial class MainPage : ContentPage
    {
        private readonly SpreadsheetLogic _logic;

        private const int LETTERS_IN_ENG = 26;
        private const int STARTING_COLUMNS = 7;
        private const int STARTING_ROWS = 7;

        public MainPage()
        {
            InitializeComponent();
            _logic = new SpreadsheetLogic(STARTING_COLUMNS, STARTING_ROWS);
            _logic.OnDisplayModeChanged += RefreshAllCellsUI;
            CreateGrid();
        }

        private void CreateGrid()
        {
            AddColumnsAndColumnLabelsUI(STARTING_COLUMNS);
            AddRowsAndCellEntries(STARTING_ROWS);
        }

        private void AddColumnsAndColumnLabelsUI(int numOfColumns)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int col = 1; col <= numOfColumns; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                AddColumnLabelUI(col);
            }
        }

        private void AddRowsAndCellEntries(int numOfRows)
        {
            for (int row = 1; row <= numOfRows; row++)
            {
                AddRowAndCellUI(row);
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете зберегти файл?", "Так", "Ні");

            if (answer)
            {
                try
                {
                    string savedFileName = await _logic.SaveFile();

                    await DisplayAlert("Успіх", $"Файл успішно збережено як '{savedFileName}'!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
                }
            }
        }

        private async void LoadButton_Clicked(object sender, EventArgs e)
        {
            var files = _logic.GetSavedFiles();

            if (files.Count == 0)
            {
                await DisplayAlert("Інфо", "Збережених файлів не знайдено.", "OK");
                return;
            }

            string action = await DisplayActionSheet("Виберіть файл для завантаження:", "Скасувати", null, files.ToArray());

            if (action != "Скасувати" && !string.IsNullOrEmpty(action))
            {
                try
                {
                    await _logic.LoadFile(action);

                    RebuildGridAfterLoad();

                    await DisplayAlert("Успіх", "Таблиця завантажена!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Помилка", $"Не вдалося прочитати файл: {ex.Message}", "OK");
                }
            }
        }

        private void RebuildGridAfterLoad()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            int newRowCount = _logic.RowCount;
            int newColCount = _logic.ColumnCount;

            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int col = 1; col <= newColCount; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                AddColumnLabelUI(col);
            }

            for (int row = 1; row <= newRowCount; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                AddRowLabelUI(row);

                for (int col = 1; col <= newColCount; col++)
                {
                    var entry = new Entry { Text = "" };
                    entry.Unfocused += Entry_Unfocused;

                    Grid.SetRow(entry, row);
                    Grid.SetColumn(entry, col);
                    grid.Children.Add(entry);
                }
            }
            RefreshAllCellsUI();
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете вийти ? ", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "Лабороторна робота 1. Студента Зектера Богдана", "OK");
        }

        private void AddRowButton_Clicked(object sender, EventArgs e)
        {
            _logic.AddRow(grid.ColumnDefinitions.Count - 1);

            int newRowIndex = grid.RowDefinitions.Count;
            AddRowAndCellUI(newRowIndex);
        }

        private void AddRowAndCellUI(int row)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            AddRowLabelUI(row);

            for (int col = 1; col < grid.ColumnDefinitions.Count; col++)
            {
                var entry = new Entry { Text = "" };
                entry.Unfocused += Entry_Unfocused;
                Grid.SetRow(entry, row);
                Grid.SetColumn(entry, col);
                grid.Children.Add(entry);
            }
        }

        private void AddRowLabelUI(int row)
        {
            var label = new Label
            {
                Text = row.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);
        }

        private void DeleteRowButton_Clicked(object sender, EventArgs e)
        {
            int lastRowIndex = grid.RowDefinitions.Count - 1;

            if (lastRowIndex > 1)
            {
                _logic.DeleteLastRow();

                for (int i = grid.Children.Count - 1; i >= 0; i--)
                {
                    if (grid.Children[i] is View child && Grid.GetRow(child) == lastRowIndex)
                    {
                        grid.Children.Remove(child);
                    }
                }

                _logic.RefreshAll();
                grid.RowDefinitions.RemoveAt(lastRowIndex);
                RefreshAllCellsUI();
            }



        }

        private void AddColumnButton_Clicked(object sender, EventArgs e)
        {
            _logic.AddColumn();

            int newColumnIndex = grid.ColumnDefinitions.Count;

            grid.ColumnDefinitions.Add(new ColumnDefinition());

            AddColumnLabelUI(newColumnIndex);

            for (int row = 1; row < grid.RowDefinitions.Count; row++)
            {
                var entry = new Entry { Text = "" };
                entry.Unfocused += Entry_Unfocused;
                Grid.SetRow(entry, row);
                Grid.SetColumn(entry, newColumnIndex);
                grid.Children.Add(entry);
            }
        }

        private void AddColumnLabelUI(int col)
        {
            var label = new Label
            {
                Text = _logic.GetColumnName(col),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, col);
            grid.Children.Add(label);
        }

        private void DeleteColumnButton_Clicked(object sender, EventArgs e)
        {
            int lastColumnIndex = grid.ColumnDefinitions.Count - 1;

            if (lastColumnIndex > 1)
            {
                _logic.DeleteLastColumn();

                for (int i = grid.Children.Count - 1; i >= 0; i--)
                {
                    if (grid.Children[i] is View child && Grid.GetColumn(child) == lastColumnIndex)
                    {
                        grid.Children.Remove(child);
                    }
                }

                _logic.RefreshAll();
                grid.ColumnDefinitions.RemoveAt(lastColumnIndex);
                RefreshAllCellsUI();
            }
        }
        private void ToggleViewButton_Clicked(object sender, EventArgs e)
        {
            _logic.ToggleDisplayMode();
        }

        private void Entry_Unfocused(object? sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                var row = Grid.GetRow(entry) - 1;
                var col = Grid.GetColumn(entry) - 1;
                var content = entry.Text;

                _logic.ProcessCellUpdate(row, col, content);

                RefreshAllCellsUI();
            }
        }

        private void RefreshAllCellsUI()
        {
            foreach (var child in grid.Children)
            {
                if (child is Entry entry)
                {
                    int row = Grid.GetRow(entry) - 1;
                    int col = Grid.GetColumn(entry) - 1;

                    if (row >= 0 && col >= 0)
                    {
                        entry.Text = _logic.GetCellDisplayString(row, col);
                        
                        // Set tooltip only if there's an error
                        string errorMessage = _logic.GetCellExpressionMessage(row, col);
                        if (!string.IsNullOrWhiteSpace(errorMessage))
                        {
                            ToolTipProperties.SetText(entry, errorMessage);
                        }
                        else
                        {
                            ToolTipProperties.SetText(entry, "");
                        }
                    }
                }
            }
        }
    }
}
