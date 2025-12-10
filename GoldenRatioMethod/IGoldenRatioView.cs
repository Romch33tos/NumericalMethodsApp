using System;

namespace NumericalMethodsApp.Views
{
  public interface IGoldenRatioView
  {
    string FunctionExpression { get; set; }
    string LowerBound { get; set; }
    string UpperBound { get; set; }
    string Epsilon { get; set; }
    bool FindMinimum { get; set; }
    bool FindMaximum { get; set; }
    bool FindZero { get; set; } 
    string ResultText { get; set; }

    event EventHandler CalculateRequested;
    event EventHandler ClearAllRequested;
    event EventHandler HelpRequested;
    event EventHandler ModeChanged;

    void ShowError(string message);
    void ShowInfo(string message);
    void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum);
    void UpdatePlotForZero(double lowerBound, double upperBound, double zeroX);
    void ClearPlot();
  }
}
