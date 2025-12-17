using System;

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
}
