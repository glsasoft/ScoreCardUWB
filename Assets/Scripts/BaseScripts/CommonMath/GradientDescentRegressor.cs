using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LetsPiu.Maths
{
    [Serializable]
    public abstract class GradientDescentRegressor<TIn, TOut>
    {
        [Serializable]
        public struct TSample
        {
            public TIn x;
            public TOut y;
        }

        public List<float> parameters = new();
        public List<TSample> samples = new();

        public float iterativeFactor = 0.125f;
        public int iterationSampleCount = 32;
        public float terminateFactor = 1e-4f;

        private int currentIterationStep;
        private int totalIterationStep;
        
        private float currentIterativeFactor;
        private const float minIterativeFactor = 1e-24f;
        
        private int currentIterationCount;
        private const int maxIterationCount = 65536;
        
        private float currentLose;
        private float deltaLose;
        private float terminateDeltaLose => terminateFactor * currentIterativeFactor;

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private class GenericsGradientDescentRegressorMonitor : GradientDescentRegressorMonitor
        {
            private readonly GradientDescentRegressor<TIn, TOut> regressor;
            
            public override int iterationSampleCount => regressor.iterationSampleCount;
            public override int currentIterationStep => regressor.currentIterationStep;
            public override int totalIterationStep => regressor.totalIterationStep;

            public override float initializedIterativeFactor => regressor.iterativeFactor;
            public override float currentIterativeFactor => regressor.currentIterativeFactor;
            public override float minIterativeFactor => GradientDescentRegressor<TIn, TOut>.minIterativeFactor;

            public override int currentIterationCount => regressor.currentIterationCount;
            public override int maxIterationCount => GradientDescentRegressor<TIn, TOut>.maxIterationCount;

            public override float currentLose => regressor.currentLose;
            public override float deltaLose => regressor.deltaLose;
            public override float terminateFactor => regressor.terminateFactor;

            public GenericsGradientDescentRegressorMonitor(GradientDescentRegressor<TIn, TOut> regressor)
            {
                this.regressor = regressor;
            }
        }

        private StringBuilder logger = new();

        private float[] sumGrad, sampleGrad;
        private int[] sampleOrder;
        
        private System.Random random = new();

        private class SampleEnumerable : IEnumerable<TSample>
        {
            private class SampleEnumerator : IEnumerator<TSample>
            {
                private readonly SampleEnumerable enumerable;
                private int curr;
            
                public SampleEnumerator(SampleEnumerable enumerable)
                {
                    this.enumerable = enumerable;
                    Reset();
                }
            
                public bool MoveNext() => ++curr < enumerable.end;
                public void Reset() => curr = enumerable.begin - 1;
                public TSample Current => enumerable.regressor.samples[enumerable.regressor.sampleOrder[curr]];
                object IEnumerator.Current => Current;
                public void Dispose() { }
            }

            private readonly GradientDescentRegressor<TIn, TOut> regressor;
            private readonly int begin, end;

            public SampleEnumerable(GradientDescentRegressor<TIn, TOut> regressor, int begin, int end)
            {
                this.regressor = regressor;
                this.begin = begin;
                this.end = end;
            }

            public IEnumerator<TSample> GetEnumerator() => new SampleEnumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public abstract TOut Sample(TIn x);
        protected abstract float Lose(TOut a, TOut b);
        protected abstract void LoseGrad(TSample sample, float[] grad);

        public virtual void Predict() { }
        
        protected virtual void BeforeIterate(IEnumerable<TSample> partialSamples) { }
        
        protected virtual void AfterIterate() { }

        public float Lose()
        {
            float lose = 0;

            foreach (TSample sample in samples)
            {
                float l = Lose(sample.y, Sample(sample.x));
                lose += l * l;
            }

            return lose / samples.Count;
        }

        public void Iterate() => Iterate(iterativeFactor);

        private void Iterate(float iterFac)
        {
            if (sumGrad is null || sumGrad.Length != parameters.Count)
            {
                sumGrad = new float[parameters.Count];
                sampleGrad = new float[parameters.Count];
            }

            if (sampleOrder is null || sampleOrder.Length != samples.Count)
                sampleOrder = new int[samples.Count];

            totalIterationStep = samples.Count / iterationSampleCount;

            for (int i = 0; i < samples.Count; i++) sampleOrder[i] = i;
            for (int i = 0; i < samples.Count; i++)
            {
                int j = random.Next(i, samples.Count);
                (sampleOrder[i], sampleOrder[j]) = (sampleOrder[j], sampleOrder[i]);
            }

            void IteratePartial(int begin, int end)
            {
                BeforeIterate(new SampleEnumerable(this, begin, end));
                
                for (int i = 0; i < sumGrad.Length; i++) sumGrad[i] = 0;

                foreach (TSample sample in new SampleEnumerable(this, begin, end))
                {
                    LoseGrad(sample, sampleGrad);
                    for (int i = 0; i < sumGrad.Length; i++) sumGrad[i] += sampleGrad[i];
                }
                
                for (int i = 0; i < sumGrad.Length; i++)
                    parameters[i] -= iterFac * sumGrad[i] / (end - begin);

                AfterIterate();
            }

            if (totalIterationStep == 0)
            {
                currentIterationStep = 0;
                IteratePartial(0, samples.Count);
            }
            else
            {
                for (currentIterationStep = 0; currentIterationStep < totalIterationStep; currentIterationStep++)
                    IteratePartial(currentIterationStep * iterationSampleCount,
                                   (currentIterationStep + 1) * iterationSampleCount);
            }
        }

        public bool AutoCalculate()
        {
            logger.Clear();

            int attempt = 0;
            
            for (currentIterativeFactor = iterativeFactor;
                 currentIterativeFactor >= minIterativeFactor;
                 currentIterativeFactor /= 2)
            {
                attempt++;

                logger.Append($"Attempt {attempt}: Iterative Factor = {currentIterativeFactor}\n");
                
                Predict();
                currentLose = Lose();

                // logger.Append($"\tPrediction: Lose = {currentLose}\n");

                for (currentIterationCount = 0;; currentIterationCount++)
                {
                    Iterate(currentIterativeFactor);
                    float prevLose = currentLose;
                    currentLose = Lose();

                    // logger.Append($"\tIteration {currentIterationCount}: Lose = {currentLose}\n");

                    if (float.IsNaN(currentLose) || float.IsInfinity(currentLose))
                    {
                        logger.Append($"\tFailed With Infinity Lose In {currentIterationCount} Iterations.\n");
                        break;
                    }
                    deltaLose = prevLose - currentLose;
                    if (deltaLose < 0)
                    {
                        logger.Append($"\tSucceeded With Negative Delta Lose In {currentIterationCount} Iterations.\n");
                        return true;
                    }
                    if (deltaLose < terminateDeltaLose)
                    {
                        logger.Append($"\tSucceeded With Low Enough Delta Lose In {currentIterationCount} Iterations.\n");
                        return true;
                    }
                    if (currentIterationCount >= maxIterationCount)
                    {
                        logger.Append($"\tFailed With Too Many Iterations In {currentIterationCount} Iterations.\n");
                        return false;
                    }
                }
            }

            logger.Append("Failed With Too Many Attempts.\n");
            Predict();
            return false;
        }

        public GradientDescentRegressorMonitor GetMonitor()
        {
            return new GenericsGradientDescentRegressorMonitor(this);
        }

        public string GetLogs()
        {
            return logger.ToString();
        }
    }
}