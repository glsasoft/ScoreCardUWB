using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Maths
{
    public static class CircleFit 
    {
    // 使用线性最小二乘法拟合圆，返回圆心和半径的估算值。
    // points:输入的二维点列表
    // center：圆心坐标
    // radius：半径
        public static void EstimateCircleLinear(List<Vector2> points, out Vector2 center, out float radius)
        {
            int n = points.Count;
            if (n < 3)
            {
                throw new ArgumentException("至少需要3个点才能拟合圆。");
            }

            // 构建矩阵A和向量b
            double sumX = 0, sumY = 0;
            double[,] A = new double[n, 3];
            double[] B = new double[n];

            for (int i = 0; i < n; i++)
            {
                double x = points[i].x;
                double y = points[i].y;
                A[i, 0] = x;
                A[i, 1] = y;
                A[i, 2] = 1.0;
                B[i] = -(x * x + y * y);
            }

            // 解 Ax = B 的最小二乘解
            double[] result = SolveLeastSquares(A, B);

            double D = result[0], E = result[1], F = result[2];
            double a = -D / 2.0;
            double b = -E / 2.0;
            double r = Math.Sqrt(a * a + b * b - F);

            center = new Vector2((float)a, (float)b);
            radius = (float)r;
        }


        // 解最小二乘问题 Ax = B（使用正规方程 AᵀA x = AᵀB）
        private static double[] SolveLeastSquares(double[,] A, double[] B)
        {
            int rows = A.GetLength(0);
            int cols = A.GetLength(1);
            double[,] At = new double[cols, rows];

            // 转置 A
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    At[j, i] = A[i, j];

            // 计算 At * A 和 At * B
            double[,] AtA = new double[cols, cols];
            double[] AtB = new double[cols];

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    AtA[i, j] = 0;
                    for (int k = 0; k < rows; k++)
                        AtA[i, j] += At[i, k] * A[k, j];
                }

                AtB[i] = 0;
                for (int k = 0; k < rows; k++)
                    AtB[i] += At[i, k] * B[k];
            }

            return SolveLinearSystem(AtA, AtB);
        }
        
        // 解线性方程组 Ax = B（高斯消元法）
        private static double[] SolveLinearSystem(double[,] A, double[] B)
        {
            int n = B.Length;
            double[,] mat = new double[n, n + 1];

            // 构造增广矩阵
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    mat[i, j] = A[i, j];
                mat[i, n] = B[i];
            }

            // 高斯消元
            for (int i = 0; i < n; i++)
            {
                // 选主元
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                    if (Math.Abs(mat[k, i]) > Math.Abs(mat[maxRow, i]))
                        maxRow = k;

                // 交换行
                for (int k = i; k < n + 1; k++)
                {
                    double tmp = mat[maxRow, k];
                    mat[maxRow, k] = mat[i, k];
                    mat[i, k] = tmp;
                }

                // 消元
                for (int k = i + 1; k < n; k++)
                {
                    double factor = mat[k, i] / mat[i, i];
                    for (int j = i; j < n + 1; j++)
                        mat[k, j] -= factor * mat[i, j];
                }
            }

            // 回代
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = mat[i, n];
                for (int j = i + 1; j < n; j++)
                    x[i] -= mat[i, j] * x[j];
                x[i] /= mat[i, i];
            }

            return x;
        }
    }
}
