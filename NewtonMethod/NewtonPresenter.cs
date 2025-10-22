using System;
using System.Globalization;
using NumericalMethodsApp.Models;
using NumericalMethodsApp.Views;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace NumericalMethodsApp.Presenters
{
  public class NewtonPresenter
  {
    private readonly INewtonView _view;
    private readonly NewtonModel _model;
    private PlotModel _plotModel;

    public NewtonPresenter(INewtonView view)
    {
      _view = view;
      _model = new NewtonModel();
      InitializePlot();
      SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
      _view.CalculateRequested += OnCalculateRequested;
      _view.ClearAllRequested += OnClearAllRequested;
      _view.HelpRequested += OnHelpRequested;
      _view.ModeChanged += OnModeChanged;
      _view.NextStepRequested += OnNextStepRequested;
    }

    private void OnClearAllRequested(object sender, EventArgs e)
    {
      _view.FunctionExpression = "";
      _view.InitialPoint = "";
      _view.DisplayIntervalStart = "-5";
      _view.DisplayIntervalEnd = "5";
      _view.Epsilon = "0.001";
      _view.FindMinimum = true;
      _view.FindMaximum = false;
      _view.ResultText = "";

      _model.ResetCalculation();
      SetCalculationInProgress(false);
      _view.SetStepModeActive(false);
      ClearPlot();
    }

    private void OnHelpRequested(object sender, EventArgs e)
    {
      string helpText = @"Метод Ньютона для оптимизации

Инструкция:
1. Введите функцию f(x) в поле 'Функция f(x)'
2. Укажите начальное приближение x₀
3. Задайте интервал отображения графика
4. Задайте точность вычисления ε
5. Выберите режим поиска (минимум или максимум)
6. Нажмите 'Вычислить полностью' для полного расчета
7. Используйте 'Следующий шаг' для пошагового просмотра

Примечание: 
- Метод Ньютона ищет точки, где первая производная равна нулю
- Тип экстремума определяется по знаку второй производной
- Для пошагового режима начните с кнопки 'Следующий шаг'
- После начала пошагового режима кнопка 'Вычислить полностью' блокируется
- Для нового расчета используйте 'Очистить все'";

      _view.ShowInfo(helpText);
    }

    private void OnModeChanged(object sender, EventArgs e)
    {
      _model.ResetCalculation();
      SetCalculationInProgress(false);
      _view.SetStepModeActive(false);
    }

    private void SetCalculationInProgress(bool inProgress)
    {
      _view.CalculationInProgress = inProgress;
    }
  }
}
