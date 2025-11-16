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
}
