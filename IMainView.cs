namespace NumericalMethodsApp
{
  public interface IMainView
  {
    void ShowMessage(string message);
    void CloseApplication();
    void OpenMethodWindow(string methodName, Window window, Button button);
  }
}
