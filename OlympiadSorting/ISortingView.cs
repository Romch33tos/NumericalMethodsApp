using System.Windows;
using System.Windows.Controls;

public interface ISortingView
{
  string RowsText { get; set; }
  string MinValueText { get; set; }
  string MaxValueText { get; set; }
  string MaxIterationsText { get; set; }
  bool IsBubbleSortChecked { get; set; }
  bool IsInsertionSortChecked { get; set; }
  bool IsShakerSortChecked { get; set; }
  bool IsQuickSortChecked { get; set; }
  bool IsBogosortChecked { get; set; }
  bool IsAscendingChecked { get; set; }
  bool IsDescendingChecked { get; set; }
  bool StartSortingEnabled { get; set; }

  event RoutedEventHandler HelpClicked;
  event RoutedEventHandler ApplySizeClicked;
  event RoutedEventHandler RandomGenerateClicked;
  event RoutedEventHandler ImportCsvClicked;
  event RoutedEventHandler ImportGoogleClicked;
  event RoutedEventHandler ClearAllClicked;
  event RoutedEventHandler StartSortingClicked;
  event RoutedEventHandler CheckBoxChecked;
  event System.EventHandler<DataGridCellEditEndingEventArgs> OriginalDataGridCellEditEnding;

  void SetOriginalDataGridItemsSource(object source);
  void SetSortedDataGridItemsSource(object source);
  void SetResultsDataGridItemsSource(object source);
}
