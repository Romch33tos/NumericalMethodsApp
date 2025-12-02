using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;

namespace NumericalMethodsApp.DefiniteIntegralMethod
{
  public partial class DefeniteIntegralMethod : Window, IDefiniteIntegralView
  {
    private string functionExpression = "x*x - 2*x + 1";
    private double lowerBound = 1;
    private double upperBound = 2;
    private double epsilon = 0.001;
    private string resultText = "";
    private IntegrationMethod selectedMethod = IntegrationMethod.LeftRectangle;
    private PlotModel plotModel;

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler CalculateRequested;
    public event EventHandler ClearAllRequested;
    public event EventHandler HelpRequested;
    public event EventHandler<IntegrationMethod> MethodChanged;

    private DefiniteIntegralPresenter presenter;

    public DefeniteIntegralMethod()
    {
      InitializeComponent();
      DataContext = this;
      presenter = new DefiniteIntegralPresenter(this);
      InitializePlot();
      LeftRectRadioButton.IsChecked = true;
      WireUpEvents();
    }

    private void WireUpEvents()
    {
      LeftRectRadioButton.Checked += (sender, eventArgs) => MethodChanged?.Invoke(this, IntegrationMethod.LeftRectangle);
      RightRectRadioButton.Checked += (sender, eventArgs) => MethodChanged?.Invoke(this, IntegrationMethod.RightRectangle);
      MidRectRadioButton.Checked += (sender, eventArgs) => MethodChanged?.Invoke(this, IntegrationMethod.MidpointRectangle);
      TrapezoidRadioButton.Checked += (sender, eventArgs) => MethodChanged?.Invoke(this, IntegrationMethod.Trapezoidal);
      SimpsonRadioButton.Checked += (sender, eventArgs) => MethodChanged?.Invoke(this, IntegrationMethod.Simpson);
    }

    public string FunctionExpression
    {
      get => functionExpression;
      set
      {
        functionExpression = value;
        OnPropertyChanged(nameof(FunctionExpression));
      }
    }

    public double LowerBound
    {
      get => lowerBound;
      set
      {
        lowerBound = value;
        OnPropertyChanged(nameof(LowerBound));
      }
    }

    public double UpperBound
    {
      get => upperBound;
      set
      {
        upperBound = value;
        OnPropertyChanged(nameof(UpperBound));
      }
    }

    public double Epsilon
    {
      get => epsilon;
      set
      {
        epsilon = value;
        OnPropertyChanged(nameof(Epsilon));
      }
    }

    public string ResultText
    {
      get => resultText;
      set
      {
        resultText = value;
        OnPropertyChanged(nameof(ResultText));
      }
    }

    public IntegrationMethod SelectedMethod
    {
      get => selectedMethod;
      set
      {
        selectedMethod = value;
        OnPropertyChanged(nameof(SelectedMethod));
      }
    }

    public PlotModel PlotModel
    {
      get => plotModel;
      set
      {
        plotModel = value;
        PlotViewControl.Model = plotModel;
        PlotViewControl.InvalidatePlot();
      }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void InitializePlot()
    {
      PlotModel plotModel = new PlotModel
      {
        PlotAreaBorderColor = OxyColors.LightGray,
        PlotAreaBorderThickness = new OxyThickness(1),
        Background = OxyColors.White
      };

      OxyPlot.Axes.LinearAxis xAxis = new OxyPlot.Axes.LinearAxis
      {
        Position = OxyPlot.Axes.AxisPosition.Bottom,
        Title = "x",
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash
      };

      OxyPlot.Axes.LinearAxis yAxis = new OxyPlot.Axes.LinearAxis
      {
        Position = OxyPlot.Axes.AxisPosition.Left,
        Title = "f(x)",
        MajorGridlineColor = OxyColors.LightGray,
        MajorGridlineStyle = LineStyle.Dash
      };

      plotModel.Axes.Add(xAxis);
      plotModel.Axes.Add(yAxis);
      PlotViewControl.Model = plotModel;
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs eventArgs)
    {
      CalculateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs eventArgs)
    {
      ClearAllRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Help_Click(object sender, RoutedEventArgs eventArgs)
    {
      HelpRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ShowError(string message)
    {
      MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message)
    {
      MessageBox.Show(message, "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowInformation(string message)
    {
      MessageBox.Show(message, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
    }
  }
}