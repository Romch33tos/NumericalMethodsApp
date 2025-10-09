using System.Collections.Generic;
using System.Threading.Tasks;

namespace NumericalMethodsApp
{
  public interface ILinearEquationsModel
  {
    Task<(double[] solution, double executionTimeMs)> SolveWithGaussAsync(double[,] matrixA, double[] vectorB);
    Task<(double[] solution, double executionTimeMs)> SolveWithJordanGaussAsync(double[,] matrixA, double[] vectorB);
    Task<(double[] solution, double executionTimeMs)> SolveWithCramerAsync(double[,] matrixA, double[] vectorB);
    double[,] GenerateRandomMatrix(int rows, int cols, int minValue = 1, int maxValue = 100);
    double[] GenerateRandomVector(int size, int minValue = 1, int maxValue = 100);
    (double[,] matrixA, double[] vectorB) ImportFromCsv(string filePath);
    void ExportToCsv(string filePath, double[,] matrixA, double[] vectorB, double[] vectorX, List<ExecutionResult> results);
    bool ValidateMatrix(double[,] matrix);
    bool ValidateVector(double[] vector);
    bool ValidateSystem(double[,] matrixA, double[] vectorB);
  }
}