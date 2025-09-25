using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Text.RegularExpressions;

namespace NumericalMethodsApp
{
  public partial class DichotomyMethod : Window, IDichotomyView
  {
    private DichotomyPresenter _presenter;
    private PlotModel _plotModel;

    public string FunctionExpression => TextBoxFunction.Text;
    public string StartIntervalText => TextBoxA.Text;
    public string EndIntervalText => TextBoxB.Text;
    public string EpsilonText => TextBoxEpsilon.Text;

    public DichotomyMethod()
    {
      InitializeComponent();
      InitializePlot();
      _presenter = new DichotomyPresenter(this);
      ResultsPanel.Visibility = Visibility.Visible;
      ResultText.Text = "Ответ: ";

      KeyboardNavigation.SetTabIndex(TextBoxFunction, 0);
      KeyboardNavigation.SetTabIndex(TextBoxA, 1);
      KeyboardNavigation.SetTabIndex(TextBoxB, 2);
      KeyboardNavigation.SetTabIndex(TextBoxEpsilon, 3);
      KeyboardNavigation.SetTabIndex(CalculateButton, 4);
    }

    private void InitializePlot()
    {
      _plotModel = new PlotModel { Title = "" };
      _plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x", FontSize = 14 });
      _plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "f(x)", FontSize = 14 });
      PlotView.Model = _plotModel;
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult result = MessageBox.Show("Вы действительно хотите очистить все данные?",
                                               "Подтверждение очистки",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Question);

      if (result == MessageBoxResult.Yes)
      {
        _presenter.ClearAll();
        TextBoxFunction.Focus();
      }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      _presenter.CalculateRoots();
    }

    public void SetResult(string result)
    {
      ResultText.Text = result;
      ResultsPanel.Visibility = Visibility.Visible;
    }

    public void ShowError(string message)
    {
      MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message)
    {
      MessageBox.Show(message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowInformation(string message)
    {
      MessageBox.Show(message, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ClearPlot()
    {
      _plotModel.Series.Clear();
      PlotView.InvalidatePlot(true);
    }

    public void ClearInputs()
    {
      TextBoxFunction.Text = "";
      TextBoxA.Text = "";
      TextBoxB.Text = "";
      TextBoxEpsilon.Text = "0.001";
    }

    public void PlotFunction(double startInterval, double endInterval, double[] roots)
    {
      _plotModel.Series.Clear();

      var functionSeries = new LineSeries
      {
        Title = $"f(x) = {FunctionExpression}",
        Color = OxyColors.Blue,
        StrokeThickness = 2
      };

      int pointsCount = 200;
      double stepSize = (endInterval - startInterval) / pointsCount;

      for (int pointIndex = 0; pointIndex <= pointsCount; pointIndex++)
      {
        double x = startInterval + pointIndex * stepSize;
        try
        {
          double y = _presenter.EvaluateFunction(FunctionExpression, x);
          functionSeries.Points.Add(new DataPoint(x, y));
        }
        catch
        {
        }
      }

      _plotModel.Series.Add(functionSeries);

      if (roots != null && roots.Length > 0)
      {
        var rootsSeries = new ScatterSeries
        {
          Title = "Корни",
          MarkerType = MarkerType.Circle,
          MarkerSize = 6,
          MarkerFill = OxyColors.Red
        };

        foreach (double root in roots)
        {
          try
          {
            double y = _presenter.EvaluateFunction(FunctionExpression, root);
            rootsSeries.Points.Add(new ScatterPoint(root, y));
          }
          catch
          {
          }
        }

        _plotModel.Series.Add(rootsSeries);
      }

      PlotView.InvalidatePlot(true);
    }

    public void FocusFunctionTextBox()
    {
      TextBoxFunction.Focus();
    }

    public void FocusStartIntervalTextBox()
    {
      TextBoxA.Focus();
    }

    public void FocusEndIntervalTextBox()
    {
      TextBoxB.Focus();
    }

    public void FocusEpsilonTextBox()
    {
      TextBoxEpsilon.Focus();
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
      Regex regex = new Regex(@"^[-]?[0-9]*[.,]?[0-9]*$");
      e.Handled = !regex.IsMatch((sender as TextBox).Text + e.Text);
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        if (sender == TextBoxFunction)
          TextBoxA.Focus();
        else if (sender == TextBoxA)
          TextBoxB.Focus();
        else if (sender == TextBoxB)
          TextBoxEpsilon.Focus();
        else if (sender == TextBoxEpsilon)
          CalculateButton_Click(sender, e);
      }
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Tab)
      {
        var currentTextBox = sender as TextBox;
        if (currentTextBox != null)
        {
          if (currentTextBox == TextBoxFunction)
            TextBoxA.Focus();
          else if (currentTextBox == TextBoxA)
            TextBoxB.Focus();
          else if (currentTextBox == TextBoxB)
            TextBoxEpsilon.Focus();
          else if (currentTextBox == TextBoxEpsilon)
            CalculateButton.Focus();

          e.Handled = true;
        }
      }
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      _presenter.ShowHelp();
    }
  }
}
