public class DichotomyModel
{
  private const int MaxIterationsCount = 1000;

  public double FindRoot(string functionExpression, double startInterval, double endInterval, double epsilon)
  {
    double functionAtStart = EvaluateFunction(functionExpression, startInterval);
    double functionAtEnd = EvaluateFunction(functionExpression, endInterval);

    if (Math.Abs(functionAtStart) < epsilon)
      return startInterval;

    if (Math.Abs(functionAtEnd) < epsilon)
      return endInterval;

    double currentStart = startInterval;
    double currentEnd = endInterval;
    double functionAtCurrentStart = functionAtStart;
    int iterationCount = 0;

    while (Math.Abs(currentEnd - currentStart) > epsilon && iterationCount < MaxIterationsCount)
    {
      double midpoint = (currentStart + currentEnd) / 2;
      double functionAtMidpoint = EvaluateFunction(functionExpression, midpoint);

      if (Math.Abs(functionAtMidpoint) < epsilon)
        return midpoint;

      if (functionAtCurrentStart * functionAtMidpoint < 0)
      {
        currentEnd = midpoint;
      }
      else
      {
        currentStart = midpoint;
        functionAtCurrentStart = functionAtMidpoint;
      }

      ++iterationCount;
    }

    return (currentStart + currentEnd) / 2;
  }

  public double EvaluateFunction(string functionExpression, double xValue)
  {
    string expression = functionExpression.Trim().Replace(',', '.');
    expression = System.Text.RegularExpressions.Regex.Replace(expression, @"(\w+)\s*\^\s*(\w+)", "pow($1, $2)");

    Expression ncalcExpression = new Expression(expression);
    ncalcExpression.Parameters["x"] = xValue;

    ncalcExpression.EvaluateFunction += delegate (string name, FunctionArgs args)
    {
      if (name.Equals("sin", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Sin(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("cos", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Cos(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("tan", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Tan(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("exp", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Exp(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("log", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Log(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("log10", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Log10(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("sqrt", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Sqrt(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("abs", StringComparison.OrdinalIgnoreCase))
        args.Result = Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate()));
      else if (name.Equals("pow", StringComparison.OrdinalIgnoreCase))
      {
        double baseValue = Convert.ToDouble(args.Parameters[0].Evaluate());
        double exponent = Convert.ToDouble(args.Parameters[1].Evaluate());
        args.Result = Math.Pow(baseValue, exponent);
      }
    };

    object result = ncalcExpression.Evaluate();
    return Convert.ToDouble(result);
  }

  public bool HasRootOnInterval(string functionExpression, double startInterval, double endInterval)
  {
    double functionAtStart = EvaluateFunction(functionExpression, startInterval);
    double functionAtEnd = EvaluateFunction(functionExpression, endInterval);
    return functionAtStart * functionAtEnd <= 0;
  }

  public double ParseDouble(string text)
  {
    text = text.Replace(',', '.');
    return double.Parse(text, CultureInfo.InvariantCulture);
  }
}
