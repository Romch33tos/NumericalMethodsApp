using System.Windows;
using System.Windows.Controls;

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
  }
}
