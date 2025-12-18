namespace NumericalMethodsApp
{
  public interface ICoordinateDescentView
  {
    string FunctionExpression { get; set; }
    string XStart { get; set; }
    string YStart { get; set; }
    string Epsilon { get; set; }
    string StepSize { get; set; }
    string Result { get; set; }

    event EventHandler CalculateClicked;
    event EventHandler ClearAllClicked;
    event EventHandler HelpClicked;

    void ShowError(string message);
    void ClearResults();
    void UpdatePlot(OxyPlot.PlotModel plotModel);
  }
}
