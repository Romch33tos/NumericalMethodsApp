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
      view.ModeChanged += OnModeChanged;
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
            model.CalculationResult.Point, model.CalculationResult.Value,
            model.FindMinimum);
      }
      catch (Exception ex)
      {
        calculationSuccessful = false;
        view.IsModeLocked = false;
        view.ShowError($"Ошибка при вычислении: {ex.Message}\nРекомендация: попробуйте изменить интервал поиска.");
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
              model.CalculationResult.Point, model.CalculationResult.Value,
              model.FindMinimum);
        }
        else
        {
          var step = model.IterationSteps[stepIndex];
          double currentEstimate = step.CurrentEstimate;
          double functionValue = model.EvaluateFunction(currentEstimate);

          UpdateStepPlot(step, currentEstimate, functionValue);

          string extremumType = model.FindMinimum ? "Минимум" : "Максимум";
          view.ResultText = $"Шаг {step.Iteration}:\n" +
                           $"Интервал: [{Math.Round(step.LowerBound, 5)}, {Math.Round(step.UpperBound, 5)}]\n" +
                           $"Длина интервала: {Math.Round(step.IntervalLength, 5)}\n" +
                           $"{extremumType}: x = {Math.Round(currentEstimate, 5)}";
        }
      }
    }

    private void DisplayFinalResult()
    {
      string extremumType = model.FindMinimum ? "минимум" : "максимум";
      view.ResultText = $"Найден {extremumType}:\n" +
                       $"x = {Math.Round(model.CalculationResult.Point, 5)}, " +
                       $"f(x) = {Math.Round(model.CalculationResult.Value, 5)}\n" +
                       $"Количество итераций: {model.IterationSteps.Count}";
    }

    private void UpdateStepPlot(NewtonIterationStep step, double currentEstimate, double functionValue)
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

        var intervalSeries = new LineSeries
        {
          Title = "Текущий интервал",
          Color = OxyColors.Gray,
          StrokeThickness = 3
        };

        double minY = double.MaxValue;
        double maxY = double.MinValue;

        for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
        {
          double currentX = model.LowerBound + pointIndex * stepSize;
          try
          {
            double currentY = model.EvaluateFunction(currentX);
            if (!double.IsInfinity(currentY) && !double.IsNaN(currentY))
            {
              minY = Math.Min(minY, currentY);
              maxY = Math.Max(maxY, currentY);
            }
          }
          catch
          {
          }
        }

        double intervalY = minY - 0.1 * (maxY - minY);
        intervalSeries.Points.Add(new DataPoint(step.LowerBound, intervalY));
        intervalSeries.Points.Add(new DataPoint(step.UpperBound, intervalY));

        plotModel.Series.Add(intervalSeries);

        var estimateSeries = new ScatterSeries
        {
          Title = "Текущая оценка",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = OxyColors.Red
        };

        estimateSeries.Points.Add(new ScatterPoint(currentEstimate, functionValue));

        plotModel.Series.Add(estimateSeries);

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
                          "4. Выберите тип экстремума\n" +
                          "5. Нажмите 'Вычислить'";

      view.ShowInfo(helpMessage);
    }

    private void OnModeChanged(object sender, EventArgs e)
    {
      if (!calculationSuccessful)
      {
        view.ClearPlot();
        view.ResultText = string.Empty;
        view.CurrentStep = 0;
        view.TotalSteps = 0;
      }
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

      if (!view.FindMinimum && !view.FindMaximum)
      {
        view.ShowError("Выберите тип экстремума для поиска");
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
      model.FindMinimum = view.FindMinimum;
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
        MajorGridlineStyle = LineStyle.Dash
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
        MajorGridlineStyle = LineStyle.Dash
      };

      plotModel.Axes.Add(xAxis);
      plotModel.Axes.Add(yAxis);

      view.SetPlotModel(plotModel);
    }

    public void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum)
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
          Title = isMinimum ? "Минимум" : "Максимум",
          MarkerType = MarkerType.Circle,
          MarkerSize = 8,
          MarkerFill = isMinimum ? OxyColors.Green : OxyColors.Red
        };

        extremumSeries.Points.Add(new ScatterPoint(extremumX, extremumY));

        plotModel.Series.Add(extremumSeries);

        plotModel.InvalidatePlot(true);
        view.SetPlotModel(plotModel);
      }
      catch (Exception ex)
      {
        view.ShowError($"Ошибка при построении графика: {ex.Message}");
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
