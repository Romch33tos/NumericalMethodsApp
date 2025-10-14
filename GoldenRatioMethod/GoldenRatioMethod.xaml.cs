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
    private string _resultText;

    public GoldenRatioPresenter Presenter { get; private set; }

    public GoldenRatioMethod()
    {
      InitializeComponent();
      Presenter = new GoldenRatioPresenter(this);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
