public class DichotomyModel
{
  private const int MaxIterationsCount = 1000;

  public CalculationResult FindRoot(string functionExpression, double startInterval, double endInterval, double epsilon)
  {
    if (!HasRootOnInterval(functionExpression, startInterval, endInterval))
    {
      return new CalculationResult
      {
        Success = false,
        Message = "Функция не меняет знак на заданном интервале"
      };
    }

    try
    {
      double root = FindRootInternal(functionExpression, startInterval, endInterval, epsilon);
      double functionValue = EvaluateFunction(functionExpression, root);
      
      return new CalculationResult
      {
        Success = true,
        Root = root,
        FunctionValue = functionValue
      };
    }
    catch (Exception ex)
    {
      return new CalculationResult
      {
        Success = false,
        Message = $"Ошибка при поиске корня: {ex.Message}"
      };
    }
  }

  private double FindRootInternal(string functionExpression, double startInterval, double endInterval, double epsilon)
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
    try
    {
      if (string.IsNullOrWhiteSpace(functionExpression))
        throw new ArgumentException("Функция не задана");

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

      if (result == null)
        throw new ArgumentException("Не удалось вычислить выражение");

      double value = Convert.ToDouble(result);

      if (double.IsInfinity(value) || double.IsNaN(value))
        throw new ArgumentException("Функция не определена в данной точке");

      return value;
    }
    catch (Exception ex)
    {
      throw new ArgumentException($"Невозможно вычислить функцию: {ex.Message}");
    }
  }

  public bool HasRootOnInterval(string functionExpression, double startInterval, double endInterval)
  {
    try
    {
      double functionAtStart = EvaluateFunction(functionExpression, startInterval);
      double functionAtEnd = EvaluateFunction(functionExpression, endInterval);
      return functionAtStart * functionAtEnd <= 0;
    }
    catch
    {
      return false;
    }
  }

  public double ParseDouble(string text)
  {
    text = text.Replace(',', '.');
    if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
    {
      return result;
    }
    throw new ArgumentException("Некорректное числовое значение");
  }

  public bool IsValidNumber(string text)
  {
    try
    {
      ParseDouble(text);
      return true;
    }
    catch
    {
      return false;
    }
  }
}

public class CalculationResult
{
  public bool Success { get; set; }
  public double Root { get; set; }
  public double FunctionValue { get; set; }
  public string Message { get; set; }
}
