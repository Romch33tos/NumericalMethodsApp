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
      message += "Эта функциональность будет реализована в будущем.";

      _view.ShowMessage(message);
    }

    public void ShowAbout()
    {
      string aboutText = "Численные методы v1.0\n\n" +
                        "Приложение для реализации основных численных методов:";

      _view.ShowMessage(aboutText);
    }

    public void ExitApplication()
    {
      _view.CloseApplication();
    }
  }
}