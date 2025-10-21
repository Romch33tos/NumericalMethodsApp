using System;
using System.Collections.Generic;

namespace NumericalMethodsApp.Models
{
  public class NewtonModel
  {
    public string FunctionExpression { get; set; }
    public double InitialPoint { get; set; }
    public double Epsilon { get; set; }
    public bool FindMinimum { get; set; }
    public bool FindMaximum { get; set; }

    public CalculationResultModel CalculationResult { get; private set; }
    public List<IterationStep> IterationSteps { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public bool CalculationComplete { get; private set; }
    public bool StepModeStarted { get; private set; }

    public NewtonModel()
    {
      Epsilon = 0.001;
      CalculationResult = new CalculationResultModel();
      IterationSteps = new List<IterationStep>();
      CurrentStepIndex = -1;
      CalculationComplete = false;
      StepModeStarted = false;
    }
  }

  public class IterationStep
  {
    public int IterationNumber { get; set; }
    public double Point { get; set; }
    public double FunctionValue { get; set; }
    public double FirstDerivative { get; set; }
    public double SecondDerivative { get; set; }
    public double NextPoint { get; set; }
  }

  public class CalculationResultModel
  {
    public double Point { get; set; }
    public double Value { get; set; }
    public bool IsMinimum { get; set; }
    public bool Success { get; set; }
  }
}
