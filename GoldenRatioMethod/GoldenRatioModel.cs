using System;
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
        iterationCounter++;

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
      try
      {
        string expressionText = FunctionExpression.Replace(" ", "");
        expressionText = ConvertToNCalcExpression(expressionText);

        expressionText = expressionText.Replace("e^-", "exp(-");
        expressionText = expressionText.Replace("e^", "exp(");

        NCalc.Expression functionExpression = new NCalc.Expression(expressionText);
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
                functionArgs.Result = Math.Exp(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "sqrt":
                functionArgs.Result = Math.Sqrt(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "abs":
                functionArgs.Result = Math.Abs(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
                break;
              case "pow":
                functionArgs.Result = Math.Pow(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()), Convert.ToDouble(functionArgs.Parameters[1].Evaluate()));
                break;
            }
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
      expression = ConvertPowerOperators(expression);
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
