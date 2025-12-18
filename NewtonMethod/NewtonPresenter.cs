using System;
using System.Globalization;
using NumericalMethodsApp.Models;
using NumericalMethodsApp.Views;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace NumericalMethodsApp.Presenters
{
  public class NewtonPresenter
  {
    private readonly INewtonView view;
    private readonly NewtonModel model;
    private PlotModel plotModel;
    private bool calculationSuccessful;

    public NewtonPresenter(INewtonView view)
    {
      this.view = view;
      model = new NewtonModel();
      calculationSuccessful = false;
      InitializePlot();
      SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
      view.CalculateRequested += OnCalculateRequested;
      view.ClearAllRequested += OnClearAllRequested;
      view.HelpRequested += OnHelpRequested;
      view.StepChanged += OnStepChanged;
    }

    private void OnCalculateRequested(object sender, EventArgs e)
    {
      try
      {
        if (!ValidateInput())
          return;

        UpdateModelFromView();
        model.Calculate();

        calculationSuccessful = true;
        view.IsModeLocked = true;

        DisplayFinalResult();
        view.TotalSteps = model.IterationSteps.Count;
        view.CurrentStep = model.IterationSteps.Count;

        view.UpdatePlot(model.LowerBound, model.UpperBound,
            model.CalculationResult.Point, model.CalculationResult.Value);
      }
      catch (Exception ex)
      {
        calculationSuccessful = false;
        view.IsModeLocked = false;
        view.ShowError($"Ошибка при вычислении: {ex.Message}");
      }
    }

    private void OnStepChanged(object sender, int stepIndex)
    {
      if (stepIndex >= 0 && stepIndex <= model.IterationSteps.Count)
      {
        if (stepIndex == model.IterationSteps.Count)
        {
          DisplayFinalResult();
          view.UpdatePlot(model.LowerBound, model.UpperBound,
              model.CalculationResult.Point, model.CalculationResult.Value);
        }
        else
        {
          var step = model.IterationSteps[stepIndex];
          double currentPoint = step.CurrentPoint;
          double functionValue = step.FunctionValue;

          UpdateStepPlot(step, currentPoint, functionValue);

          string extremumType = "Минимум";
          int decimalPlaces = GetDecimalPlacesFromEpsilon();
          string format = $"F{decimalPlaces}";

          view.ResultText = $"Текущая точка: x = {step.CurrentPoint.ToString(format, CultureInfo.InvariantCulture)}\n" +
                            $"Следующая точка: x = {step.NextPoint.ToString(format, CultureInfo.InvariantCulture)}\n" +
                            $"Первая производная: {step.FirstDerivative.ToString(format, CultureInfo.InvariantCulture)}\n" +
                            $"Вторая производная: {step.SecondDerivative.ToString(format, CultureInfo.InvariantCulture)}\n" +
                            $"{extremumType}: f(x) = {functionValue.ToString(format, CultureInfo.InvariantCulture)}";
        }
      }
    }

    private int GetDecimalPlacesFromEpsilon()
    {
      double epsilon = model.Epsilon;
      if (epsilon <= 0) return 5;

      if (epsilon >= 1) return 0;

      string epsilonStr = epsilon.ToString("G15", CultureInfo.InvariantCulture);
      int decimalPlaces = 0;

      if (epsilonStr.Contains("."))
      {
        string decimalPart = epsilonStr.Split('.')[1];
        decimalPlaces = decimalPart.Length;
      }
      else if (epsilonStr.Contains(","))
      {
        string decimalPart = epsilonStr.Split(',')[1];
        decimalPlaces = decimalPart.Length;
      }
      else if (epsilonStr.Contains("E-"))
      {
        int exponent = int.Parse(epsilonStr.Split(new[] { "E-" }, StringSplitOptions.None)[1]);
        decimalPlaces = exponent;
      }

      return Math.Max(decimalPlaces, 1);
    }

    private void DisplayFinalResult()
    {
      string extremumType = "минимум";
      int decimalPlaces = GetDecimalPlacesFromEpsilon();
      string format = $"F{decimalPlaces}";

      view.ResultText = $"Найден {extremumType}:\n" +
                       $"x = {model.CalculationResult.Point.ToString(format, CultureInfo.InvariantCulture)}, " +
                       $"f(x) = {model.CalculationResult.Value.ToString(format, CultureInfo.InvariantCulture)}\n" +
                       $"Количество итераций: {model.IterationSteps.Count}";
    }

    private void UpdateStepPlot(NewtonIterationStep step, double currentPoint, double functionValue)
    {
      plotModel.Series.Clear();

      try
      {
        var functionSeries = new LineSeries
        {
          Title = "f(x)",
          Color = OxyColors.Blue,
          StrokeThickness = 2
        };

        int pointsCount = 100;
        double stepSize = (model.UpperBound - model.LowerBound) / pointsCount;

        for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
        {
          double currentX = model.LowerBound + pointIndex * stepSize;
          try
          {
            double currentY = model.EvaluateFunction(currentX);
            if (!double.IsInfinity(currentY) && !double.IsNaN(currentY))
            {
              functionSeries.Points.Add(new DataPoint(currentX, currentY));
            }
          }
          catch
          {
          }
        }

        plotModel.Series.Add(functionSeries);

        var tangentSeries = new LineSeries
        {
          Title = "Касательная",
          Color = OxyColors.Red,
          StrokeThickness = 1,
          LineStyle = LineStyle.Dash
        };

        double tangentSlope = step.FirstDerivative;
        double tangentIntercept = functionValue - tangentSlope * currentPoint;

        for (int pointIndex = 0; pointIndex <= pointsCount; pointIndex += 10)
        {
          double currentX = model.LowerBound + pointIndex * stepSize;
          double tangentY = tangentSlope * currentX + tangentIntercept;
          tangentSeries.Points.Add(new DataPoint(currentX, tangentY));
        }

        plotModel.Series.Add(tangentSeries);

        var currentPointSeries = new ScatterSeries
        {
          Title = "Текущая точка",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = OxyColors.Red
        };

        currentPointSeries.Points.Add(new ScatterPoint(currentPoint, functionValue));

        plotModel.Series.Add(currentPointSeries);

        UpdatePlotScale();

        plotModel.InvalidatePlot(true);
        view.SetPlotModel(plotModel);
      }
      catch (Exception ex)
      {
        view.ShowError($"Ошибка при построении графика: {ex.Message}");
      }
    }

    private void OnClearAllRequested(object sender, EventArgs e)
    {
      model.Reset();
      view.FunctionExpression = string.Empty;
      view.LowerBound = string.Empty;
      view.UpperBound = string.Empty;
      view.Epsilon = "0.001";
      view.ResultText = string.Empty;
      view.CurrentStep = 0;
      view.TotalSteps = 0;
      view.IsModeLocked = false;
      calculationSuccessful = false;
      view.ClearPlot();
    }

    private void OnHelpRequested(object sender, EventArgs e)
    {
      string helpMessage = "Метод Ньютона\n\n" +
                          "Принцип работы:\n" +
                          "- Использует первую и вторую производные для поиска экстремума\n" +
                          "- Итерационная формула: xₙ₊₁ = xₙ - f'(xₙ)/f''(xₙ)\n\n" +
                          "Ограничения метода:\n" +
                          "- Функция должна быть дважды дифференцируемой\n" +
                          "- Вторая производная не должна быть равна нулю\n" +
                          "- Начальное приближение должно быть близко к экстремуму\n\n" +
                          "Поддерживаемые функции:\n" +
                          "- Тригонометрические: sin(x), cos(x), tan(x)\n" +
                          "- Экспоненциальные: exp(x), e^x\n" +
                          "- Логарифмические: log(x) - натуральный, log10(x) - десятичный\n" +
                          "- Степенные: x^2, pow(x, y)\n" +
                          "- Квадратный корень: sqrt(x)\n" +
                          "- Модуль: abs(x)\n\n" +
                          "Инструкция:\n" +
                          "1. Введите функцию f(x)\n" +
                          "2. Укажите интервал поиска [A, B]\n" +
                          "3. Задайте точность ε\n" +
                          "4. Нажмите 'Вычислить'";

      view.ShowInfo(helpMessage);
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(view.FunctionExpression))
      {
        view.ShowError("Введите функцию для вычисления");
        return false;
      }

      if (!NewtonModel.TryParseDouble(view.LowerBound, out double lowerBound))
      {
        view.ShowError("Некорректное значение нижней границы интервала");
        return false;
      }

      if (!NewtonModel.TryParseDouble(view.UpperBound, out double upperBound))
      {
        view.ShowError("Некорректное значение верхней границы интервала");
        return false;
      }

      if (lowerBound >= upperBound)
      {
        view.ShowError("Нижняя граница должна быть меньше верхней");
        return false;
      }

      if (!NewtonModel.TryParseDouble(view.Epsilon, out double epsilon))
      {
        view.ShowError("Некорректное значение точности ε");
        return false;
      }

      if (epsilon <= 0)
      {
        view.ShowError("Точность ε должна быть положительным числом");
        return false;
      }

      return true;
    }

    private void UpdateModelFromView()
    {
      model.FunctionExpression = view.FunctionExpression;
      model.LowerBound = NewtonModel.ParseDouble(view.LowerBound);
      model.UpperBound = NewtonModel.ParseDouble(view.UpperBound);
      model.Epsilon = NewtonModel.ParseDouble(view.Epsilon);
      model.FindMinimum = true;
    }

    private void InitializePlot()
    {
      plotModel = new PlotModel
      {
        Title = "",
        TitleColor = OxyColors.Black,
        TitleFontSize = 14,
        TitleFontWeight = FontWeights.Bold
      };

      var xAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        TitleColor = OxyColors.Black,
        TitleFontSize = 12,
        AxislineColor = OxyColors.Black,
        TicklineColor = OxyColors.Black,
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Solid,
        MinorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        IsPanEnabled = true,
        IsZoomEnabled = true
      };

      var yAxis = new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "f(x)",
        TitleColor = OxyColors.Black,
        TitleFontSize = 12,
        AxislineColor = OxyColors.Black,
        TicklineColor = OxyColors.Black,
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Solid,
        MinorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        IsPanEnabled = true,
        IsZoomEnabled = true
      };

      plotModel.Axes.Add(xAxis);
      plotModel.Axes.Add(yAxis);

      view.SetPlotModel(plotModel);
    }

    public void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY)
    {
      try
      {
        plotModel.Series.Clear();

        var functionSeries = new LineSeries
        {
          Title = "f(x)",
          Color = OxyColors.Blue,
          StrokeThickness = 2
        };

        int pointsCount = 200;
        double stepSize = (upperBound - lowerBound) / pointsCount;

        for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
        {
          double currentX = lowerBound + pointIndex * stepSize;
          try
          {
            double currentY = model.EvaluateFunction(currentX);
            if (!double.IsInfinity(currentY) && !double.IsNaN(currentY))
            {
              functionSeries.Points.Add(new DataPoint(currentX, currentY));
            }
          }
          catch
          {
          }
        }

        plotModel.Series.Add(functionSeries);

        var extremumSeries = new ScatterSeries
        {
          Title = "Экстремум",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = OxyColors.Green
        };

        extremumSeries.Points.Add(new ScatterPoint(extremumX, extremumY));

        plotModel.Series.Add(extremumSeries);

        UpdatePlotScale();

        plotModel.InvalidatePlot(true);
        view.SetPlotModel(plotModel);
      }
      catch (Exception ex)
      {
        view.ShowError($"Ошибка при построении графика: {ex.Message}");
      }
    }

    private void UpdatePlotScale()
    {
      if (plotModel.Series.Count == 0) return;

      double minX = double.MaxValue;
      double maxX = double.MinValue;
      double minY = double.MaxValue;
      double maxY = double.MinValue;

      foreach (var series in plotModel.Series)
      {
        if (series is LineSeries lineSeries)
        {
          foreach (var point in lineSeries.Points)
          {
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);
          }
        }
        else if (series is ScatterSeries scatterSeries)
        {
          foreach (var point in scatterSeries.Points)
          {
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);
          }
        }
      }

      double rangeX = maxX - minX;
      double rangeY = maxY - minY;
      double maxRange = Math.Max(rangeX, rangeY);

      if (maxRange > 0)
      {
        double centerX = (minX + maxX) / 2;
        double centerY = (minY + maxY) / 2;

        var xAxis = plotModel.Axes[0] as LinearAxis;
        var yAxis = plotModel.Axes[1] as LinearAxis;

        if (xAxis != null && yAxis != null)
        {
          double padding = maxRange * 0.1;
          xAxis.Minimum = centerX - maxRange / 2 - padding;
          xAxis.Maximum = centerX + maxRange / 2 + padding;
          yAxis.Minimum = centerY - maxRange / 2 - padding;
          yAxis.Maximum = centerY + maxRange / 2 + padding;
        }
      }
    }

    public void ResetModel()
    {
      model.Reset();
      calculationSuccessful = false;
      view.IsModeLocked = false;
    }
  }
}
