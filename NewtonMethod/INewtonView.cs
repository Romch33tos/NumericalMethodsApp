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
    string ResultText { get; set; }
    int CurrentStep { get; set; }
    int TotalSteps { get; set; }
    bool IsModeLocked { get; set; }

    event EventHandler CalculateRequested;
    event EventHandler ClearAllRequested;
    event EventHandler HelpRequested;
    event EventHandler<int> StepChanged;

    void ShowError(string message);
    void ShowInfo(string message);
    void UpdatePlot(double lowerBound, double upperBound, double extremumX, double extremumY);
    void ClearPlot();

    PlotModel GetPlotModel();
    void SetPlotModel(PlotModel model);
  }
}
