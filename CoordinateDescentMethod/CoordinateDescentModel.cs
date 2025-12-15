using NCalc;
using System;
using System.Collections.Generic;

namespace NumericalMethodsApp
{
  public class CoordinateDescentModel
  {
    public string FunctionExpression { get; private set; }
    private Expression parsedExpression;
    private double currentX;
    private double currentY;
    private double epsilon;
    private const int MaxIterations = 10000;
    private const double SearchRange = 2.0;
    private const double SearchStep = 0.01;

    public List<Point2D> OptimizationPath { get; private set; }
    public Point2D MinimumPoint { get; private set; }
    public int IterationsCount { get; private set; }
    public double FinalFunctionValue { get; private set; }
    public string OptimizationStatus { get; private set; }

    public CoordinateDescentModel()
    {
      OptimizationPath = new List<Point2D>();
      OptimizationStatus = "Не выполнена";
    }

    public bool SetFunction(string expression)
    {
      try
      {
        FunctionExpression = expression;
        parsedExpression = new Expression(expression, EvaluateOptions.IgnoreCase);
        parsedExpression.Parameters["x"] = 0.0;
        parsedExpression.Parameters["y"] = 0.0;

        double testResult = Convert.ToDouble(parsedExpression.Evaluate());

        parsedExpression.Parameters["x"] = 1.0;
        parsedExpression.Parameters["y"] = 2.0;
        double testResult2 = Convert.ToDouble(parsedExpression.Evaluate());

        if (Math.Abs(testResult - testResult2) < 1e-12)
        {
          parsedExpression.Parameters["x"] = -1.0;
          parsedExpression.Parameters["y"] = 3.0;
          double testResult3 = Convert.ToDouble(parsedExpression.Evaluate());

          if (Math.Abs(testResult - testResult3) < 1e-12)
          {
            return false;
          }
        }

        return true;
      }
      catch
      {
        return false;
      }
    }

    public bool SetInitialPoint(double initialX, double initialY)
    {
      if (double.IsNaN(initialX) || double.IsInfinity(initialX) || double.IsNaN(initialY) || double.IsInfinity(initialY))
        return false;

      currentX = initialX;
      currentY = initialY;
      return true;
    }

    public bool SetEpsilon(double epsilonValue)
    {
      if (epsilonValue <= 0 || epsilonValue > 1)
        return false;

      epsilon = epsilonValue;
      return true;
    }

    private double EvaluateFunction(double inputX, double inputY)
    {
      try
      {
        if (Math.Abs(inputX) < 1e-12 && FunctionExpression.Contains("/x"))
          return double.PositiveInfinity;
        if (Math.Abs(inputY) < 1e-12 && FunctionExpression.Contains("/y"))
          return double.PositiveInfinity;

        parsedExpression.Parameters["x"] = inputX;
        parsedExpression.Parameters["y"] = inputY;
        var result = parsedExpression.Evaluate();
        return Convert.ToDouble(result);
      }
      catch
      {
        return double.NaN;
      }
    }

    private double FindOptimalX(double fixedY)
    {
      double leftBound = currentX - SearchRange;
      double rightBound = currentX + SearchRange;
      double stepSize = SearchStep;

      double optimalX = leftBound;
      double minValue = EvaluateFunction(leftBound, fixedY);

      if (double.IsNaN(minValue))
        return currentX;

      for (double candidateX = leftBound + stepSize; candidateX <= rightBound; candidateX += stepSize)
      {
        double currentValue = EvaluateFunction(candidateX, fixedY);
        if (!double.IsNaN(currentValue) && currentValue < minValue)
        {
          minValue = currentValue;
          optimalX = candidateX;
        }
      }

      return optimalX;
    }

    private double FindOptimalY(double fixedX)
    {
      double lowerBound = currentY - SearchRange;
      double upperBound = currentY + SearchRange;
      double stepSize = SearchStep;

      double optimalY = lowerBound;
      double minValue = EvaluateFunction(fixedX, lowerBound);

      if (double.IsNaN(minValue))
        return currentY;

      for (double candidateY = lowerBound + stepSize; candidateY <= upperBound; candidateY += stepSize)
      {
        double currentValue = EvaluateFunction(fixedX, candidateY);
        if (!double.IsNaN(currentValue) && currentValue < minValue)
        {
          minValue = currentValue;
          optimalY = candidateY;
        }
      }

      return optimalY;
    }

    public OptimizationResult PerformOptimization()
    {
      if (parsedExpression == null)
        return OptimizationResult.Failure("Функция не задана");

      OptimizationPath.Clear();
      OptimizationPath.Add(new Point2D(currentX, currentY));

      int iterationCounter = 0;
      bool isConverged = false;

      while (!isConverged && iterationCounter < MaxIterations)
      {
        ++iterationCounter;

        double previousX = currentX;
        double previousY = currentY;

        currentX = FindOptimalX(currentY);
        if (double.IsNaN(currentX))
          return OptimizationResult.Failure("Ошибка вычисления функции при оптимизации по X");

        OptimizationPath.Add(new Point2D(currentX, currentY));

        currentY = FindOptimalY(currentX);
        if (double.IsNaN(currentY))
          return OptimizationResult.Failure("Ошибка вычисления функции при оптимизации по Y");

        OptimizationPath.Add(new Point2D(currentX, currentY));

        double deltaX = Math.Abs(currentX - previousX);
        double deltaY = Math.Abs(currentY - previousY);

        if (deltaX < epsilon && deltaY < epsilon)
        {
          isConverged = true;
        }
      }

      IterationsCount = iterationCounter;
      MinimumPoint = new Point2D(currentX, currentY);
      FinalFunctionValue = EvaluateFunction(currentX, currentY);

      if (double.IsNaN(FinalFunctionValue))
        return OptimizationResult.Failure("Не удалось вычислить значение функции в найденной точке");

      OptimizationStatus = isConverged ? "Сходимость достигнута" :
          iterationCounter >= MaxIterations ? "Достигнут предел итераций" : "Выполнено";

      return OptimizationResult.Success();
    }
  }
}
