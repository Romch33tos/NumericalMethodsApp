using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public class LeastSquaresPresenter
  {
    private readonly ILeastSquaresView view;
    private readonly LeastSquaresModel model;
    private readonly Random randomGenerator = new Random();

    public LeastSquaresPresenter(ILeastSquaresView view)
    {
      this.view = view;
      model = new LeastSquaresModel();
      SubscribeToEvents();
      InitializeView();
      UpdateDataGrid();
    }

    private void SubscribeToEvents()
    {
      view.DimensionChanged += HandleDimensionChanged;
      view.ApplyDimensionClicked += HandleApplyDimensionClicked;
      view.CalculateClicked += HandleCalculateClicked;
      view.ClearAllClicked += HandleClearAllClicked;
      view.RandomGenerateClicked += HandleRandomGenerateClicked;
      view.ImportCsvClicked += HandleImportCsvClicked;
      view.ImportGoogleSheetsClicked += HandleImportGoogleSheetsClicked;
      view.HelpClicked += HandleHelpClicked;
      view.CellEditEnding += HandleCellEditEnding;
    }

    private void InitializeView()
    {
      view.DimensionText = model.Dimension.ToString();
      view.RangeStartText = model.RangeStart.ToString(CultureInfo.InvariantCulture);
      view.RangeEndText = model.RangeEnd.ToString(CultureInfo.InvariantCulture);
      view.PrecisionText = model.Precision.ToString(CultureInfo.InvariantCulture);
      view.IsDataGridEnabled = false;
      UpdateDataGrid();
    }

    private void HandleDimensionChanged(object sender, RoutedEventArgs e)
    {
      view.IsDataGridEnabled = false;
    }

    private void HandleApplyDimensionClicked(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(view.DimensionText, out int dimension) && dimension > 0)
      {
        model.UpdateDimension(dimension);
        view.IsDataGridEnabled = true;
        UpdateDataGrid();
      }
      else
      {
        view.ShowMessage("Пожалуйста, введите корректное положительное число для размерности", "Ошибка", MessageBoxType.Error);
        view.DimensionText = "3";
      }
    }

    private void HandleCalculateClicked(object sender, RoutedEventArgs e)
    {
      if (model.Points.Count < 2)
      {
        view.ShowMessage("Недостаточно точек для расчета", "Ошибка", MessageBoxType.Error);
        return;
      }

      if (!double.TryParse(view.PrecisionText, NumberStyles.Any, CultureInfo.InvariantCulture, out double precision) || precision <= 0)
      {
        view.ShowMessage("Укажите корректную точность (положительное число)", "Ошибка", MessageBoxType.Error);
        return;
      }

      var coefficients = model.CalculatePolynomialCoefficients();
      if (coefficients == null)
      {
        view.ShowMessage("Не удалось вычислить коэффициенты", "Ошибка", MessageBoxType.Error);
        return;
      }

      int decimalPlaces = GetDecimalPlacesFromPrecision(precision);
      string formatString = $"F{decimalPlaces}";

      string resultText = "Коэффициенты полинома:\n";
      for (int coefficientIndex = 0; coefficientIndex < coefficients.Length; ++coefficientIndex)
      {
        resultText += $"a{coefficientIndex + 1} = {coefficients[coefficientIndex].ToString(formatString, CultureInfo.InvariantCulture)}\n";
      }

      resultText += "\nАппроксимирующая функция:\n";
      resultText += "f(x) = ";
      for (int coefficientIndex = 0; coefficientIndex < coefficients.Length; ++coefficientIndex)
      {
        if (coefficientIndex == 0)
        {
          resultText += coefficients[coefficientIndex].ToString(formatString, CultureInfo.InvariantCulture);
        }
        else
        {
          string sign = coefficients[coefficientIndex] >= 0 ? " + " : " - ";
          resultText += $"{sign}{Math.Abs(coefficients[coefficientIndex]).ToString(formatString, CultureInfo.InvariantCulture)}*x^{coefficientIndex}";
        }
      }

      view.ResultText = resultText;
    }

    private void HandleClearAllClicked(object sender, RoutedEventArgs e)
    {
      MessageBoxResult result = view.ShowMessage(
          "Вы действительно хотите очистить все данные?",
          "Подтверждение очистки",
          MessageBoxType.Question);

      if (result != MessageBoxResult.Yes) return;

      model.Dimension = 3;
      model.RangeStart = 0;
      model.RangeEnd = 10;
      model.Precision = 0.001;
      model.Points.Clear();

      view.DimensionText = model.Dimension.ToString();
      view.RangeStartText = model.RangeStart.ToString(CultureInfo.InvariantCulture);
      view.RangeEndText = model.RangeEnd.ToString(CultureInfo.InvariantCulture);
      view.PrecisionText = model.Precision.ToString(CultureInfo.InvariantCulture);
      view.IsDataGridEnabled = false;
      view.ResultText = "";
      UpdateDataGrid();
    }

    private void HandleRandomGenerateClicked(object sender, RoutedEventArgs e)
    {
      if (!double.TryParse(view.RangeStartText, NumberStyles.Any, CultureInfo.InvariantCulture, out double rangeStart) ||
          !double.TryParse(view.RangeEndText, NumberStyles.Any, CultureInfo.InvariantCulture, out double rangeEnd))
      {
        view.ShowMessage("Укажите корректный диапазон значений", "Ошибка", MessageBoxType.Error);
        return;
      }

      if (rangeStart >= rangeEnd)
      {
        view.ShowMessage("Начало диапазона должно быть меньше конца", "Ошибка", MessageBoxType.Error);
        return;
      }

      for (int pointIndex = 0; pointIndex < model.Points.Count; ++pointIndex)
      {
        double xValue = rangeStart + (randomGenerator.NextDouble() * (rangeEnd - rangeStart));
        double yValue = rangeStart + (randomGenerator.NextDouble() * (rangeEnd - rangeStart));
        model.Points[pointIndex] = new DataPoint(Math.Round(xValue, 2), Math.Round(yValue, 2));
      }

      view.IsDataGridEnabled = true;
      UpdateDataGrid();
    }

    private void HandleCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      if (e.EditAction == DataGridEditAction.Commit)
      {
        var textBox = e.EditingElement as TextBox;
        if (textBox != null)
        {
          if (!double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
          {
            view.ShowMessage("Введите корректное число", "Ошибка", MessageBoxType.Error);
            e.Cancel = true;
          }
          else
          {
            int rowIndex = e.Row.GetIndex();
            if (rowIndex < model.Points.Count)
            {
              var propertyName = (e.Column.Header.ToString());
              if (propertyName == "X")
              {
                model.Points[rowIndex] = new DataPoint(value, model.Points[rowIndex].Y);
              }
              else if (propertyName == "Y")
              {
                model.Points[rowIndex] = new DataPoint(model.Points[rowIndex].X, value);
              }
            }
          }
        }
      }
    }

    private void UpdateDataGrid()
    {
      var dataGridItems = new List<GridDataPoint>();
      for (int pointIndex = 0; pointIndex < model.Points.Count; ++pointIndex)
      {
        dataGridItems.Add(new GridDataPoint
        {
          Index = pointIndex + 1,
          X = model.Points[pointIndex].X,
          Y = model.Points[pointIndex].Y
        });
      }
      view.UpdateDataGrid(dataGridItems);
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
  }
}
