using System;
using System.Globalization;
using NCalc;

namespace NumericalMethodsApp
{
  public class DichotomyPresenter
  {
    private readonly IDichotomyView view;
    private const int MaxIterations = 1000;

    public DichotomyPresenter(IDichotomyView view)
    {
      this.view = view;
    }

    public void CalculateMinimum()
    {
      try
      {
        if (!ValidateInputs())
          return;

        string function = view.FunctionExpression.Trim();
        double startInterval = ParseDouble(view.StartIntervalText);
        double endInterval = ParseDouble(view.EndIntervalText);
        double epsilon = ParseDouble(view.EpsilonText);

        if (startInterval >= endInterval)
        {
          view.ShowError("Начало интервала (a) должно быть меньше конца интервала (b).");
          return;
        }

        if (epsilon <= 0)
        {
          view.ShowError("Точность (ε) должна быть положительным числом.");
          return;
        }

        var result = FindMinimumDichotomy(function, startInterval, endInterval, epsilon);
        double minimumX = result.x;
        double minimumY = result.y;

        view.PlotFunction(startInterval, endInterval, minimumX, minimumY);
        view.SetResult($"Ответ: минимум функции находится в точке x = {minimumX:F6}, f(x) = {minimumY:F6}");
      }
      catch (Exception ex)
      {
        view.ShowError($"Ошибка при вычислениях: {ex.Message}");
      }
    }

    public void ClearAll()
    {
      view.ClearPlot();
      view.SetResult("Ответ: ");
    }

    public void ShowHelp()
    {
      string helpText = @"Метод дихотомии

Метод дихотомии (половинного деления) используется для поиска минимума функции на заданном интервале.

Параметры:
• Функция f(x): математическое выражение
• Начало интервала (a): левая граница интервала поиска
• Конец интервала (b): правая граница интервала поиска
• Точность (ε): желаемая точность нахождения минимума

Доступные функции:
• Основные операторы: +, -, *, /
• Тригонометрические: sin(x), cos(x), tan(x)
• Экспоненциальные: exp(x), log(x), log10(x)
• Другие: sqrt(x), abs(x), pow(x,y)

Инструкция:
1. Введите функцию и параметры
2. Используйте Tab для перехода между полями
3. Нажмите Вычислить минимум/Enter для ввода данных
4. Для очистки полей используйте кнопку Очистить все";

      view.ShowInformation(helpText);
    }

    public double EvaluateFunction(string function, double xValue)
    {
      try
      {
        string expression = function.Trim();

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
        throw new ArgumentException($"Невозможно вычислить функцию '{function}' при x={xValue}: {ex.Message}");
      }
    }

    private bool ValidateInputs()
    {
      if (string.IsNullOrWhiteSpace(view.FunctionExpression))
      {
        view.ShowError("Введите функцию f(x).");
        view.FocusFunctionTextBox();
        return false;
      }

      if (!IsValidNumber(view.StartIntervalText))
      {
        view.ShowError("Введите корректное число для начала интервала (a).");
        view.FocusStartIntervalTextBox();
        return false;
      }

      if (!IsValidNumber(view.EndIntervalText))
      {
        view.ShowError("Введите корректное число для конца интервала (b).");
        view.FocusEndIntervalTextBox();
        return false;
      }

      if (!IsValidNumber(view.EpsilonText) || ParseDouble(view.EpsilonText) <= 0)
      {
        view.ShowError("Введите корректное положительное число для точности (ε).");
        view.FocusEpsilonTextBox();
        return false;
      }

      try
      {
        double startInterval = ParseDouble(view.StartIntervalText);
        double endInterval = ParseDouble(view.EndIntervalText);
        double testValue1 = startInterval;
        double testValue2 = (startInterval + endInterval) / 2;
        double testValue3 = endInterval;

        EvaluateFunction(view.FunctionExpression, testValue1);
        EvaluateFunction(view.FunctionExpression, testValue2);
        EvaluateFunction(view.FunctionExpression, testValue3);
      }
      catch (Exception ex)
      {
        view.ShowError($"Некорректный синтаксис функции: {ex.Message}\n\nИспользуйте доступные математические функции: sin(x), cos(x), exp(x), log(x), sqrt(x), abs(x), pow(x,y) и др.");
        view.FocusFunctionTextBox();
        return false;
      }

      return true;
    }

    private (double x, double y) FindMinimumDichotomy(string function, double startInterval, double endInterval, double epsilon)
    {
      double firstPoint, secondPoint;
      int iterationCount = 0;

      while (Math.Abs(endInterval - startInterval) > epsilon && iterationCount < MaxIterations)
      {
        firstPoint = (startInterval + endInterval - epsilon / 2) / 2;
        secondPoint = (startInterval + endInterval + epsilon / 2) / 2;

        double firstFunctionValue = EvaluateFunction(function, firstPoint);
        double secondFunctionValue = EvaluateFunction(function, secondPoint);

        if (firstFunctionValue < secondFunctionValue)
          endInterval = secondPoint;
        else
          startInterval = firstPoint;

        iterationCount++;
      }

      if (iterationCount >= MaxIterations)
      {
        view.ShowWarning($"Достигнуто максимальное количество итераций ({MaxIterations}). " +
                       $"Возможно, функция не имеет минимума на данном интервале или требуется увеличить интервал.");
      }

      double minimumX = (startInterval + endInterval) / 2;
      double minimumY = EvaluateFunction(function, minimumX);

      return (minimumX, minimumY);
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