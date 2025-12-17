using System;
using System.Globalization;
using System.Windows;

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
