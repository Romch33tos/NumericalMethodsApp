using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NumericalMethodsApp
{
  public class LinearEquationsModel
  {
    private readonly Random random;

    public LinearEquationsModel()
    {
      random = new Random();
    }

    public (double[,] matrixA, double[] vectorB) ImportFromCsv(string filePath)
    {
      var lines = File.ReadAllLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

      if (lines.Length < 2)
      {
        throw new ArgumentException("Файл должен содержать как минимум 2 строки");
      }

      int matrixStartLine = -1;
      int vectorStartLine = -1;

      for (int lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
      {
        if (lines[lineIndex].StartsWith("=== Матрица A ==="))
          matrixStartLine = lineIndex;
        else if (lines[lineIndex].StartsWith("=== Вектор B ==="))
          vectorStartLine = lineIndex;
      }

      if (matrixStartLine == -1 || vectorStartLine == -1)
      {
        throw new ArgumentException("Неверный формат CSV файла");
      }

      var matrixData = new List<string[]>();
      for (int lineIndex = matrixStartLine + 1; lineIndex < vectorStartLine; ++lineIndex)
      {
        if (string.IsNullOrWhiteSpace(lines[lineIndex])) continue;
        matrixData.Add(lines[lineIndex].Split(','));
      }

      int rowCount = matrixData.Count;
      int columnCount = matrixData[0].Length;

      var matrixA = new double[rowCount, columnCount];
      for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex)
        {
          if (!double.TryParse(matrixData[rowIndex][columnIndex], out double value))
          {
            throw new ArgumentException($"Неверное значение в матрице A: [{rowIndex}, {columnIndex}]");
          }
          matrixA[rowIndex, columnIndex] = Math.Round(value, 3);
        }
      }

      var vectorValues = lines[vectorStartLine + 1].Split(',');
      if (vectorValues.Length != rowCount)
      {
        throw new ArgumentException("Размер вектора B не соответствует матрице A");
      }

      var vectorB = new double[rowCount];
      for (int elementIndex = 0; elementIndex < rowCount; ++elementIndex)
      {
        if (!double.TryParse(vectorValues[elementIndex], out double value))
        {
          throw new ArgumentException($"Неверное значение в векторе B: [{elementIndex}]");
        }
        vectorB[elementIndex] = Math.Round(value, 3);
      }

      return (matrixA, vectorB);
    }

    public double[,] GenerateRandomMatrix(int rowCount, int columnCount, int minValue = 1, int maxValue = 15)
    {
      var matrix = new double[rowCount, columnCount];
      for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex)
        {
          matrix[rowIndex, columnIndex] = random.Next(minValue, maxValue + 1);
        }
      }
      return matrix;
    }

    public double[] GenerateRandomVector(int size, int minValue = 1, int maxValue = 15)
    {
      var vector = new double[size];
      for (int elementIndex = 0; elementIndex < size; ++elementIndex)
      {
        vector[elementIndex] = random.Next(minValue, maxValue + 1);
      }
      return vector;
    }

    public bool ValidateMatrix(double[,] matrix)
    {
      return matrix != null && matrix.GetLength(0) >= 2 && matrix.GetLength(1) >= 2;
    }

    public bool ValidateVector(double[] vector)
    {
      return vector != null && vector.Length >= 2;
    }

    public bool ValidateSystem(double[,] matrixA, double[] vectorB)
    {
      return ValidateMatrix(matrixA) && ValidateVector(vectorB) && matrixA.GetLength(0) == vectorB.Length;
    }

    public async Task<(double[] solution, double executionTimeMs)> SolveWithGaussAsync(double[,] coefficients, double[] constants)
    {
      return await Task.Run(() =>
      {
        var startTime = DateTime.Now;
        try
        {
          var solution = SolveWithGauss(coefficients, constants);
          var endTime = DateTime.Now;
          return (solution, (endTime - startTime).TotalMilliseconds);
        }
        catch
        {
          var endTime = DateTime.Now;
          throw;
        }
      });
    }

    public async Task<(double[] solution, double executionTimeMs)> SolveWithJordanGaussAsync(double[,] coefficients, double[] constants)
    {
      return await Task.Run(() =>
      {
        var startTime = DateTime.Now;
        try
        {
          var solution = SolveWithJordanGauss(coefficients, constants);
          var endTime = DateTime.Now;
          return (solution, (endTime - startTime).TotalMilliseconds);
        }
        catch
        {
          var endTime = DateTime.Now;
          throw;
        }
      });
    }

    public async Task<(double[] solution, double executionTimeMs)> SolveWithCramerAsync(double[,] coefficients, double[] constants)
    {
      return await Task.Run(() =>
      {
        var startTime = DateTime.Now;
        try
        {
          var solution = SolveWithCramer(coefficients, constants);
          var endTime = DateTime.Now;
          return (solution, (endTime - startTime).TotalMilliseconds);
        }
        catch
        {
          var endTime = DateTime.Now;
          throw;
        }
      });
    }

    private double[] SolveWithGauss(double[,] coefficients, double[] constants)
    {
      int rowCount = coefficients.GetLength(0);
      int columnCount = coefficients.GetLength(1);

      double[,] augmentedMatrix = CreateAugmentedMatrix(coefficients, constants);
      int rank = 0;

      for (int pivotColumn = 0; pivotColumn < columnCount && rank < rowCount; ++pivotColumn)
      {
        int maxRowIndex = rank;
        double maxValue = Math.Abs(augmentedMatrix[rank, pivotColumn]);

        for (int rowIndex = rank + 1; rowIndex < rowCount; ++rowIndex)
        {
          if (Math.Abs(augmentedMatrix[rowIndex, pivotColumn]) > maxValue)
          {
            maxValue = Math.Abs(augmentedMatrix[rowIndex, pivotColumn]);
            maxRowIndex = rowIndex;
          }
        }

        if (Math.Abs(augmentedMatrix[maxRowIndex, pivotColumn]) < 1e-12)
        {
          continue;
        }

        if (maxRowIndex != rank)
        {
          for (int columnIndex = pivotColumn; columnIndex <= columnCount; ++columnIndex)
          {
            (augmentedMatrix[rank, columnIndex], augmentedMatrix[maxRowIndex, columnIndex]) =
              (augmentedMatrix[maxRowIndex, columnIndex], augmentedMatrix[rank, columnIndex]);
          }
        }

        double pivotElement = augmentedMatrix[rank, pivotColumn];
        for (int columnIndex = pivotColumn; columnIndex <= columnCount; ++columnIndex)
        {
          augmentedMatrix[rank, columnIndex] /= pivotElement;
        }

        for (int rowIndex = rank + 1; rowIndex < rowCount; ++rowIndex)
        {
          double factor = augmentedMatrix[rowIndex, pivotColumn];
          for (int columnIndex = pivotColumn; columnIndex <= columnCount; ++columnIndex)
          {
            augmentedMatrix[rowIndex, columnIndex] -= factor * augmentedMatrix[rank, columnIndex];
          }
        }

        ++rank;
      }

      for (int rowIndex = rank; rowIndex < rowCount; ++rowIndex)
      {
        if (Math.Abs(augmentedMatrix[rowIndex, columnCount]) > 1e-12)
        {
          throw new InvalidOperationException("Система несовместна");
        }
      }

      if (rank < columnCount)
      {
        throw new InvalidOperationException("Система имеет бесконечное число решений");
      }

      double[] solution = new double[columnCount];
      for (int rowIndex = rank - 1; rowIndex >= 0; --rowIndex)
      {
        int pivotColumn = -1;
        for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex)
        {
          if (Math.Abs(augmentedMatrix[rowIndex, columnIndex]) > 1e-12)
          {
            pivotColumn = columnIndex;
            break;
          }
        }

        if (pivotColumn >= 0)
        {
          double value = augmentedMatrix[rowIndex, columnCount];
          for (int columnIndex = pivotColumn + 1; columnIndex < columnCount; ++columnIndex)
          {
            value -= augmentedMatrix[rowIndex, columnIndex] * solution[columnIndex];
          }
          solution[pivotColumn] = Math.Round(value, 6);
        }
      }

      return solution;
    }

    private double[] SolveWithJordanGauss(double[,] coefficients, double[] constants)
    {
      int rowCount = coefficients.GetLength(0);
      int columnCount = coefficients.GetLength(1);

      double[,] augmentedMatrix = CreateAugmentedMatrix(coefficients, constants);
      int rank = 0;

      for (int pivotColumn = 0; pivotColumn < columnCount && rank < rowCount; ++pivotColumn)
      {
        int maxRowIndex = rank;
        double maxValue = Math.Abs(augmentedMatrix[rank, pivotColumn]);

        for (int rowIndex = rank + 1; rowIndex < rowCount; ++rowIndex)
        {
          if (Math.Abs(augmentedMatrix[rowIndex, pivotColumn]) > maxValue)
          {
            maxValue = Math.Abs(augmentedMatrix[rowIndex, pivotColumn]);
            maxRowIndex = rowIndex;
          }
        }

        if (Math.Abs(augmentedMatrix[maxRowIndex, pivotColumn]) < 1e-12)
        {
          continue;
        }

        if (maxRowIndex != rank)
        {
          for (int columnIndex = 0; columnIndex <= columnCount; ++columnIndex)
          {
            (augmentedMatrix[rank, columnIndex], augmentedMatrix[maxRowIndex, columnIndex]) =
              (augmentedMatrix[maxRowIndex, columnIndex], augmentedMatrix[rank, columnIndex]);
          }
        }

        double pivotElement = augmentedMatrix[rank, pivotColumn];
        for (int columnIndex = 0; columnIndex <= columnCount; ++columnIndex)
        {
          augmentedMatrix[rank, columnIndex] /= pivotElement;
        }

        for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
        {
          if (rowIndex != rank)
          {
            double factor = augmentedMatrix[rowIndex, pivotColumn];
            for (int columnIndex = 0; columnIndex <= columnCount; ++columnIndex)
            {
              augmentedMatrix[rowIndex, columnIndex] -= factor * augmentedMatrix[rank, columnIndex];
            }
          }
        }

        ++rank;
      }

      for (int rowIndex = rank; rowIndex < rowCount; ++rowIndex)
      {
        if (Math.Abs(augmentedMatrix[rowIndex, columnCount]) > 1e-12)
        {
          throw new InvalidOperationException("Система несовместна");
        }
      }

      if (rank < columnCount)
      {
        throw new InvalidOperationException("Система имеет бесконечное число решений");
      }

      double[] solution = new double[columnCount];
      for (int rowIndex = 0; rowIndex < rank; ++rowIndex)
      {
        int pivotColumn = -1;
        for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex)
        {
          if (Math.Abs(augmentedMatrix[rowIndex, columnIndex]) > 1e-12)
          {
            pivotColumn = columnIndex;
            break;
          }
        }

        if (pivotColumn >= 0)
        {
          solution[pivotColumn] = Math.Round(augmentedMatrix[rowIndex, columnCount], 6);
        }
      }

      return solution;
    }

    private double[] SolveWithCramer(double[,] coefficients, double[] constants)
    {
      int systemSize = constants.Length;

      if (coefficients.GetLength(0) != coefficients.GetLength(1))
      {
        throw new InvalidOperationException("Метод Крамера применим только для квадратных матриц");
      }

      if (coefficients.GetLength(0) != systemSize)
      {
        throw new InvalidOperationException("Размерность матрицы и вектора не совпадают");
      }

      double mainDeterminant = CalculateDeterminant(coefficients);

      if (Math.Abs(mainDeterminant) < 1e-12)
      {
        throw new InvalidOperationException("Определитель матрицы равен нулю - система либо не имеет решений, либо имеет бесконечное число решений");
      }

      double[] solution = new double[systemSize];
      for (int variableIndex = 0; variableIndex < systemSize; ++variableIndex)
      {
        double[,] modifiedMatrix = ReplaceColumn(coefficients, constants, variableIndex);
        double columnDeterminant = CalculateDeterminant(modifiedMatrix);
        solution[variableIndex] = Math.Round(columnDeterminant / mainDeterminant, 6);
      }

      return solution;
    }

    private double[,] ReplaceColumn(double[,] originalMatrix, double[] newColumn, int columnIndex)
    {
      int matrixSize = newColumn.Length;
      double[,] resultMatrix = (double[,])originalMatrix.Clone();

      for (int rowIndex = 0; rowIndex < matrixSize; ++rowIndex)
      {
        resultMatrix[rowIndex, columnIndex] = newColumn[rowIndex];
      }

      return resultMatrix;
    }

    private double CalculateDeterminant(double[,] matrix)
    {
      int matrixSize = matrix.GetLength(0);

      if (matrixSize == 1)
      {
        return matrix[0, 0];
      }

      if (matrixSize == 2)
      {
        return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
      }

      double determinant = 0;
      for (int columnIndex = 0; columnIndex < matrixSize; ++columnIndex)
      {
        double[,] minorMatrix = GetMinor(matrix, 0, columnIndex);
        double sign = (columnIndex % 2 == 0) ? 1 : -1;
        determinant += sign * matrix[0, columnIndex] * CalculateDeterminant(minorMatrix);
      }

      return determinant;
    }

    private double[,] GetMinor(double[,] matrix, int skipRow, int skipColumn)
    {
      int matrixSize = matrix.GetLength(0);
      double[,] minorMatrix = new double[matrixSize - 1, matrixSize - 1];

      int minorRowIndex = 0;
      for (int rowIndex = 0; rowIndex < matrixSize; ++rowIndex)
      {
        if (rowIndex == skipRow) continue;

        int minorColumnIndex = 0;
        for (int columnIndex = 0; columnIndex < matrixSize; ++columnIndex)
        {
          if (columnIndex == skipColumn) continue;
          minorMatrix[minorRowIndex, minorColumnIndex] = matrix[rowIndex, columnIndex];
          ++minorColumnIndex;
        }
        ++minorRowIndex;
      }

      return minorMatrix;
    }

    private double[,] CreateAugmentedMatrix(double[,] coefficients, double[] constants)
    {
      int rowCount = coefficients.GetLength(0);
      int columnCount = coefficients.GetLength(1);
      double[,] augmentedMatrix = new double[rowCount, columnCount + 1];

      for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex)
      {
        for (int columnIndex = 0; columnIndex < columnCount; ++columnIndex)
        {
          augmentedMatrix[rowIndex, columnIndex] = coefficients[rowIndex, columnIndex];
        }
        augmentedMatrix[rowIndex, columnCount] = constants[rowIndex];
      }

      return augmentedMatrix;
    }
  }
}
