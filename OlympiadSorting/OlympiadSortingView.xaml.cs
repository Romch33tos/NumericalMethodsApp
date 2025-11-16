using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using System.Collections;

namespace NumericalMethodsApp.OlympiadSorting
{
  public partial class OlympiadSortingView : Window, ISortingView
  {
    private SortingPresenter presenter;

    public OlympiadSortingView()
    {
      InitializeComponent();
      presenter = new SortingPresenter(this);
    }

    public string RowsText
    {
      get => RowsTextBox.Text;
      set => RowsTextBox.Text = value;
    }

    public string MinValueText
    {
      get => MinValueTextBox.Text;
      set => MinValueTextBox.Text = value;
    }

    public string MaxValueText
    {
      get => MaxValueTextBox.Text;
      set => MaxValueTextBox.Text = value;
    }

    public string MaxIterationsText
    {
      get => MaxIterationsTextBox.Text;
      set => MaxIterationsTextBox.Text = value;
    }

    public bool IsBubbleSortChecked
    {
      get => BubbleSortCheckBox.IsChecked == true;
      set => BubbleSortCheckBox.IsChecked = value;
    }

    public bool IsInsertionSortChecked
    {
      get => InsertionSortCheckBox.IsChecked == true;
      set => InsertionSortCheckBox.IsChecked = value;
    }

    public bool IsShakerSortChecked
    {
      get => ShakerSortCheckBox.IsChecked == true;
      set => ShakerSortCheckBox.IsChecked = value;
    }

    public bool IsQuickSortChecked
    {
      get => QuickSortCheckBox.IsChecked == true;
      set => QuickSortCheckBox.IsChecked = value;
    }

    public bool IsBogosortChecked
    {
      get => BogosortCheckBox.IsChecked == true;
      set => BogosortCheckBox.IsChecked = value;
    }

    public bool IsAscendingChecked
    {
      get => AscendingRadioButton.IsChecked == true;
      set => AscendingRadioButton.IsChecked = value;
    }

    public bool IsDescendingChecked
    {
      get => DescendingRadioButton.IsChecked == true;
      set => DescendingRadioButton.IsChecked = value;
    }

    public bool StartSortingEnabled
    {
      get => StartSorting.IsEnabled;
      set => StartSorting.IsEnabled = value;
    }

    public event RoutedEventHandler HelpClicked;
    public event RoutedEventHandler ApplySizeClicked;
    public event RoutedEventHandler RandomGenerateClicked;
    public event RoutedEventHandler ImportCsvClicked;
    public event RoutedEventHandler ImportGoogleClicked;
    public event RoutedEventHandler ClearAllClicked;
    public event RoutedEventHandler StartSortingClicked;
    public event RoutedEventHandler CheckBoxChecked;
    public event System.EventHandler<DataGridCellEditEndingEventArgs> OriginalDataGridCellEditEnding;

    private void Help_Click(object sender, RoutedEventArgs e) => HelpClicked?.Invoke(sender, e);
    private void ApplySize_Click(object sender, RoutedEventArgs e) => ApplySizeClicked?.Invoke(sender, e);
    private void RandomGenerate_Click(object sender, RoutedEventArgs e) => RandomGenerateClicked?.Invoke(sender, e);
    private void ImportCsv_Click(object sender, RoutedEventArgs e) => ImportCsvClicked?.Invoke(sender, e);
    private void ImportGoogle_Click(object sender, RoutedEventArgs e) => ImportGoogleClicked?.Invoke(sender, e);
    private void ClearAll_Click(object sender, RoutedEventArgs e) => ClearAllClicked?.Invoke(sender, e);
    private void StartSorting_Click(object sender, RoutedEventArgs e) => StartSortingClicked?.Invoke(sender, e);
    private void CheckBox_Checked(object sender, RoutedEventArgs e) => CheckBoxChecked?.Invoke(sender, e);
    private void OriginalDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => OriginalDataGridCellEditEnding?.Invoke(sender, e);

    // Добавленные обработчики событий
    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    public void SetOriginalDataGridItemsSource(object source)
    {
      OriginalDataGrid.ItemsSource = source as IEnumerable;
    }

    public void SetSortedDataGridItemsSource(object source)
    {
      SortedDataGrid.ItemsSource = source as IEnumerable;
    }

    public void SetResultsDataGridItemsSource(object source)
    {
      ResultsDataGrid.ItemsSource = source as IEnumerable;
    }

    public void SetPerformanceChartModel(PlotModel model)
    {
      PerformanceChart.Model = model;
    }

    public void AddNumericValidation(TextBox textBox)
    {
      textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
    }

    private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
      foreach (char character in e.Text)
      {
        if (!char.IsDigit(character) && character != '-')
        {
          e.Handled = true;
          return;
        }
      }

      TextBox textBox = sender as TextBox;
      if (e.Text == "-" && textBox != null)
      {
        if (textBox.SelectionStart != 0 || textBox.Text.Contains("-"))
        {
          e.Handled = true;
        }
      }
    }
  }
}