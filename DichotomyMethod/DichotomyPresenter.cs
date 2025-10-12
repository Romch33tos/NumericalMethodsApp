using System;
using System.Globalization;
using NCalc;

namespace NumericalMethodsApp
{
  public class DichotomyPresenter
  {
    private readonly IDichotomyView _view;
    private const int MaxIterationsCount = 1000;

    public DichotomyPresenter(IDichotomyView view)
    {
      _view = view;
    }

    public void CalculateRoots()
    {
      try
      {
        if (!ValidateInputs())
          return;

        string functionExpression = _view.FunctionExpression.Trim();
        double startInterval = ParseDouble(_view.StartIntervalText);
        double endInterval = ParseDouble(_view.EndIntervalText);
        double epsilon = ParseDouble(_view.EpsilonText);

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

        double functionAtStart = EvaluateFunction(functionExpression, startInterval);
        double functionAtEnd = EvaluateFunction(functionExpression, endInterval);

        if (Math.Abs(functionAtStart) < epsilon)
        {
          ShowSingleRootResult(functionExpression, startInterval, functionAtStart);
          return;
        }

        if (Math.Abs(functionAtEnd) < epsilon)
        {
          ShowSingleRootResult(functionExpression, endInterval, functionAtEnd);
          return;
        }

        if (functionAtStart * functionAtEnd > 0)
        {
          double? possibleRoot = FindPossibleRoot(functionExpression, startInterval, endInterval, epsilon);

          if (possibleRoot.HasValue)
          {
            double functionValueAtRoot = EvaluateFunction(functionExpression, possibleRoot.Value);
            ShowSingleRootResult(functionExpression, possibleRoot.Value, functionValueAtRoot);
            return;
          }

          string resultText = "На заданном интервале корней нет.";
          _view.SetResult(resultText);
          _view.PlotFunction(startInterval, endInterval, new double[0]);
          return;
        }

        double root = FindRootByDichotomy(functionExpression, startInterval, endInterval, epsilon);
        double functionValueAtFoundRoot = EvaluateFunction(functionExpression, root);

        ShowSingleRootResult(functionExpression, root, functionValueAtFoundRoot);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при вычислениях: {ex.Message}");
      }
    }

    private double? FindPossibleRoot(string functionExpression, double startInterval, double endInterval, double epsilon)
    {
      try
      {
        int checkPoints = 100;
        double step = (endInterval - startInterval) / (checkPoints + 1);

        double previousValue = EvaluateFunction(functionExpression, startInterval);

        for (int pointIndex = 1; pointIndex <= checkPoints; ++pointIndex)
        {
          double currentX = startInterval + pointIndex * step;
          double currentValue;

          try
          {
            currentValue = EvaluateFunction(functionExpression, currentX);
          }
          catch
          {
            continue;
          }

          if (Math.Abs(currentValue) < epsilon * 10)
          {
            return currentX;
          }

          if (previousValue * currentValue < 0)
          {
            double subStart = startInterval + (pointIndex - 1) * step;
            double subEnd = currentX;

            try
            {
              return FindRootByDichotomy(functionExpression, subStart, subEnd, epsilon);
            }
            catch
            {
            }
          }

          previousValue = currentValue;
        }

        return null;
      }
      catch
      {
        return null;
      }
    }

    private double FindRootByDichotomy(string functionExpression, double startInterval, double endInterval, double epsilon)
    {
      double functionAtStart = EvaluateFunction(functionExpression, startInterval);
      double functionAtEnd = EvaluateFunction(functionExpression, endInterval);
      int iterationCount = 0;

      double currentStart = startInterval;
      double currentEnd = endInterval;
      double functionAtCurrentStart = functionAtStart;

      while (Math.Abs(currentEnd - currentStart) > epsilon && iterationCount < MaxIterationsCount)
      {
        double midpoint = (currentStart + currentEnd) / 2;
        double functionAtMidpoint;

        try
        {
          functionAtMidpoint = EvaluateFunction(functionExpression, midpoint);
        }
        catch
        {
          midpoint = (currentStart + currentEnd * 0.99) / 2;
          functionAtMidpoint = EvaluateFunction(functionExpression, midpoint);
        }

        if (Math.Abs(functionAtMidpoint) < epsilon)
          return midpoint;

        if (functionAtCurrentStart * functionAtMidpoint < 0)
        {
          currentEnd = midpoint;
          functionAtEnd = functionAtMidpoint;
        }
        else
        {
          currentStart = midpoint;
          functionAtCurrentStart = functionAtMidpoint;
        }

        ++iterationCount;
      }

      return (currentStart + currentEnd) / 2;
    }

    private void ShowSingleRootResult(string functionExpression, double root, double functionValue)
    {
      string resultText = $"Найден корень уравнения: x = {Math.Round(root, 3)}";
      _view.SetResult(resultText);

      double startInterval = ParseDouble(_view.StartIntervalText);
      double endInterval = ParseDouble(_view.EndIntervalText);
      _view.PlotFunction(startInterval, endInterval, new double[] { root });
    }

    public void ClearAll()
    {
      _view.ClearPlot();
      _view.SetResult("Ответ: ");
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

Результаты:
• Единственный корень: отображается значение корня
• Нет корней: информация о значениях функции на концах интервала

Доступные функции:
• Основные операторы: +, -, *, /, ^
• Тригонометрические: sin(x), cos(x), tan(x)
• Экспоненциальные: exp(x), log(x), log10(x)
• Другие: sqrt(x), abs(x), pow(x,y)

Примеры:
• x^2 - 4
• sin(x)
• exp(x) - 2
• 1/x + 1

Примечание: для функций с разрывами (например, 1/x) выбирайте интервал, не включающий точку разрыва.";

      _view.ShowInformation(helpText);
    }

    public double EvaluateFunction(string functionExpression, double xValue)
    {
      try
      {
        string expression = functionExpression.Trim();
        expression = expression.Replace(',', '.');

        expression = System.Text.RegularExpressions.Regex.Replace(
          expression,
          @"(\w+)\s*\^\s*(\w+)",
          "pow($1, $2)"
        );

        if (Math.Abs(xValue) < 1e-15)
        {
          if (expression.Contains("/x") || expression.Contains("/ x") ||
              expression.Contains("/  x") || expression.Contains("/  x"))
          {
            throw new ArgumentException("Деление на ноль");
          }
        }

        NCalc.Expression ncalcExpression = new NCalc.Expression(expression);
        ncalcExpression.Parameters["x"] = xValue;

        ncalcExpression.EvaluateParameter += delegate (string name, ParameterArgs args)
        {
          if (name == "x")
          {
            args.Result = xValue;
          }
        };

        ncalcExpression.EvaluateFunction += delegate (string name, FunctionArgs args)
        {
          try
          {
            if (name.Equals("sin", StringComparison.OrdinalIgnoreCase))
              args.Result = Math.Sin(Convert.ToDouble(args.Parameters[0].Evaluate()));
            else if (name.Equals("cos", StringComparison.OrdinalIgnoreCase))
              args.Result = Math.Cos(Convert.ToDouble(args.Parameters[0].Evaluate()));
            else if (name.Equals("tan", StringComparison.OrdinalIgnoreCase))
              args.Result = Math.Tan(Convert.ToDouble(args.Parameters[0].Evaluate()));
            else if (name.Equals("exp", StringComparison.OrdinalIgnoreCase))
              args.Result = Math.Exp(Convert.ToDouble(args.Parameters[0].Evaluate()));
            else if (name.Equals("log", StringComparison.OrdinalIgnoreCase))
            {
              double argument = Convert.ToDouble(args.Parameters[0].Evaluate());
              if (argument <= 0) throw new ArgumentException("Логарифм от неположительного числа");
              args.Result = Math.Log(argument);
            }
            else if (name.Equals("log10", StringComparison.OrdinalIgnoreCase))
            {
              double argument = Convert.ToDouble(args.Parameters[0].Evaluate());
              if (argument <= 0) throw new ArgumentException("Логарифм от неположительного числа");
              args.Result = Math.Log10(argument);
            }
            else if (name.Equals("sqrt", StringComparison.OrdinalIgnoreCase))
            {
              double argument = Convert.ToDouble(args.Parameters[0].Evaluate());
              if (argument < 0) throw new ArgumentException("Квадратный корень от отрицательного числа");
              args.Result = Math.Sqrt(argument);
            }
            else if (name.Equals("abs", StringComparison.OrdinalIgnoreCase))
              args.Result = Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate()));
            else if (name.Equals("pow", StringComparison.OrdinalIgnoreCase))
            {
              double baseValue = Convert.ToDouble(args.Parameters[0].Evaluate());
              double exponent = Convert.ToDouble(args.Parameters[1].Evaluate());
              args.Result = Math.Pow(baseValue, exponent);
            }
          }
          catch (Exception ex)
          {
            throw new ArgumentException($"Ошибка вычисления функции {name}: {ex.Message}");
          }
        };

        object result = ncalcExpression.Evaluate();

        if (result == null)
          throw new ArgumentException("Не удалось вычислить выражение");

        double value = Convert.ToDouble(result);

        if (double.IsInfinity(value) || double.IsNaN(value))
          throw new ArgumentException("Функция не определена в данной точке");

        return value;
      }
      catch (Exception ex)
      {
        throw new ArgumentException($"Невозможно вычислить функцию '{functionExpression}' при x={xValue}: {ex.Message}");
      }
    }

    private bool ValidateInputs()
    {
      if (string.IsNullOrWhiteSpace(_view.FunctionExpression))
      {
        _view.ShowError("Введите функцию f(x).");
        _view.FocusFunctionTextBox();
        return false;
      }

      if (!IsValidNumber(_view.StartIntervalText))
      {
        _view.ShowError("Введите корректное число для начала интервала (a).");
        _view.FocusStartIntervalTextBox();
        return false;
      }

      if (!IsValidNumber(_view.EndIntervalText))
      {
        _view.ShowError("Введите корректное число для конца интервала (b).");
        _view.FocusEndIntervalTextBox();
        return false;
      }

      if (!IsValidNumber(_view.EpsilonText) || ParseDouble(_view.EpsilonText) <= 0)
      {
        _view.ShowError("Введите корректное положительное число для точности (ε).");
        _view.FocusEpsilonTextBox();
        return false;
      }

      try
      {
        double startInterval = ParseDouble(_view.StartIntervalText);
        double endInterval = ParseDouble(_view.EndIntervalText);

        EvaluateFunction(_view.FunctionExpression, startInterval);
        EvaluateFunction(_view.FunctionExpression, endInterval);

        EvaluateFunction(_view.FunctionExpression, (startInterval + endInterval) / 4);
        EvaluateFunction(_view.FunctionExpression, (startInterval + endInterval) / 2);
        EvaluateFunction(_view.FunctionExpression, (3 * startInterval + endInterval) / 4);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка вычисления функции: {ex.Message}\n\nВыберите другой интервал, исключающий точки разрыва функции.");
        _view.FocusFunctionTextBox();
        return false;
      }

      return true;
    }

    private double ParseDouble(string text)
    {
      text = text.Replace(',', '.');
      if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
      {
        return result;
      }
      throw new ArgumentException("Некорректное числовое значение");
    }

    private bool IsValidNumber(string text)
    {
      try
      {
        ParseDouble(text);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
