using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NumericalMethodsApp.OlympiadSorting
{
  public partial class OlympiadSortingView : Window
  {
    public OlympiadSortingView()
    {
      InitializeComponent();
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(
          "Справка по использованию приложения:\n\n" +
          "1. Укажите размерность массива и диапазон значений\n" +
          "2. Выберите алгоритмы сортировки\n" +
          "3. Настройте параметры сортировки\n" +
          "4. Нажмите 'Запустить сортировку'\n\n" +
          "Результаты будут отображены в таблице и на графике.",
          "Справка",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
    }

    private void RandomGenerate_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Функция случайной генерации данных будет реализована позже.",
          "В разработке",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void ImportCsv_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Функция импорта из CSV будет реализована позже.",
          "В разработке",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void ImportGoogle_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Функция импорта из Google Таблиц будет реализована позже.",
          "В разработке",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Функция очистки всех данных будет реализована позже.",
          "В разработке",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void StartSorting_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Функция запуска сортировки будет реализована позже.",
          "В разработке",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
    }

    private void ResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void ApplySize_Click(object sender, RoutedEventArgs e)
    {
      if (int.TryParse(RowsTextBox.Text, out int size) && size > 0)
      {
        MessageBox.Show($"Размер массива установлен: {size} элементов",
            "Размер применен",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
      }
      else
      {
        MessageBox.Show("Пожалуйста, введите корректное положительное число для размера массива",
            "Ошибка",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        RowsTextBox.Text = "5";
      }
    }
  }
}
