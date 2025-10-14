using System;

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
