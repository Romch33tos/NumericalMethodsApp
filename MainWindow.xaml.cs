using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace NumericalMethodsApp
{
  public partial class MainWindow : Window, IMainView
  {
    private MainPresenter presenter;
    private Dictionary<string, Window> openMethodWindows;

    public MainWindow()
    {
      InitializeComponent();
      presenter = new MainPresenter(this);
      openMethodWindows = new Dictionary<string, Window>();
    }

    private void MethodButton_Click(object sender, RoutedEventArgs e)
    {
      var button = sender as Button;
      string methodName = button.Tag.ToString();

      if (openMethodWindows.ContainsKey(methodName) && openMethodWindows[methodName] != null)
      {
        openMethodWindows[methodName].Activate();
        return;
      }

      if (methodName == "Method1")
      {
        DichotomyMethod dichotomyWindow = new DichotomyMethod();
        dichotomyWindow.Closed += (s, args) => MethodWindow_Closed(methodName);
        dichotomyWindow.Show();

        button.IsEnabled = false;
        openMethodWindows[methodName] = dichotomyWindow;
      }
      else
      {
        presenter.OpenMethod(methodName);
      }
    }

    private void MethodWindow_Closed(string methodName)
    {
      var button = FindName(methodName + "Button") as Button;
      if (button != null)
      {
        button.IsEnabled = true;
      }

      if (openMethodWindows.ContainsKey(methodName))
      {
        openMethodWindows[methodName] = null;
      }
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
      presenter.ShowAbout();
    }

    public void ShowMessage(string message)
    {
      MessageBox.Show(message, "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void CloseApplication()
    {
      Application.Current.Shutdown();
    }
  }
}
