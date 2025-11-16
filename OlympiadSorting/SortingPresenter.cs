using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace NumericalMethodsApp.OlympiadSorting
{
  public class SortingPresenter
  {
    private readonly ISortingView view;
    private readonly SortingModel model;
    private readonly SortingAlgorithms algorithms;

    public SortingPresenter(ISortingView sortingView)
    {
      view = sortingView;
      model = new SortingModel();
      algorithms = new SortingAlgorithms();

      InitializeView();
      SubscribeToEvents();
    }

    private void InitializeView()
    {
      view.RowsText = "5";
      view.MinValueText = "0";
      view.MaxValueText = "100";
      view.MaxIterationsText = "1000";

      view.AddNumericValidation(GetTextBoxByName("RowsTextBox"));
      view.AddNumericValidation(GetTextBoxByName("MinValueTextBox"));
      view.AddNumericValidation(GetTextBoxByName("MaxValueTextBox"));
      view.AddNumericValidation(GetTextBoxByName("MaxIterationsTextBox"));

      RefreshDataGrids();
      UpdateButtonsState();
    }

    private void SubscribeToEvents()
    {
      view.HelpClicked += OnHelpClicked;
      view.ApplySizeClicked += OnApplySizeClicked;
      view.RandomGenerateClicked += OnRandomGenerateClicked;
      view.ImportCsvClicked += OnImportCsvClicked;
      view.ClearAllClicked += OnClearAllClicked;
      view.StartSortingClicked += OnStartSortingClicked;
      view.CheckBoxChecked += OnCheckBoxChecked;
      view.OriginalDataGridCellEditEnding += OnOriginalDataGridCellEditEnding;
    }

    private TextBox GetTextBoxByName(string name)
    {
      return (view as OlympiadSortingView)?.FindName(name) as TextBox;
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(
        "Справка по использованию приложения:\n\n" +
        "1. Укажите размерность массива и нажмите 'Применить размер'\n" +
        "2. Введите данные в таблицу или сгенерируйте случайные\n" +
        "3. Выберите алгоритмы сортировки\n" +
        "4. Настройте параметры сортировки\n" +
        "5. Нажмите 'Начать' для запуска сортировки\n\n" +
        "Результаты будут отображены в таблице и на графике.",
        "Справка",
        MessageBoxButton.OK,
        MessageBoxImage.Information);
    }

    private void OnApplySizeClicked(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(view.RowsText, out int size) && size > 0)
      {
        List<int> newArray = new List<int>(new int[size]);

        for (int index = 0; index < Math.Min(model.OriginalArray.Count, size); ++index)
        {
          newArray[index] = model.OriginalArray[index];
        }

        model.OriginalArray = newArray;
        RefreshDataGrids();
        UpdateButtonsState();
      }
      else
      {
        MessageBox.Show("Пожалуйста, введите корректное положительное число для размера массива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        view.RowsText = "5";
      }
    }

    private void OnRandomGenerateClicked(object sender, RoutedEventArgs e)
    {
      if (!int.TryParse(view.RowsText, out int size) || size <= 0)
      {
        MessageBox.Show("Сначала укажите корректный размер массива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!int.TryParse(view.MinValueText, out int min) || !int.TryParse(view.MaxValueText, out int max))
      {
        MessageBox.Show("Укажите корректный диапазон значений", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (min >= max)
      {
        MessageBox.Show("Минимальное значение должно быть меньше максимального", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      model.OriginalArray.Clear();
      for (int index = 0; index < size; ++index)
      {
        model.OriginalArray.Add(model.RandomGenerator.Next(min, max + 1));
      }

      RefreshDataGrids();
      UpdateButtonsState();
    }

    private void OnImportCsvClicked(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
      openFileDialog.Title = "Импорт данных из CSV";

      if (openFileDialog.ShowDialog() == true)
      {
        try
        {
          string[] lines = File.ReadAllLines(openFileDialog.FileName);
          model.OriginalArray.Clear();

          foreach (string line in lines)
          {
            string[] values = line.Split(',');
            foreach (string value in values)
            {
              if (int.TryParse(value.Trim(), out int number))
              {
                model.OriginalArray.Add(number);
              }
            }
          }

          view.RowsText = model.OriginalArray.Count.ToString();
          RefreshDataGrids();
          UpdateButtonsState();
          MessageBox.Show($"Успешно импортировано {model.OriginalArray.Count} элементов", "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка при импорте CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void OnClearAllClicked(object sender, RoutedEventArgs e)
    {
      model.OriginalArray.Clear();
      model.SortResults.Clear();
      RefreshDataGrids();
      view.SetResultsDataGridItemsSource(null);
      UpdateButtonsState();
    }

    private void OnStartSortingClicked(object sender, RoutedEventArgs e)
    {
      if (model.OriginalArray.Count == 0)
      {
        MessageBox.Show("Нет данных для сортировки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!int.TryParse(view.MaxIterationsText, out int maxIterations) || maxIterations <= 0)
      {
        MessageBox.Show("Укажите корректное ограничение итераций", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      bool ascending = view.IsAscendingChecked;
      model.SortResults.Clear();

      if (view.IsBubbleSortChecked)
      {
        var result = algorithms.BubbleSort(model.OriginalArray.ToArray(), ascending, maxIterations);
        model.SortResults.Add(result);
        CheckIterationLimit(result);
      }

      if (view.IsInsertionSortChecked)
      {
        var result = algorithms.InsertionSort(model.OriginalArray.ToArray(), ascending, maxIterations);
        model.SortResults.Add(result);
        CheckIterationLimit(result);
      }

      if (view.IsShakerSortChecked)
      {
        var result = algorithms.ShakerSort(model.OriginalArray.ToArray(), ascending, maxIterations);
        model.SortResults.Add(result);
        CheckIterationLimit(result);
      }

      if (view.IsQuickSortChecked)
      {
        var result = algorithms.QuickSort(model.OriginalArray.ToArray(), ascending, maxIterations);
        model.SortResults.Add(result);
        CheckIterationLimit(result);
      }

      if (view.IsBogosortChecked)
      {
        var result = algorithms.BogoSort(model.OriginalArray.ToArray(), ascending, maxIterations);
        model.SortResults.Add(result);
        CheckIterationLimit(result);
      }

      DisplayResults();
      UpdateSortedArray();
    }

    private void CheckIterationLimit(SortResult result)
    {
      if (result.IterationLimitExceeded)
      {
        MessageBox.Show($"Превышен лимит итераций. {result.AlgorithmName} сортировка прервана.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    private void OnCheckBoxChecked(object sender, RoutedEventArgs e)
    {
      UpdateButtonsState();
    }

    private void OnOriginalDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      if (e.EditAction == DataGridEditAction.Commit)
      {
        var textBox = e.EditingElement as TextBox;
        if (textBox != null)
        {
          if (!int.TryParse(textBox.Text, out int value))
          {
            MessageBox.Show("Введите целое число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Cancel = true;
          }
          else
          {
            int rowIndex = e.Row.GetIndex();
            if (rowIndex < model.OriginalArray.Count)
            {
              model.OriginalArray[rowIndex] = value;
            }
          }
        }
      }
    }

    private void UpdateButtonsState()
    {
      bool hasData = model.OriginalArray.Count > 0;
      bool hasAlgorithms = view.IsBubbleSortChecked ||
                          view.IsInsertionSortChecked ||
                          view.IsShakerSortChecked ||
                          view.IsQuickSortChecked ||
                          view.IsBogosortChecked;

      view.StartSortingEnabled = hasData && hasAlgorithms;
    }

    private void RefreshDataGrids()
    {
      var dataSource = new List<DataItem>();
      for (int index = 0; index < model.OriginalArray.Count; ++index)
      {
        dataSource.Add(new DataItem { Index = index, Value = model.OriginalArray[index] });
      }

      view.SetOriginalDataGridItemsSource(dataSource);
      view.SetSortedDataGridItemsSource(null);
    }

    private void UpdateSortedArray()
    {
      var bestResult = model.SortResults.OrderBy(result => result.TimeMs).FirstOrDefault();
      if (bestResult != null)
      {
        var sortedDataSource = new List<DataItem>();
        for (int index = 0; index < bestResult.SortedArray.Length; ++index)
        {
          sortedDataSource.Add(new DataItem { Index = index, Value = bestResult.SortedArray[index] });
        }
        view.SetSortedDataGridItemsSource(sortedDataSource);
      }
    }

    private void DisplayResults()
    {
      view.SetResultsDataGridItemsSource(null);
      view.SetResultsDataGridItemsSource(model.SortResults);
    }
  }
}
