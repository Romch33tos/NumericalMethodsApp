using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NumericalMethodsApp.Presenters;

namespace NumericalMethodsApp.Views
{
  public partial class GoldenRatioMethod : Window, INotifyPropertyChanged
  {
    private string _functionExpression;
    private string _lowerBound;
    private string _upperBound;
    private string _epsilon = "0.001";

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
