using System;
using OxyPlot;

namespace NumericalMethodsApp.Views
{
  public interface INewtonView
  {
    string FunctionExpression { get; set; }
    string LowerBound { get; set; }
    string UpperBound { get; set; }
    string Epsilon { get; set; }
    bool FindMinimum { get; set; }
    bool FindMaximum { get; set; }
    string ResultText { get; set; }
    int CurrentStep { get; set; }
    int TotalSteps { get; set; }
    bool IsModeLocked { get; set; }

    event EventHandler CalculateRequested;
    event EventHandler ClearAllRequested;
    event EventHandler HelpRequested;
    event EventHandler ModeChanged;
    event EventHandler<int> StepChanged;

    void ShowError(string message);
    void ShowInfo(string message);
    void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY, bool isMinimum);
    void ClearPlot();

    PlotModel GetPlotModel();
    void SetPlotModel(PlotModel model);
  }
}
