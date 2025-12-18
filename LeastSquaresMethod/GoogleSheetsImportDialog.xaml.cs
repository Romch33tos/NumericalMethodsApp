using System.Windows;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public partial class GoogleSheetsImportDialog : Window
  {
    public string SheetsUrl { get; private set; }

    public GoogleSheetsImportDialog()
    {
      InitializeComponent();
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(UrlTextBox.Text))
      {
        MessageBox.Show("Введите ссылку на Google Таблицу", "Ошибка",
                       MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!UrlTextBox.Text.StartsWith("https://docs.google.com/spreadsheets/"))
      {
        MessageBox.Show("Введите корректную ссылку на Google Таблицу", "Ошибка",
                       MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      SheetsUrl = UrlTextBox.Text.Trim();
      DialogResult = true;
      Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }
  }
}