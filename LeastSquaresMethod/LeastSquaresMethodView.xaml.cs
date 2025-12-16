using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public partial class LeastSquaresMethodView : Window
  {
    private List<DataPoint> dataPoints = new List<DataPoint>();
    private Random randomGenerator = new Random();

    public LeastSquaresMethodView()
    {
      InitializeComponent();
      InitializeView();
      SubscribeToEvents();
    }

    private void InitializeView()
    {
      DimensionTextBox.Text = "3";
      RangeStartTextBox.Text = "0";
      RangeEndTextBox.Text = "10";
      PrecisionTextBox.Text = "0.001";
      UpdateDataGrid();
      UpdatePlot();
    }

    private void SubscribeToEvents()
    {
      DimensionTextBox.TextChanged += OnDimensionTextChanged;
      RangeStartTextBox.TextChanged += OnRangeTextChanged;
      RangeEndTextBox.TextChanged += OnRangeTextChanged;
      PrecisionTextBox.TextChanged += OnPrecisionTextChanged;
    }

    private void OnDimensionTextChanged(object sender, TextChangedEventArgs e)
    {
      if (int.TryParse(DimensionTextBox.Text, out int dimension) && dimension > 0)
      {
        while (dataPoints.Count < dimension)
        {
          dataPoints.Add(new DataPoint(0, 0));
        }
        while (dataPoints.Count > dimension)
        {
          dataPoints.RemoveAt(dataPoints.Count - 1);
        }
        UpdateDataGrid();
      }
    }

    private void OnRangeTextChanged(object sender, TextChangedEventArgs e)
    {
      UpdateDataGrid();
    }

    private void OnPrecisionTextChanged(object sender, TextChangedEventArgs e)
    {
      UpdateDataGrid();
    }

    private void UpdateDataGrid()
    {
      var items = new List<DataPointItem>();
      for (int index = 0; index < dataPoints.Count; ++index)
      {
        items.Add(new DataPointItem
        {
          Index = index + 1,
          X = dataPoints[index].X,
          Y = dataPoints[index].Y
        });
      }
      PointsDataGrid.ItemsSource = items;
    }

    private void UpdatePlot()
    {
      var plotModel = new PlotModel
      {
        Title = "Метод наименьших квадратов",
        Background = OxyColors.White
      };

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "X",
        MajorGridlineStyle = LineStyle.Solid,
        MinorGridlineStyle = LineStyle.Dot,
        MajorGridlineColor = OxyColors.LightGray,
        MinorGridlineColor = OxyColors.LightGray.WithAlpha(40)
      });

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "Y",
        MajorGridlineStyle = LineStyle.Solid,
        MinorGridlineStyle = LineStyle.Dot,
        MajorGridlineColor = OxyColors.LightGray,
        MinorGridlineColor = OxyColors.LightGray.WithAlpha(40)
      });

      var scatterSeries = new ScatterSeries
      {
        Title = "Исходные точки",
        MarkerType = MarkerType.Circle,
        MarkerSize = 6,
        MarkerFill = OxyColors.Red,
        MarkerStroke = OxyColors.DarkRed,
        MarkerStrokeThickness = 1
      };

      foreach (var point in dataPoints)
      {
        scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
      }

      plotModel.Series.Add(scatterSeries);

      if (dataPoints.Count >= 2)
      {
        var coefficients = CalculateCoefficients();
        if (coefficients != null)
        {
          var lineSeries = new LineSeries
          {
            Title = "Аппроксимирующая функция",
            Color = OxyColors.Blue,
            StrokeThickness = 2
          };

          double minX = dataPoints.Min(p => p.X);
          double maxX = dataPoints.Max(p => p.X);
          double step = (maxX - minX) / 100;

          for (double x = minX - 1; x <= maxX + 1; x += step)
          {
            double y = EvaluatePolynomial(x, coefficients);
            lineSeries.Points.Add(new DataPoint(x, y));
          }

          plotModel.Series.Add(lineSeries);
        }
      }

      MainPlot.Model = plotModel;
    }

    private double[] CalculateCoefficients()
    {
      if (dataPoints.Count < 2) return null;

      int n = dataPoints.Count;
      double[,] matrix = new double[n, n + 1];

      for (int row = 0; row < n; ++row)
      {
        for (int col = 0; col < n; ++col)
        {
          matrix[row, col] = Math.Pow(dataPoints[row].X, col);
        }
        matrix[row, n] = dataPoints[row].Y;
      }

      for (int pivot = 0; pivot < n; ++pivot)
      {
        double maxElement = Math.Abs(matrix[pivot, pivot]);
        int maxRow = pivot;
        for (int row = pivot + 1; row < n; ++row)
        {
          if (Math.Abs(matrix[row, pivot]) > maxElement)
          {
            maxElement = Math.Abs(matrix[row, pivot]);
            maxRow = row;
          }
        }

        if (maxElement < double.Epsilon) return null;

        if (maxRow != pivot)
        {
          for (int col = pivot; col <= n; ++col)
          {
            double temp = matrix[pivot, col];
            matrix[pivot, col] = matrix[maxRow, col];
            matrix[maxRow, col] = temp;
          }
        }

        for (int row = pivot + 1; row < n; ++row)
        {
          double factor = matrix[row, pivot] / matrix[pivot, pivot];
          for (int col = pivot; col <= n; ++col)
          {
            matrix[row, col] -= factor * matrix[pivot, col];
          }
        }
      }

      double[] coefficients = new double[n];
      for (int i = n - 1; i >= 0; --i)
      {
        coefficients[i] = matrix[i, n];
        for (int j = i + 1; j < n; ++j)
        {
          coefficients[i] -= matrix[i, j] * coefficients[j];
        }
        coefficients[i] /= matrix[i, i];
      }

      return coefficients;
    }

    private double EvaluatePolynomial(double x, double[] coefficients)
    {
      double result = 0;
      for (int i = 0; i < coefficients.Length; ++i)
      {
        result += coefficients[i] * Math.Pow(x, i);
      }
      return result;
    }

    private void RandomGenerateButton_Click(object sender, RoutedEventArgs e)
    {
      if (!double.TryParse(RangeStartTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double min) ||
          !double.TryParse(RangeEndTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double max))
      {
        MessageBox.Show("Укажите корректный диапазон значений", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (min >= max)
      {
        MessageBox.Show("Начало диапазона должно быть меньше конца", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      for (int index = 0; index < dataPoints.Count; ++index)
      {
        double x = min + (randomGenerator.NextDouble() * (max - min));
        double y = min + (randomGenerator.NextDouble() * (max - min));
        dataPoints[index] = new DataPoint(Math.Round(x, 2), Math.Round(y, 2));
      }

      UpdateDataGrid();
      UpdatePlot();
    }

    private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
      openFileDialog.Title = "Импорт данных из CSV";

      if (openFileDialog.ShowDialog() == true)
      {
        try
        {
          string[] lines = File.ReadAllLines(openFileDialog.FileName);
          dataPoints.Clear();

          foreach (string line in lines)
          {
            string[] values = line.Split(',');
            if (values.Length >= 2)
            {
              if (double.TryParse(values[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                  double.TryParse(values[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
              {
                dataPoints.Add(new DataPoint(x, y));
              }
            }
          }

          DimensionTextBox.Text = dataPoints.Count.ToString();
          UpdateDataGrid();
          UpdatePlot();
          MessageBox.Show($"Успешно импортировано {dataPoints.Count} точек", "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Ошибка при импорте CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void ImportGoogleSheetsButton_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        GoogleSheetsImportDialog dialog = new GoogleSheetsImportDialog();
        if (dialog.ShowDialog() == true)
        {
          string url = dialog.SheetsUrl;
          string csvUrl = ConvertGoogleSheetsUrlToCsv(url);

          using (WebClient client = new WebClient())
          {
            string csvData = client.DownloadString(csvUrl);
            dataPoints.Clear();

            using (StringReader reader = new StringReader(csvData))
            {
              string line;
              while ((line = reader.ReadLine()) != null)
              {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');
                if (values.Length >= 2)
                {
                  string xStr = values[0].Trim().Replace("\"", "");
                  string yStr = values[1].Trim().Replace("\"", "");

                  if (!string.IsNullOrEmpty(xStr) && !string.IsNullOrEmpty(yStr) &&
                      double.TryParse(xStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                      double.TryParse(yStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                  {
                    dataPoints.Add(new DataPoint(x, y));
                  }
                }
              }
            }

            DimensionTextBox.Text = dataPoints.Count.ToString();
            UpdateDataGrid();
            UpdatePlot();

            MessageBox.Show($"Успешно импортировано {dataPoints.Count} точек из Google Таблиц",
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

        var pathParts = path.Split('/');
        string sheetId = pathParts.Length >= 4 ? pathParts[3] :
            throw new ArgumentException("Неверный формат ссылки на Google Таблицу");

        string gid = "0";
        if (uri.Fragment.Contains("gid="))
        {
          gid = uri.Fragment.Split('=')[1];
        }

        return $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";
      }
      catch (Exception ex)
      {
        throw new ArgumentException($"Неверный формат ссылки на Google Таблицу: {ex.Message}");
      }
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult result = MessageBox.Show(
          "Вы действительно хотите очистить все данные?",
          "Подтверждение очистки",
          MessageBoxButton.YesNo,
          MessageBoxImage.Question);

      if (result != MessageBoxResult.Yes) return;

      DimensionTextBox.Text = "3";
      RangeStartTextBox.Text = "0";
      RangeEndTextBox.Text = "10";
      PrecisionTextBox.Text = "0.001";
      dataPoints.Clear();
      for (int i = 0; i < 3; ++i)
      {
        dataPoints.Add(new DataPoint(0, 0));
      }
      UpdateDataGrid();
      UpdatePlot();
      ResultTextBox.Text = "";
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      if (dataPoints.Count < 2)
      {
        MessageBox.Show("Недостаточно точек для расчета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!double.TryParse(PrecisionTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double precision) || precision <= 0)
      {
        MessageBox.Show("Укажите корректную точность (положительное число)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      var coefficients = CalculateCoefficients();
      if (coefficients == null)
      {
        MessageBox.Show("Не удалось вычислить коэффициенты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      int decimalPlaces = GetDecimalPlacesFromPrecision(precision);
      string formatString = $"F{decimalPlaces}";

      string resultText = "Коэффициенты полинома:\n";
      for (int i = 0; i < coefficients.Length; ++i)
      {
        resultText += $"a{i} = {coefficients[i].ToString(formatString, CultureInfo.InvariantCulture)}\n";
      }

      resultText += "\nАппроксимирующая функция:\n";
      resultText += "f(x) = ";
      for (int i = 0; i < coefficients.Length; ++i)
      {
        if (i == 0)
        {
          resultText += coefficients[i].ToString(formatString, CultureInfo.InvariantCulture);
        }
        else
        {
          string sign = coefficients[i] >= 0 ? " + " : " - ";
          resultText += $"{sign}{Math.Abs(coefficients[i]).ToString(formatString, CultureInfo.InvariantCulture)}*x^{i}";
        }
      }

      ResultTextBox.Text = resultText;
      UpdatePlot();
    }

    private int GetDecimalPlacesFromPrecision(double precision)
    {
      if (precision >= 1) return 0;
      if (precision >= 0.1) return 1;
      if (precision >= 0.01) return 2;
      if (precision >= 0.001) return 3;
      if (precision >= 0.0001) return 4;
      if (precision >= 0.00001) return 5;
      return 6;
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(
          "Справка по использованию приложения:\n\n" +
          "1. Укажите размерность (n) - степень полинома + 1\n" +
          "2. Введите точки (x, y) в таблицу или сгенерируйте случайные\n" +
          "3. Укажите точность вычислений (ε)\n" +
          "4. Нажмите 'Вычислить' для расчета коэффициентов\n" +
          "5. Результаты отобразятся в текстовом поле и на графике\n\n" +
          "Для импорта данных используйте кнопки 'Импорт из CSV' или 'Импорт из Google Таблиц'",
          "Справка",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void ApplyDimensionButton_Click(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(DimensionTextBox.Text, out int dimension) && dimension > 0)
      {
        while (dataPoints.Count < dimension)
        {
          dataPoints.Add(new DataPoint(0, 0));
        }
        while (dataPoints.Count > dimension)
        {
          dataPoints.RemoveAt(dataPoints.Count - 1);
        }
        UpdateDataGrid();
        UpdatePlot();
      }
      else
      {
        MessageBox.Show("Пожалуйста, введите корректное положительное число для размерности", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        DimensionTextBox.Text = "3";
      }
    }

    public class DataPoint
    {
      public double X { get; set; }
      public double Y { get; set; }

      public DataPoint(double x, double y)
      {
        X = x;
        Y = y;
      }
    }

    public class DataPointItem
    {
      public int Index { get; set; }
      public double X { get; set; }
      public double Y { get; set; }
    }

    private void PointsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      if (e.EditAction == DataGridEditAction.Commit)
      {
        var textBox = e.EditingElement as TextBox;
        if (textBox != null)
        {
          if (!double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
          {
            MessageBox.Show("Введите корректное число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Cancel = true;
          }
          else
          {
            int rowIndex = e.Row.GetIndex();
            if (rowIndex < dataPoints.Count)
            {
              var propertyName = (e.Column.Header.ToString());
              if (propertyName == "X")
              {
                dataPoints[rowIndex] = new DataPoint(value, dataPoints[rowIndex].Y);
              }
              else if (propertyName == "Y")
              {
                dataPoints[rowIndex] = new DataPoint(dataPoints[rowIndex].X, value);
              }
              UpdatePlot();
            }
          }
        }
      }
    }
  }
}