using System;
using NumericalMethodsApp.Models;
using NumericalMethodsApp.Views;

namespace NumericalMethodsApp.Presenters
{
  public class GoldenRatioPresenter
  {
    private readonly IGoldenRatioView _view;
    private readonly GoldenRatioModel _model;

    public GoldenRatioPresenter(IGoldenRatioView view)
    {
      _view = view;
      _model = new GoldenRatioModel();
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
  }
}
