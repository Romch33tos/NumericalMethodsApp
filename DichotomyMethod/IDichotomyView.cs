namespace NumericalMethodsApp
{
  public interface IDichotomyView
  {
    string FunctionExpression { get; }
    string StartIntervalText { get; }
    string EndIntervalText { get; }
    string EpsilonText { get; }

    void SetResult(string result);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInformation(string message);
    void ClearPlot();
    void ClearInputs();
    void PlotFunction(double startInterval, double endInterval, double[] roots);
    void FocusFunctionTextBox();
    void FocusStartIntervalTextBox();
    void FocusEndIntervalTextBox();
    void FocusEpsilonTextBox();
  }
}
