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
    }

    private void OnNextStepRequested(object sender, EventArgs e)
    {
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

    private void SetCalculationInProgress(bool inProgress)
    {
      _view.CalculationInProgress = inProgress;
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
