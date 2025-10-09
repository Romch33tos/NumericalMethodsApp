using System;
using System.Collections.Generic;
using System.Windows;

namespace NumericalMethodsApp
{
  public interface ILinearEquationsView
  {
    int MatrixRows { get; set; }
    int MatrixCols { get; set; }
    double[,] MatrixA { get; set; }
    double[] VectorB { get; set; }
    double[] VectorX { get; set; }

    bool IsGaussSelected { get; set; }
    bool IsJordanGaussSelected { get; set; }
    bool IsCramerSelected { get; set; }

    event Action ApplyDimensionsClicked;
    event Action RandomGenerationClicked;
    event Action ImportFromCsvClicked;
    event Action ImportFromGoogleSheetsClicked;
    event Action SolveClicked;
    event Action ExportToCsvClicked;
    event Action ClearAllClicked;
    event Action HelpClicked;

    void DisplaySolution(double[] solution);
    void DisplayExecutionResults(List<ExecutionResult> results);
    void ShowMessage(string message, string title, MessageBoxImage icon);
    bool ShowConfirmation(string message, string title);
    void ClearAllData();
    void UpdateControlsState(bool isEnabled);
  }

  public class ExecutionResult
  {
    public string MethodName { get; set; }
    public double ExecutionTimeMs { get; set; }
    public string Status { get; set; }
    public string ErrorMessage { get; set; }
    public string DetailedStatus { get; set; }
  }
}
