using NCalc;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
    private double searchStep;
    private const int MaxIterations = 10000;
    private const double SearchRange = 2.0;

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

    public bool SetStepSize(double stepValue)
    {
      if (stepValue <= 0)
        return false;

      searchStep = stepValue;
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

      double optimalX = leftBound;
      double minValue = EvaluateFunction(leftBound, fixedY);

      if (double.IsNaN(minValue))
        return currentX;

      for (double candidateX = leftBound + searchStep; candidateX <= rightBound; candidateX += searchStep)
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

      double optimalY = lowerBound;
      double minValue = EvaluateFunction(fixedX, lowerBound);

      if (double.IsNaN(minValue))
        return currentY;

      for (double candidateY = lowerBound + searchStep; candidateY <= upperBound; candidateY += searchStep)
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

    public PlotModel CreatePlotModel()
    {
      var plotModel = new PlotModel
      {
        Title = "",
      };

      SetupAxes(plotModel);
      AddContourLines(plotModel);
      AddOptimizationPath(plotModel);
      AddSpecialPoints(plotModel);

      return plotModel;
    }

    private void SetupAxes(PlotModel plotModel)
    {
      var xAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        Minimum = -5,
        Maximum = 5,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0)
      };
      plotModel.Axes.Add(xAxis);

      var yAxis = new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "y",
        Minimum = -5,
        Maximum = 5,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0)
      };
      plotModel.Axes.Add(yAxis);
    }

    private void AddContourLines(PlotModel plotModel)
    {
      const int gridResolution = 50;
      double[,] functionValues = new double[gridResolution, gridResolution];

      double minX = -5;
      double maxX = 5;
      double minY = -5;
      double maxY = 5;

      double xIncrement = (maxX - minX) / (gridResolution - 1);
      double yIncrement = (maxY - minY) / (gridResolution - 1);

      for (int rowIndex = 0; rowIndex < gridResolution; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < gridResolution; ++columnIndex)
        {
          double xCoordinate = minX + rowIndex * xIncrement;
          double yCoordinate = minY + columnIndex * yIncrement;
          functionValues[rowIndex, columnIndex] = EvaluateFunction(xCoordinate, yCoordinate);
        }
      }

      var contourSeries = new ContourSeries
      {
        ColumnCoordinates = GenerateCoordinateArray(minX, maxX, gridResolution),
        RowCoordinates = GenerateCoordinateArray(minY, maxY, gridResolution),
        Data = functionValues,
        ContourLevels = new double[] { 1, 2, 5, 10, 20, 30, 50 },
        ContourColors = new[] { OxyColors.LightBlue, OxyColors.Blue, OxyColors.DarkBlue },
        LabelBackground = OxyColors.Transparent,
        StrokeThickness = 1
      };

      plotModel.Series.Add(contourSeries);
    }

    private double[] GenerateCoordinateArray(double minimum, double maximum, int pointCount)
    {
      double[] coordinates = new double[pointCount];
      double step = (maximum - minimum) / (pointCount - 1);

      for (int index = 0; index < pointCount; ++index)
      {
        coordinates[index] = minimum + index * step;
      }

      return coordinates;
    }

    private void AddOptimizationPath(PlotModel plotModel)
    {
      if (OptimizationPath.Count < 2)
        return;

      var pathSeries = new LineSeries
      {
        Title = "Траектория",
        Color = OxyColors.Red,
        StrokeThickness = 2,
        MarkerType = MarkerType.Circle,
        MarkerSize = 3,
        MarkerFill = OxyColors.Red
      };

      foreach (var point in OptimizationPath)
      {
        pathSeries.Points.Add(new DataPoint(point.X, point.Y));
      }

      plotModel.Series.Add(pathSeries);
    }

    private void AddSpecialPoints(PlotModel plotModel)
    {
      if (OptimizationPath.Count == 0)
        return;

      var startPoint = OptimizationPath[0];
      var startSeries = new ScatterSeries
      {
        Title = "Начальная точка",
        MarkerType = MarkerType.Circle,
        MarkerSize = 7,
        MarkerFill = OxyColors.Green
      };
      startSeries.Points.Add(new ScatterPoint(startPoint.X, startPoint.Y));
      plotModel.Series.Add(startSeries);

      if (OptimizationPath.Count > 1)
      {
        var endPoint = OptimizationPath[OptimizationPath.Count - 1];
        var endSeries = new ScatterSeries
        {
          Title = "Найденный минимум",
          MarkerType = MarkerType.Diamond,
          MarkerSize = 9,
          MarkerFill = OxyColors.Orange
        };
        endSeries.Points.Add(new ScatterPoint(endPoint.X, endPoint.Y));
        plotModel.Series.Add(endSeries);
      }
    }
  }

  public class Point2D
  {
    public double X { get; set; }
    public double Y { get; set; }

    public Point2D(double x, double y)
    {
      X = x;
      Y = y;
    }
  }

  public class OptimizationResult
  {
    public bool IsSuccessful { get; }
    public string ErrorMessage { get; }

    private OptimizationResult(bool isSuccessful, string errorMessage)
    {
      IsSuccessful = isSuccessful;
      ErrorMessage = errorMessage;
    }

    public static OptimizationResult Success()
    {
      return new OptimizationResult(true, null);
    }

    public static OptimizationResult Failure(string errorMessage)
    {
      return new OptimizationResult(false, errorMessage);
    }
  }
}
