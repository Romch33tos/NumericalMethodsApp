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
