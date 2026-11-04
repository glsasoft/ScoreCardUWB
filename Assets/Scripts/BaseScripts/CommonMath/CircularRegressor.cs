using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Maths
{
    [Serializable]
    public class CircularRegressor : GradientDescentRegressor<float, float>
    {
        private const int parameterCount = 2;

        public float radius;

        public float originX
        {
            get => parameters[0];
            set => parameters[0] = value;
        }
        
        public float originY
        {
            get => parameters[1];
            set => parameters[1] = value;
        }
        
        public CircularRegressor()
        {
            parameters.Capacity = parameterCount;
            while (parameters.Count < parameterCount) parameters.Add(0);
            terminateFactor = 1e-6f;
        }
        
        public override float Sample(float x)
        {
            x -= originX;
            return originY + Mathf.Sqrt(radius * radius - x * x);
        }

        private float Dist(float x, float y)
        {
            x -= originX;
            y -= originY;
            return Mathf.Sqrt(x * x + y * y);
        }

        protected override float Lose(float a, float b)
        {
            if (samples.Count == 0) return 0;
            
            float avg = 0;
            foreach (TSample sample in samples) avg += Dist(sample.x, sample.y);
            avg /= samples.Count;

            float lose = 0;
            foreach (TSample sample in samples)
            {
                float dist = Dist(sample.x, sample.y) - avg;
                lose += dist * dist;
            }
            lose /= samples.Count;

            return lose;
        }

        private void CalcRadius()
        {
            radius = 0;
            foreach (TSample sample in samples) radius += Dist(sample.x, sample.y);
            radius /= samples.Count;
        }
        
        // public override void Predict()
        // {
        //     originX = 0;
        //     originY = 0;
        //     foreach (TSample sample in samples)
        //     {
        //         originX += sample.x;
        //         originY += sample.y;
        //     }
        //     originX /= samples.Count;
        //     originY /= samples.Count;
        //     CalcRadius();
        // }

        public override void Predict()
        {
            List<Vector2> points = new(samples.Count);
            foreach (TSample sample in samples) points.Add(new Vector2(sample.x, sample.y));
            CircleFit.EstimateCircleLinear(points, out Vector2 center, out radius);
            originX = center.x;
            originY = center.y;
        }

        private float avgDist;
        private float avgFactorX, avgFactorY;

        protected override void BeforeIterate(IEnumerable<TSample> partialSamples)
        {
            int count = 0;
            avgDist = 0;
            avgFactorX = 0;
            avgFactorY = 0;
            foreach (TSample sample in partialSamples)
            {
                float dist = Dist(sample.x, sample.y);
                avgDist += dist;
                avgFactorX += (originX - sample.x) / dist;
                avgFactorY += (originY - sample.y) / dist;
                count++;
            }

            avgDist /= count;
            avgFactorX /= count;
            avgFactorY /= count;
        }

        protected override void LoseGrad(TSample sample, float[] grad)
        {
            float dist = Dist(sample.x, sample.y);
            grad[0] = (dist - avgDist) * ((originX - sample.x) / dist - avgFactorX);
            grad[1] = (dist - avgDist) * ((originY - sample.y) / dist - avgFactorY);
        }

        protected override void AfterIterate()
        {
            CalcRadius();
        }
    }
}