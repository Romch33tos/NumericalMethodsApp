using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using NumericalMethodsApp.Presenters;

namespace NumericalMethodsApp.Views
{
  public partial class NewtonMethod : Window, INewtonView, INotifyPropertyChanged
  {
    private string _functionExpression;
    private string _initialPoint;
    private string _displayIntervalStart = "-5";
    private string _displayIntervalEnd = "5";
    private string _epsilon = "0.001";
    private bool _findMinimum = true;
    private bool _findMaximum;
    private string _resultText;
    private bool _calculationInProgress;
    private bool _stepModeActive;

    public NewtonPresenter Presenter { get; private set; }

    public NewtonMethod()
    {
      InitializeComponent();
      DataContext = this;
      Presenter = new NewtonPresenter(this);
      UpdateButtonStates();
    }

    public string FunctionExpression
    {
      get => _functionExpression;
      set
      {
        _functionExpression = value;
        OnPropertyChanged();
      }
    }

    public string InitialPoint
    {
      get => _initialPoint;
      set
      {
        _initialPoint = value;
        OnPropertyChanged();
      }
    }

    public string DisplayIntervalStart
    {
      get => _displayIntervalStart;
      set
      {
        _displayIntervalStart = value;
        OnPropertyChanged();
      }
    }

    public string DisplayIntervalEnd
    {
      get => _displayIntervalEnd;
      set
      {
        _displayIntervalEnd = value;
        OnPropertyChanged();
      }
    }

    public string Epsilon
    {
      get => _epsilon;
      set
      {
        _epsilon = value;
        OnPropertyChanged();
      }
    }

    public bool FindMinimum
    {
      get => _findMinimum;
      set
      {
        _findMinimum = value;
        OnPropertyChanged();
      }
    }

    public bool FindMaximum
    {
      get => _findMaximum;
      set
      {
        _findMaximum = value;
        OnPropertyChanged();
      }
    }

    public string ResultText
    {
      get => _resultText;
      set
      {
        _resultText = value;
        OnPropertyChanged();
      }
    }

    public bool CalculationInProgress
    {
      get => _calculationInProgress;
      set
      {
        _calculationInProgress = value;
        OnPropertyChanged();
        UpdateButtonStates();
      }
    }

    public bool StepModeActive
    {
      get => _stepModeActive;
      set
      {
        _stepModeActive = value;
        OnPropertyChanged();
        UpdateButtonStates();
      }
    }

    public event EventHandler CalculateRequested;
    public event EventHandler ClearAllRequested;
    public event EventHandler HelpRequested;
    public event EventHandler ModeChanged;
    public event EventHandler NextStepRequested;

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      CalculateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      MessageBoxResult confirmationResult = MessageBox.Show(
          "Вы уверены, что хотите очистить все данные?",
          "Подтверждение очистки",
          MessageBoxButton.YesNo,
          MessageBoxImage.Question);

      if (confirmationResult == MessageBoxResult.Yes)
      {
        ClearAllRequested?.Invoke(this, EventArgs.Empty);
      }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
      HelpRequested?.Invoke(this, EventArgs.Empty);
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
      if (!CalculationInProgress && !StepModeActive)
      {
        ModeChanged?.Invoke(this, EventArgs.Empty);
      }
    }

    private void NextStepButton_Click(object sender, RoutedEventArgs e)
    {
      NextStepRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ShowError(string message)
    {
      MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string message)
    {
      MessageBox.Show(message, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum)
    {
      Presenter.UpdatePlot(lowerBound, upperBound, extremumX, extremumY, isMinimum);
    }

    public void ClearPlot()
    {
      PlotViewControl.Model?.Series.Clear();
      PlotViewControl.Model?.InvalidatePlot(true);
    }

    public void UpdateButtonStates()
    {
      CalculateFullButton.IsEnabled = !CalculationInProgress && !StepModeActive;
      NextStepButton.IsEnabled = !CalculationInProgress;
      ClearAllButton.IsEnabled = true;

      MaxRadioButton.IsEnabled = !CalculationInProgress && !StepModeActive;
      MinRadioButton.IsEnabled = !CalculationInProgress && !StepModeActive;
    }

    public void SetStepModeActive(bool active)
    {
      StepModeActive = active;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
