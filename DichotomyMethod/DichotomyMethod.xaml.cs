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

      TextBoxFunction.Text = "x^2 - 4";
      TextBoxA.Text = "1";
      TextBoxB.Text = "3";
      TextBoxEpsilon.Text = "0.001";

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

      int segmentsCount = 200;
      double stepSize = (endInterval - startInterval) / segmentsCount;

      LineSeries functionSeries = new LineSeries
      {
        Title = "f(x)",
        Color = OxyColors.Blue,
        StrokeThickness = 2
      };

      for (int segmentIndex = 0; segmentIndex <= segmentsCount; ++segmentIndex)
      {
        double x = startInterval + segmentIndex * stepSize;
        try
        {
          double y = _presenter.EvaluateFunction(FunctionExpression, x);

          if (!double.IsInfinity(y) && !double.IsNaN(y) && Math.Abs(y) < 1e10)
          {
            functionSeries.Points.Add(new DataPoint(x, y));
          }
        }
        catch
        {
        }
      }

      if (functionSeries.Points.Count > 0)
      {
        _plotModel.Series.Add(functionSeries);
      }

      if (roots != null && roots.Length > 0)
      {
        var rootSeries = new ScatterSeries
        {
          Title = "Корень",
          MarkerType = MarkerType.Circle,
          MarkerSize = 6,
          MarkerFill = OxyColors.Red
        };

        foreach (double root in roots)
        {
          try
          {
            double y = _presenter.EvaluateFunction(FunctionExpression, root);
            rootSeries.Points.Add(new ScatterPoint(root, y));
          }
          catch
          {
            rootSeries.Points.Add(new ScatterPoint(root, 0));
          }
        }

        _plotModel.Series.Add(rootSeries);
      }

      var zeroLine = new LineSeries
      {
        Title = "y = 0",
        Color = OxyColors.Gray,
        StrokeThickness = 1,
        LineStyle = LineStyle.Dash
      };
      zeroLine.Points.Add(new DataPoint(startInterval, 0));
      zeroLine.Points.Add(new DataPoint(endInterval, 0));
      _plotModel.Series.Add(zeroLine);

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
