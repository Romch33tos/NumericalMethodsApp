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
    private readonly Random random = new Random();

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

    public SortResult InsertionSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      for (int currentIndex = 1; currentIndex < sortedArray.Length && iterationsCount < maxIterations; ++currentIndex)
      {
        int keyValue = sortedArray[currentIndex];
        int previousIndex = currentIndex - 1;

        while (previousIndex >= 0 && iterationsCount < maxIterations)
        {
          bool shouldMove = ascending ?
            sortedArray[previousIndex] > keyValue :
            sortedArray[previousIndex] < keyValue;

          if (shouldMove)
          {
            sortedArray[previousIndex + 1] = sortedArray[previousIndex];
            --previousIndex;
            ++iterationsCount;
          }
          else
          {
            break;
          }
        }
        sortedArray[previousIndex + 1] = keyValue;
        ++iterationsCount;
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
      }

      return new SortResult("Вставками", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    public SortResult ShakerSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      int leftBoundary = 0;
      int rightBoundary = sortedArray.Length - 1;

      while (leftBoundary <= rightBoundary && iterationsCount < maxIterations)
      {
        for (int forwardIndex = leftBoundary; forwardIndex < rightBoundary && iterationsCount < maxIterations; ++forwardIndex)
        {
          bool shouldSwap = ascending ?
            sortedArray[forwardIndex] > sortedArray[forwardIndex + 1] :
            sortedArray[forwardIndex] < sortedArray[forwardIndex + 1];

          if (shouldSwap)
          {
            int temporaryValue = sortedArray[forwardIndex];
            sortedArray[forwardIndex] = sortedArray[forwardIndex + 1];
            sortedArray[forwardIndex + 1] = temporaryValue;
          }
          ++iterationsCount;
        }
        --rightBoundary;

        for (int backwardIndex = rightBoundary; backwardIndex > leftBoundary && iterationsCount < maxIterations; --backwardIndex)
        {
          bool shouldSwap = ascending ?
            sortedArray[backwardIndex - 1] > sortedArray[backwardIndex] :
            sortedArray[backwardIndex - 1] < sortedArray[backwardIndex];

          if (shouldSwap)
          {
            int temporaryValue = sortedArray[backwardIndex];
            sortedArray[backwardIndex] = sortedArray[backwardIndex - 1];
            sortedArray[backwardIndex - 1] = temporaryValue;
          }
          ++iterationsCount;
        }
        ++leftBoundary;
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
      }

      return new SortResult("Шейкерная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    public SortResult QuickSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      QuickSortRecursive(sortedArray, 0, sortedArray.Length - 1, ascending, ref iterationsCount, maxIterations);

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
      }

      return new SortResult("Быстрая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private void QuickSortRecursive(int[] array, int lowIndex, int highIndex, bool ascending, ref int iterationsCount, int maxIterations)
    {
      if (lowIndex < highIndex && iterationsCount < maxIterations)
      {
        int pivotIndex = Partition(array, lowIndex, highIndex, ascending, ref iterationsCount, maxIterations);
        QuickSortRecursive(array, lowIndex, pivotIndex - 1, ascending, ref iterationsCount, maxIterations);
        QuickSortRecursive(array, pivotIndex + 1, highIndex, ascending, ref iterationsCount, maxIterations);
      }
      ++iterationsCount;
    }

    private int Partition(int[] array, int lowIndex, int highIndex, bool ascending, ref int iterationsCount, int maxIterations)
    {
      int pivotValue = array[highIndex];
      int partitionIndex = lowIndex - 1;

      for (int currentIndex = lowIndex; currentIndex < highIndex && iterationsCount < maxIterations; ++currentIndex)
      {
        bool shouldSwap = ascending ?
          array[currentIndex] <= pivotValue :
          array[currentIndex] >= pivotValue;

        if (shouldSwap)
        {
          ++partitionIndex;
          int temporaryValue = array[partitionIndex];
          array[partitionIndex] = array[currentIndex];
          array[currentIndex] = temporaryValue;
        }
        ++iterationsCount;
      }

      int temporaryValue2 = array[partitionIndex + 1];
      array[partitionIndex + 1] = array[highIndex];
      array[highIndex] = temporaryValue2;

      return partitionIndex + 1;
    }

    public SortResult BogoSort(int[] array, bool ascending, int maxIterations)
    {
      int[] sortedArray = (int[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      while (!IsSorted(sortedArray, ascending) && iterationsCount < maxIterations)
      {
        ShuffleArray(sortedArray);
        ++iterationsCount;
      }

      stopwatch.Stop();

      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
      }

      return new SortResult("Болотная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private bool IsSorted(int[] array, bool ascending)
    {
      for (int index = 0; index < array.Length - 1; ++index)
      {
        if (ascending && array[index] > array[index + 1])
          return false;
        if (!ascending && array[index] < array[index + 1])
          return false;
      }
      return true;
    }

    private void ShuffleArray(int[] array)
    {
      for (int index = 0; index < array.Length; ++index)
      {
        int randomIndex = random.Next(array.Length);
        int temporaryValue = array[index];
        array[index] = array[randomIndex];
        array[randomIndex] = temporaryValue;
      }
    }
  }
}
