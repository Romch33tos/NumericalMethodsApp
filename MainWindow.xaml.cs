using System.Windows;
using System.Windows.Controls;

namespace NumericalMethodsApp
{
  public partial class MainWindow : Window, IMainView
  {
    private MainPresenter _presenter;

    public MainWindow()
    {
      InitializeComponent();
      _presenter = new MainPresenter(this);
    }

    private void MethodButton_Click(object sender, RoutedEventArgs e)
    {
      var button = sender as Button;
      _presenter.OpenMethod(button.Tag.ToString());
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
      _presenter.ShowAbout();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
      _presenter.ExitApplication();
    }

    public void ShowMessage(string message)
    {
      MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void CloseApplication()
    {
      Application.Current.Shutdown();
    }
  }
}