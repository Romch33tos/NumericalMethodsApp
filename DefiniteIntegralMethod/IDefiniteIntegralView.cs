using System;
using System.Collections.Generic;
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
    List<IntegrationMethod> SelectedMethods { get; set; }
    PlotModel PlotModel { get; set; }
    bool UseFixedPartitions { get; set; }
    bool AutoPartitions { get; set; }
    int FixedPartitions { get; set; }

    bool IsLeftRectSelected { get; set; }
    bool IsRightRectSelected { get; set; }
    bool IsMidRectSelected { get; set; }
    bool IsTrapezoidSelected { get; set; }
    bool IsSimpsonSelected { get; set; }

    event EventHandler CalculateRequested;
    event EventHandler ClearAllRequested;
    event EventHandler HelpRequested;
    event EventHandler<IntegrationMethod> MethodChanged;

    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInformation(string message);
  }
}
