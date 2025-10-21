using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace NumericalMethodsApp.Models
{
  public class NewtonModel
  {
    public string FunctionExpression { get; set; }
    public double InitialPoint { get; set; }
    public double Epsilon { get; set; }
    public bool FindMinimum { get; set; }
    public bool FindMaximum { get; set; }

    public CalculationResultModel CalculationResult { get; private set; }
    public List<IterationStep> IterationSteps { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public bool CalculationComplete { get; private set; }
    public bool StepModeStarted { get; private set; }

    public NewtonModel()
    {
      Epsilon = 0.001;
      CalculationResult = new CalculationResultModel();
      IterationSteps = new List<IterationStep>();
      CurrentStepIndex = -1;
      CalculationComplete = false;
      StepModeStarted = false;
    }

    public void Calculate()
    {
      if (!ValidateInput())
      {
        throw new InvalidOperationException("Некорректные входные данные");
      }

      IterationSteps.Clear();
      CurrentStepIndex = -1;
      CalculationComplete = false;
      StepModeStarted = false;

      double currentPoint = InitialPoint;
      int iterationCounter = 0;
      double previousPoint = currentPoint;

      while (iterationCounter < 1000)
      {
        ++iterationCounter;

        double firstDerivative = CalculateFirstDerivative(currentPoint);
        double secondDerivative = CalculateSecondDerivative(currentPoint);

        if (Math.Abs(secondDerivative) < 1e-15)
        {
          throw new Exception("Вторая производная близка к нулю. Метод Ньютона не может быть применен.");
        }

        double nextPoint = currentPoint - firstDerivative / secondDerivative;

        IterationSteps.Add(new IterationStep
        {
          IterationNumber = iterationCounter,
          Point = currentPoint,
          FunctionValue = EvaluateFunction(currentPoint),
          FirstDerivative = firstDerivative,
          SecondDerivative = secondDerivative,
          NextPoint = nextPoint
        });

        if (Math.Abs(firstDerivative) < Epsilon)
        {
          break;
        }

        if (Math.Abs(nextPoint - currentPoint) < Epsilon)
        {
          break;
        }

        if (double.IsInfinity(nextPoint) || double.IsNaN(nextPoint))
        {
          throw new Exception("Метод расходится. Попробуйте другое начальное приближение.");
        }

        if (iterationCounter > 1 && Math.Abs(nextPoint - previousPoint) < 1e-15)
        {
          break;
        }

        previousPoint = currentPoint;
        currentPoint = nextPoint;
      }

      double resultPoint = currentPoint;
      double functionValue = EvaluateFunction(resultPoint);
      double finalSecondDerivative = CalculateSecondDerivative(resultPoint);

      bool isActuallyMinimum = finalSecondDerivative > 0;
      bool isActuallyMaximum = finalSecondDerivative < 0;
      bool isExtremum = Math.Abs(finalSecondDerivative) > 1e-10;

      bool success = isExtremum &&
              (Math.Abs(CalculateFirstDerivative(resultPoint)) < Epsilon) &&
              ((FindMinimum && isActuallyMinimum) || (FindMaximum && isActuallyMaximum));

      CalculationResult = new CalculationResultModel
      {
        Point = Math.Round(resultPoint, 6),
        Value = Math.Round(functionValue, 6),
        IsMinimum = isActuallyMinimum,
        Success = success
      };

      CalculationComplete = true;
    }

    public double CalculateFirstDerivative(double point)
    {
      const double step = 1e-6;
      double forwardPoint = EvaluateFunction(point + step);
      double backwardPoint = EvaluateFunction(point - step);
      return (forwardPoint - backwardPoint) / (2 * step);
    }

    public double CalculateSecondDerivative(double point)
    {
      const double step = 1e-6;
      double forwardPoint = EvaluateFunction(point + step);
      double centerPoint = EvaluateFunction(point);
      double backwardPoint = EvaluateFunction(point - step);
      return (forwardPoint - 2 * centerPoint + backwardPoint) / (step * step);
    }

    public double EvaluateFunction(double inputValue)
    {
      NCalc.Expression functionExpression = null;

      try
      {
        string expressionText = FunctionExpression.Replace(" ", "");

        if (expressionText.ToLower() == "x" || expressionText.ToLower() == "y=x")
        {
          return inputValue;
        }

        if (expressionText.ToLower().StartsWith("y="))
        {
          expressionText = expressionText.Substring(2);
        }

        if (expressionText.Trim() == "x")
        {
          return inputValue;
        }

        expressionText = ConvertToNCalcExpression(expressionText);

        expressionText = expressionText.Replace("e^-", "exp(-");
        expressionText = expressionText.Replace("e^", "exp(");

        functionExpression = new NCalc.Expression(expressionText);
        functionExpression.Parameters["x"] = inputValue;
        functionExpression.Parameters["e"] = Math.E;
        functionExpression.Parameters["pi"] = Math.PI;

        functionExpression.EvaluateFunction += (functionName, functionArgs) =>
        {
          try
          {
            switch (functionName.ToLower())
            {
              case "sin":
                functionArgs.Result = Math.Sin(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "cos":
                functionArgs.Result = Math.Cos(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "tan":
                functionArgs.Result = Math.Tan(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "log":
                functionArgs.Result = Math.Log(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "log10":
                functionArgs.Result = Math.Log10(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "exp":
                double expArg = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
                if (expArg > 100) functionArgs.Result = double.PositiveInfinity;
                else if (expArg < -100) functionArgs.Result = 0.0;
                else functionArgs.Result = Math.Exp(expArg);
                break;
              case "sqrt":
                double sqrtArg = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
                if (sqrtArg < 0) throw new Exception("Квадратный корень из отрицательного числа");
                functionArgs.Result = Math.Sqrt(sqrtArg);
                break;
              case "abs":
                functionArgs.Result = Math.Abs(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "pow":
                double baseValue = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
                double exponentValue = Convert.ToDouble(functionArgs.Parameters[1].Evaluate());

                if (baseValue == 0 && exponentValue > 0) functionArgs.Result = 0.0;
                else if (baseValue == 0 && exponentValue <= 0) throw new Exception("Ноль в отрицательной степени");
                else if (Math.Abs(baseValue) < 1e-10 && exponentValue > 100) functionArgs.Result = 0.0;
                else if (Math.Abs(baseValue) > 1e10 && exponentValue > 100) functionArgs.Result = double.PositiveInfinity;
                else functionArgs.Result = Math.Pow(baseValue, exponentValue);
                break;
            }
          }
          catch (OverflowException)
          {
            functionArgs.Result = double.PositiveInfinity;
          }
          catch (Exception ex)
          {
            throw new Exception($"Ошибка в функции {functionName}: {ex.Message}");
          }
        };

        object resultObject = functionExpression.Evaluate();

        if (resultObject is double doubleResult)
        {
          if (double.IsInfinity(doubleResult) || double.IsNaN(doubleResult))
            throw new Exception("Результат вычисления функции не является конечным числом");
          return doubleResult;
        }
        else if (resultObject is int intResult)
        {
          return intResult;
        }
        else if (resultObject is decimal decimalResult)
        {
          return (double)decimalResult;
        }
        else if (resultObject is long longResult)
        {
          return longResult;
        }
        else
        {
          throw new Exception($"Неподдерживаемый тип результата: {resultObject.GetType()}");
        }
      }
      catch (Exception evaluationException)
      {
        throw new Exception($"Ошибка вычисления функции в точке x={inputValue}: {evaluationException.Message}");
      }
    }

    private string ConvertToNCalcExpression(string expression)
    {
      expression = expression.Replace(" ", "");

      if (expression.ToLower() == "x" || expression.ToLower() == "y=x")
      {
        return "x";
      }

      if (expression.ToLower().StartsWith("y="))
      {
        expression = expression.Substring(2);
      }

      expression = ConvertPowerOperators(expression);
      return expression;
    }

    private string ConvertPowerOperators(string expression)
    {
      string pattern = @"([a-zA-Z0-9\.\(\)]+)\^([a-zA-Z0-9\.\(\)]+)";
      while (Regex.IsMatch(expression, pattern))
      {
        expression = Regex.Replace(expression, pattern, "pow($1, $2)");
      }
      return expression;
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(FunctionExpression))
        return false;

      if (IsConstantFunction(FunctionExpression))
        return false;

      if (!TryParseDouble(InitialPoint.ToString(), out double initialPoint))
        return false;

      if (Epsilon <= 0)
        return false;

      try
      {
        EvaluateFunction(initialPoint);
        CalculateFirstDerivative(initialPoint);
        CalculateSecondDerivative(initialPoint);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public bool IsConstantFunction(string expression)
    {
      string cleanExpression = expression.Replace(" ", "").ToLower();

      if (!cleanExpression.Contains("x"))
        return true;

      if (cleanExpression == "x")
        return false;

      string testExpression = cleanExpression.Replace("x", "");

      if (string.IsNullOrWhiteSpace(testExpression))
        return false;

      try
      {
        NCalc.Expression testExpr = new NCalc.Expression(testExpression);
        testExpr.Parameters["e"] = Math.E;
        testExpr.Parameters["pi"] = Math.PI;

        object result1 = testExpr.Evaluate();
        return result1 != null;
      }
      catch
      {
        return false;
      }
    }

    public static double ParseDouble(string text)
    {
      text = text.Replace(",", ".");
      return double.Parse(text, CultureInfo.InvariantCulture);
    }

    public static bool TryParseDouble(string text, out double result)
    {
      text = text.Replace(",", ".");
      return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }
  }

  public class IterationStep
  {
    public int IterationNumber { get; set; }
    public double Point { get; set; }
    public double FunctionValue { get; set; }
    public double FirstDerivative { get; set; }
    public double SecondDerivative { get; set; }
    public double NextPoint { get; set; }
  }

  public class CalculationResultModel
  {
    public double Point { get; set; }
    public double Value { get; set; }
    public bool IsMinimum { get; set; }
    public bool Success { get; set; }
  }
}
