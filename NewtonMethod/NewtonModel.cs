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
