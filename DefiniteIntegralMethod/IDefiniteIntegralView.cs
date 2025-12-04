using System;
using System.ComponentModel;
using OxyPlot;

namespace NumericalMethodsApp.DefiniteIntegralMethod
{
  public interface IDefiniteIntegralView : INotifyPropertyChanged
  {
    string FunctionExpression { get; set; }
    double LowerBound { get; set; }
    double UpperBound { get; set; }
    double Epsilon { get; set; }
    string ResultText { get; set; }
    IntegrationMethod SelectedMethod { get; set; }
    PlotModel PlotModel { get; set; }

    event EventHandler CalculateRequested;
    event EventHandler ClearAllRequested;
    event EventHandler HelpRequested;
    event EventHandler<IntegrationMethod> MethodChanged;

    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInformation(string message);
  }
}