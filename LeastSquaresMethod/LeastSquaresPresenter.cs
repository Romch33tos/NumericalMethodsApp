using System;
using System.Globalization;
using System.Windows;

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
      view.RangeStartText = model.RangeStart.ToString(CultureInfo.InvariantCulture);
      view.RangeEndText = model.RangeEnd.ToString(CultureInfo.InvariantCulture);
      view.PrecisionText = model.Precision.ToString(CultureInfo.InvariantCulture);
      view.IsDataGridEnabled = false;
    }

    private void HandleDimensionChanged(object sender, RoutedEventArgs e)
    {
      view.IsDataGridEnabled = false;
    }

    private void HandleApplyDimensionClicked(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(view.DimensionText, out int dimension) && dimension > 0)
      {
        model.UpdateDimension(dimension);
        view.IsDataGridEnabled = true;
      }
      else
      {
        view.ShowMessage("Пожалуйста, введите корректное положительное число для размерности", "Ошибка", MessageBoxType.Error);
        view.DimensionText = "3";
      }
    }
  }
}
