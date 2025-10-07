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
        var dialog = new Microsoft.Win32.OpenFileDialog
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
