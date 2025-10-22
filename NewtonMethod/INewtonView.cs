using System;

namespace NumericalMethodsApp.Views
{
  public interface INewtonView
  {
    string FunctionExpression { get; set; }
    string InitialPoint { get; set; }
    string DisplayIntervalStart { get; set; }
    string DisplayIntervalEnd { get; set; }
    string Epsilon { get; set; }
    bool FindMinimum { get; set; }
    bool FindMaximum { get; set; }
    string ResultText { get; set; }
    bool CalculationInProgress { get; set; }

    event EventHandler CalculateRequested;
    event EventHandler ClearAllRequested;
    event EventHandler HelpRequested;
    event EventHandler ModeChanged;
    event EventHandler NextStepRequested;

    void ShowError(string message);
    void ShowInfo(string message);
    void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum);
    void ClearPlot();
  }
}
