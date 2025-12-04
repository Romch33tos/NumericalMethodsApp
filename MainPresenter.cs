using NumericalMethodsApp.DefiniteIntegralMethod;
using NumericalMethodsApp.OlympiadSorting;
using NumericalMethodsApp.Views;
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
          GoldenRatioMethod goldenRatioWindow = new GoldenRatioMethod();
          view.OpenMethodWindow(methodName, goldenRatioWindow, button);
          break;
        case "Method3":
          OlympiadSortingView sortingWindow = new OlympiadSortingView();
          view.OpenMethodWindow(methodName, sortingWindow, button);
          break;
        case "Method4":
          string message = $"Метод {methodName} будет реализован в будущем.";
          view.ShowMessage(message);
          break;
        case "Method5":
          LinearEquationsView slauWindow = new LinearEquationsView();
          view.OpenMethodWindow(methodName, slauWindow, button);
          break;
        case "Method6":
          NewtonMethod newtonWindow = new NewtonMethod();
          view.OpenMethodWindow(methodName, newtonWindow, button);
          break;
        case "Method7":
          DefeniteIntegralMethod integralWindow = new DefeniteIntegralMethod();
          view.OpenMethodWindow(methodName, integralWindow, button);
          break;
        case "Method8":
          message = $"Метод {methodName} будет реализован в будущем.";
          view.ShowMessage(message);
          break;
        default:
          message = $"Метод {methodName} будет реализован в будущем.";
          view.ShowMessage(message);
          break;
      }
    }

    public void ShowAbout()
    {
      string aboutText = "Данное приложение предназначено для реализации следующих численных методов:\n\n" +
                       "- Метод дихотомии\n" +
                       "- Метод золотого сечения\n" +
                       "- Олимпиадные сортировки\n" +
                       "- Метод покоординатного спуска\n" +
                       "- Решение СЛАУ (Гаусс, Жордан-Гаусс, Крамер)\n" +
                       "- Метод Ньютона\n" +
                       "- Решение определенного интеграла\n" +
                       "- Метод наименьших квадратов";

      view.ShowMessage(aboutText);
    }

    public void ExitApplication()
    {
      view.CloseApplication();
    }
  }
}
