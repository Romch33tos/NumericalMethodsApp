using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NumericalMethodsApp.Models
{
  public class NewtonModel
  {
    public string FunctionExpression { get; set; } = string.Empty;
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double Epsilon { get; set; }
    public bool FindMinimum { get; set; } = true;

    public NewtonCalculationResult CalculationResult { get; private set; }
    public List<NewtonIterationStep> IterationSteps { get; private set; }

    public NewtonModel()
    {
      Epsilon = 0.001;
      CalculationResult = new NewtonCalculationResult();
      IterationSteps = new List<NewtonIterationStep>();
    }

    public void Calculate()
    {
      if (!ValidateInput())
      {
        throw new InvalidOperationException("Некорректные входные данные");
      }

      if (!IsNewtonMethodApplicable())
      {
        throw new InvalidOperationException("Метод Ньютона не применим для данной функции на указанном интервале");
      }

      IterationSteps.Clear();
      double resultPoint = NewtonOptimization(LowerBound, UpperBound, Epsilon, FindMinimum);
      double functionValue = EvaluateFunction(resultPoint);

      CalculationResult = new NewtonCalculationResult
      {
        Point = resultPoint,
        Value = functionValue,
        IsMinimum = FindMinimum,
        Success = true
      };
    }

    private double NewtonOptimization(double lowerBound, double upperBound, double epsilonValue, bool findMinimum)
    {
      int iterationCount = 0;
      double currentPoint = (lowerBound + upperBound) / 2;

      while (true)
      {
        ++iterationCount;

        double firstDerivative = CalculateFirstDerivative(currentPoint);
        double secondDerivative = CalculateSecondDerivative(currentPoint);

        if (Math.Abs(secondDerivative) < 1e-10)
        {
          throw new InvalidOperationException("Вторая производная близка к нулю - метод Ньютона не применим");
        }

        if (Math.Abs(firstDerivative) < epsilonValue)
        {
          break;
        }

        double nextPoint = currentPoint - firstDerivative / secondDerivative;

        if (nextPoint < lowerBound || nextPoint > upperBound || double.IsInfinity(nextPoint) || double.IsNaN(nextPoint))
        {
          throw new InvalidOperationException("Метод Ньютона расходится на данном интервале");
        }

        var step = new NewtonIterationStep
        {
          Iteration = iterationCount,
          CurrentPoint = currentPoint,
          NextPoint = nextPoint,
          FirstDerivative = firstDerivative,
          SecondDerivative = secondDerivative,
          FunctionValue = EvaluateFunction(currentPoint)
        };

        IterationSteps.Add(step);

        if (Math.Abs(nextPoint - currentPoint) < epsilonValue)
        {
          currentPoint = nextPoint;
          break;
        }

        currentPoint = nextPoint;

        if (iterationCount > 1000)
        {
          throw new InvalidOperationException("Превышено максимальное количество итераций");
        }
      }

      return currentPoint;
    }

    private double CalculateFirstDerivative(double x)
    {
      double stepSize = 1e-6;
      return (EvaluateFunction(x + stepSize) - EvaluateFunction(x - stepSize)) / (2 * stepSize);
    }

    private double CalculateSecondDerivative(double x)
    {
      double stepSize = 1e-6;
      return (EvaluateFunction(x + stepSize) - 2 * EvaluateFunction(x) + EvaluateFunction(x - stepSize)) / (stepSize * stepSize);
    }

    public double EvaluateFunction(double inputValue)
    {
      NCalc.Expression functionExpression = null;

      try
      {
        string processedExpression = FunctionExpression.Replace(" ", "");

        if (processedExpression.ToLower() == "x" || processedExpression.ToLower() == "y=x")
        {
          return inputValue;
        }

        if (processedExpression.ToLower().StartsWith("y="))
        {
          processedExpression = processedExpression.Substring(2);
        }

        if (processedExpression.Trim() == "x")
        {
          return inputValue;
        }

        processedExpression = ConvertToNCalcExpression(processedExpression);

        processedExpression = processedExpression.Replace("e^-", "exp(-");
        processedExpression = processedExpression.Replace("e^", "exp(");

        functionExpression = new NCalc.Expression(processedExpression);
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
                double exponentArgument = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
                if (exponentArgument > 100) functionArgs.Result = double.PositiveInfinity;
                else if (exponentArgument < -100) functionArgs.Result = 0.0;
                else functionArgs.Result = Math.Exp(exponentArgument);
                break;
              case "sqrt":
                double sqrtArgument = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
                if (sqrtArgument < 0) throw new Exception("Квадратный корень из отрицательного числа");
                functionArgs.Result = Math.Sqrt(sqrtArgument);
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

        object evaluationResult = functionExpression.Evaluate();

        if (evaluationResult is double doubleResult)
        {
          if (double.IsInfinity(doubleResult) || double.IsNaN(doubleResult))
            throw new Exception("Результат вычисления функции не является конечным числом");
          return doubleResult;
        }
        else if (evaluationResult is int intResult)
        {
          return intResult;
        }
        else if (evaluationResult is decimal decimalResult)
        {
          return (double)decimalResult;
        }
        else if (evaluationResult is long longResult)
        {
          return longResult;
        }
        else
        {
          throw new Exception($"Неподдерживаемый тип результата: {evaluationResult.GetType()}");
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
      var powerPattern = @"([a-zA-Z0-9\.\(\)]+)\^([a-zA-Z0-9\.\(\)]+)";
      while (Regex.IsMatch(expression, powerPattern))
      {
        expression = Regex.Replace(expression, powerPattern, "pow($1, $2)");
      }
      return expression;
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(FunctionExpression))
        return false;

      if (IsConstantFunction(FunctionExpression))
        return false;

      if (LowerBound >= UpperBound)
        return false;

      if (Epsilon <= 0)
        return false;

      string processedExpression = ConvertToNCalcExpression(FunctionExpression);
      if (string.IsNullOrWhiteSpace(processedExpression))
        return false;

      string discontinuityCheckResult = CheckForDiscontinuities(LowerBound, UpperBound);
      if (!string.IsNullOrEmpty(discontinuityCheckResult))
      {
        throw new Exception($"Обнаружен разрыв функции: {discontinuityCheckResult}");
      }

      try
      {
        EvaluateFunction(LowerBound);
        EvaluateFunction(UpperBound);
        EvaluateFunction((LowerBound + UpperBound) / 2);
      }
      catch
      {
        return false;
      }

      return true;
    }

    private bool IsNewtonMethodApplicable()
    {
      const int testPoints = 10;
      double step = (UpperBound - LowerBound) / testPoints;

      for (int counter = 0; counter <= testPoints; ++counter)
      {
        double x = LowerBound + counter * step;
        try
        {
          double secondDerivative = CalculateSecondDerivative(x);
          if (Math.Abs(secondDerivative) < 1e-10)
          {
            return false;
          }
        }
        catch
        {
          return false;
        }
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
        var testExpr = new NCalc.Expression(testExpression);
        testExpr.Parameters["e"] = Math.E;
        testExpr.Parameters["pi"] = Math.PI;

        var result = testExpr.Evaluate();
        return result != null;
      }
      catch
      {
        return false;
      }
    }

    private string CheckForDiscontinuities(double lowerBound, double upperBound)
    {
      const int testPointsCount = 50;
      double stepSize = (upperBound - lowerBound) / testPointsCount;
      double? previousValue = null;

      for (int pointIndex = 0; pointIndex <= testPointsCount; ++pointIndex)
      {
        double testPoint = lowerBound + pointIndex * stepSize;
        try
        {
          double currentValue = EvaluateFunction(testPoint);

          if (previousValue.HasValue)
          {
            double difference = Math.Abs(currentValue - previousValue.Value);
            double maxAllowedDifference = Math.Max(Math.Abs(previousValue.Value) * 10, 1000);

            if (difference > maxAllowedDifference && !double.IsInfinity(currentValue) && !double.IsInfinity(previousValue.Value))
            {
              return $"Резкое изменение значения функции между x={testPoint - stepSize} и x={testPoint}";
            }
          }

          previousValue = currentValue;
        }
        catch (Exception ex)
        {
          return $"Функция не определена в точке x={testPoint}: {ex.Message}";
        }
      }

      return null;
    }

    public void Reset()
    {
      CalculationResult = new NewtonCalculationResult();
      IterationSteps.Clear();
      FunctionExpression = string.Empty;
      LowerBound = 0;
      UpperBound = 0;
      Epsilon = 0.001;
      FindMinimum = true;
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

  public class NewtonCalculationResult
  {
    public double Point { get; set; }
    public double Value { get; set; }
    public bool IsMinimum { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
  }

  public class NewtonIterationStep
  {
    public int Iteration { get; set; }
    public double CurrentPoint { get; set; }
    public double NextPoint { get; set; }
    public double FirstDerivative { get; set; }
    public double SecondDerivative { get; set; }
    public double FunctionValue { get; set; }
  }
}
