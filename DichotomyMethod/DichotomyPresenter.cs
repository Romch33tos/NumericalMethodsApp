using System;
using System.Collections.Generic;
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
        double intervalStart = ParseDouble(_view.StartIntervalText);
        double intervalEnd = ParseDouble(_view.EndIntervalText);
        double epsilon = ParseDouble(_view.EpsilonText);

        if (intervalStart >= intervalEnd)
        {
          _view.ShowError("Начало интервала (a) должно быть меньше конца интервала (b).");
          return;
        }

        if (epsilon <= 0)
        {
          _view.ShowError("Точность (ε) должна быть положительным числом.");
          return;
        }

        List<double> roots = FindAllRoots(functionExpression, intervalStart, intervalEnd, epsilon);

        if (roots.Count == 0)
        {
          _view.SetResult("Корней на заданном интервале не найдено.");
          _view.PlotFunction(intervalStart, intervalEnd, roots.ToArray());
        }
        else
        {
          string resultText = $"Найдено корней: {roots.Count}\n";
          for (int i = 0; i < roots.Count; i++)
          {
            resultText += $"Корень {i + 1}: x = {roots[i]:F6}\n";
          }

          _view.SetResult(resultText);
          _view.PlotFunction(intervalStart, intervalEnd, roots.ToArray());
        }
      }
      catch (Exception ex)
      {
        _view.ShowError($"Ошибка при вычислениях: {ex.Message}");
      }
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

Параметры:
• Функция f(x): математическое выражение
• Начало интервала (a): левая граница интервала поиска
• Конец интервала (b): правая граница интервала поиска
• Точность (ε): желаемая точность нахождения корня

Доступные функции:
• Основные операторы: +, -, *, /
• Тригонометрические: sin(x), cos(x), tan(x)
• Экспоненциальные: exp(x), log(x), log10(x)
• Другие: sqrt(x), abs(x), pow(x,y)

Инструкция:
1. Введите функцию и параметры
2. Используйте Tab для перехода между полями
3. Нажмите Вычислить/Enter для ввода данных
4. Для очистки полей используйте кнопку Очистить все";

      _view.ShowInformation(helpText);
    }

    public double EvaluateFunction(string functionExpression, double xValue)
    {
      try
      {
        string expression = functionExpression.Trim();

        NCalc.Expression ncalcExpression = new NCalc.Expression(expression);
        ncalcExpression.Parameters["x"] = xValue;

        ncalcExpression.EvaluateFunction += delegate (string name, FunctionArgs args)
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
            args.Result = Math.Log(Convert.ToDouble(args.Parameters[0].Evaluate()));
          else if (name.Equals("log10", StringComparison.OrdinalIgnoreCase))
            args.Result = Math.Log10(Convert.ToDouble(args.Parameters[0].Evaluate()));
          else if (name.Equals("sqrt", StringComparison.OrdinalIgnoreCase))
            args.Result = Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate()));
          else if (name.Equals("abs", StringComparison.OrdinalIgnoreCase))
            args.Result = Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate()));
          else if (name.Equals("pow", StringComparison.OrdinalIgnoreCase))
            args.Result = Math.Pow(Convert.ToDouble(args.Parameters[0].Evaluate()),
                                  Convert.ToDouble(args.Parameters[1].Evaluate()));
        };

        object result = ncalcExpression.Evaluate();

        if (result == null)
          throw new ArgumentException("Не удалось вычислить выражение");

        return Convert.ToDouble(result);
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
        double intervalStart = ParseDouble(_view.StartIntervalText);
        double intervalEnd = ParseDouble(_view.EndIntervalText);
        double testValue1 = intervalStart;
        double testValue2 = (intervalStart + intervalEnd) / 2;
        double testValue3 = intervalEnd;

        EvaluateFunction(_view.FunctionExpression, testValue1);
        EvaluateFunction(_view.FunctionExpression, testValue2);
        EvaluateFunction(_view.FunctionExpression, testValue3);
      }
      catch (Exception ex)
      {
        _view.ShowError($"Некорректный синтаксис функции: {ex.Message}\n\nИспользуйте доступные математические функции: sin(x), cos(x), exp(x), log(x), sqrt(x), abs(x), pow(x,y) и др.");
        _view.FocusFunctionTextBox();
        return false;
      }

      return true;
    }

    private List<double> FindAllRoots(string functionExpression, double intervalStart, double intervalEnd, double epsilon)
    {
      List<double> roots = new List<double>();

      int segmentsCount = 1000;
      double stepSize = (intervalEnd - intervalStart) / segmentsCount;

      double[] functionValues = new double[segmentsCount + 1];
      double[] xValues = new double[segmentsCount + 1];

      for (int i = 0; i <= segmentsCount; i++)
      {
        xValues[i] = intervalStart + i * stepSize;
        functionValues[i] = EvaluateFunction(functionExpression, xValues[i]);
      }

      for (int i = 0; i <= segmentsCount; i++)
      {
        double x = xValues[i];
        double functionValue = functionValues[i];

        if (Math.Abs(functionValue) < epsilon)
        {
          if (!IsRootAlreadyFound(roots, x, epsilon * 10))
          {
            roots.Add(x);
            i += (int)(segmentsCount * 0.01);
          }
          continue;
        }

        if (i < segmentsCount)
        {
          double nextX = xValues[i + 1];
          double nextFunctionValue = functionValues[i + 1];

          if (functionValue * nextFunctionValue < 0)
          {
            double root = FindSingleRoot(functionExpression, x, nextX, epsilon);
            if (!IsRootAlreadyFound(roots, root, epsilon * 10))
            {
              roots.Add(root);
              i += (int)(segmentsCount * 0.01);
            }
          }
        }
      }

      roots.Sort();
      return roots;
    }

    private bool IsRootAlreadyFound(List<double> roots, double root, double tolerance)
    {
      foreach (double existingRoot in roots)
      {
        if (Math.Abs(existingRoot - root) < tolerance)
          return true;
      }
      return false;
    }

    private double FindSingleRoot(string functionExpression, double intervalStart, double intervalEnd, double epsilon)
    {
      double functionValueAtStart = EvaluateFunction(functionExpression, intervalStart);
      double functionValueAtEnd = EvaluateFunction(functionExpression, intervalEnd);

      if (Math.Abs(functionValueAtStart) < epsilon)
        return intervalStart;
      if (Math.Abs(functionValueAtEnd) < epsilon)
        return intervalEnd;

      int iterationCount = 0;
      double midpoint = 0;

      while (Math.Abs(intervalEnd - intervalStart) > epsilon && iterationCount < MaxIterationsCount)
      {
        midpoint = (intervalStart + intervalEnd) / 2;
        double functionValueAtMidpoint = EvaluateFunction(functionExpression, midpoint);

        if (Math.Abs(functionValueAtMidpoint) < epsilon)
          return midpoint;

        if (functionValueAtStart * functionValueAtMidpoint < 0)
        {
          intervalEnd = midpoint;
          functionValueAtEnd = functionValueAtMidpoint;
        }
        else
        {
          intervalStart = midpoint;
          functionValueAtStart = functionValueAtMidpoint;
        }

        iterationCount++;
      }

      return (intervalStart + intervalEnd) / 2;
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
