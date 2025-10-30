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
    private string functionExpression = string.Empty;
    private string lowerBound = string.Empty;
    private string upperBound = string.Empty;
    private string epsilon = "0.001";
    private bool findMinimum;
    private bool findMaximum;
    private string resultText = string.Empty;
    private int currentStep;
    private int totalSteps;
    private bool isModeLocked;

    public NewtonPresenter Presenter { get; private set; }

    public event EventHandler CalculateRequested;
    public event EventHandler ClearAllRequested;
    public event EventHandler HelpRequested;
    public event EventHandler ModeChanged;
    public event EventHandler<int> StepChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public NewtonMethod()
    {
      InitializeComponent();
      Presenter = new NewtonPresenter(this);
      UpdateStepButtons();
    }

    public string FunctionExpression
    {
      get => functionExpression;
      set
      {
        functionExpression = value;
        OnPropertyChanged();
      }
    }

    public string LowerBound
    {
      get => lowerBound;
      set
      {
        lowerBound = value;
        OnPropertyChanged();
      }
    }

    public string UpperBound
    {
      get => upperBound;
      set
      {
        upperBound = value;
        OnPropertyChanged();
      }
    }

    public string Epsilon
    {
      get => epsilon;
      set
      {
        epsilon = value;
        OnPropertyChanged();
      }
    }

    public bool FindMinimum
    {
      get => findMinimum;
      set
      {
        findMinimum = value;
        OnPropertyChanged();
      }
    }

    public bool FindMaximum
    {
      get => findMaximum;
      set
      {
        findMaximum = value;
        OnPropertyChanged();
      }
    }

    public string ResultText
    {
      get => resultText;
      set
      {
        resultText = value;
        OnPropertyChanged();
      }
    }

    public int CurrentStep
    {
      get => currentStep;
      set
      {
        currentStep = value;
        OnPropertyChanged();
        UpdateStepInfo();
        UpdateStepButtons();
      }
    }

    public int TotalSteps
    {
      get => totalSteps;
      set
      {
        totalSteps = value;
        OnPropertyChanged();
        UpdateStepInfo();
        UpdateStepButtons();
      }
    }

    public bool IsModeLocked
    {
      get => isModeLocked;
      set
      {
        isModeLocked = value;
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
      MessageBoxResult confirmationResult = MessageBox.Show(
          "Вы уверены, что хотите очистить все данные?",
          "Подтверждение очистки",
          MessageBoxButton.YesNo,
          MessageBoxImage.Question);

      if (confirmationResult == MessageBoxResult.Yes)
      {
        IsModeLocked = false;
        ClearAllRequested?.Invoke(this, EventArgs.Empty);
        CurrentStep = 0;
        TotalSteps = 0;
      }
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
      HelpRequested?.Invoke(this, EventArgs.Empty);
    }

    private void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
      if (!string.IsNullOrEmpty(ResultText))
      {
        ResultText = "";
        ClearPlot();
        CurrentStep = 0;
        TotalSteps = 0;

        Presenter?.ResetModel();
      }

      ModeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StepBackButton_Click(object sender, RoutedEventArgs e)
    {
      if (CurrentStep > 0)
      {
        CurrentStep--;
        StepChanged?.Invoke(this, CurrentStep);
      }
    }

    private void StepForwardButton_Click(object sender, RoutedEventArgs e)
    {
      if (CurrentStep < TotalSteps)
      {
        CurrentStep++;
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

    public void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum)
    {
      Presenter?.UpdatePlot(lowerBound, upperBound, extremumX, extremumY, isMinimum);
    }

    public void ClearPlot()
    {
      PlotViewControl.Model?.Series.Clear();
      PlotViewControl.Model?.InvalidatePlot(true);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
