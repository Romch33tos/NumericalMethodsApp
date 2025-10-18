public class DichotomyModel
{
  private const int MaxIterationsCount = 1000;
  private const double SafetyOffset = 1e-10;

  public CalculationResult FindRoot(string functionExpression, double startInterval, double endInterval, double epsilon)
  {
    if (!HasRootOnInterval(functionExpression, startInterval, endInterval))
    {
      return new CalculationResult
      {
        Success = false,
        Message = "Функция не меняет знак на заданном интервале. Возможно, корней несколько, их нет, или функция имеет разрыв."
      };
    }

    try
    {
      double functionAtStart = EvaluateFunction(functionExpression, startInterval);
      double functionAtEnd = EvaluateFunction(functionExpression, endInterval);

      if (Math.Abs(functionAtStart) < epsilon)
      {
        return new CalculationResult
        {
          Success = true,
          Root = startInterval,
          FunctionValue = functionAtStart,
          Iterations = 0
        };
      }

      if (Math.Abs(functionAtEnd) < epsilon)
      {
        return new CalculationResult
        {
          Success = true,
          Root = endInterval,
          FunctionValue = functionAtEnd,
          Iterations = 0
        };
      }

      double currentStart = startInterval;
      double currentEnd = endInterval;
      double functionAtCurrentStart = functionAtStart;
      int iterationCount = 0;

      while (Math.Abs(currentEnd - currentStart) > epsilon && iterationCount < MaxIterationsCount)
      {
        double midpoint = (currentStart + currentEnd) / 2;
        double functionAtMidpoint = EvaluateFunction(functionExpression, midpoint);

        if (Math.Abs(functionAtMidpoint) < epsilon)
        {
          return new CalculationResult
          {
            Success = true,
            Root = midpoint,
            FunctionValue = functionAtMidpoint,
            Iterations = iterationCount + 1
          };
        }

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

      double finalRoot = (currentStart + currentEnd) / 2;
      double finalFunctionValue = EvaluateFunction(functionExpression, finalRoot);

      return new CalculationResult
      {
        Success = true,
        Root = finalRoot,
        FunctionValue = finalFunctionValue,
        Iterations = iterationCount
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

  public double EvaluateFunction(string functionExpression, double xValue)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(functionExpression))
        throw new ArgumentException("Функция не задана");

      string expression = functionExpression.Trim().Replace(',', '.');
      
      if (IsConstantExpression(expression))
        throw new ArgumentException("Функция должна зависеть от переменной x");

      expression = System.Text.RegularExpressions.Regex.Replace(expression, @"(\w+)\s*\^\s*(\w+)", "pow($1, $2)");

      CheckForDivisionByZero(expression, xValue);

      Expression ncalcExpression = new Expression(expression);
      ncalcExpression.Parameters["x"] = xValue;

      ncalcExpression.EvaluateFunction += delegate (string name, FunctionArgs args)
      {
        try
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
          {
            double argument = Convert.ToDouble(args.Parameters[0].Evaluate());
            if (argument <= 0) throw new ArgumentException("Логарифм от неположительного числа");
            args.Result = Math.Log(argument);
          }
          else if (name.Equals("log10", StringComparison.OrdinalIgnoreCase))
          {
            double argument = Convert.ToDouble(args.Parameters[0].Evaluate());
            if (argument <= 0) throw new ArgumentException("Логарифм от неположительного числа");
            args.Result = Math.Log10(argument);
          }
          else if (name.Equals("sqrt", StringComparison.OrdinalIgnoreCase))
          {
            double argument = Convert.ToDouble(args.Parameters[0].Evaluate());
            if (argument < 0) throw new ArgumentException("Квадратный корень от отрицательного числа");
            args.Result = Math.Sqrt(argument);
          }
          else if (name.Equals("abs", StringComparison.OrdinalIgnoreCase))
            args.Result = Math.Abs(Convert.ToDouble(args.Parameters[0].Evaluate()));
          else if (name.Equals("pow", StringComparison.OrdinalIgnoreCase))
          {
            double baseValue = Convert.ToDouble(args.Parameters[0].Evaluate());
            double exponent = Convert.ToDouble(args.Parameters[1].Evaluate());
            args.Result = Math.Pow(baseValue, exponent);
          }
        }
        catch (Exception ex)
        {
          throw new ArgumentException($"Ошибка вычисления функции {name}: {ex.Message}");
        }
      };

      object result = ncalcExpression.Evaluate();

      if (result == null)
        throw new ArgumentException("Не удалось вычислить выражение");

      double value = Convert.ToDouble(result);

      if (double.IsInfinity(value))
        throw new ArgumentException("Функция стремится к бесконечности в данной точке");

      if (double.IsNaN(value))
        throw new ArgumentException("Функция не определена в данной точке");

      return value;
    }
    catch (Exception ex)
    {
      throw new ArgumentException($"Невозможно вычислить функцию '{functionExpression}' при x={xValue}: {ex.Message}");
    }
  }

  public bool HasRootOnInterval(string functionExpression, double startInterval, double endInterval)
  {
    try
    {
      double functionAtStart = EvaluateFunction(functionExpression, startInterval);
      double functionAtEnd = EvaluateFunction(functionExpression, endInterval);

      if (double.IsInfinity(functionAtStart) || double.IsInfinity(functionAtEnd) ||
          double.IsNaN(functionAtStart) || double.IsNaN(functionAtEnd))
        return false;

      return functionAtStart * functionAtEnd <= 0;
    }
    catch
    {
      return false;
    }
  }

  public List<double> FindDiscontinuityPoints(string functionExpression, double startInterval, double endInterval)
  {
    List<double> discontinuityPoints = new List<double>();
    int checkPoints = 100;
    double step = (endInterval - startInterval) / checkPoints;

    for (int counter = 0; counter <= checkPoints; ++counter)
    {
      double x = startInterval + counter * step;
      try
      {
        EvaluateFunction(functionExpression, x);
      }
      catch
      {
        discontinuityPoints.Add(x);
      }
    }

    return discontinuityPoints;
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

  public bool IsConstantExpression(string expression)
  {
    string cleanExpression = expression.ToLower().Replace(" ", "");

    if (!cleanExpression.Contains("x"))
      return true;

    if (cleanExpression.Replace("x", "").Replace("+", "").Replace("-", "").Replace("*", "").Replace("/", "").Replace("^", "").Replace("(", "").Replace(")", "").Length == 0)
      return false;

    string testExpression = cleanExpression.Replace("x", "1");
    try
    {
      Expression expr1 = new Expression(testExpression);
      object result1 = expr1.Evaluate();

      testExpression = cleanExpression.Replace("x", "2");
      Expression expr2 = new Expression(testExpression);
      object result2 = expr2.Evaluate();

      if (result1 != null && result2 != null)
      {
        double val1 = Convert.ToDouble(result1);
        double val2 = Convert.ToDouble(result2);
        return Math.Abs(val1 - val2) < 1e-10;
      }
    }
    catch
    {
    }

    return false;
  }

  private void CheckForDivisionByZero(string expression, double xValue)
  {
    if (Math.Abs(xValue) < SafetyOffset)
    {
      string lowerExpression = expression.ToLower();
      if (lowerExpression.Contains("/x") || lowerExpression.Contains("/ x") ||
          lowerExpression.Contains("/(x)") || lowerExpression.Contains("/ (x)"))
      {
        throw new ArgumentException("Деление на ноль");
      }
    }
  }
}

public class CalculationResult
{
  public bool Success { get; set; }
  public double Root { get; set; }
  public double FunctionValue { get; set; }
  public int Iterations { get; set; }
  public string Message { get; set; }
}
