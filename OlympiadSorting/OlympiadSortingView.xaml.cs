using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Microsoft.Win32;
using System.Net;

namespace NumericalMethodsApp.OlympiadSorting
{
  public partial class OlympiadSortingView : Window
  {
    private List<int> originalArray = new List<int>();
    private List<SortResult> sortResults = new List<SortResult>();
    private Random random = new Random();

    public OlympiadSortingView()
    {
      InitializeComponent();
      InitializeDataGrids();
      UpdateButtonsState();
      InitializeInputValidation();
    }

    private void InitializeDataGrids()
    {
      OriginalDataGrid.IsReadOnly = false;
      OriginalDataGrid.CanUserAddRows = false;
      OriginalDataGrid.CanUserDeleteRows = false;

      RefreshDataGrids();
    }

    private void InitializeInputValidation()
    {
      RowsTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
      MinValueTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
      MaxValueTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
      MaxIterationsTextBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
    }

    private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
      foreach (char character in e.Text)
      {
        if (!char.IsDigit(character) && character != '-')
        {
          e.Handled = true;
          return;
        }
      }

      TextBox textBox = sender as TextBox;
      if (e.Text == "-" && textBox != null)
      {
        if (textBox.SelectionStart != 0 || textBox.Text.Contains("-"))
        {
          e.Handled = true;
        }
      }
    }

    private void UpdateButtonsState()
    {
      bool hasData = originalArray.Count > 0;
      bool hasAlgorithms = BubbleSortCheckBox.IsChecked == true ||
                          InsertionSortCheckBox.IsChecked == true ||
                          ShakerSortCheckBox.IsChecked == true ||
                          QuickSortCheckBox.IsChecked == true ||
                          BogosortCheckBox.IsChecked == true;

      StartSorting.IsEnabled = hasData && hasAlgorithms;
    }

    private void Help_Click(object sender, RoutedEventArgs e)
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

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
      UpdateButtonsState();
    }

    private void RandomGenerate_Click(object sender, RoutedEventArgs e)
    {
      if (!int.TryParse(RowsTextBox.Text, out int size) || size <= 0)
      {
        MessageBox.Show("Сначала укажите корректный размер массива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!int.TryParse(MinValueTextBox.Text, out int min) || !int.TryParse(MaxValueTextBox.Text, out int max))
      {
        MessageBox.Show("Укажите корректный диапазон значений", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (min >= max)
      {
        MessageBox.Show("Минимальное значение должно быть меньше максимального", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      originalArray.Clear();
      for (int index = 0; index < size; ++index)
      {
        originalArray.Add(random.Next(min, max + 1));
      }

      RefreshDataGrids();
      UpdateButtonsState();
    }

    private void ImportCsv_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
      openFileDialog.Title = "Импорт данных из CSV";

      if (openFileDialog.ShowDialog() == true)
      {
        try
        {
          string[] lines = File.ReadAllLines(openFileDialog.FileName);
          originalArray.Clear();

          foreach (string line in lines)
          {
            string[] values = line.Split(',');
            foreach (string value in values)
            {
              if (int.TryParse(value.Trim(), out int number))
              {
                originalArray.Add(number);
              }
            }
          }

          RowsTextBox.Text = originalArray.Count.ToString();
          RefreshDataGrids();
          UpdateButtonsState();
          MessageBox.Show($"Успешно импортировано {originalArray.Count} элементов", "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка при импорте CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void ImportGoogle_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        GoogleSheetsImportDialog dialog = new GoogleSheetsImportDialog();
        if (dialog.ShowDialog() == true)
        {
          string url = dialog.SheetsUrl;

          // Конвертируем URL Google Таблиц в CSV формат
          string csvUrl = ConvertGoogleSheetsUrlToCsv(url);

          using (WebClient client = new WebClient())
          {
            string csvData = client.DownloadString(csvUrl);
            originalArray.Clear();

            // Парсим CSV данные
            using (StringReader reader = new StringReader(csvData))
            {
              string line;
              while ((line = reader.ReadLine()) != null)
              {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');
                foreach (string value in values)
                {
                  string trimmedValue = value.Trim().Replace("\"", "");
                  if (!string.IsNullOrEmpty(trimmedValue) && int.TryParse(trimmedValue, out int number))
                  {
                    originalArray.Add(number);
                  }
                }
              }
            }

            // Обновляем интерфейс
            RowsTextBox.Text = originalArray.Count.ToString();
            RefreshDataGrids();
            UpdateButtonsState();

            MessageBox.Show($"Успешно импортировано {originalArray.Count} элементов из Google Таблиц",
                          "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Ошибка при импорте из Google Таблиц: {ex.Message}",
                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private string ConvertGoogleSheetsUrlToCsv(string url)
    {
      try
      {
        Uri uri = new Uri(url);
        string path = uri.AbsolutePath;

        // Извлекаем ID таблицы из пути
        var pathParts = path.Split('/');
        string sheetId = pathParts.Length >= 4 ? pathParts[3] :
                        throw new ArgumentException("Неверный формат ссылки на Google Таблицу");

        // Извлекаем ID листа
        string gid = "0";
        if (uri.Fragment.Contains("gid="))
        {
          gid = uri.Fragment.Split('=')[1];
        }

        // Формируем URL для экспорта в CSV
        return $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";
      }
      catch (Exception ex)
      {
        throw new ArgumentException($"Неверный формат ссылки на Google Таблицу: {ex.Message}");
      }
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
      originalArray.Clear();
      sortResults.Clear();
      RefreshDataGrids();
      ResultsDataGrid.ItemsSource = null;

      PerformanceChart.Model = new PlotModel();

      UpdateButtonsState();
    }

    private void StartSorting_Click(object sender, RoutedEventArgs e)
    {
      if (originalArray.Count == 0)
      {
        MessageBox.Show("Нет данных для сортировки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!int.TryParse(MaxIterationsTextBox.Text, out int maxIterations) || maxIterations <= 0)
      {
        MessageBox.Show("Укажите корректное ограничение итераций", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      bool ascending = AscendingRadioButton.IsChecked == true;
      sortResults.Clear();

      if (BubbleSortCheckBox.IsChecked == true)
      {
        var result = BubbleSort(originalArray.ToArray(), ascending, maxIterations);
        sortResults.Add(result);
      }

      if (InsertionSortCheckBox.IsChecked == true)
      {
        var result = InsertionSort(originalArray.ToArray(), ascending, maxIterations);
        sortResults.Add(result);
      }

      if (ShakerSortCheckBox.IsChecked == true)
      {
        var result = ShakerSort(originalArray.ToArray(), ascending, maxIterations);
        sortResults.Add(result);
      }

      if (QuickSortCheckBox.IsChecked == true)
      {
        var result = QuickSort(originalArray.ToArray(), ascending, maxIterations);
        sortResults.Add(result);
      }

      if (BogosortCheckBox.IsChecked == true)
      {
        var result = BogoSort(originalArray.ToArray(), ascending, maxIterations);
        sortResults.Add(result);
      }

      DisplayResults();
      UpdateSortedArray();
    }

    private void ResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ApplySize_Click(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(RowsTextBox.Text, out int size) && size > 0)
      {
        List<int> newArray = new List<int>(new int[size]);

        for (int index = 0; index < Math.Min(originalArray.Count, size); ++index)
        {
          newArray[index] = originalArray[index];
        }

        originalArray = newArray;
        RefreshDataGrids();
        UpdateButtonsState();
      }
      else
      {
        MessageBox.Show("Пожалуйста, введите корректное положительное число для размера массива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        RowsTextBox.Text = "5";
      }
    }

    private void RefreshDataGrids()
    {
      var dataSource = new List<DataItem>();
      for (int index = 0; index < originalArray.Count; ++index)
      {
        dataSource.Add(new DataItem { Index = index, Value = originalArray[index] });
      }

      OriginalDataGrid.ItemsSource = dataSource;
      SortedDataGrid.ItemsSource = null;
    }

    private void UpdateSortedArray()
    {
      var bestResult = sortResults.OrderBy(result => result.TimeMs).FirstOrDefault();
      if (bestResult != null)
      {
        var sortedDataSource = new List<DataItem>();
        for (int index = 0; index < bestResult.SortedArray.Length; ++index)
        {
          sortedDataSource.Add(new DataItem { Index = index, Value = bestResult.SortedArray[index] });
        }
        SortedDataGrid.ItemsSource = sortedDataSource;
      }
    }

    private void DisplayResults()
    {
      ResultsDataGrid.ItemsSource = null;
      ResultsDataGrid.ItemsSource = sortResults;

      var plotModel = new PlotModel { Title = "" };

      var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
      var valueAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Minimum = 0,
        Title = ""
      };

      var barSeries = new BarSeries { FillColor = OxyColors.SteelBlue };

      foreach (var result in sortResults)
      {
        categoryAxis.Labels.Add(result.AlgorithmName);
        barSeries.Items.Add(new BarItem { Value = result.TimeMs });
      }

      plotModel.Axes.Add(categoryAxis);
      plotModel.Axes.Add(valueAxis);
      plotModel.Series.Add(barSeries);

      PerformanceChart.Model = plotModel;
    }

    private void OriginalDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
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
            if (rowIndex < originalArray.Count)
            {
              originalArray[rowIndex] = value;
            }
          }
        }
      }
    }

    private SortResult BubbleSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      for (int outerIndex = 0; outerIndex < sortedArray.Length - 1 && iterationsCount < maxIterations; ++outerIndex)
      {
        for (int innerIndex = 0; innerIndex < sortedArray.Length - outerIndex - 1 && iterationsCount < maxIterations; ++innerIndex)
        {
          bool shouldSwap = ascending ?
            sortedArray[innerIndex] > sortedArray[innerIndex + 1] :
            sortedArray[innerIndex] < sortedArray[innerIndex + 1];

          if (shouldSwap)
          {
            int temporaryValue = sortedArray[innerIndex];
            sortedArray[innerIndex] = sortedArray[innerIndex + 1];
            sortedArray[innerIndex + 1] = temporaryValue;
          }
          ++iterationsCount;
        }
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
        MessageBox.Show($"Превышен лимит итераций ({maxIterations}). Сортировка пузырьком прервана.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      return new SortResult("Пузырьковая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private SortResult InsertionSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      for (int currentIndex = 1; currentIndex < sortedArray.Length && iterationsCount < maxIterations; ++currentIndex)
      {
        int keyValue = sortedArray[currentIndex];
        int previousIndex = currentIndex - 1;

        while (previousIndex >= 0 && iterationsCount < maxIterations)
        {
          bool shouldMove = ascending ?
            sortedArray[previousIndex] > keyValue :
            sortedArray[previousIndex] < keyValue;

          if (shouldMove)
          {
            sortedArray[previousIndex + 1] = sortedArray[previousIndex];
            --previousIndex;
            ++iterationsCount;
          }
          else
          {
            break;
          }
        }
        sortedArray[previousIndex + 1] = keyValue;
        ++iterationsCount;
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
        MessageBox.Show($"Превышен лимит итераций ({maxIterations}). Сортировка вставками прервана.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      return new SortResult("Вставками", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private SortResult ShakerSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      int leftBoundary = 0;
      int rightBoundary = sortedArray.Length - 1;

      while (leftBoundary <= rightBoundary && iterationsCount < maxIterations)
      {
        for (int forwardIndex = leftBoundary; forwardIndex < rightBoundary && iterationsCount < maxIterations; ++forwardIndex)
        {
          bool shouldSwap = ascending ?
            sortedArray[forwardIndex] > sortedArray[forwardIndex + 1] :
            sortedArray[forwardIndex] < sortedArray[forwardIndex + 1];

          if (shouldSwap)
          {
            int temporaryValue = sortedArray[forwardIndex];
            sortedArray[forwardIndex] = sortedArray[forwardIndex + 1];
            sortedArray[forwardIndex + 1] = temporaryValue;
          }
          ++iterationsCount;
        }
        --rightBoundary;

        for (int backwardIndex = rightBoundary; backwardIndex > leftBoundary && iterationsCount < maxIterations; --backwardIndex)
        {
          bool shouldSwap = ascending ?
            sortedArray[backwardIndex - 1] > sortedArray[backwardIndex] :
            sortedArray[backwardIndex - 1] < sortedArray[backwardIndex];

          if (shouldSwap)
          {
            int temporaryValue = sortedArray[backwardIndex];
            sortedArray[backwardIndex] = sortedArray[backwardIndex - 1];
            sortedArray[backwardIndex - 1] = temporaryValue;
          }
          ++iterationsCount;
        }
        ++leftBoundary;
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
        MessageBox.Show($"Превышен лимит итераций ({maxIterations}). Шейкерная сортировка прервана.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      return new SortResult("Шейкерная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private SortResult QuickSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      QuickSortRecursive(sortedArray, 0, sortedArray.Length - 1, ascending, ref iterationsCount, maxIterations);

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
        MessageBox.Show($"Превышен лимит итераций ({maxIterations}). Быстрая сортировка прервана.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      return new SortResult("Быстрая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private void QuickSortRecursive(int[] array, int lowIndex, int highIndex, bool ascending, ref int iterationsCount, int maxIterations)
    {
      if (lowIndex < highIndex && iterationsCount < maxIterations)
      {
        int pivotIndex = Partition(array, lowIndex, highIndex, ascending, ref iterationsCount, maxIterations);
        QuickSortRecursive(array, lowIndex, pivotIndex - 1, ascending, ref iterationsCount, maxIterations);
        QuickSortRecursive(array, pivotIndex + 1, highIndex, ascending, ref iterationsCount, maxIterations);
      }
      ++iterationsCount;
    }

    private int Partition(int[] array, int lowIndex, int highIndex, bool ascending, ref int iterationsCount, int maxIterations)
    {
      int pivotValue = array[highIndex];
      int partitionIndex = lowIndex - 1;

      for (int currentIndex = lowIndex; currentIndex < highIndex && iterationsCount < maxIterations; ++currentIndex)
      {
        bool shouldSwap = ascending ?
          array[currentIndex] <= pivotValue :
          array[currentIndex] >= pivotValue;

        if (shouldSwap)
        {
          ++partitionIndex;
          int temporaryValue = array[partitionIndex];
          array[partitionIndex] = array[currentIndex];
          array[currentIndex] = temporaryValue;
        }
        ++iterationsCount;
      }

      int temporaryValue2 = array[partitionIndex + 1];
      array[partitionIndex + 1] = array[highIndex];
      array[highIndex] = temporaryValue2;

      return partitionIndex + 1;
    }

    private SortResult BogoSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      while (!IsSorted(sortedArray, ascending) && iterationsCount < maxIterations)
      {
        ShuffleArray(sortedArray);
        ++iterationsCount;
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
        MessageBox.Show($"Превышен лимит итераций ({maxIterations}). Болотная сортировка прервана.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      return new SortResult("Болотная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private bool IsSorted(int[] array, bool ascending)
    {
      for (int index = 0; index < array.Length - 1; ++index)
      {
        if (ascending && array[index] > array[index + 1])
          return false;
        if (!ascending && array[index] < array[index + 1])
          return false;
      }
      return true;
    }

    private void ShuffleArray(int[] array)
    {
      for (int index = 0; index < array.Length; ++index)
      {
        int randomIndex = random.Next(array.Length);
        int temporaryValue = array[index];
        array[index] = array[randomIndex];
        array[randomIndex] = temporaryValue;
      }
    }
  }

  public class SortResult
  {
    public string AlgorithmName { get; set; }
    public double TimeMs { get; set; }
    public int Iterations { get; set; }
    public int[] SortedArray { get; set; }
    public bool IterationLimitExceeded { get; set; }

    public SortResult(string algorithmName, double timeMs, int iterations, int[] sortedArray, bool iterationLimitExceeded = false)
    {
      AlgorithmName = algorithmName;
      TimeMs = Math.Round(timeMs, 3);
      Iterations = iterations;
      SortedArray = sortedArray;
      IterationLimitExceeded = iterationLimitExceeded;
    }
  }

  public class DataItem
  {
    public int Index { get; set; }
    public int Value { get; set; }
  }
}