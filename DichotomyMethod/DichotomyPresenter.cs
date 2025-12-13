using System;
using System.Linq;

namespace NumericalMethodsApp
{
  public class DichotomyPresenter
  {
    private readonly IDichotomyView _view;
    private readonly DichotomyModel _model;

    public DichotomyPresenter(IDichotomyView view)
    {
      _view = view;
      _model = new DichotomyModel();
    }

    public void CalculateRoots()
    {
      try
      {
        if (!ValidateInputs())
          return;

        string functionExpression = _view.FunctionExpression.Trim();
        double startInterval = _model.ParseDouble(_view.StartIntervalText);
        double endInterval = _model.ParseDouble(_view.EndIntervalText);
        double epsilon = _model.ParseDouble(_view.EpsilonText);

        if (startInterval >= endInterval)
        {
          _view.ShowError("Начало интервала (a) должно быть меньше конца интервала (b).");
          return;
        }

        if (epsilon <= 0)
        {
          _view.ShowError("Точность (ε) должна быть положительным числом.");
          return;
        }

        var discontinuityPoints = _model.FindDiscontinuityPoints(functionExpression, startInterval, endInterval);
        if (discontinuityPoints.Any())
        {
          int discontinuityPrecision = GetPrecisionFromEpsilon(epsilon);
          string pointsText = string.Join(", ", discontinuityPoints.Select(p => Math.Round(p, discontinuityPrecision)));
          _view.ShowWarning($"Обнаружены точки разрыва функции: {pointsText}\n\nРекомендуется изменить интервал, исключив эти точки.");
        }

        CalculationResult result = _model.FindRoot(functionExpression, startInterval, endInterval, epsilon);

        if (result.Success)
        {
          int precision = GetPrecisionFromEpsilon(epsilon);

          string resultText = $"Найден корень уравнения: x = {result.Root.ToString($"F{precision}")}\n" +
                             $"Значение функции: f(x) = {result.FunctionValue.ToString($"F{precision}")}";
          _view.SetResult(resultText);
          _view.PlotFunction(startInterval, endInterval, new double[] { result.Root });
        }
        else
        {
          _view.ShowWarning(result.Message);
          _view.SetResult("Корень не найден");
          _view.PlotFunction(startInterval, endInterval, new double[0]);
        }
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при вычислениях: {ex.Message}");
      }
    }

    private int GetPrecisionFromEpsilon(double epsilon)
    {
      if (epsilon <= 0) return 10;

      double logValue = -Math.Log10(epsilon);

      int precision = (int)Math.Round(logValue, MidpointRounding.AwayFromZero);

      return Math.Max(0, precision);
    }

    public double EvaluateFunction(string functionExpression, double xValue)
    {
      return _model.EvaluateFunction(functionExpression, xValue);
    }

    public void ClearAll()
    {
      _view.ClearPlot();
      _view.SetResult("");
      _view.ClearInputs();
    }

    public void ShowHelp()
    {
      string helpText = @"Метод дихотомии

Метод дихотомии (половинного деления) используется для поиска корней уравнения на заданном интервале.

Условия применения:
• Функция должна быть непрерывной на [a,b]
• f(a) * f(b) < 0 (функция имеет разные знаки на концах интервала)

Параметры:
• Функция f(x): математическое выражение
• Начало интервала (a): левая граница интервала поиска
• Конец интервала (b): правая граница интервала поиска  
• Точность (ε): желаемая точность нахождения корня

Доступные функции:
• Основные операторы: +, -, *, /, ^
• Тригонометрические: sin(x), cos(x), tan(x)
• Экспоненциальные: exp(x), log(x), log10(x)
• Другие: sqrt(x), abs(x), pow(x,y)";

      _view.ShowInformation(helpText);
    }

    private bool ValidateInputs()
    {
      if (string.IsNullOrWhiteSpace(_view.FunctionExpression))
      {
        _view.ShowError("Введите функцию f(x).");
        _view.FocusFunctionTextBox();
        return false;
      }

      string cleanFunction = _view.FunctionExpression.Trim();
      if (cleanFunction.Length < 2)
      {
        _view.ShowError("Функция должна содержать переменную x и математические операторы.");
        _view.FocusFunctionTextBox();
        return false;
      }

      if (!cleanFunction.Contains("x"))
      {
        _view.ShowError("Функция должна содержать переменную x.");
        _view.FocusFunctionTextBox();
        return false;
      }

      try
      {
        if (_model.IsConstantExpression(cleanFunction))
        {
          _view.ShowError("Функция должна действительно зависеть от переменной x. Проверьте правильность ввода.");
          _view.FocusFunctionTextBox();
          return false;
        }
      }
      catch
      {
      }

      if (!_model.IsValidNumber(_view.StartIntervalText))
      {
        _view.ShowError("Введите корректное число для начала интервала (a).");
        _view.FocusStartIntervalTextBox();
        return false;
      }

      if (!_model.IsValidNumber(_view.EndIntervalText))
      {
        _view.ShowError("Введите корректное число для конца интервала (b).");
        _view.FocusEndIntervalTextBox();
        return false;
      }

      if (!_model.IsValidNumber(_view.EpsilonText) || _model.ParseDouble(_view.EpsilonText) <= 0)
      {
        _view.ShowError("Введите корректное положительное число для точности (ε).");
        _view.FocusEpsilonTextBox();
        return false;
      }

      double startInterval = _model.ParseDouble(_view.StartIntervalText);
      double endInterval = _model.ParseDouble(_view.EndIntervalText);

      try
      {
        _model.EvaluateFunction(cleanFunction, startInterval);
        _model.EvaluateFunction(cleanFunction, endInterval);
        _model.EvaluateFunction(cleanFunction, (startInterval + endInterval) / 2);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка вычисления функции: {ex.Message}\n\nВыберите другой интервал, исключающий точки разрыва функции.");
        _view.FocusFunctionTextBox();
        return false;
      }

      return true;
    }
  }
}
