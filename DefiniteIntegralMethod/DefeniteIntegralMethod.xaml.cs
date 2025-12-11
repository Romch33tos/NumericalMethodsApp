using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
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
    private List<IntegrationMethod> selectedMethods = new List<IntegrationMethod>();
    private PlotModel plotModel;
    private bool useFixedPartitions = false;
    private bool autoPartitions = true;
    private int fixedPartitions = 100;

    private bool isLeftRectSelected = false;
    private bool isRightRectSelected = false;
    private bool isMidRectSelected = false;
    private bool isTrapezoidSelected = false;
    private bool isSimpsonSelected = false;

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
      WireUpEvents();
    }

    private void WireUpEvents()
    {
      LeftRectCheckBox.Checked += (sender, eventArgs) => UpdateSelectedMethods();
      LeftRectCheckBox.Unchecked += (sender, eventArgs) => UpdateSelectedMethods();
      RightRectCheckBox.Checked += (sender, eventArgs) => UpdateSelectedMethods();
      RightRectCheckBox.Unchecked += (sender, eventArgs) => UpdateSelectedMethods();
      MidRectCheckBox.Checked += (sender, eventArgs) => UpdateSelectedMethods();
      MidRectCheckBox.Unchecked += (sender, eventArgs) => UpdateSelectedMethods();
      TrapezoidCheckBox.Checked += (sender, eventArgs) => UpdateSelectedMethods();
      TrapezoidCheckBox.Unchecked += (sender, eventArgs) => UpdateSelectedMethods();
      SimpsonCheckBox.Checked += (sender, eventArgs) => UpdateSelectedMethods();
      SimpsonCheckBox.Unchecked += (sender, eventArgs) => UpdateSelectedMethods();
    }

    private void UpdateSelectedMethods()
    {
      SelectedMethods.Clear();

      if (IsLeftRectSelected) SelectedMethods.Add(IntegrationMethod.LeftRectangle);
      if (IsRightRectSelected) SelectedMethods.Add(IntegrationMethod.RightRectangle);
      if (IsMidRectSelected) SelectedMethods.Add(IntegrationMethod.MidpointRectangle);
      if (IsTrapezoidSelected) SelectedMethods.Add(IntegrationMethod.Trapezoidal);
      if (IsSimpsonSelected) SelectedMethods.Add(IntegrationMethod.Simpson);

      OnPropertyChanged(nameof(SelectedMethods));
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

    public List<IntegrationMethod> SelectedMethods
    {
      get => selectedMethods;
      set
      {
        selectedMethods = value;
        OnPropertyChanged(nameof(SelectedMethods));
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

    public bool UseFixedPartitions
    {
      get => useFixedPartitions;
      set
      {
        useFixedPartitions = value;
        OnPropertyChanged(nameof(UseFixedPartitions));
      }
    }

    public bool AutoPartitions
    {
      get => autoPartitions;
      set
      {
        autoPartitions = value;
        OnPropertyChanged(nameof(AutoPartitions));
      }
    }

    public int FixedPartitions
    {
      get => fixedPartitions;
      set
      {
        fixedPartitions = value;
        OnPropertyChanged(nameof(FixedPartitions));
      }
    }

    public bool IsLeftRectSelected
    {
      get => isLeftRectSelected;
      set
      {
        isLeftRectSelected = value;
        OnPropertyChanged(nameof(IsLeftRectSelected));
      }
    }

    public bool IsRightRectSelected
    {
      get => isRightRectSelected;
      set
      {
        isRightRectSelected = value;
        OnPropertyChanged(nameof(IsRightRectSelected));
      }
    }

    public bool IsMidRectSelected
    {
      get => isMidRectSelected;
      set
      {
        isMidRectSelected = value;
        OnPropertyChanged(nameof(IsMidRectSelected));
      }
    }

    public bool IsTrapezoidSelected
    {
      get => isTrapezoidSelected;
      set
      {
        isTrapezoidSelected = value;
        OnPropertyChanged(nameof(IsTrapezoidSelected));
      }
    }

    public bool IsSimpsonSelected
    {
      get => isSimpsonSelected;
      set
      {
        isSimpsonSelected = value;
        OnPropertyChanged(nameof(IsSimpsonSelected));
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
