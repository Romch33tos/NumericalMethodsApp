using System.Windows;
using System.Windows.Controls;

namespace NumericalMethodsApp
{
  public class MainPresenter
  {
    private readonly IMainView view;

    public MainPresenter(IMainView view)
    {
      this.view = view;
    }

    public void OpenMethod(string methodName, Button button)
    {
      switch (methodName)
      {
        case "Method1":
          DichotomyMethod dichotomyWindow = new DichotomyMethod();
          view.OpenMethodWindow(methodName, dichotomyWindow, button);
          break;
        case "Method2":
          GoldenRatioView goldenRatioWindow = new GoldenRatioView();
          view.OpenMethodWindow(methodName, goldenRatioWindow, button);
          break;
        case "Method5":
          LinearEquationsView slauWindow = new LinearEquationsView();
          view.OpenMethodWindow(methodName, slauWindow, button);
          break;
        default:
          string message = $"Метод {methodName} будет реализован в будущем.";
          view.ShowMessage(message);
          break;
      }
    }

    public void ShowAbout()
    {
      string aboutText = "Данное приложение предназначено для реализации следующих численных методов:\n\n" +
                       "- Метод дихотомии\n" +
                       "- Решение СЛАУ (Гаусс, Жордан-Гаусс, Крамер)\n" +
                       "- Метод Ньютона\n" +
                       "- Метод золотого сечения\n" +
                       "- Олимпиадные сортировки\n" +
                       "- Метод покоординатного спуска\n" +
                       "- Метод наименьших квадратов\n" +
                       "- Решение определенного интеграла";

      view.ShowMessage(aboutText);
    }

    public void ExitApplication()
    {
      view.CloseApplication();
    }
  }
}
