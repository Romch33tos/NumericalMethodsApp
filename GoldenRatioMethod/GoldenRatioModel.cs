using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NumericalMethodsApp.Models
{
  public class GoldenRatioModel
  {
    public const double GoldenRatioConstant = 1.618033988749895;

    public string FunctionExpression { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double Epsilon { get; set; }
    public bool FindMinimum { get; set; }

    public CalculationResult CalculationResult { get; private set; }

    public GoldenRatioModel()
    {
      Epsilon = 0.001;
      CalculationResult = new CalculationResult();
    }

    public void Calculate()
    {
      if (!ValidateInput())
      {
        throw new InvalidOperationException("Некорректные входные данные");
      }

      double resultPoint = GoldenRatioSearch(LowerBound, UpperBound, Epsilon, FindMinimum);
      double functionValue = EvaluateFunction(resultPoint);

      CalculationResult = new CalculationResult
      {
        Point = resultPoint,
        Value = functionValue,
        IsMinimum = FindMinimum,
        Success = true
      };
    }

    private double GoldenRatioSearch(double lowerBound, double upperBound, double epsilonValue, bool findMinimum)
    {
      int iterationCounter = 0;
      double currentLower = lowerBound;
      double currentUpper = upperBound;

      while (Math.Abs(currentUpper - currentLower) > epsilonValue)
      {
        ++iterationCounter;

        double leftPoint = currentUpper - (currentUpper - currentLower) / GoldenRatioConstant;
        double rightPoint = currentLower + (currentUpper - currentLower) / GoldenRatioConstant;

        double leftFunctionValue = EvaluateFunction(leftPoint);
        double rightFunctionValue = EvaluateFunction(rightPoint);

        if (findMinimum)
        {
          if (leftFunctionValue >= rightFunctionValue)
          {
            currentLower = leftPoint;
          }
          else
          {
            currentUpper = rightPoint;
          }
        }
        else
        {
          if (leftFunctionValue <= rightFunctionValue)
          {
            currentLower = leftPoint;
          }
          else
          {
            currentUpper = rightPoint;
          }
        }

        if (iterationCounter > 1000)
        {
          throw new Exception("Превышено максимальное количество итераций. Проверьте корректность функции и интервала.");
        }
      }

      return (currentLower + currentUpper) / 2;
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
      expression = ConvertImplicitMultiplication(expression);
      return expression;
    }

    private string ConvertPowerOperators(string expression)
    {
      var pattern = @"([a-zA-Z0-9\.\(\)]+)\^([a-zA-Z0-9\.\(\)]+)";
      while (Regex.IsMatch(expression, pattern))
      {
        expression = Regex.Replace(expression, pattern, "pow($1, $2)");
      }
      return expression;
    }

    private string ConvertImplicitMultiplication(string expression)
    {
      var numberVariablePattern = @"(\d+\.?\d*)([a-zA-Z\(])";
      var variableNumberPattern = @"([a-zA-Z\)])(\d+\.?\d*)";

      expression = Regex.Replace(expression, numberVariablePattern, "$1*$2");
      expression = Regex.Replace(expression, variableNumberPattern, "$1*$2");

      return expression;
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(FunctionExpression))
        return false;

      if (IsConstantFunction(FunctionExpression))
        return false;

      if (IsTrivialExpression(FunctionExpression))
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

        var result1 = testExpr.Evaluate();
        return result1 != null;
      }
      catch
      {
        return false;
      }
    }

    private bool IsTrivialExpression(string expression)
    {
      string cleanExpression = expression.Replace(" ", "").ToLower();

      if (double.TryParse(cleanExpression, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        return true;

      try
      {
        double test1 = EvaluateFunction(1.0);
        double test2 = EvaluateFunction(2.0);

        if (Math.Abs(test1 - test2) < 1e-10)
          return true;

        string[] trivialPatterns = {
          "0*x", "x*0", "0x", "x0", 
          "x-x", 
          "x/x", 
          "x^0",
        };

        foreach (string pattern in trivialPatterns)
        {
          if (cleanExpression.Contains(pattern))
            return true;
        }

        if (!cleanExpression.Contains("x"))
          return true;

        return false;
      }
      catch
      {
        return false;
      }
    }

    private string CheckForDiscontinuities(double lowerBound, double upperBound)
    {
      const int testPointsCount = 50;
      double step = (upperBound - lowerBound) / testPointsCount;
      double? previousValue = null;

      for (int counter = 0; counter <= testPointsCount; ++counter)
      {
        double testPoint = lowerBound + counter * step;
        try
        {
          double currentValue = EvaluateFunction(testPoint);

          if (previousValue.HasValue)
          {
            double difference = Math.Abs(currentValue - previousValue.Value);
            double maxAllowedDifference = Math.Max(Math.Abs(previousValue.Value) * 10, 1000);

            if (difference > maxAllowedDifference && !double.IsInfinity(currentValue) && !double.IsInfinity(previousValue.Value))
            {
              return $"Резкое изменение значения функции между x={testPoint - step} и x={testPoint}";
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

  public class CalculationResult
  {
    public double Point { get; set; }
    public double Value { get; set; }
    public bool IsMinimum { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
  }
}
