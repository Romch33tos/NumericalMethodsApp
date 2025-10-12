using System;
using System.Windows;
using System.Windows.Controls;

namespace NumericalMethodsApp
{
  public partial class GoogleSheetsImportDialog : Window
  {
    public string Url { get; private set; }

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

      Url = UrlTextBox.Text.Trim();
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