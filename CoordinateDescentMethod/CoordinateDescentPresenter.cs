using System;
using System.Globalization;

namespace NumericalMethodsApp
{
  public class CoordinateDescentPresenter
  {
    private readonly ICoordinateDescentView view;
    private readonly CoordinateDescentModel model;

    public CoordinateDescentPresenter(ICoordinateDescentView view)
    {
      this.view = view;
      this.model = new CoordinateDescentModel();

      this.view.CalculateClicked += OnCalculateClicked;
      this.view.ClearAllClicked += OnClearAllClicked;
      this.view.HelpClicked += OnHelpClicked;
    }

    private double epsilon;
    private void OnCalculateClicked(object sender, EventArgs e)
    {
      try
      {
        string functionExpression = view.FunctionExpression?.Trim();
        if (string.IsNullOrWhiteSpace(functionExpression))
        {
          view.ShowError("Введите функцию f(x, y)");
          return;
        }

        if (!model.SetFunction(functionExpression))
        {
          view.ShowError("Неверное выражение функции. Используйте x и y как переменные.");
          return;
        }

        if (!ValidateAndParseInput(view.XStart, "начального x", out double xStart))
        {
          view.ShowError("Неверное значение начального x");
          return;
        }

        if (!ValidateAndParseInput(view.YStart, "начального y", out double yStart))
        {
          view.ShowError("Неверное значение начального y");
          return;
        }

        if (!ValidateAndParseInput(view.Epsilon, "точности", out double epsilon) || epsilon <= 0)
        {
          view.ShowError("Точность должна быть положительным числом");
          return;
        }

        if (epsilon > 1)
        {
          view.ShowError("Точность должна быть меньше или равна 1");
          return;
        }

        this.epsilon = epsilon;

        model.SetInitialPoint(xStart, yStart);
        model.SetEpsilon(epsilon);

        var optimizationResult = model.PerformOptimization();
        if (!optimizationResult.IsSuccessful)
        {
          view.ShowError(optimizationResult.ErrorMessage);
          return;
        }

        DisplayResults();
        var plotModel = model.CreatePlotModel();
        view.UpdatePlot(plotModel);
      }
      catch (Exception ex)
      {
        view.ShowError($"Непредвиденная ошибка: {ex.Message}");
      }
    }

    private bool ValidateAndParseInput(string input, string parameterName, out double value)
    {
      value = 0;
      if (string.IsNullOrWhiteSpace(input))
      {
        view.ShowError($"Введите значение {parameterName}");
        return false;
      }

      return double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private void DisplayResults()
    {
      int decimalPlaces = 0;
      double tempEpsilon = epsilon;

      while (tempEpsilon < 1)
      {
        decimalPlaces++;
        tempEpsilon *= 10;
      }

      string format = $"F{decimalPlaces}";

      string resultText = "Найдена точка минимума функции:\n";
      resultText += $"x = {model.MinimumPoint.X.ToString(format, CultureInfo.InvariantCulture)}\n";
      resultText += $"y = {model.MinimumPoint.Y.ToString(format, CultureInfo.InvariantCulture)}";

      view.Result = resultText;
    }

    private void OnClearAllClicked(object sender, EventArgs e)
    {
      view.ClearResults();
    }

    private void OnHelpClicked(object sender, EventArgs e)
    {
      string helpMessage = "Метод покоординатного спуска\n\n" +
                          "1. Введите функцию f(x, y) в поле 'Функция'\n" +
                          "2. Укажите начальные приближения для x и y\n" +
                          "3. Задайте точность вычислений ε (0 < ε ≤ 1)\n" +
                          "4. Нажмите 'Вычислить'\n\n" +
                          "Примеры функций:\n" +
                          "x^2 + y^2\n" +
                          "sin(x) + cos(y)\n" +
                          "exp(-(x^2 + y^2))\n" +
                          "x^2 + 2*y^2 - 4*x + 4*y\n\n" +
                          "Доступные операторы: +, -, *, /, ^\n" +
                          "Доступные функции: sin, cos, tan, exp, log, sqrt, abs";

      System.Windows.MessageBox.Show(helpMessage, "Справка",
          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
  }
}
