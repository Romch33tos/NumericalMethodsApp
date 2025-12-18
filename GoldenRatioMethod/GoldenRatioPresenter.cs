using System;
using System.Globalization;
using NumericalMethodsApp.Models;
using NumericalMethodsApp.Views;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace NumericalMethodsApp.Presenters
{
  public class GoldenRatioPresenter
  {
    private readonly IGoldenRatioView _view;
    private readonly GoldenRatioModel _model;
    private PlotModel _plotModel;

    public GoldenRatioPresenter(IGoldenRatioView view)
    {
      _view = view;
      _model = new GoldenRatioModel();
      InitializePlot();
      SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
      _view.CalculateRequested += OnCalculateRequested;
      _view.ClearAllRequested += OnClearAllRequested;
      _view.HelpRequested += OnHelpRequested;
      _view.ModeChanged += OnModeChanged;
    }

    private void OnCalculateRequested(object sender, EventArgs e)
    {
      try
      {
        if (!ValidateInput())
          return;

        UpdateModelFromView();
        _model.Calculate();

        DisplayResult();

        if (_model.FindZero)
        {
          UpdatePlotForZero(_model.LowerBound, _model.UpperBound, _model.CalculationResult.Point);
        }
        else
        {
          UpdatePlot(_model.LowerBound, _model.UpperBound,
              _model.CalculationResult.Point, _model.CalculationResult.Value,
              _model.FindMinimum);
        }
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при вычислении: {ex.Message}");
      }
    }

    private void OnClearAllRequested(object sender, EventArgs e)
    {
      _view.FunctionExpression = "";
      _view.LowerBound = "";
      _view.UpperBound = "";
      _view.Epsilon = "0.001";
      _view.FindMinimum = false;
      _view.FindMaximum = false;
      _view.FindZero = false;
      _view.ResultText = "";
      ClearPlot();
    }

    private void OnHelpRequested(object sender, EventArgs e)
    {
      string helpText = @"Метод золотого сечения

Инструкция по использованию:

1. ВВОД ФУНКЦИИ
   - Используйте переменную x в выражении
   - Поддерживаемые операции: +, -, *, /, ^ (степень)
   - Поддерживаемые функции: sin(x), cos(x), tan(x), log(x), log10(x), exp(x), sqrt(x), abs(x)
   - Константы: pi (3.14159...), e (2.71828...)
   - Примеры корректных выражений:
     * x^2 + 2*x + 1
     * sin(x) * cos(x)
     * log(x) + 0.5*x
     * 0.1*x + x^2
     * exp(-0.5*x^2)

2. ЗАДАНИЕ ИНТЕРВАЛА
   - Начало интервала (A) должно быть меньше конца интервала (B)
   - Убедитесь, что функция определена на всем интервале
   - Для поиска нуля: функция должна иметь разные знаки на концах интервала

3. ВЫБОР ТОЧНОСТИ
   - Точность ε должна быть положительным числом
   - Меньшие значения дают более точный результат, но требуют больше итераций
   - Рекомендуемое значение: 0.001
   - Количество знаков после запятой в результатах зависит от заданной точности

4. ВЫБОР РЕЖИМА
   - Минимум: поиск наименьшего значения функции на интервале
   - Максимум: поиск наибольшего значения функции на интервале
   - Ноль: поиск корня функции (нуля)

5. ВИЗУАЛИЗАЦИЯ
   - График функции отображается синей линией
   - Найденный экстремум отмечается красной (минимум) или зеленой (максимум) точкой
   - Найденный ноль отмечается фиолетовой точкой
   - Для навигации по графику используйте левую кнопку мыши для перемещения и колесо для масштабирования

Особенности работы:
- Метод золотого сечения гарантированно находит экстремум унимодальной функции
- Для поиска нуля функция должна иметь разные знаки на концах интервала
- Функция должна быть непрерывной на заданном интервале
- При возникновении ошибок попробуйте изменить интервал поиска";

      _view.ShowInfo(helpText);
    }

    private void OnModeChanged(object sender, EventArgs e)
    {
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(_view.FunctionExpression))
      {
        _view.ShowError("Введите функцию f(x)");
        return false;
      }

      if (!GoldenRatioModel.TryParseDouble(_view.LowerBound, out double lowerBound) ||
          !GoldenRatioModel.TryParseDouble(_view.UpperBound, out double upperBound))
      {
        _view.ShowError("Границы интервала должны быть вещественными числами");
        return false;
      }

      if (lowerBound >= upperBound)
      {
        _view.ShowError("Начало интервала (A) должно быть меньше конца интервала (B)");
        return false;
      }

      if (!GoldenRatioModel.TryParseDouble(_view.Epsilon, out double epsilonValue) || epsilonValue <= 0)
      {
        _view.ShowError("Точность ε должна быть положительным вещественным числом");
        return false;
      }

      if (!_view.FindMinimum && !_view.FindMaximum && !_view.FindZero)
      {
        _view.ShowError("Выберите режим поиска (минимум, максимум или ноль)");
        return false;
      }

      try
      {
        _model.FunctionExpression = _view.FunctionExpression;

        if (_view.FindZero)
        {
          double fLower = _model.EvaluateFunction(lowerBound);
          double fUpper = _model.EvaluateFunction(upperBound);

          if (Math.Sign(fLower) == Math.Sign(fUpper))
          {
            _view.ShowError("Для поиска нуля функция должна иметь разные знаки на концах интервала");
            return false;
          }
        }
        else
        {
          _model.EvaluateFunction(lowerBound);
          _model.EvaluateFunction(upperBound);
          _model.EvaluateFunction((lowerBound + upperBound) / 2);
        }
      }
      catch (Exception functionException)
      {
        _view.ShowError($"Ошибка в функции: {functionException.Message}\nПопробуйте изменить интервал поиска.");
        return false;
      }

      return true;
    }

    private void UpdateModelFromView()
    {
      _model.FunctionExpression = _view.FunctionExpression;
      _model.LowerBound = GoldenRatioModel.ParseDouble(_view.LowerBound);
      _model.UpperBound = GoldenRatioModel.ParseDouble(_view.UpperBound);
      _model.Epsilon = GoldenRatioModel.ParseDouble(_view.Epsilon);
      _model.FindMinimum = _view.FindMinimum;
      _model.FindZero = _view.FindZero;
    }

    private void DisplayResult()
    {
      var result = _model.CalculationResult;
      double epsilon = _model.Epsilon;

      int decimalPlaces = GetDecimalPlacesFromEpsilon(epsilon);
      string formatString = $"F{decimalPlaces}";

      if (result.IsZero)
      {
        double roundedX = Math.Round(result.Point, decimalPlaces);
        _view.ResultText = $"Найден ноль функции:\n" +
                         $"x = {roundedX.ToString(formatString)}, f(x) = 0";
      }
      else
      {
        double roundedX = Math.Round(result.Point, decimalPlaces);
        double roundedY = Math.Round(result.Value, decimalPlaces);
        string extremumType = result.IsMinimum ? "минимум" : "максимум";
        _view.ResultText = $"Найден {extremumType}:\n" +
                         $"x = {roundedX.ToString(formatString)}, f(x) = {roundedY.ToString(formatString)}";
      }
    }

    private int GetDecimalPlacesFromEpsilon(double epsilon)
    {
      if (epsilon <= 0)
        return 5;

      double log10 = Math.Log10(epsilon);
      int decimalPlaces = (int)Math.Ceiling(Math.Abs(log10));

      if (decimalPlaces < 1) return 1;
      if (decimalPlaces > 15) return 15;
      return decimalPlaces;
    }

    private void InitializePlot()
    {
      _plotModel = new PlotModel
      {
        TitleFontSize = 14,
        TitleFontWeight = OxyPlot.FontWeights.Bold,
        PlotAreaBorderColor = OxyColors.LightGray,
        PlotAreaBorderThickness = new OxyThickness(1),
        IsLegendVisible = false
      };

      _plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        TitleFontSize = 12,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 0)
      });

      _plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "f(x)",
        TitleFontSize = 12,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 0)
      });

      if (_view is GoldenRatioMethod wpfView)
      {
        wpfView.PlotViewControl.Model = _plotModel;
      }
    }

    public void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum)
    {
      _plotModel.Series.Clear();

      try
      {
        var functionSeries = new LineSeries
        {
          Title = "f(x)",
          Color = OxyColors.Blue,
          StrokeThickness = 2
        };

        int pointsCount = 100;
        double stepSize = (upperBound - lowerBound) / pointsCount;

        for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
        {
          double currentX = lowerBound + pointIndex * stepSize;
          try
          {
            double currentY = _model.EvaluateFunction(currentX);
            if (!double.IsInfinity(currentY) && !double.IsNaN(currentY))
            {
              functionSeries.Points.Add(new DataPoint(currentX, currentY));
            }
          }
          catch
          {
          }
        }

        _plotModel.Series.Add(functionSeries);

        var extremumSeries = new ScatterSeries
        {
          Title = isMinimum ? "Минимум" : "Максимум",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = isMinimum ? OxyColors.Red : OxyColors.Green
        };

        extremumSeries.Points.Add(new ScatterPoint(extremumX, extremumY));
        _plotModel.Series.Add(extremumSeries);

        AdjustPlotBounds(lowerBound, upperBound);

        _plotModel.InvalidatePlot(true);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении графика: {ex.Message}");
      }
    }

    public void UpdatePlotForZero(double lowerBound, double upperBound, double zeroX)
    {
      _plotModel.Series.Clear();

      try
      {
        var functionSeries = new LineSeries
        {
          Title = "f(x)",
          Color = OxyColors.Blue,
          StrokeThickness = 2
        };

        int pointsCount = 100;
        double stepSize = (upperBound - lowerBound) / pointsCount;

        for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
        {
          double currentX = lowerBound + pointIndex * stepSize;
          try
          {
            double currentY = _model.EvaluateFunction(currentX);
            if (!double.IsInfinity(currentY) && !double.IsNaN(currentY))
            {
              functionSeries.Points.Add(new DataPoint(currentX, currentY));
            }
          }
          catch
          {
          }
        }

        _plotModel.Series.Add(functionSeries);

        var zeroSeries = new ScatterSeries
        {
          Title = "Ноль",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = OxyColors.Purple
        };

        zeroSeries.Points.Add(new ScatterPoint(zeroX, 0));
        _plotModel.Series.Add(zeroSeries);

        AdjustPlotBounds(lowerBound, upperBound);

        _plotModel.InvalidatePlot(true);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении графика для нуля: {ex.Message}");
      }
    }

    private void ClearPlot()
    {
      _plotModel?.Series.Clear();
      _plotModel?.InvalidatePlot(true);
    }

    private void AdjustPlotBounds(double lowerBound, double upperBound)
    {
      double xMargin = (upperBound - lowerBound) * 0.1;
      double yRange = GetFunctionValueRange(lowerBound, upperBound);
      double yMargin = Math.Max(yRange * 0.1, 0.1);

      var xAxis = _plotModel.Axes[0] as LinearAxis;
      var yAxis = _plotModel.Axes[1] as LinearAxis;

      if (xAxis != null)
      {
        xAxis.Minimum = lowerBound - xMargin;
        xAxis.Maximum = upperBound + xMargin;
      }

      if (yAxis != null)
      {
        double minY = double.MaxValue;
        double maxY = double.MinValue;

        foreach (var series in _plotModel.Series)
        {
          if (series is LineSeries lineSeries)
          {
            foreach (var point in lineSeries.Points)
            {
              if (point.Y < minY) minY = point.Y;
              if (point.Y > maxY) maxY = point.Y;
            }
          }
        }

        if (Math.Abs(maxY - minY) < 0.001)
        {
          minY -= 1;
          maxY += 1;
        }

        yAxis.Minimum = minY - yMargin;
        yAxis.Maximum = maxY + yMargin;
      }
    }

    private double GetFunctionValueRange(double lowerBound, double upperBound)
    {
      double minValue = double.MaxValue;
      double maxValue = double.MinValue;

      int pointsCount = 50;
      double stepSize = (upperBound - lowerBound) / pointsCount;

      for (int index = 0; index <= pointsCount; ++index)
      {
        double currentX = lowerBound + index * stepSize;
        try
        {
          double currentY = _model.EvaluateFunction(currentX);
          if (currentY < minValue) minValue = currentY;
          if (currentY > maxValue) maxValue = currentY;
        }
        catch
        {
        }
      }

      return maxValue - minValue;
    }
  }
}
