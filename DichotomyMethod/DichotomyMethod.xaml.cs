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
    private DichotomyPresenter presenter;
    private PlotModel plotModel;
    private double foundMinimumX;
    private double foundMinimumY;

    public string FunctionExpression => TextBoxFunction.Text;
    public string StartIntervalText => TextBoxA.Text;
    public string EndIntervalText => TextBoxB.Text;
    public string EpsilonText => TextBoxEpsilon.Text;

    public DichotomyMethod()
    {
      InitializeComponent();
      InitializePlot();
      presenter = new DichotomyPresenter(this);
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
      plotModel = new PlotModel { Title = "" };
      plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x", FontSize = 14 });
      plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "f(x)", FontSize = 14 });
      PlotView.Model = plotModel;
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult result = MessageBox.Show("Вы действительно хотите очистить все данные?",
                                               "Подтверждение очистки",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Question);

      if (result == MessageBoxResult.Yes)
      {
        presenter.ClearAll();
        TextBoxFunction.Focus();
      }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      presenter.CalculateMinimum();
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
      plotModel.Series.Clear();
      PlotView.InvalidatePlot(true);
    }

    public void PlotFunction(double startInterval, double endInterval, double minX, double minY)
    {
      foundMinimumX = minX;
      foundMinimumY = minY;

      plotModel.Series.Clear();

      var lineSeries = new LineSeries
      {
        Title = $"f(x) = {FunctionExpression}",
        Color = OxyColors.Blue,
        StrokeThickness = 2
      };

      int pointsCount = 200;
      double step = (endInterval - startInterval) / pointsCount;

      for (int pointIndex = 0; pointIndex <= pointsCount; pointIndex++)
      {
        double x = startInterval + pointIndex * step;
        try
        {
          double y = presenter.EvaluateFunction(FunctionExpression, x);
          lineSeries.Points.Add(new DataPoint(x, y));
        }
        catch
        {
        }
      }

      plotModel.Series.Add(lineSeries);

      var scatterSeries = new ScatterSeries
      {
        Title = "Минимум",
        MarkerType = MarkerType.Circle,
        MarkerSize = 5,
        MarkerFill = OxyColors.Red
      };
      scatterSeries.Points.Add(new ScatterPoint(minX, minY));

      plotModel.Series.Add(scatterSeries);

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
        var current = sender as TextBox;
        if (current != null)
        {
          if (current == TextBoxFunction)
            TextBoxA.Focus();
          else if (current == TextBoxA)
            TextBoxB.Focus();
          else if (current == TextBoxB)
            TextBoxEpsilon.Focus();
          else if (current == TextBoxEpsilon)
            CalculateButton.Focus();

          e.Handled = true;
        }
      }
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      presenter.ShowHelp();
    }
  }
}