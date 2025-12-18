using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public interface ILeastSquaresView
  {
    string DimensionText { get; set; }
    string RangeStartText { get; set; }
    string RangeEndText { get; set; }
    string PrecisionText { get; set; }
    string ResultText { get; set; }
    bool IsDataGridEnabled { get; set; }

    void UpdateDataGrid(List<GridDataPoint> dataPoints);
    void UpdatePlot(List<DataPoint> points, double[] coefficients);
    MessageBoxResult ShowMessage(string message, string caption, MessageBoxType messageBoxType);
    void ClearPlot();

    event RoutedEventHandler DimensionChanged;
    event RoutedEventHandler ApplyDimensionClicked;
    event RoutedEventHandler CalculateClicked;
    event RoutedEventHandler ClearAllClicked;
    event RoutedEventHandler RandomGenerateClicked;
    event RoutedEventHandler ImportCsvClicked;
    event RoutedEventHandler ImportGoogleSheetsClicked;
    event RoutedEventHandler HelpClicked;
    event System.EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding;
  }

  public enum MessageBoxType
  {
    Information,
    Error,
    Question
  }

  public class GridDataPoint
  {
    public int Index { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
  }
}
