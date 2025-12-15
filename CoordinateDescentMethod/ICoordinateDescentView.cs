using System;

namespace NumericalMethodsApp
{
  public interface ICoordinateDescentView
  {
    string FunctionExpression { get; }
    string XStart { get; }
    string YStart { get; }
    string Epsilon { get; }
    string Result { set; }

    event EventHandler CalculateClicked;
    event EventHandler ClearAllClicked;
    event EventHandler HelpClicked;

    void ShowError(string message);
    void ClearResults();
    void UpdatePlot(OxyPlot.PlotModel plotModel);
  }
}
