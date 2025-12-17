using System;
using System.Collections.Generic;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public class DataPoint
  {
    public double X { get; set; }
    public double Y { get; set; }

    public DataPoint(double x, double y)
    {
      if (double.IsNaN(x) || double.IsInfinity(x))
        throw new ArgumentException("Недопустимое значение X.");
      if (double.IsNaN(y) || double.IsInfinity(y))
        throw new ArgumentException("Недопустимое значение Y.");

      X = x;
      Y = y;
    }
  }

  public class LeastSquaresModel
  {
    public List<DataPoint> Points { get; } = new List<DataPoint>();
    public int Dimension { get; set; } = 3;
    public double RangeStart { get; set; } = 0;
    public double RangeEnd { get; set; } = 10;
    public double Precision { get; set; } = 0.001;

    public void UpdateDimension(int newDimension)
    {
      if (newDimension < 1)
        throw new ArgumentException("Размерность должна быть положительной.");

      Dimension = newDimension;

      while (Points.Count < Dimension)
      {
        Points.Add(new DataPoint(0, 0));
      }

      while (Points.Count > Dimension)
      {
        Points.RemoveAt(Points.Count - 1);
      }
    }

    public double[] CalculatePolynomialCoefficients()
    {
      if (Points.Count < 2)
        return null;

      foreach (var point in Points)
      {
        if (double.IsNaN(point.X) || double.IsInfinity(point.X) ||
            double.IsNaN(point.Y) || double.IsInfinity(point.Y))
          return null;
      }

      int pointCount = Points.Count;
      double[,] matrix = new double[pointCount, pointCount + 1];

      for (int rowIndex = 0; rowIndex < pointCount; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < pointCount; ++columnIndex)
        {
          double value = Math.Pow(Points[rowIndex].X, columnIndex);
          if (double.IsNaN(value) || double.IsInfinity(value))
            return null;
          matrix[rowIndex, columnIndex] = value;
        }
        matrix[rowIndex, pointCount] = Points[rowIndex].Y;
      }

      for (int pivotIndex = 0; pivotIndex < pointCount; ++pivotIndex)
      {
        double maxElement = Math.Abs(matrix[pivotIndex, pivotIndex]);
        int maxRowIndex = pivotIndex;

        for (int rowIndex = pivotIndex + 1; rowIndex < pointCount; ++rowIndex)
        {
          double absValue = Math.Abs(matrix[rowIndex, pivotIndex]);
          if (double.IsNaN(absValue) || double.IsInfinity(absValue))
            return null;
          if (absValue > maxElement)
          {
            maxElement = absValue;
            maxRowIndex = rowIndex;
          }
        }

        if (maxElement < double.Epsilon)
          return null;

        if (maxRowIndex != pivotIndex)
        {
          for (int columnIndex = pivotIndex; columnIndex <= pointCount; ++columnIndex)
          {
            double temporaryValue = matrix[pivotIndex, columnIndex];
            matrix[pivotIndex, columnIndex] = matrix[maxRowIndex, columnIndex];
            matrix[maxRowIndex, columnIndex] = temporaryValue;
          }
        }

        double pivotValue = matrix[pivotIndex, pivotIndex];
        if (Math.Abs(pivotValue) < double.Epsilon)
          return null;

        for (int rowIndex = pivotIndex + 1; rowIndex < pointCount; ++rowIndex)
        {
          double factor = matrix[rowIndex, pivotIndex] / pivotValue;
          if (double.IsNaN(factor) || double.IsInfinity(factor))
            return null;

          for (int columnIndex = pivotIndex; columnIndex <= pointCount; ++columnIndex)
          {
            double newValue = matrix[rowIndex, columnIndex] - factor * matrix[pivotIndex, columnIndex];
            if (double.IsNaN(newValue) || double.IsInfinity(newValue))
              return null;
            matrix[rowIndex, columnIndex] = newValue;
          }
        }
      }

      double[] coefficients = new double[pointCount];
      for (int coefficientIndex = pointCount - 1; coefficientIndex >= 0; --coefficientIndex)
      {
        double divisor = matrix[coefficientIndex, coefficientIndex];
        if (Math.Abs(divisor) < double.Epsilon)
          return null;

        coefficients[coefficientIndex] = matrix[coefficientIndex, pointCount];
        for (int columnIndex = coefficientIndex + 1; columnIndex < pointCount; ++columnIndex)
        {
          coefficients[coefficientIndex] -= matrix[coefficientIndex, columnIndex] * coefficients[columnIndex];
        }
        coefficients[coefficientIndex] /= divisor;

        if (double.IsNaN(coefficients[coefficientIndex]) || double.IsInfinity(coefficients[coefficientIndex]))
          return null;
      }

      return coefficients;
    }
  }
}
