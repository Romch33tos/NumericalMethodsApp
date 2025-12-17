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
  }
}
