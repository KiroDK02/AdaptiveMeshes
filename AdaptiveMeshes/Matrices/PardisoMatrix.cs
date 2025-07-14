using AdaptiveMeshes.Interfaces;
using Quasar.Native;

namespace AdaptiveMeshes.Matrices
{
    public class PardisoMatrix : IMatrix, IPardisoMatrix<double>
    {
        public PardisoMatrix(SortedSet<int>[] profile, PardisoMatrixType _type)
        {
            type = _type;

            SetProfile(profile);
        }

        public int N => ia.Length - 1;

        int IPardisoMatrix<double>.n => N;

        PardisoMatrixType type;
        PardisoMatrixType IPardisoMatrix<double>.MatrixType => type;

        double[] a = [];
        ReadOnlySpan<double> IPardisoMatrix<double>.a => a;

        int[] ia = [];
        ReadOnlySpan<int> IPardisoMatrix<double>.ia => ia;

        int[] ja = [];
        ReadOnlySpan<int> IPardisoMatrix<double>.ja => ja;

        public void AddLocal(int[] dofs, double[,] matrix, double coeff = 1.0)
        {
            for (int i = 0; i < dofs.Length; i++)
            {
                for (int j = i; j < dofs.Length; j++)
                {
                    int di = dofs[i];
                    int dj = dofs[j];

                    if (di <= dj)
                    {
                        int i0 = ia[di];
                        int i1 = ia[di + 1];
                        int targetIndex = BinarySearch(ja, dj, i0, i1 - 1);

                        if (targetIndex != -1)
                            a.ThreadSafeAdd(targetIndex, coeff * matrix[i, j]);
                    }
                    else
                    {
                        int i0 = ia[dj];
                        int i1 = ia[dj + 1];
                        int targetIndex = BinarySearch(ja, di, i0, i1 - 1);

                        if (targetIndex != -1)
                            a.ThreadSafeAdd(targetIndex, coeff * matrix[i, j]);
                    }
                }
            }
        }

        public void Clear()
        {
            Array.Clear(a);
        }

        public void MultVect(double[] to, double[] result)
        {
            Array.Fill(result, 0.0);

            for (int i = 0; i < N; i++)
            {
                int k1 = ia[i];
                int k2 = ia[i + 1];

                result[i] += AlgorithmsLA.SparseMult(a.AsSpan(k1, k2 - k1), ja.AsSpan(k1, k2 - k1), to);
            }

            for (int i = 0; i < N; i++)
            {
                int k1 = ia[i] + 1;
                int k2 = ia[i + 1];

                AlgorithmsLA.SparseAdd(result, ja.AsSpan(k1, k2 - k1), a.AsSpan(k1, k2 - k1), to[i]);
            }
        }

        public void SetProfile(SortedSet<int>[] profile)
        {
            BuildUpperTriangleRowSparseMatrixPortrait(profile);

            a = new double[ia[^1]];
        }

        public void Symmetrize(int dof, double value, double[] RightPart)
        {
            int ia0 = 0;
            int ia1 = 0;

            for (int i = 0; i < dof; i++)
            {
                ia0 = ia[i];
                ia1 = ia[i + 1];

                int targetIndex = BinarySearch(ja, dof, ia0, ia1 - 1);

                if (targetIndex != -1)
                {
                    RightPart.ThreadSafeAdd(i, -a[targetIndex] * value);
                    a.ThreadSafeSet(targetIndex, 0);
                }
            }

            ia0 = ia[dof];
            ia1 = ia[dof + 1];

            a.ThreadSafeSet(ia0, 1.0);

            for (int ind = ia0 + 1; ind < ia1; ind++)
            {
                int j = ja[ind];

                RightPart.ThreadSafeAdd(j, -a[ind] * value);
                a.ThreadSafeSet(ind, 0);
            }
        }

        private void BuildUpperTriangleRowSparseMatrixPortrait(SortedSet<int>[] profile)
        {
            ia = new int[profile.Length + 1];

            for (int i = 0; i < profile.Length; i++)
                ia[i + 1] = ia[i] + profile[i].Where(j => j >= i).Count();

            ja = new int[ia[^1]];

            for (int i = 0; i < profile.Length; i++)
                profile[i].Where(j => j >= i).ToArray().AsSpan().CopyTo(ja.AsSpan(ia[i]));
        }

        private static int BinarySearch(int[] array, int target, int low, int high)
        {
            while (low <= high)
            {
                int mid = (low + high) / 2;
                int midValue = array[mid];

                if (midValue == target)
                    return mid;
                else if (midValue < target)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return -1;
        }
    }
}
