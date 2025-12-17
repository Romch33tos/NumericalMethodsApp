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
  }
}
