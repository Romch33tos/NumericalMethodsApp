using System;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using NCalc;

namespace NumericalMethodsApp.DefiniteIntegralMethod
{
  public class DefiniteIntegralModel
  {
    public double EvaluateFunction(string expression, double xValue)
    {
      try
      {
        NCalc.Expression expressionEvaluator = new NCalc.Expression(expression);
        expressionEvaluator.Parameters["x"] = xValue;
        expressionEvaluator.Parameters["pi"] = Math.PI;
        expressionEvaluator.Parameters["e"] = Math.E;

        expressionEvaluator.EvaluateFunction += (functionName, functionArgs) =>
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
              double tangentValue = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
              if (Math.Abs(Math.Cos(tangentValue)) < 1e-15)
                throw new DivideByZeroException();
              functionArgs.Result = Math.Tan(tangentValue);
              break;
            case "log":
              double logarithmValue = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
              if (logarithmValue <= 0)
                throw new ArgumentException();
              functionArgs.Result = Math.Log(logarithmValue);
              break;
            case "log10":
              double logarithmBase10Value = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
              if (logarithmBase10Value <= 0)
                throw new ArgumentException();
              functionArgs.Result = Math.Log10(logarithmBase10Value);
              break;
            case "exp":
              functionArgs.Result = Math.Exp(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
              break;
            case "sqrt":
              double squareRootValue = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
              if (squareRootValue < 0)
                throw new ArgumentException();
              functionArgs.Result = Math.Sqrt(squareRootValue);
              break;
            case "abs":
              functionArgs.Result = Math.Abs(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
              break;
            case "asin":
              double arcsineValue = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
              if (Math.Abs(arcsineValue) > 1)
                throw new ArgumentException();
              functionArgs.Result = Math.Asin(arcsineValue);
              break;
            case "acos":
              double arccosineValue = Convert.ToDouble(functionArgs.Parameters[0].Evaluate());
              if (Math.Abs(arccosineValue) > 1)
                throw new ArgumentException();
              functionArgs.Result = Math.Acos(arccosineValue);
              break;
            case "atan":
              functionArgs.Result = Math.Atan(Convert.ToDouble(functionArgs.Parameters[0].Evaluate()));
              break;
          }
        };

        object result = expressionEvaluator.Evaluate();
        if (expressionEvaluator.HasErrors())
        {
          throw new ArgumentException();
        }

        return Convert.ToDouble(result);
      }
      catch (DivideByZeroException)
      {
        throw;
      }
      catch
      {
        throw;
      }
    }

    public bool IsConstantExpression(string expression)
    {
      return !expression.ToLower().Contains("x");
    }

    public (double result, int partitions) CalculateIntegral(IntegrationMethod method, string expression,
                                                            double lowerBound, double upperBound, double epsilon,
                                                            int? fixedPartitions = null)
    {
      Func<double, double> function = xValue => EvaluateFunction(expression, xValue);

      if (fixedPartitions.HasValue)
      {
        double result = CalculateFixedPartitions(method, function, lowerBound, upperBound, fixedPartitions.Value);
        return (result, fixedPartitions.Value);
      }

      switch (method)
      {
        case IntegrationMethod.Trapezoidal:
          return CalculateAdaptiveTrapezoidal(function, lowerBound, upperBound, epsilon);
        case IntegrationMethod.Simpson:
          return CalculateAdaptiveSimpson(function, lowerBound, upperBound, epsilon);
        default:
          return CalculateAdaptiveRectangle(method, function, lowerBound, upperBound, epsilon);
      }
    }

    private double CalculateFixedPartitions(IntegrationMethod method, Func<double, double> function,
                                           double lowerBound, double upperBound, int partitions)
    {
      switch (method)
      {
        case IntegrationMethod.Trapezoidal:
          return CalculateTrapezoidal(function, lowerBound, upperBound, partitions);
        case IntegrationMethod.Simpson:
          return CalculateSimpson(function, lowerBound, upperBound, partitions);
        default:
          return CalculateRectangle(method, function, lowerBound, upperBound, partitions);
      }
    }

    private (double result, int partitions) CalculateAdaptiveRectangle(IntegrationMethod method,
                                                                       Func<double, double> function,
                                                                       double lowerBound, double upperBound, double tolerance)
    {
      int maxPartitions = 1000000;
      int currentPartitions = 4;
      double previousResult = CalculateRectangle(method, function, lowerBound, upperBound, currentPartitions);
      double currentResult = previousResult;

      for (int iterationIndex = 0; iterationIndex < 50; ++iterationIndex)
      {
        currentPartitions *= 2;
        if (currentPartitions > maxPartitions) break;

        currentResult = CalculateRectangle(method, function, lowerBound, upperBound, currentPartitions);
        double errorEstimate = Math.Abs(currentResult - previousResult);

        if (errorEstimate < tolerance * Math.Abs(currentResult) && errorEstimate < tolerance)
          break;

        previousResult = currentResult;
      }

      return (currentResult, currentPartitions);
    }

    private double CalculateRectangle(IntegrationMethod method, Func<double, double> function,
                                     double lowerBound, double upperBound, int partitions)
    {
      double stepSize = (upperBound - lowerBound) / partitions;
      double areaSum = 0;

      for (int partitionIndex = 0; partitionIndex < partitions; ++partitionIndex)
      {
        double xCoordinate = 0;
        switch (method)
        {
          case IntegrationMethod.LeftRectangle:
            xCoordinate = lowerBound + partitionIndex * stepSize;
            break;
          case IntegrationMethod.RightRectangle:
            xCoordinate = lowerBound + (partitionIndex + 1) * stepSize;
            break;
          case IntegrationMethod.MidpointRectangle:
            xCoordinate = lowerBound + (partitionIndex + 0.5) * stepSize;
            break;
        }
        areaSum += function(xCoordinate);
      }

      return areaSum * stepSize;
    }

    private (double result, int partitions) CalculateAdaptiveTrapezoidal(Func<double, double> function,
                                                                        double lowerBound, double upperBound, double tolerance)
    {
      int maxPartitions = 1000000;
      int currentPartitions = 4;
      double previousResult = CalculateTrapezoidal(function, lowerBound, upperBound, currentPartitions);
      double currentResult = previousResult;

      for (int iterationIndex = 0; iterationIndex < 50; ++iterationIndex)
      {
        currentPartitions *= 2;
        if (currentPartitions > maxPartitions) break;

        currentResult = CalculateTrapezoidal(function, lowerBound, upperBound, currentPartitions);
        double errorEstimate = Math.Abs(currentResult - previousResult);

        if (errorEstimate < tolerance * Math.Abs(currentResult) && errorEstimate < tolerance)
          break;

        previousResult = currentResult;
      }

      return (currentResult, currentPartitions);
    }

    private double CalculateTrapezoidal(Func<double, double> function, double lowerBound, double upperBound, int partitions)
    {
      double stepSize = (upperBound - lowerBound) / partitions;
      double areaSum = (function(lowerBound) + function(upperBound)) / 2;

      for (int partitionIndex = 1; partitionIndex < partitions; ++partitionIndex)
      {
        double xCoordinate = lowerBound + partitionIndex * stepSize;
        areaSum += function(xCoordinate);
      }

      return areaSum * stepSize;
    }

    private (double result, int partitions) CalculateAdaptiveSimpson(Func<double, double> function,
                                                                     double lowerBound, double upperBound, double tolerance)
    {
      int maxPartitions = 1000000;
      int currentPartitions = 4;
      double previousResult = CalculateSimpson(function, lowerBound, upperBound, currentPartitions);
      double currentResult = previousResult;

      for (int iterationIndex = 0; iterationIndex < 50; ++iterationIndex)
      {
        currentPartitions *= 2;
        if (currentPartitions > maxPartitions) break;

        currentResult = CalculateSimpson(function, lowerBound, upperBound, currentPartitions);
        double errorEstimate = Math.Abs(currentResult - previousResult);

        if (errorEstimate < tolerance * Math.Abs(currentResult) && errorEstimate < tolerance)
          break;

        previousResult = currentResult;
      }

      return (currentResult, currentPartitions);
    }

    private double CalculateSimpson(Func<double, double> function, double lowerBound, double upperBound, int partitions)
    {
      if (partitions % 2 != 0) ++partitions;

      double stepSize = (upperBound - lowerBound) / partitions;
      double areaSum = function(lowerBound) + function(upperBound);

      for (int partitionIndex = 1; partitionIndex < partitions; ++partitionIndex)
      {
        double xCoordinate = lowerBound + partitionIndex * stepSize;
        double coefficient = (partitionIndex % 2 == 0) ? 2 : 4;
        areaSum += coefficient * function(xCoordinate);
      }

      return areaSum * stepSize / 3;
    }

    public PlotModel CreatePlotModel(string expression, double lowerBound, double upperBound,
                                     IntegrationMethod method, Color methodColor, int partitions)
    {
      PlotModel plotModel = new PlotModel
      {
        PlotAreaBorderColor = OxyColors.LightGray,
        PlotAreaBorderThickness = new OxyThickness(1),
        Background = OxyColors.White
      };

      LinearAxis xAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash
      };
      LinearAxis yAxis = new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "f(x)",
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash
      };
      plotModel.Axes.Add(xAxis);
      plotModel.Axes.Add(yAxis);

      AddFunctionSeries(plotModel, expression, lowerBound, upperBound);

      switch (method)
      {
        case IntegrationMethod.Trapezoidal:
          AddTrapezoidalVisualization(plotModel, expression, lowerBound, upperBound, methodColor, partitions);
          break;
        case IntegrationMethod.Simpson:
          AddSimpsonVisualization(plotModel, expression, lowerBound, upperBound, methodColor, partitions);
          break;
        default:
          AddRectangleVisualization(plotModel, expression, lowerBound, upperBound, method, methodColor, partitions);
          break;
      }

      return plotModel;
    }

    private void AddFunctionSeries(PlotModel plotModel, string expression, double lowerBound, double upperBound)
    {
      LineSeries functionSeries = new LineSeries
      {
        Title = "f(x)",
        Color = OxyColors.Blue,
        StrokeThickness = 3
      };

      int pointsCount = 200;
      double stepSize = (upperBound - lowerBound) / pointsCount;

      for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
      {
        double xCoordinate = lowerBound + pointIndex * stepSize;
        try
        {
          double yCoordinate = EvaluateFunction(expression, xCoordinate);
          functionSeries.Points.Add(new DataPoint(xCoordinate, yCoordinate));
        }
        catch { }
      }

      plotModel.Series.Add(functionSeries);
    }

    private void AddRectangleVisualization(PlotModel plotModel, string expression, double lowerBound,
                                          double upperBound, IntegrationMethod method, Color methodColor, int partitions)
    {
      double stepSize = (upperBound - lowerBound) / partitions;
      OxyColor oxyColor = OxyColor.FromRgb(methodColor.R, methodColor.G, methodColor.B);

      int maxDisplayRectangles = 100;
      int displayStep = Math.Max(1, partitions / maxDisplayRectangles);

      for (int partitionIndex = 0; partitionIndex < partitions; partitionIndex += displayStep)
      {
        double xLeft = lowerBound + partitionIndex * stepSize;
        double xRight = xLeft + stepSize * displayStep;
        double rectangleHeight = 0;

        switch (method)
        {
          case IntegrationMethod.LeftRectangle:
            rectangleHeight = EvaluateFunction(expression, xLeft);
            break;
          case IntegrationMethod.RightRectangle:
            rectangleHeight = EvaluateFunction(expression, xRight);
            break;
          case IntegrationMethod.MidpointRectangle:
            rectangleHeight = EvaluateFunction(expression, (xLeft + xRight) / 2);
            break;
        }

        RectangleAnnotation rectangle = new RectangleAnnotation
        {
          MinimumX = xLeft,
          MaximumX = xRight,
          MinimumY = 0,
          MaximumY = rectangleHeight,
          Fill = OxyColor.FromArgb(80, methodColor.R, methodColor.G, methodColor.B),
          Stroke = oxyColor,
          StrokeThickness = 1
        };

        plotModel.Annotations.Add(rectangle);
      }
    }

    private void AddTrapezoidalVisualization(PlotModel plotModel, string expression, double lowerBound,
                                            double upperBound, Color methodColor, int partitions)
    {
      double stepSize = (upperBound - lowerBound) / partitions;
      OxyColor oxyColor = OxyColor.FromRgb(methodColor.R, methodColor.G, methodColor.B);

      int maxDisplayTrapezoids = 100;
      int displayStep = Math.Max(1, partitions / maxDisplayTrapezoids);

      for (int partitionIndex = 0; partitionIndex < partitions; partitionIndex += displayStep)
      {
        double xLeft = lowerBound + partitionIndex * stepSize;
        double xRight = xLeft + stepSize * displayStep;
        double yLeft = EvaluateFunction(expression, xLeft);
        double yRight = EvaluateFunction(expression, xRight);

        PolygonAnnotation polygon = new PolygonAnnotation
        {
          Fill = OxyColor.FromArgb(80, methodColor.R, methodColor.G, methodColor.B),
          Stroke = oxyColor,
          StrokeThickness = 1
        };

        polygon.Points.Add(new DataPoint(xLeft, 0));
        polygon.Points.Add(new DataPoint(xLeft, yLeft));
        polygon.Points.Add(new DataPoint(xRight, yRight));
        polygon.Points.Add(new DataPoint(xRight, 0));

        plotModel.Annotations.Add(polygon);
      }
    }

    private void AddSimpsonVisualization(PlotModel plotModel, string expression, double lowerBound,
                                        double upperBound, Color methodColor, int partitions)
    {
      double stepSize = (upperBound - lowerBound) / partitions;
      OxyColor oxyColor = OxyColor.FromRgb(methodColor.R, methodColor.G, methodColor.B);

      int maxDisplayParabolas = 30;
      int displayStep = Math.Max(2, partitions / maxDisplayParabolas);
      if (displayStep % 2 != 0) ++displayStep;

      for (int partitionIndex = 0; partitionIndex < partitions; partitionIndex += displayStep)
      {
        double xStart = lowerBound + partitionIndex * stepSize;
        double xMiddle = xStart + stepSize;
        double xEnd = xMiddle + stepSize;

        if (xEnd > upperBound) break;

        double yStart = EvaluateFunction(expression, xStart);
        double yMiddle = EvaluateFunction(expression, xMiddle);
        double yEnd = EvaluateFunction(expression, xEnd);

        LineSeries parabolaSeries = new LineSeries
        {
          Color = oxyColor,
          StrokeThickness = 3
        };

        int parabolaPoints = 50;
        for (int pointIndex = 0; pointIndex <= parabolaPoints; ++pointIndex)
        {
          double interpolationParameter = (double)pointIndex / parabolaPoints;
          double xCoordinate = xStart + interpolationParameter * (xEnd - xStart);
          double yCoordinate = LagrangeInterpolation(xCoordinate, xStart, xMiddle, xEnd, yStart, yMiddle, yEnd);
          parabolaSeries.Points.Add(new DataPoint(xCoordinate, yCoordinate));
        }

        plotModel.Series.Add(parabolaSeries);

        AreaSeries areaSeries = new AreaSeries
        {
          Color = OxyColors.Transparent,
          Fill = OxyColor.FromArgb(60, methodColor.R, methodColor.G, methodColor.B),
          StrokeThickness = 0
        };

        for (int pointIndex = 0; pointIndex <= parabolaPoints; ++pointIndex)
        {
          double interpolationParameter = (double)pointIndex / parabolaPoints;
          double xCoordinate = xStart + interpolationParameter * (xEnd - xStart);
          double yCoordinate = LagrangeInterpolation(xCoordinate, xStart, xMiddle, xEnd, yStart, yMiddle, yEnd);
          areaSeries.Points.Add(new DataPoint(xCoordinate, yCoordinate));
          areaSeries.Points2.Add(new DataPoint(xCoordinate, 0));
        }

        plotModel.Series.Add(areaSeries);
      }
    }

    private double LagrangeInterpolation(double x, double x0, double x1, double x2, double y0, double y1, double y2)
    {
      double term0 = y0 * (x - x1) * (x - x2) / ((x0 - x1) * (x0 - x2));
      double term1 = y1 * (x - x0) * (x - x2) / ((x1 - x0) * (x1 - x2));
      double term2 = y2 * (x - x0) * (x - x1) / ((x2 - x0) * (x2 - x1));
      return term0 + term1 + term2;
    }

    public int GetDecimalPlaces(double epsilon)
    {
      if (epsilon <= 0) return 6;
      return Math.Max(1, (int)Math.Ceiling(-Math.Log10(epsilon)));
    }

    public Color GetMethodColor(IntegrationMethod method)
    {
      return method switch
      {
        IntegrationMethod.LeftRectangle => Colors.Red,
        IntegrationMethod.RightRectangle => Colors.Orange,
        IntegrationMethod.MidpointRectangle => Colors.Purple,
        IntegrationMethod.Trapezoidal => Colors.Green,
        IntegrationMethod.Simpson => Colors.Red,
        _ => Colors.Blue
      };
    }
  }
}
