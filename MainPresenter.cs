namespace NumericalMethodsApp
{
  public class MainPresenter
  {
    private readonly IMainView _view;

    public MainPresenter(IMainView view)
    {
      _view = view;
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

      _view.ShowMessage(message);
    }

    public void ShowAbout()
    {
      string aboutText = "Численные методы v1.0\n\n" +
                        "Приложение для реализации основных численных методов:\n" +
                        "- Метод дихотомии\n" +
                        "- Метод золотого сечения\n" +
                        "- Метод Ньютона\n" +
                        "- И другие методы оптимизации";

      _view.ShowMessage(aboutText);
    }

    public void ExitApplication()
    {
      _view.CloseApplication();
    }
  }
}
