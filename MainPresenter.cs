namespace NumericalMethodsApp
{
  public class MainPresenter
  {
    private readonly IMainView view;

    public MainPresenter(IMainView view)
    {
      this.view = view;
    }

    public void OpenMethod(string methodName)
    {
      string message = $"Открытие метода: {methodName}\n\n";

      switch (methodName)
      {
        case "Method1":
          message = "Метод дихотомии уже открыт в отдельном окне.";
          break;
        case "Method2":
          message += "Метод золотого сечения будет реализован в будущем.";
          break;
        case "Method3":
          message += "Метод Ньютона будет реализован в будущем.";
          break;
        default:
          message += "Эта функциональность будет реализована в будущем.";
          break;
      }

      view.ShowMessage(message);
    }

    public void ShowAbout()
    {
      string aboutText = "Данное приложение предназначено для реализации следующих численных методов:\n\n" +
                         "- Метод дихотомии\n" +
                         "- Метод Ньютона\n" +
                         "- Метод золотого сечения\n" +
                         "- Олимпиадные сортировки\n" +
                         "- Метод покоординатного спуска\n" +
                         "- Метод наименьших квадратов";

      view.ShowMessage(aboutText);
    }

    public void ExitApplication()
    {
      view.CloseApplication();
    }
  }
}
