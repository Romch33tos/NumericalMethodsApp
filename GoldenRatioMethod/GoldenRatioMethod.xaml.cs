using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using NumericalMethodsApp.Presenters;

namespace NumericalMethodsApp.Views
{
  public partial class GoldenRatioMethod : Window, IGoldenRatioView, INotifyPropertyChanged
  {
    private string _functionExpression;
    private string _lowerBound;
    private string _upperBound;
    private string _epsilon = "0.001";
    private bool _findMinimum;
    private bool _findMaximum;
    private bool _findZero;
    private string _resultText;

    public GoldenRatioPresenter Presenter { get; private set; }

    public GoldenRatioMethod()
    {
      InitializeComponent();
      Presenter = new GoldenRatioPresenter(this);
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

    public bool FindZero
    {
      get => _findZero;
      set
      {
        _findZero = value;
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

    public event EventHandler CalculateRequested;
    public event EventHandler ClearAllRequested;
    public event EventHandler HelpRequested;
    public event EventHandler ModeChanged;

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
      ModeChanged?.Invoke(this, EventArgs.Empty);
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

    public void UpdatePlotForZero(double lowerBound, double upperBound, double zeroX)
    {
      Presenter?.UpdatePlotForZero(lowerBound, upperBound, zeroX);
    }

    public void ClearPlot()
    {
      PlotViewControl.Model?.Series.Clear();
      PlotViewControl.Model?.InvalidatePlot(true);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
