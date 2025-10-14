using System;
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
        _view.UpdatePlot(_model.LowerBound, _model.UpperBound,
            _model.CalculationResult.Point, _model.CalculationResult.Value,
            _model.FindMinimum);
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
      _view.ResultText = "";
      _view.ClearPlot();
    }

    private void OnHelpRequested(object sender, EventArgs e)
    {
      string helpText = @"Метод золотого сечения

Инструкция:
1. Введите функцию f(x) в поле 'Функция f(x)'
2. Укажите границы интервала [a, b]
3. Задайте точность вычисления ε
4. Выберите режим поиска (минимум или максимум)
5. Нажмите 'Вычислить'

Примеры функций:
• x^2 + 2*x + 1
• sin(x) + cos(x)
• exp(-x^2)
• log(x) (для x > 0)

Примечание: используйте точку как разделитель дробной части.";

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

      if (!_view.FindMinimum && !_view.FindMaximum)
      {
        _view.ShowError("Выберите режим поиска (минимум или максимум)");
        return false;
      }

      try
      {
        _model.FunctionExpression = _view.FunctionExpression;
        _model.EvaluateFunction(lowerBound);
        _model.EvaluateFunction(upperBound);
        _model.EvaluateFunction((lowerBound + upperBound) / 2);
      }
      catch (Exception functionException)
      {
        _view.ShowError($"Ошибка в функции: {functionException.Message}");
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
    }

    private void DisplayResult()
    {
      var result = _model.CalculationResult;
      string extremumType = result.IsMinimum ? "минимум" : "максимум";
      _view.ResultText = $"Найден {extremumType}:\n" +
                       $"x = {Math.Round(result.Point, 3)}, f(x) = {Math.Round(result.Value, 3)}";
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
        MajorGridlineStyle = LineStyle.Dot,
        MajorGridlineColor = OxyColors.LightGray,
        IsPanEnabled = false,
        IsZoomEnabled = false
      });

      _plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "f(x)",
        TitleFontSize = 12,
        MajorGridlineStyle = LineStyle.Dot,
        MajorGridlineColor = OxyColors.LightGray,
        IsPanEnabled = false,
        IsZoomEnabled = false
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
        var lineSeries = new LineSeries
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
              lineSeries.Points.Add(new DataPoint(currentX, currentY));
            }
          }
          catch
          {
          }
        }

        _plotModel.Series.Add(lineSeries);

        var pointSeries = new ScatterSeries
        {
          Title = isMinimum ? "Минимум" : "Максимум",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = isMinimum ? OxyColors.Red : OxyColors.Green
        };

        pointSeries.Points.Add(new ScatterPoint(extremumX, extremumY));
        _plotModel.Series.Add(pointSeries);

        AdjustPlotBounds(lowerBound, upperBound, extremumY);

        _plotModel.InvalidatePlot(true);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении графика: {ex.Message}");
      }
    }

    private void AdjustPlotBounds(double lowerBound, double upperBound, double extremumY)
    {
      double xMargin = (upperBound - lowerBound) * 0.1;
      double yRange = GetFunctionValueRange(lowerBound, upperBound);
      double yMargin = yRange * 0.1;

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

        if (extremumY < minY) minY = extremumY;
        if (extremumY > maxY) maxY = extremumY;

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
