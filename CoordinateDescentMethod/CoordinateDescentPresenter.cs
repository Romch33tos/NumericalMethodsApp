using System;
using System.Globalization;

namespace NumericalMethodsApp
{
  public class CoordinateDescentPresenter
  {
    private readonly ICoordinateDescentView view;
    private readonly CoordinateDescentModel model;
    private double epsilon;

    public CoordinateDescentPresenter(ICoordinateDescentView view)
    {
      this.view = view;
      this.model = new CoordinateDescentModel();

      this.view.CalculateClicked += OnCalculateClicked;
      this.view.ClearAllClicked += OnClearAllClicked;
      this.view.HelpClicked += OnHelpClicked;
    }

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
  }
}
