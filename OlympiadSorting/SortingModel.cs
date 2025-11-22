using System;
using System.Collections.Generic;

namespace NumericalMethodsApp.OlympiadSorting
{
  public class SortingAlgorithms
  {
    private readonly Random random = new Random();

    public SortResult BubbleSort(double[] array, bool ascending, int maxIterations)
    {
      double[] sortedArray = (double[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      bool swapped;
      for (int outerIndex = 0; outerIndex < sortedArray.Length - 1; ++outerIndex)
      {
        swapped = false;
        for (int innerIndex = 0; innerIndex < sortedArray.Length - outerIndex - 1; ++innerIndex)
        {
          ++iterationsCount;
          if (iterationsCount >= maxIterations)
          {
            iterationLimitExceeded = true;
            stopwatch.Stop();
            return new SortResult("Пузырьковая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
          }

          bool shouldSwap = ascending ?
              sortedArray[innerIndex] > sortedArray[innerIndex + 1] :
              sortedArray[innerIndex] < sortedArray[innerIndex + 1];

          if (shouldSwap)
          {
            double temporaryValue = sortedArray[innerIndex];
            sortedArray[innerIndex] = sortedArray[innerIndex + 1];
            sortedArray[innerIndex + 1] = temporaryValue;
            swapped = true;
          }
        }
        if (!swapped) break;
      }

      stopwatch.Stop();
      return new SortResult("Пузырьковая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    public SortResult InsertionSort(double[] array, bool ascending, int maxIterations)
    {
      double[] sortedArray = (double[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      for (int currentIndex = 1; currentIndex < sortedArray.Length; ++currentIndex)
      {
        double keyValue = sortedArray[currentIndex];
        int previousIndex = currentIndex - 1;

        while (previousIndex >= 0)
        {
          ++iterationsCount;
          if (iterationsCount >= maxIterations)
          {
            iterationLimitExceeded = true;
            stopwatch.Stop();
            return new SortResult("Вставками", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
          }

          bool shouldMove = ascending ?
              sortedArray[previousIndex] > keyValue :
              sortedArray[previousIndex] < keyValue;

          if (shouldMove)
          {
            sortedArray[previousIndex + 1] = sortedArray[previousIndex];
            --previousIndex;
          }
          else
          {
            break;
          }
        }
        sortedArray[previousIndex + 1] = keyValue;
      }

      stopwatch.Stop();
      return new SortResult("Вставками", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    public SortResult ShakerSort(double[] array, bool ascending, int maxIterations)
    {
      double[] sortedArray = (double[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      int leftBoundary = 0;
      int rightBoundary = sortedArray.Length - 1;
      bool wasSwapped;

      do
      {
        wasSwapped = false;

        for (int currentIndex = leftBoundary; currentIndex < rightBoundary; ++currentIndex)
        {
          ++iterationsCount;
          if (iterationsCount >= maxIterations)
          {
            iterationLimitExceeded = true;
            stopwatch.Stop();
            return new SortResult("Шейкерная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
          }

          bool shouldSwap = ascending ?
              sortedArray[currentIndex] > sortedArray[currentIndex + 1] :
              sortedArray[currentIndex] < sortedArray[currentIndex + 1];

          if (shouldSwap)
          {
            double temporaryValue = sortedArray[currentIndex];
            sortedArray[currentIndex] = sortedArray[currentIndex + 1];
            sortedArray[currentIndex + 1] = temporaryValue;
            wasSwapped = true;
          }
        }

        --rightBoundary;

        wasSwapped = false;

        for (int currentIndex = rightBoundary; currentIndex >= leftBoundary; --currentIndex)
        {
          ++iterationsCount;
          if (iterationsCount >= maxIterations)
          {
            iterationLimitExceeded = true;
            stopwatch.Stop();
            return new SortResult("Шейкерная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
          }

          bool shouldSwap = ascending ?
              sortedArray[currentIndex] > sortedArray[currentIndex + 1] :
              sortedArray[currentIndex] < sortedArray[currentIndex + 1];

          if (shouldSwap)
          {
            double temporaryValue = sortedArray[currentIndex];
            sortedArray[currentIndex] = sortedArray[currentIndex + 1];
            sortedArray[currentIndex + 1] = temporaryValue;
            wasSwapped = true;
          }
        }

        ++leftBoundary;

      } while (wasSwapped && leftBoundary <= rightBoundary);

      stopwatch.Stop();
      return new SortResult("Шейкерная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    public SortResult QuickSort(double[] array, bool ascending, int maxIterations)
    {
      double[] sortedArray = (double[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      QuickSortRecursive(sortedArray, 0, sortedArray.Length - 1, ascending, ref iterationsCount, maxIterations, ref iterationLimitExceeded);

      stopwatch.Stop();
      return new SortResult("Быстрая", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private void QuickSortRecursive(double[] array, int lowIndex, int highIndex, bool ascending, ref int iterationsCount, int maxIterations, ref bool iterationLimitExceeded)
    {
      ++iterationsCount;
      if (iterationsCount >= maxIterations)
      {
        iterationLimitExceeded = true;
        return;
      }

      if (lowIndex < highIndex)
      {
        int pivotIndex = Partition(array, lowIndex, highIndex, ascending, ref iterationsCount, maxIterations, ref iterationLimitExceeded);
        if (iterationLimitExceeded) return;

        QuickSortRecursive(array, lowIndex, pivotIndex - 1, ascending, ref iterationsCount, maxIterations, ref iterationLimitExceeded);
        if (iterationLimitExceeded) return;

        QuickSortRecursive(array, pivotIndex + 1, highIndex, ascending, ref iterationsCount, maxIterations, ref iterationLimitExceeded);
      }
    }

    private int Partition(double[] array, int lowIndex, int highIndex, bool ascending, ref int iterationsCount, int maxIterations, ref bool iterationLimitExceeded)
    {
      double pivotValue = array[highIndex];
      int partitionIndex = lowIndex - 1;

      for (int currentIndex = lowIndex; currentIndex < highIndex; ++currentIndex)
      {
        ++iterationsCount;
        if (iterationsCount >= maxIterations)
        {
          iterationLimitExceeded = true;
          return partitionIndex + 1;
        }

        bool shouldSwap = ascending ?
            array[currentIndex] <= pivotValue :
            array[currentIndex] >= pivotValue;

        if (shouldSwap)
        {
          ++partitionIndex;
          double temporaryValue = array[partitionIndex];
          array[partitionIndex] = array[currentIndex];
          array[currentIndex] = temporaryValue;
        }
      }

      double temporaryValue2 = array[partitionIndex + 1];
      array[partitionIndex + 1] = array[highIndex];
      array[highIndex] = temporaryValue2;

      return partitionIndex + 1;
    }

    public SortResult BogoSort(double[] array, bool ascending, int maxIterations)
    {
      double[] sortedArray = (double[])array.Clone();
      int iterationsCount = 0;
      bool iterationLimitExceeded = false;
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      while (!IsSorted(sortedArray, ascending))
      {
        ++iterationsCount;
        if (iterationsCount >= maxIterations)
        {
          iterationLimitExceeded = true;
          break;
        }
        ShuffleArray(sortedArray);
      }

      stopwatch.Stop();
      return new SortResult("Болотная", stopwatch.Elapsed.TotalMilliseconds, iterationsCount, sortedArray, iterationLimitExceeded);
    }

    private bool IsSorted(double[] array, bool ascending)
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

    private void ShuffleArray(double[] array)
    {
      for (int index = 0; index < array.Length; ++index)
      {
        int randomIndex = random.Next(array.Length);
        double temporaryValue = array[index];
        array[index] = array[randomIndex];
        array[randomIndex] = temporaryValue;
      }
    }
  }

  public class SortResult
  {
    public string AlgorithmName { get; }
    public double TimeMs { get; }
    public int Iterations { get; }
    public double[] SortedArray { get; }
    public bool IterationLimitExceeded { get; }

    public SortResult(string algorithmName, double timeMs, int iterations, double[] sortedArray, bool iterationLimitExceeded)
    {
      AlgorithmName = algorithmName;
      TimeMs = timeMs;
      Iterations = iterations;
      SortedArray = sortedArray;
      IterationLimitExceeded = iterationLimitExceeded;
    }
  }

  public class DataItem
  {
    public int Index { get; set; }
    public double Value { get; set; }
  }

  public class SortingModel
  {
    public List<double> OriginalArray { get; set; } = new List<double>();
    public List<SortResult> SortResults { get; set; } = new List<SortResult>();
    public Random RandomGenerator { get; } = new Random();
  }
}
