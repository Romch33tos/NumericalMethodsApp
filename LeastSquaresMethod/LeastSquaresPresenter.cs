using System;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public class LeastSquaresPresenter
  {
    private readonly ILeastSquaresView view;
    private readonly LeastSquaresModel model;
    private readonly Random randomGenerator = new Random();

    public LeastSquaresPresenter(ILeastSquaresView view)
    {
      this.view = view;
      model = new LeastSquaresModel();
      SubscribeToEvents();
      InitializeView();
    }

    private void SubscribeToEvents()
    {
      view.DimensionChanged += HandleDimensionChanged;
      view.ApplyDimensionClicked += HandleApplyDimensionClicked;
      view.CalculateClicked += HandleCalculateClicked;
      view.ClearAllClicked += HandleClearAllClicked;
      view.RandomGenerateClicked += HandleRandomGenerateClicked;
      view.ImportCsvClicked += HandleImportCsvClicked;
      view.ImportGoogleSheetsClicked += HandleImportGoogleSheetsClicked;
      view.HelpClicked += HandleHelpClicked;
      view.CellEditEnding += HandleCellEditEnding;
    }

    private void InitializeView()
    {
      view.DimensionText = model.Dimension.ToString();
      view.RangeStartText = model.RangeStart.ToString();
      view.RangeEndText = model.RangeEnd.ToString();
      view.PrecisionText = model.Precision.ToString();
      view.IsDataGridEnabled = false;
    }
  }
}
