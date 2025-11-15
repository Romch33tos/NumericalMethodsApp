using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NumericalMethodsApp.Presenters;
using OxyPlot;

namespace NumericalMethodsApp.Views
{
  public partial class NewtonMethod : Window, INewtonView, INotifyPropertyChanged
  {
    private string _functionExpression = string.Empty;
    private string _lowerBound = string.Empty;
    private string _upperBound = string.Empty;
    private string _epsilon = "0.001";
    private string _resultText = string.Empty;
    private int _currentStep;
    private int _totalSteps;
    private bool _isModeLocked;

    public NewtonPresenter Presenter { get; private set; }

    public event EventHandler CalculateRequested;
    public event EventHandler ClearAllRequested;
    public event EventHandler HelpRequested;
    public event EventHandler<int> StepChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public NewtonMethod()
    {
      InitializeComponent();
      Presenter = new NewtonPresenter(this);
      DataContext = this;
      UpdateStepInfo();
      UpdateStepButtons();
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

    public string LowerBound
    {
      get => _lowerBound;
      set
      {
        _lowerBound = value;
        OnPropertyChanged();
      }
    }

    public string UpperBound
    {
      get => _upperBound;
      set
      {
        _upperBound = value;
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

    public string ResultText
    {
      get => _resultText;
      set
      {
        _resultText = value;
        OnPropertyChanged();
      }
    }

    public int CurrentStep
    {
      get => _currentStep;
      set
      {
        _currentStep = value;
        OnPropertyChanged();
        UpdateStepInfo();
        UpdateStepButtons();
      }
    }

    public int TotalSteps
    {
      get => _totalSteps;
      set
      {
        _totalSteps = value;
        OnPropertyChanged();
        UpdateStepInfo();
        UpdateStepButtons();
      }
    }

    public bool IsModeLocked
    {
      get => _isModeLocked;
      set
      {
        _isModeLocked = value;
        OnPropertyChanged();
      }
    }

    public PlotModel GetPlotModel()
    {
      return PlotViewControl.Model;
    }

    public void SetPlotModel(PlotModel model)
    {
      PlotViewControl.Model = model;
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      CalculateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      var result = MessageBox.Show(
          "Вы уверены, что хотите очистить все данные?",
          "Подтверждение очистки",
          MessageBoxButton.YesNo,
          MessageBoxImage.Question);

      if (result == MessageBoxResult.Yes)
      {
        ClearAllRequested?.Invoke(this, EventArgs.Empty);
      }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
      HelpRequested?.Invoke(this, EventArgs.Empty);
    }

    private void StepBackButton_Click(object sender, RoutedEventArgs e)
    {
      if (CurrentStep > 0)
      {
        --CurrentStep;
        StepChanged?.Invoke(this, CurrentStep);
      }
    }

    private void StepForwardButton_Click(object sender, RoutedEventArgs e)
    {
      if (CurrentStep < TotalSteps)
      {
        ++CurrentStep;
        StepChanged?.Invoke(this, CurrentStep);
      }
    }

    private void UpdateStepInfo()
    {
      StepInfoLabel.Content = $"Шаг: {CurrentStep}/{TotalSteps}";
    }

    private void UpdateStepButtons()
    {
      StepBackButton.IsEnabled = CurrentStep > 0;
      StepForwardButton.IsEnabled = CurrentStep < TotalSteps;
    }

    public void ShowError(string message)
    {
      MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string message)
    {
      MessageBox.Show(message, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY)
    {
      Presenter.UpdatePlot(lowerBound, upperBound, extremumX, extremumY);
    }

    public void ClearPlot()
    {
      var emptyModel = new PlotModel();
      emptyModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Bottom, Title = "x" });
      emptyModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = OxyPlot.Axes.AxisPosition.Left, Title = "f(x)" });
      PlotViewControl.Model = emptyModel;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
