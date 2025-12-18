using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NumericalMethodsApp
{
  public partial class CoordinateDescentView : Window, ICoordinateDescentView
  {
    private CoordinateDescentPresenter presenter;
    private PlotModel currentPlotModel;

    public CoordinateDescentView()
    {
      InitializeComponent();
      presenter = new CoordinateDescentPresenter(this);
      InitializePlot();
      TextBoxEpsilon.Text = "0.001";
      TextBoxStepSize.Text = "0.01";
    }

    private void InitializePlot()
    {
      currentPlotModel = new PlotModel
      {
        Title = "",
        Background = OxyColors.White,
        PlotAreaBorderThickness = new OxyThickness(1),
        PlotAreaBorderColor = OxyColors.LightGray
      };

      var xAxis = new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        Minimum = -5,
        Maximum = 5,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(15, 0, 0, 0)
      };
      currentPlotModel.Axes.Add(xAxis);

      var yAxis = new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "y",
        Minimum = -5,
        Maximum = 5,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(15, 0, 0, 0)
      };
      currentPlotModel.Axes.Add(yAxis);

      PlotView.Model = currentPlotModel;
    }

    public string FunctionExpression
    {
      get => TextBoxFunction.Text;
      set => TextBoxFunction.Text = value;
    }

    public string XStart
    {
      get => TextBoxX.Text;
      set => TextBoxX.Text = value;
    }

    public string YStart
    {
      get => TextBoxY.Text;
      set => TextBoxY.Text = value;
    }

    public string Epsilon
    {
      get => TextBoxEpsilon.Text;
      set => TextBoxEpsilon.Text = value;
    }

    public string StepSize
    {
      get => TextBoxStepSize.Text;
      set => TextBoxStepSize.Text = value;
    }

    public string Result
    {
      get => ResultText.Text;
      set => ResultText.Text = value;
    }

    public event EventHandler CalculateClicked;
    public event EventHandler ClearAllClicked;
    public event EventHandler HelpClicked;

    public void UpdatePlot(PlotModel plotModel)
    {
      currentPlotModel = plotModel;
      PlotView.Model = currentPlotModel;
      PlotView.InvalidatePlot(true);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      CalculateClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      ClearAllClicked?.Invoke(this, EventArgs.Empty);
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      HelpClicked?.Invoke(this, EventArgs.Empty);
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
      Regex regex = new Regex(@"^[-]?[0-9]*[.,]?[0-9]*$");
      e.Handled = !regex.IsMatch((sender as System.Windows.Controls.TextBox).Text + e.Text);
    }

    public void ShowError(string message)
    {
      MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ClearResults()
    {
      FunctionExpression = string.Empty;
      XStart = string.Empty;
      YStart = string.Empty;
      Epsilon = "0.001";
      StepSize = "0.01";
      Result = string.Empty;
      InitializePlot();
    }
  }
}
