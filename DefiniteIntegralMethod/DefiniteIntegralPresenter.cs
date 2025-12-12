using System;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

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

        string results = "";
        int decimalPlaces = model.GetDecimalPlaces(view.Epsilon);

        if (view.SelectedMethods.Count == 0)
        {
          view.ShowWarning("Выберите хотя бы один метод интегрирования");
          return;
        }

        foreach (IntegrationMethod method in view.SelectedMethods)
        {
          try
          {
            int? partitions = view.UseFixedPartitions ? view.FixedPartitions : (int?)null;

            (double result, int actualPartitions) = model.CalculateIntegral(
                method,
                view.FunctionExpression,
                view.LowerBound,
                view.UpperBound,
                view.Epsilon,
                partitions
            );

            string methodName = GetMethodName(method);
            results += $"{methodName}\n";
            results += $"Значение функции: {result.ToString($"F{decimalPlaces}")}\n";
            results += $"Количество разбиений: {actualPartitions}\n\n";
          }
          catch (Exception ex)
          {
            results += $"{GetMethodName(method)}\n";
            results += $"Ошибка: {ex.Message}\n";
            results += $"Количество разбиений: 0\n\n";
          }
        }

        view.ResultText = results.TrimEnd();

        if (view.SelectedMethods.Count == 1)
        {
          Color methodColor = model.GetMethodColor(view.SelectedMethods[0]);
          view.PlotModel = model.CreatePlotModel(
              view.FunctionExpression,
              view.LowerBound,
              view.UpperBound,
              view.SelectedMethods[0],
              methodColor,
              view.UseFixedPartitions ? view.FixedPartitions : 50
          );
        }
        else
        {
          PlotModel plotModel = new PlotModel
          {
            PlotAreaBorderColor = OxyColors.LightGray,
            PlotAreaBorderThickness = new OxyThickness(1),
            Background = OxyColors.White
          };

          LinearAxis xAxis = new LinearAxis
          {
            Position = AxisPosition.Bottom,
            Title = "x",
            MajorGridlineColor = OxyColors.LightGray,
            MajorGridlineStyle = LineStyle.Dash
          };

          LinearAxis yAxis = new LinearAxis
          {
            Position = AxisPosition.Left,
            Title = "f(x)",
            MajorGridlineColor = OxyColors.LightGray,
            MajorGridlineStyle = LineStyle.Dash
          };

          plotModel.Axes.Add(xAxis);
          plotModel.Axes.Add(yAxis);

          LineSeries functionSeries = new LineSeries
          {
            Title = "f(x)",
            Color = OxyColors.Blue,
            StrokeThickness = 3
          };

          int pointsCount = 200;
          double stepSize = (view.UpperBound - view.LowerBound) / pointsCount;

          for (int pointIndex = 0; pointIndex <= pointsCount; ++pointIndex)
          {
            double xCoordinate = view.LowerBound + pointIndex * stepSize;
            try
            {
              double yCoordinate = model.EvaluateFunction(view.FunctionExpression, xCoordinate);
              functionSeries.Points.Add(new DataPoint(xCoordinate, yCoordinate));
            }
            catch { }
          }

          plotModel.Series.Add(functionSeries);
          view.PlotModel = plotModel;
        }
      }
      catch (Exception exception)
      {
        view.ShowError($"Ошибка при вычислении: {exception.Message}");
      }
    }

    private string GetMethodName(IntegrationMethod method)
    {
      return method switch
      {
        IntegrationMethod.LeftRectangle => "Метод левых прямоугольников",
        IntegrationMethod.RightRectangle => "Метод правых прямоугольников",
        IntegrationMethod.MidpointRectangle => "Метод средних прямоугольников",
        IntegrationMethod.Trapezoidal => "Метод трапеций",
        IntegrationMethod.Simpson => "Метод Симпсона",
        _ => "Неизвестный метод"
      };
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

      if (view.UseFixedPartitions && (view.FixedPartitions <= 0 || view.FixedPartitions > 1000000))
      {
        view.ShowWarning("Количество разбиений должно быть от 1 до 1000000");
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
      view.FixedPartitions = 100;
      view.UseFixedPartitions = false;
      view.AutoPartitions = true;
      view.SelectedMethods.Clear();
      view.IsLeftRectSelected = false;
      view.IsRightRectSelected = false;
      view.IsMidRectSelected = false;
      view.IsTrapezoidSelected = false;
      view.IsSimpsonSelected = false;

      PlotModel plotModel = new PlotModel
      {
        PlotAreaBorderColor = OxyColors.LightGray,
        PlotAreaBorderThickness = new OxyThickness(1),
        Background = OxyColors.White
      };

      LinearAxis xAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash
      };

      LinearAxis yAxis = new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "f(x)",
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash
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
                     "4. Выберите режим подбора разбиений\n" +
                     "5. Выберите один или несколько методов интегрирования\n" +
                     "6. Нажмите 'Вычислить' для расчета\n\n" +
                     "Поддерживаемые операции: +, -, *, /, ^, sin, cos, tan, log, exp, sqrt, abs, asin, acos, atan\n" +
                     "Константы: pi, e\n" +
                     "Примеры функций: x^2 + 2*x + 1, sin(x)*cos(x), exp(-x^2), sqrt(1-x^2)";

      view.ShowInformation(helpText);
    }

    private void OnMethodChanged(object sender, IntegrationMethod method)
    {
    }
  }
}
