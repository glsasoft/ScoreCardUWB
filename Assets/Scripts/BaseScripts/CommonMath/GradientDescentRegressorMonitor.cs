using UnityEngine;

namespace LetsPiu.Maths
{
    public abstract class GradientDescentRegressorMonitor
    {
        public abstract int iterationSampleCount { get; }
        public abstract int currentIterationStep { get; }
        public abstract int totalIterationStep { get; }
        
        public abstract float initializedIterativeFactor { get; }
        public abstract float currentIterativeFactor { get; }
        public abstract float minIterativeFactor { get; }
        
        public abstract int currentIterationCount { get; }
        public abstract int maxIterationCount { get; }
        
        public abstract float currentLose { get; }
        public abstract float deltaLose { get; }
        public abstract float terminateFactor { get; }
        public float terminateDeltaLose => terminateFactor * currentIterativeFactor;

        public override string ToString()
        {
            return $"Delta Lose Progress: x{deltaLose / terminateDeltaLose}\n" +
                   $"Iterative Factor Index: {Mathf.RoundToInt(Mathf.Log(initializedIterativeFactor / currentIterativeFactor, 2))}" +
                   $" / {Mathf.RoundToInt(Mathf.Log(initializedIterativeFactor / minIterativeFactor, 2))}" +
                   $"Iteration Count: {currentIterationCount} / {maxIterationCount}\n" +
                   $"Iteration Step: {currentIterationStep} / {totalIterationStep}";
        }
    }
}