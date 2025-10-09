using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NumericalMethodsApp
{
  public class LinearEquationsPresenter
  {
    private readonly ILinearEquationsView view;
    private readonly ILinearEquationsModel model;

    private double[,] currentMatrix;
    private double[] currentVectorB;
    private double[] currentSolution;
    private readonly List<ExecutionResult> executionResults;

    public LinearEquationsPresenter(ILinearEquationsView view, ILinearEquationsModel model)
    {
      this.view = view;
      this.model = model;
      executionResults = new List<ExecutionResult>();

      SubscribeToEvents();
      InitializeView();
    }

    private void SubscribeToEvents()
    {
      view.ApplyDimensionsClicked += OnApplyDimensions;
      view.RandomGenerationClicked += OnRandomGeneration;
      view.ImportFromCsvClicked += OnImportFromCsv;
      view.ImportFromGoogleSheetsClicked += OnImportFromGoogleSheets;
      view.SolveClicked += OnSolve;
      view.ExportToCsvClicked += OnExportToCsv;
      view.ClearAllClicked += OnClearAll;
      view.HelpClicked += OnHelp;
    }

    private void InitializeView()
    {
      view.MatrixRows = 0;
      view.MatrixCols = 0;
      view.UpdateControlsState(false);
    }

    private void OnApplyDimensions()
    {
      try
      {
        if (view.MatrixRows >= 2 && view.MatrixRows <= 50 &&
            view.MatrixCols >= 2 && view.MatrixCols <= 50)
        {
          CreateEmptyMatrix(view.MatrixRows, view.MatrixCols);
          view.UpdateControlsState(true);
        }
        else
        {
          view.ShowMessage("Размерность матрицы должна быть от 2 до 50", "Ошибка", MessageBoxImage.Error);
        }
      }
      catch (Exception exception)
      {
        view.ShowMessage($"Ошибка при изменении размерности: {exception.Message}", "Ошибка", MessageBoxImage.Error);
      }
    }

    private void OnRandomGeneration()
    {
      try
      {
        currentMatrix = model.GenerateRandomMatrix(view.MatrixRows, view.MatrixCols, 1, 15);
        currentVectorB = model.GenerateRandomVector(view.MatrixRows, 1, 15);

        view.MatrixA = currentMatrix;
        view.VectorB = currentVectorB;
      }
      catch (Exception exception)
      {
        view.ShowMessage($"Ошибка при генерации: {exception.Message}", "Ошибка", MessageBoxImage.Error);
      }
    }

    private void OnImportFromCsv()
    {
      try
      {
        var dialog = new OpenFileDialog
        {
          Filter = "CSV files (*.csv)|*.csv",
          Title = "Импорт данных из CSV"
        };

        if (dialog.ShowDialog() == true)
        {
          var (matrixA, vectorB) = model.ImportFromCsv(dialog.FileName);

          currentMatrix = matrixA;
          currentVectorB = vectorB;
          view.MatrixRows = matrixA.GetLength(0);
          view.MatrixCols = matrixA.GetLength(1);
          view.MatrixA = matrixA;
          view.VectorB = vectorB;

          view.UpdateControlsState(true);
        }
      }
      catch (Exception exception)
      {
        view.ShowMessage($"Ошибка при импорте: {exception.Message}", "Ошибка", MessageBoxImage.Error);
      }
    }

    private void OnImportFromGoogleSheets()
    {
      try
      {
        string url = ShowGoogleSheetsUrlDialog();
        if (!string.IsNullOrEmpty(url))
        {
          var (matrixA, vectorB) = model.ImportFromGoogleSheets(url);

          currentMatrix = matrixA;
          currentVectorB = vectorB;
          view.MatrixRows = matrixA.GetLength(0);
          view.MatrixCols = matrixA.GetLength(1);
          view.MatrixA = matrixA;
          view.VectorB = vectorB;

          view.UpdateControlsState(true);
          view.ShowMessage("Данные успешно импортированы из Google Таблицы", "Импорт завершен", MessageBoxImage.Information);
        }
      }
      catch (Exception exception)
      {
        view.ShowMessage($"Ошибка при импорте из Google Таблицы: {exception.Message}", "Ошибка", MessageBoxImage.Error);
      }
    }

    private string ShowGoogleSheetsUrlDialog()
    {
      var dialog = new GoogleSheetsImportDialog();
      if (dialog.ShowDialog() == true)
      {
        return dialog.Url;
      }
      return null;
    }

    private async void OnSolve()
    {
      try
      {
        currentMatrix = view.MatrixA;
        currentVectorB = view.VectorB;

        if (!ValidateDataTables())
        {
          view.ShowMessage("Заполните все ячейки матрицы A и вектора B корректными числами", "Ошибка", MessageBoxImage.Error);
          return;
        }

        if (!model.ValidateSystem(currentMatrix, currentVectorB))
        {
          view.ShowMessage("Некорректные данные матрицы A или вектора B", "Ошибка", MessageBoxImage.Error);
          return;
        }

        if (!view.IsGaussSelected && !view.IsJordanGaussSelected && !view.IsCramerSelected)
        {
          view.ShowMessage("Выберите хотя бы один метод решения", "Ошибка", MessageBoxImage.Error);
          return;
        }

        executionResults.Clear();
        currentSolution = null;

        bool canUseCramer = view.IsCramerSelected &&
                           currentMatrix.GetLength(0) == currentMatrix.GetLength(1) &&
                           currentMatrix.GetLength(0) <= 8;

        var tasks = new List<Task>();

        if (view.IsGaussSelected)
        {
          tasks.Add(SolveWithMethod("Метод Гаусса", model.SolveWithGaussAsync));
        }

        if (view.IsJordanGaussSelected)
        {
          tasks.Add(SolveWithMethod("Метод Жордана-Гаусса", model.SolveWithJordanGaussAsync));
        }

        if (view.IsCramerSelected)
        {
          if (!canUseCramer)
          {
            string message = currentMatrix.GetLength(0) != currentMatrix.GetLength(1)
              ? "Метод Крамера применим только для квадратных матриц"
              : "Метод Крамера слишком медленный для матриц размером более 8x8";

            executionResults.Add(new ExecutionResult
            {
              MethodName = "Метод Крамера",
              ExecutionTimeMs = 0,
              Status = "Ошибка",
              ErrorMessage = message,
              DetailedStatus = message
            });
          }
          else
          {
            tasks.Add(SolveWithMethod("Метод Крамера", model.SolveWithCramerAsync));
          }
        }

        if (tasks.Count > 0)
        {
          await Task.WhenAll(tasks);
        }

        view.DisplayExecutionResults(executionResults);

        if (currentSolution != null)
        {
          view.DisplaySolution(currentSolution);
        }
      }
      catch (Exception exception)
      {
        view.ShowMessage($"Ошибка при решении: {exception.Message}", "Ошибка", MessageBoxImage.Error);
      }
    }

    private bool ValidateDataTables()
    {
      try
      {
        var matrixA = view.MatrixA;
        var vectorB = view.VectorB;

        if (matrixA == null || matrixA.Length == 0 || vectorB == null || vectorB.Length == 0)
          return false;

        for (int rowIndex = 0; rowIndex < matrixA.GetLength(0); ++rowIndex)
        {
          for (int columnIndex = 0; columnIndex < matrixA.GetLength(1); ++columnIndex)
          {
            if (double.IsNaN(matrixA[rowIndex, columnIndex]) || double.IsInfinity(matrixA[rowIndex, columnIndex]))
              return false;
          }
        }

        for (int elementIndex = 0; elementIndex < vectorB.Length; ++elementIndex)
        {
          if (double.IsNaN(vectorB[elementIndex]) || double.IsInfinity(vectorB[elementIndex]))
            return false;
        }

        return true;
      }
      catch
      {
        return false;
      }
    }

    private async Task SolveWithMethod(string methodName, Func<double[,], double[], Task<(double[] solution, double executionTimeMs)>> solver)
    {
      double time = 0;
      try
      {
        var (solution, executionTime) = await solver(currentMatrix, currentVectorB);
        time = executionTime;

        if (currentSolution == null && solution != null)
        {
          currentSolution = solution;
        }

        executionResults.Add(new ExecutionResult
        {
          MethodName = methodName,
          ExecutionTimeMs = time,
          Status = "Успешно",
          DetailedStatus = "Решение найдено"
        });
      }
      catch (Exception exception)
      {
        string detailedStatus = GetDetailedStatus(exception.Message);

        executionResults.Add(new ExecutionResult
        {
          MethodName = methodName,
          ExecutionTimeMs = time,
          Status = "Ошибка",
          ErrorMessage = exception.Message,
          DetailedStatus = detailedStatus
        });
      }
    }

    private string GetDetailedStatus(string errorMessage)
    {
      if (errorMessage.Contains("несовместна") || errorMessage.Contains("нет решений"))
        return "Система несовместна - решений нет";
      else if (errorMessage.Contains("бесконечное число решений"))
        return "Система имеет бесконечное число решений";
      else if (errorMessage.Contains("Определитель матрицы равен нулю"))
        return "Определитель равен нулю - система либо не имеет решений, либо имеет бесконечное число решений";
      else if (errorMessage.Contains("квадратных матриц"))
        return "Метод применим только для квадратных матриц";
      else if (errorMessage.Contains("слишком медленный"))
        return "Метод слишком медленный для матриц такого размера";
      else if (errorMessage.Contains("Размерность матрицы и вектора не совпадают"))
        return "Размерность матрицы и вектора не совпадают";
      else
        return errorMessage;
    }

    private void OnExportToCsv()
    {
      try
      {
        var dialog = new SaveFileDialog
        {
          Filter = "CSV files (*.csv)|*.csv",
          Title = "Экспорт результатов в CSV"
        };

        if (dialog.ShowDialog() == true)
        {
          model.ExportToCsv(dialog.FileName, currentMatrix, currentVectorB, currentSolution, executionResults);
          view.ShowMessage("Данные успешно экспортированы в CSV файл", "Экспорт завершен", MessageBoxImage.Information);
        }
      }
      catch (Exception exception)
      {
        view.ShowMessage($"Ошибка при экспорте: {exception.Message}", "Ошибка", MessageBoxImage.Error);
      }
    }

    private void OnClearAll()
    {
      if (view.ShowConfirmation("Вы уверены, что хотите очистить все данные?", "Подтверждение очистки"))
      {
        view.ClearAllData();
        executionResults.Clear();
        currentMatrix = null;
        currentVectorB = null;
        currentSolution = null;

        InitializeView();
      }
    }

    private void OnHelp()
    {
      string helpText = @"РЕШЕНИЕ СИСТЕМ ЛИНЕЙНЫХ АЛГЕБРАИЧЕСКИХ УРАВНЕНИЙ

Формат уравнения: A∙X + B = 0

ИНСТРУКЦИЯ:
1. Укажите размерность матрицы (от 2 до 50)
2. Нажмите 'Применить размер'
3. Заполните матрицу A и вектор B
4. Выберите методы решения
5. Нажмите 'Решить СЛАУ'

ДОСТУПНЫЕ МЕТОДЫ:
• Метод Гаусса - для любых матриц
• Метод Жордана-Гаусса - для любых матриц  
• Метод Крамера - только для квадратных матриц до 8x8

ИМПОРТ ДАННЫХ:
• Из CSV файла
• Из Google Таблиц (ссылка должна быть публичной)

ВОЗМОЖНЫЕ РЕЗУЛЬТАТЫ:
• Решение найдено - система имеет единственное решение
• Система несовместна - решений нет
• Бесконечное число решений - система имеет множество решений
• Определитель равен нулю - система вырождена";

      view.ShowMessage(helpText, "Справка", MessageBoxImage.Information);
    }

    private void CreateEmptyMatrix(int rows, int columns)
    {
      currentMatrix = new double[rows, columns];
      currentVectorB = new double[rows];
      currentSolution = null;

      for (int rowIndex = 0; rowIndex < rows; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < columns; ++columnIndex)
        {
          currentMatrix[rowIndex, columnIndex] = 0;
        }
        currentVectorB[rowIndex] = 0;
      }

      view.MatrixA = currentMatrix;
      view.VectorB = currentVectorB;
      view.VectorX = null;
    }
  }
}
