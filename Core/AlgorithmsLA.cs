using System;
using Core.Vectors;

namespace Core;

public static class AlgorithmsLA
{
    public static double[] MultiplyMatrixVector(double[,] matrix, double[] vector, double coeff = 1d)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        if (cols != vector.Length)
        {
            throw new ArgumentException("Количество столбцов матрицы должно совпадать с длиной вектора.");
        }

        double[] result = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            double sum = 0;
            for (int j = 0; j < cols; j++)
            {
                sum += matrix[i, j] * vector[j];
            }

            result[i] = coeff * sum;
        }

        return result;
    }

    public static double[] MultiplyMatrixVector(double[,] matrix, ReadOnlySpan<double> vector, double coeff = 1d)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        if (cols != vector.Length)
        {
            throw new ArgumentException("Количество столбцов матрицы должно совпадать с длиной вектора.");
        }

        var result = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            var sum = 0.0;
            for (int j = 0; j < cols; j++)
            {
                sum += matrix[i, j] * vector[j];
            }

            result[i] = coeff * sum;
        }


        return result;
    }

    public static double SparseMult(ReadOnlySpan<double> gg, ReadOnlySpan<int> jg, ReadOnlySpan<double> vec)
    {
        var sum = 0.0;
        for (int i = 0; i < jg.Length; i++)
            sum += vec[jg[i]] * gg[i];
        return sum;
    }

    public static void SparseAdd(Span<double> w, ReadOnlySpan<int> jg, ReadOnlySpan<double> gg, double val)
    {
        for (int i = 0; i < jg.Length; i++)
            w[jg[i]] += gg[i] * val;
    }

    public static Vector2D SearchCenterPolygon(Vector2D[] points)
    {
        var result = Vector2D.Zero;

        foreach (var point in points)
            result += point;

        return result / points.Length;
    }
}