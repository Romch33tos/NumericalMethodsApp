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
  }
}
