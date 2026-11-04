using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Maths
{
    [Serializable]
    public class PolynomialRegressor : GradientDescentRegressor<float, float>
    {
        public int number
        {
            get => parameters.Count;
            set
            {
                while (parameters.Count < value) parameters.Add(0);
                while (parameters.Count > value) parameters.RemoveAt(parameters.Count - 1);
            }
        }

        public override float Sample(float x)
        {
            float xi = 1, y = 0;
            for (int i = 0; i < number; i++)
            {
                y += parameters[i] * xi;
                xi *= x;
            }

            return y;
        }

        protected override float Lose(float a, float b)
        {
            float delta = a - b;
            return delta * delta;
        }

        protected override void LoseGrad(TSample sample, float[] grad)
        {
            float xi = 1;
            for (int i = 0; i < grad.Length; i++)
            {
                grad[i] = xi * (Sample(sample.x) - sample.y);
                xi *= sample.x;
            }
        }

        public override void Predict()
        {
            for (int i = 0; i < number; i++) parameters[i] = 0;

            if (samples.Count == 0) return;

            float min = samples[0].x, max = samples[0].x;

            for (int i = 0; i < samples.Count; i++)
            {
                if (min > samples[i].x) min = samples[i].x;
                if (max < samples[i].x) max = samples[i].x;
            }

            if (Mathf.Approximately(min, max))
            {
                parameters[0] = samples[0].y;
                return;
            }

            float[] sum = new float[number];
            int[] count = new int[number];

            int GetGroup(float x)
            {
                for (int g = 1; g < number; g++)
                    if (x < Mathf.Lerp(min, max, (float)g / number))
                        return g - 1;
                return number - 1;
            }

            foreach (TSample sample in samples)
            {
                int group = GetGroup(sample.x);
                sum[group] += sample.y;
                count[group]++;
            }

            List<Vector2> featuredSamples = new(number);

            for (int i = 0; i < number; i++)
            {
                if (count[i] == 0) continue;
                featuredSamples.Add(new Vector2(Mathf.Lerp(min, max, (i + 0.5f) / number), sum[i] / count[i]));
            }

            void CalculateParameter(int ignore, float mult, int exp, int index = 0)
            {
                parameters[exp] += mult;
                for (int i = index; i < featuredSamples.Count; i++)
                {
                    if (i == ignore) continue;
                    CalculateParameter(ignore, -mult * featuredSamples[i].x, exp - 1, i + 1);
                }
            }

            for (int i = 0; i < featuredSamples.Count; i++)
            {
                float mult = featuredSamples[i].y;

                for (int j = 0; j < featuredSamples.Count; j++)
                {
                    if (i == j) continue;
                    mult /= featuredSamples[i].x - featuredSamples[j].x;
                }

                CalculateParameter(i, mult, featuredSamples.Count - 1);
            }
        }
    }
}