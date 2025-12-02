using System;
using System.Windows.Media;

namespace NumericalMethodsApp.DefiniteIntegralMethod
{
  public class DefiniteIntegralPresenter
  {
    private readonly IDefiniteIntegralView view;
    private readonly DefiniteIntegralModel model;

    public DefiniteIntegralPresenter(IDefiniteIntegralView view)
    {
      this.view = view;
      this.model = new DefiniteIntegralModel();
      SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
      view.CalculateRequested += OnCalculateRequested;
      view.ClearAllRequested += OnClearAllRequested;
      view.HelpRequested += OnHelpRequested;
      view.MethodChanged += OnMethodChanged;
    }

    private void OnCalculateRequested(object sender, EventArgs eventArgs)
    {
      try
      {
        if (!ValidateInput()) return;

        (double result, int partitions) = model.CalculateIntegral(
          view.SelectedMethod,
          view.FunctionExpression,
          view.LowerBound,
          view.UpperBound,
          view.Epsilon
        );

        view.ResultText = $"Значение интеграла: {result:F6}\nКоличество разбиений: {partitions}";

        Color methodColor = model.GetMethodColor(view.SelectedMethod);
        view.PlotModel = model.CreatePlotModel(
          view.FunctionExpression,
          view.LowerBound,
          view.UpperBound,
          view.SelectedMethod,
          methodColor,
          partitions
        );
      }
      catch (Exception exception)
      {
        view.ShowError($"Ошибка при вычислении: {exception.Message}");
      }
    }

    private bool ValidateInput()
    {
      if (string.IsNullOrWhiteSpace(view.FunctionExpression))
      {
        view.ShowWarning("Введите функцию для интегрирования");
        return false;
      }

      if (view.LowerBound >= view.UpperBound)
      {
        view.ShowWarning("Нижняя граница должна быть меньше верхней");
        return false;
      }

      if (view.Epsilon <= 0)
      {
        view.ShowWarning("Точность должна быть положительным числом");
        return false;
      }

      try
      {
        double testValue = (view.LowerBound + view.UpperBound) / 2;
        double result = model.EvaluateFunction(view.FunctionExpression, testValue);
        double result2 = model.EvaluateFunction(view.FunctionExpression, testValue + 1);

        if (Math.Abs(result - result2) < 1e-10 && !model.IsConstantExpression(view.FunctionExpression))
        {
          view.ShowWarning("Функция возвращает постоянное значение для разных x. Проверьте выражение.");
          return false;
        }
      }
      catch (DivideByZeroException)
      {
        view.ShowWarning("Обнаружено деление на ноль в функции");
        return false;
      }
      catch (Exception exception)
      {
        view.ShowWarning($"Некорректная функция или ошибка в выражении: {exception.Message}");
        return false;
      }

      return true;
    }

    private void OnClearAllRequested(object sender, EventArgs eventArgs)
    {
      view.FunctionExpression = "";
      view.LowerBound = 0;
      view.UpperBound = 1;
      view.Epsilon = 0.001;
      view.ResultText = "";
      view.SelectedMethod = IntegrationMethod.LeftRectangle;

      OxyPlot.PlotModel plotModel = new OxyPlot.PlotModel
      {
        PlotAreaBorderColor = OxyPlot.OxyColors.LightGray,
        PlotAreaBorderThickness = new OxyPlot.OxyThickness(1),
        Background = OxyPlot.OxyColors.White
      };

      OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis
      {
        Position = OxyPlot.Axes.AxisPosition.Bottom,
        Title = "x",
        MajorGridlineColor = OxyPlot.OxyColors.LightGray,
        MajorGridlineStyle = OxyPlot.LineStyle.Dash
      };

      OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis
      {
        Position = OxyPlot.Axes.AxisPosition.Left,
        Title = "f(x)",
        MajorGridlineColor = OxyPlot.OxyColors.LightGray,
        MajorGridlineStyle = OxyPlot.LineStyle.Dash
      };

      plotModel.Axes.Add(xAxis);
      plotModel.Axes.Add(yAxis);
      view.PlotModel = plotModel;
    }

    private void OnHelpRequested(object sender, EventArgs eventArgs)
    {
      string helpText = "Инструкция по использованию:\n\n" +
                       "1. Введите функцию f(x)\n" +
                       "2. Укажите границы интегрирования a и b\n" +
                       "3. Задайте точность вычислений ε\n" +
                       "4. Выберите метод интегрирования\n" +
                       "5. Нажмите 'Вычислить' для расчета\n\n" +
                       "Поддерживаемые операции: +, -, *, /, ^, sin, cos, tan, log, exp, sqrt, abs, asin, acos, atan\n" +
                       "Константы: pi, e\n" +
                       "Примеры функций: x^2 + 2*x + 1, sin(x)*cos(x), exp(-x^2), sqrt(1-x^2)";

      view.ShowInformation(helpText);
    }

    private void OnMethodChanged(object sender, IntegrationMethod method)
    {
      view.SelectedMethod = method;
    }
  }
}