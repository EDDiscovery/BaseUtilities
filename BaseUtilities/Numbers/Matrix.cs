﻿/*
 * Copyright © 2020 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 * 
 * EDDiscovery is not affiliated with Frontier Developments plc.
 */

namespace BaseUtils
{
    /// <summary>
    /// http://www.cs.cornell.edu/courses/cs4620/2010fa/lectures/03transforms3d.pdf
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class Matrix<T>
        where T : struct, System.IConvertible, System.IComparable<T>
    {
        private readonly int rows;
        private readonly int columns;
        private static readonly System.Func<T, T> Negate;
        private static readonly System.Func<T, T, T> Multiply;
        private static readonly System.Func<T, T> Invert;
        private static readonly System.Func<T, T, T> Add;

        protected T[,] matrix;

        static Matrix()
        {
            var unaryparam = System.Linq.Expressions.Expression.Parameter(typeof(T), "v");
            var binaryparam1 = System.Linq.Expressions.Expression.Parameter(typeof(T), "v1");
            var binaryparam2 = System.Linq.Expressions.Expression.Parameter(typeof(T), "v2");

            Negate =
                System.Linq.Expressions.Expression.Lambda<System.Func<T, T>>(
                    System.Linq.Expressions.Expression.Negate(unaryparam),
                    unaryparam
                ).Compile();
            Invert =
                System.Linq.Expressions.Expression.Lambda<System.Func<T, T>>(
                    System.Linq.Expressions.Expression.Not(unaryparam),
                    unaryparam
                ).Compile();
            Multiply =
                System.Linq.Expressions.Expression.Lambda<System.Func<T, T, T>>(
                    System.Linq.Expressions.Expression.Multiply(binaryparam1, binaryparam2),
                    binaryparam1,
                    binaryparam2
                ).Compile();
            Add =
                System.Linq.Expressions.Expression.Lambda<System.Func<T, T, T>>(
                    System.Linq.Expressions.Expression.Add(binaryparam1, binaryparam2),
                    binaryparam1,
                    binaryparam2
                ).Compile();
        }

        public Matrix(int n, int m)
        {
            matrix = new T[n, m];
            rows = n;
            columns = m;
        }

        public void SetValByIdx(int m, int n, T x)
        {
            matrix[n, m] = x;
        }

        public T GetValByIndex(int n, int m)
        {
            return matrix[n, m];
        }

        public void SetMatrix(T[] arr)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    matrix[r, c] = arr[(r * columns) + c];
                }
            }
        }

        public static Matrix<T> operator |(Matrix<T> m1, Matrix<T> m2)
        {
            var m = new Matrix<T>(m1.rows, m1.columns + m2.columns);
            for (int r = 0; r < m1.rows; r++)
            {
                for (int c = 0; c < m1.columns; c++)
                    m.matrix[r, c] = m1.matrix[r, c];
                for (int c = 0; c < m2.columns; c++)
                    m.matrix[r, c + m1.columns] = m2.matrix[r, c];
            }

            return m;
        }

        public static Matrix<T> operator *(Matrix<T> m1, Matrix<T> m2)
        {
            var m = new Matrix<T>(m1.rows, m2.columns);
            for (int r = 0; r < m.rows; r++)
            {
                for (int c = 0; c < m.columns; c++)
                {
                    T tmp = (T)System.Convert.ChangeType(0, typeof(T));
                    for (int i = 0; i < m2.rows; i++)
                    {
                        tmp = Add(tmp, Multiply(m1.matrix[r, i], m2.matrix[i, c]));
                    }

                    m.matrix[r, c] = tmp;
                }
            }

            return m;
        }

        public static Matrix<T> operator ~(Matrix<T> m)
        {
            var tmp = new Matrix<T>(m.columns, m.rows);
            for (int r = 0; r < m.rows; r++)
            {
                for (int c = 0; c < m.columns; c++)
                {
                    tmp.matrix[c, r] = m.matrix[r, c];
                }
            }

            return tmp;
        }

        public static Matrix<T> operator -(Matrix<T> m)
        {
            var tmp = new Matrix<T>(m.columns, m.rows);
            for (int r = 0; r < m.rows; r++)
            {
                for (int c = 0; c < m.columns; c++)
                {
                    tmp.matrix[r, c] = Negate(m.matrix[r, c]);
                }
            }

            return tmp;
        }
    }
}