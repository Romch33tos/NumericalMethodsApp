using System;
using NumericalMethodsApp.Models;
using NumericalMethodsApp.Views;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace NumericalMethodsApp.Presenters
{
  public class NewtonPresenter
  {
    private readonly INewtonView _view;
    private readonly NewtonModel _model;
    private PlotModel _plotModel;

    public NewtonPresenter(INewtonView view)
    {
      _view = view;
      _model = new NewtonModel();
      InitializePlot();
      SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
      _view.CalculateRequested += OnCalculateRequested;
      _view.ClearAllRequested += OnClearAllRequested;
      _view.HelpRequested += OnHelpRequested;
      _view.ModeChanged += OnModeChanged;
      _view.NextStepRequested += OnNextStepRequested;
    }

    private void OnCalculateRequested(object sender, EventArgs e)
    {
      try
      {
        if (!ValidateInput())
          return;

        SetCalculationInProgress(true);
        _view.SetStepModeActive(false);

        UpdateModelFromView();
        _model.Calculate();

        DisplayResult();
        UpdatePlotForCurrentStep();

        SetCalculationInProgress(false);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при вычислении: {ex.Message}");
        SetCalculationInProgress(false);
      }
    }

    private void OnNextStepRequested(object sender, EventArgs e)
    {
      try
      {
        if (!ValidateInput())
          return;

        if (!_model.StepModeStarted)
        {
          SetCalculationInProgress(true);
          _view.SetStepModeActive(true);
          UpdateModelFromView();
        }

        bool hasNextStep = _model.PerformNextStep();

        if (hasNextStep)
        {
          DisplayStepResult();
          UpdatePlotForCurrentStep();
        }
        else
        {
          if (_model.CalculationComplete)
          {
            DisplayResult();
            SetCalculationInProgress(false);
            _view.SetStepModeActive(false);
          }
        }
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при выполнении шага: {ex.Message}");
        SetCalculationInProgress(false);
        _view.SetStepModeActive(false);
      }
    }

    private void OnClearAllRequested(object sender, EventArgs e)
    {
      _view.FunctionExpression = "";
      _view.InitialPoint = "";
      _view.DisplayIntervalStart = "-5";
      _view.DisplayIntervalEnd = "5";
      _view.Epsilon = "0.001";
      _view.FindMinimum = true;
      _view.FindMaximum = false;
      _view.ResultText = "";

      _model.ResetCalculation();
      SetCalculationInProgress(false);
      _view.SetStepModeActive(false);
      ClearPlot();
    }

    private void OnHelpRequested(object sender, EventArgs e)
    {
      string helpText = @"Метод Ньютона для оптимизации

Инструкция:
1. Введите функцию f(x) в поле 'Функция f(x)'
2. Укажите начальное приближение x₀
3. Задайте интервал отображения графика
4. Задайте точность вычисления ε
5. Выберите режим поиска (минимум или максимум)
6. Нажмите 'Вычислить полностью' для полного расчета
7. Используйте 'Следующий шаг' для пошагового просмотра

Примечание: 
- Метод Ньютона ищет точки, где первая производная равна нулю
- Тип экстремума определяется по знаку второй производной
- Для пошагового режима начните с кнопки 'Следующий шаг'
- После начала пошагового режима кнопка 'Вычислить полностью' блокируется
- Для нового расчета используйте 'Очистить все'";

      _view.ShowInfo(helpText);
    }

    private void OnModeChanged(object sender, EventArgs e)
    {
      _model.ResetCalculation();
      SetCalculationInProgress(false);
      _view.SetStepModeActive(false);
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(_view.FunctionExpression))
      {
        _view.ShowError("Введите функцию f(x)");
        return false;
      }

      if (_model.IsConstantFunction(_view.FunctionExpression))
      {
        _view.ShowError("Функция должна зависеть от переменной x. Введите выражение с переменной x.");
        return false;
      }

      if (!NewtonModel.TryParseDouble(_view.InitialPoint, out double initialPoint))
      {
        _view.ShowError("Начальная точка должна быть вещественным числом");
        return false;
      }

      if (!NewtonModel.TryParseDouble(_view.Epsilon, out double epsilonValue) || epsilonValue <= 0)
      {
        _view.ShowError("Точность ε должна быть положительным вещественным числом");
        return false;
      }

      if (!NewtonModel.TryParseDouble(_view.DisplayIntervalStart, out double intervalStart) ||
          !NewtonModel.TryParseDouble(_view.DisplayIntervalEnd, out double intervalEnd))
      {
        _view.ShowError("Интервал отображения должен состоять из двух вещественных чисел");
        return false;
      }

      if (intervalStart >= intervalEnd)
      {
        _view.ShowError("Начало интервала должно быть меньше его конца");
        return false;
      }

      try
      {
        _model.FunctionExpression = _view.FunctionExpression;
        _model.InitialPoint = initialPoint;
        double testValue = _model.EvaluateFunction(initialPoint);
        if (double.IsInfinity(testValue) || double.IsNaN(testValue))
        {
          _view.ShowError("Функция не определена в начальной точке");
          return false;
        }
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка в функции: {ex.Message}");
        return false;
      }

      return true;
    }

    private void UpdateModelFromView()
    {
      _model.FunctionExpression = _view.FunctionExpression;
      _model.InitialPoint = NewtonModel.ParseDouble(_view.InitialPoint);
      _model.Epsilon = NewtonModel.ParseDouble(_view.Epsilon);
      _model.FindMinimum = _view.FindMinimum;
      _model.FindMaximum = _view.FindMaximum;
    }

    private void DisplayResult()
    {
      if (_model.CalculationResult.Success)
      {
        string extremumType = _model.CalculationResult.IsMinimum ? "минимум" : "максимум";
        _view.ResultText = $"Найден {extremumType} функции: x = {_model.CalculationResult.Point:F6}, f(x) = {_model.CalculationResult.Value:F6}";
      }
      else
      {
        double firstDeriv = _model.CalculateFirstDerivative(_model.CalculationResult.Point);
        double secondDeriv = _model.CalculateSecondDerivative(_model.CalculationResult.Point);

        if (Math.Abs(firstDeriv) < _model.Epsilon)
        {
          if (Math.Abs(secondDeriv) < 1e-10)
          {
            _view.ResultText = $"Найдена точка перегиба: x = {_model.CalculationResult.Point:F6}, f(x) = {_model.CalculationResult.Value:F6}";
          }
          else if (secondDeriv > 0)
          {
            _view.ResultText = $"Найден минимум: x = {_model.CalculationResult.Point:F6}, f(x) = {_model.CalculationResult.Value:F6}";
          }
          else
          {
            _view.ResultText = $"Найден максимум: x = {_model.CalculationResult.Point:F6}, f(x) = {_model.CalculationResult.Value:F6}";
          }
        }
        else
        {
          _view.ResultText = $"Метод сошелся к точке: x = {_model.CalculationResult.Point:F6}, f(x) = {_model.CalculationResult.Value:F6}, но f'(x) = {firstDeriv:F6} ≠ 0";
        }
      }
    }

    private void DisplayStepResult()
    {
      if (_model.CalculationComplete && _model.CalculationResult.Success)
      {
        string extremumType = _model.CalculationResult.IsMinimum ? "минимум" : "максимум";
        _view.ResultText = $"Найден {extremumType} функции: x = {_model.CalculationResult.Point:F6}, f(x) = {_model.CalculationResult.Value:F6}";
      }
      else if (_model.CurrentStepIndex >= 0 && _model.CurrentStepIndex < _model.IterationSteps.Count)
      {
        IterationStep currentStep = _model.IterationSteps[_model.CurrentStepIndex];

        string pointType = currentStep.SecondDerivative > 0 ? "минимум" :
                          currentStep.SecondDerivative < 0 ? "максимум" : "точка перегиба";

        _view.ResultText = $"Шаг {currentStep.IterationNumber}: " +
                          $"x = {currentStep.Point:F6}, " +
                          $"f(x) = {currentStep.FunctionValue:F6}, " +
                          $"f'(x) = {currentStep.FirstDerivative:F6}, " +
                          $"тип: {pointType}";
      }
      else
      {
        double secondDeriv = _model.CalculateSecondDerivative(_model.InitialPoint);
        string pointType = secondDeriv > 0 ? "минимум" :
                          secondDeriv < 0 ? "максимум" : "точка перегиба";

        _view.ResultText = $"Начальная точка: " +
                          $"x = {_model.InitialPoint:F6}, " +
                          $"f(x) = {_model.EvaluateFunction(_model.InitialPoint):F6}, " +
                          $"тип: {pointType}";
      }
    }

    private void SetCalculationInProgress(bool inProgress)
    {
      _view.CalculationInProgress = inProgress;
    }

    private void UpdatePlotForCurrentStep()
    {
      try
      {
        double plotLowerBound = NewtonModel.TryParseDouble(_view.DisplayIntervalStart, out double start) ? start : -5;
        double plotUpperBound = NewtonModel.TryParseDouble(_view.DisplayIntervalEnd, out double end) ? end : 5;

        double currentX = _model.CalculationResult.Point;
        double currentY = _model.CalculationResult.Value;

        UpdatePlot(plotLowerBound, plotUpperBound, currentX, currentY, _model.CalculationResult.IsMinimum);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при обновлении графика: {ex.Message}");
      }
    }

    private void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum)
    {
      try
      {
        _plotModel.Series.Clear();

        LineSeries functionSeries = new LineSeries
        {
          Color = OxyColors.Blue,
          StrokeThickness = 2,
          Title = "f(x)"
        };

        int pointCount = 200;
        for (int pointIndex = 0; pointIndex <= pointCount; ++pointIndex)
        {
          double xValue = lowerBound + (upperBound - lowerBound) * pointIndex / pointCount;
          try
          {
            double yValue = _model.EvaluateFunction(xValue);
            if (!double.IsInfinity(yValue) && !double.IsNaN(yValue))
            {
              functionSeries.Points.Add(new DataPoint(xValue, yValue));
            }
          }
          catch
          {
          }
        }

        _plotModel.Series.Add(functionSeries);

        if (!double.IsInfinity(extremumX) && !double.IsNaN(extremumX) &&
            !double.IsInfinity(extremumY) && !double.IsNaN(extremumY))
        {
          ScatterSeries extremumSeries = new ScatterSeries
          {
            MarkerType = MarkerType.Circle,
            MarkerSize = 8,
            MarkerStroke = OxyColors.Black,
            MarkerStrokeThickness = 2,
            MarkerFill = isMinimum ? OxyColors.Green : OxyColors.Red,
            Title = isMinimum ? "Текущая точка (минимум)" : "Текущая точка (максимум)"
          };
          extremumSeries.Points.Add(new ScatterPoint(extremumX, extremumY));
          _plotModel.Series.Add(extremumSeries);
        }

        _plotModel.InvalidatePlot(true);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при обновлении графика: {ex.Message}");
      }
    }

    private void InitializePlot()
    {
      _plotModel = new PlotModel
      {
        Title = "",
        Background = OxyColors.White,
        PlotAreaBorderColor = OxyColors.LightGray,
        PlotAreaBorderThickness = new OxyThickness(1)
      };

      _plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        TitleColor = OxyColors.Black,
        TitleFontSize = 12,
        AxislineColor = OxyColors.Black,
        TicklineColor = OxyColors.LightGray,
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash,
        MinorGridlineColor = OxyColors.LightGray,
        MinorGridlineStyle = LineStyle.Dot,
        FontSize = 10
      });

      _plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "f(x)",
        TitleColor = OxyColors.Black,
        TitleFontSize = 12,
        AxislineColor = OxyColors.Black,
        TicklineColor = OxyColors.LightGray,
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash,
        MinorGridlineColor = OxyColors.LightGray,
        MinorGridlineStyle = LineStyle.Dot,
        FontSize = 10
      });

      if (_view is NewtonMethod newtonView)
      {
        newtonView.PlotViewControl.Model = _plotModel;
      }
    }

    private void ClearPlot()
    {
      _plotModel.Series.Clear();
      _plotModel.InvalidatePlot(true);
    }
  }
}
