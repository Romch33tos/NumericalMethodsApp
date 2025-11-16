using System;
using System.Collections.Generic;

namespace NumericalMethodsApp.OlympiadSorting
{
  public class SortingModel
  {
    public List<int> OriginalArray { get; set; } = new List<int>();
    public List<SortResult> SortResults { get; set; } = new List<SortResult>();
    public Random RandomGenerator { get; } = new Random();
  }

  public class SortResult
  {
    public string AlgorithmName { get; set; }
    public double TimeMs { get; set; }
    public int Iterations { get; set; }
    public int[] SortedArray { get; set; }
    public bool IterationLimitExceeded { get; set; }

    public SortResult(string algorithmName, double timeMs, int iterations, int[] sortedArray, bool iterationLimitExceeded = false)
    {
      AlgorithmName = algorithmName;
      TimeMs = Math.Round(timeMs, 3);
      Iterations = iterations;
      SortedArray = sortedArray;
      IterationLimitExceeded = iterationLimitExceeded;
    }
  }

  public class DataItem
  {
    public int Index { get; set; }
    public int Value { get; set; }
  }

  public class SortingAlgorithms
  {
    public SortResult BubbleSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      for (int outerIndex = 0; outerIndex < sortedArray.Length - 1 && iterationsCount < maxIterations; ++outerIndex)
      {
        for (int innerIndex = 0; innerIndex < sortedArray.Length - outerIndex - 1 && iterationsCount < maxIterations; ++innerIndex)
        {
          bool shouldSwap = ascending ?
            sortedArray[innerIndex] > sortedArray[innerIndex + 1] :
            sortedArray[innerIndex] < sortedArray[innerIndex + 1];

          if (shouldSwap)
          {
            int temporaryValue = sortedArray[innerIndex];
            sortedArray[innerIndex] = sortedArray[innerIndex + 1];
            sortedArray[innerIndex + 1] = temporaryValue;
          }
          ++iterationsCount;
        }
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
      }

      return new SortResult("Пузырьковая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }
  }
}
