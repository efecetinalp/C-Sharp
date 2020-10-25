using Algorithms.Numeric.Decomposition;
using NUnit.Framework;
using System;
using M = Utilities.Extensions.MatrixExtensions;
using V = Utilities.Extensions.VectorExtensions;

namespace Algorithms.Tests.Numeric.Decomposition
{
    public class SvdTests
    {
        public void AssertMatrixEqual(double[,] matrix1, double[,] matrix2, double epsilon)
        {
            Assert.AreEqual(matrix1.GetLength(0), matrix2.GetLength(0));
            Assert.AreEqual(matrix1.GetLength(1), matrix2.GetLength(1));
            for (int i = 0; i < matrix1.GetLength(0); i++)
            {
                for (int j = 0; j < matrix1.GetLength(1); j++)
                {
                    Assert.AreEqual(matrix1[i, j], matrix2[i, j], epsilon, $"At index ({i}, {j})");
                }
            }
        }

        public double[,] GenerateRandomMatrix(int m, int n)
        {
            double[,] result = new double[m, n];
            Random random = new Random();
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = random.NextDouble() - 0.5;
                }
            }
            return result;
        }

        [Test]
        public void RandomUnitVector()
        {
            double epsilon = 0.0001;
            // unit vector should have length 1
            Assert.AreEqual(1, V.Magnitude(ThinSvd.RandomUnitVector(10)), epsilon);
            // unit vector with single element should be [-1] or [+1]
            Assert.AreEqual(1, Math.Abs(ThinSvd.RandomUnitVector(1)[0]), epsilon);
            // two randomly generated unit vectors should not be equal 
            Assert.AreNotEqual(ThinSvd.RandomUnitVector(10), ThinSvd.RandomUnitVector(10));
        }

        [Test]
        public void Svd_Decompose()
        {
            CheckSvd(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            CheckSvd(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
            CheckSvd(new double[,] { { 1, 0, 0, 0, 2 }, { 0, 3, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 2, 0, 0, 0 } });
        }

        [Test]
        public void Svd_Random([Random(3, 10, 5)] int m, [Random(3, 10, 5)] int n)
        {
            double[,] matrix = GenerateRandomMatrix(m, n);
            CheckSvd(matrix);
        }

        public void CheckSvd(double[,] testMatrix)
        {
            double epsilon = 1E-5;
            double[,] u;
            double[,] v;
            double[] s;
            (u, s, v) = ThinSvd.Decompose(testMatrix, 1E-8, 1000);

            for (int i = 1; i < s.Length; i++)
            {
                // singular values should be arranged from greatest to smallest
                Assert.GreaterOrEqual(s[i - 1], s[i]);
            }

            for (int i = 0; i < u.GetLength(1); i++)
            {
                double[] extracted = new double[u.GetLength(0)];
                // extract a column of u
                for (int j = 0; j < extracted.Length; j++)
                {
                    extracted[j] = u[j, i];
                }

                if (s[i] > epsilon)
                {
                    // if the singular value is non-zero, then the basis vector in u should be a unit vector
                    Assert.AreEqual(1, V.Magnitude(extracted), epsilon);
                }
                else
                {
                    // if the singular value is zero, then the basis vector in u should be zeroed out
                    Assert.AreEqual(0, V.Magnitude(extracted), epsilon);
                }
            }

            for (int i = 0; i < v.GetLength(1); i++)
            {
                double[] extracted = new double[v.GetLength(0)];
                // extract column of v
                for (int j = 0; j < extracted.Length; j++)
                {
                    extracted[j] = v[j, i];
                }

                if (s[i] > epsilon)
                {
                    // if the singular value is non-zero, then the basis vector in v should be a unit vector
                    Assert.AreEqual(1, V.Magnitude(extracted), epsilon);
                }
                else
                {
                    // if the singular value is zero, then the basis vector in v should be zeroed out
                    Assert.AreEqual(0, V.Magnitude(extracted), epsilon);
                }
            }

            // convert singular values to a diagonal matrix
            double[,] expanded = new double[s.Length, s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                expanded[i, i] = s[i];
            }


            // matrix = U * S * V^t, definition of Singular Vector Decomposition
            AssertMatrixEqual(testMatrix,
                M.MultiplyGeneral(M.MultiplyGeneral(u, expanded), M.Transpose(v)), epsilon);
            AssertMatrixEqual(testMatrix,
                M.MultiplyGeneral(u, M.MultiplyGeneral(expanded, M.Transpose(v))), epsilon);
        }
    }
}