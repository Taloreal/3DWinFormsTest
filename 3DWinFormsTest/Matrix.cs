using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace _3DWinFormsTest {

    public class Matrix {

        public const double FoV_Degrees = 45.0;
        public const double ToRads = Math.PI / 180.0;
        public const double FoV_Radians = FoV_Degrees * ToRads;

        public static Matrix View = new Matrix(4, false);
        public static Matrix Projection = new Matrix(4, false);

        private double[,] Data;
        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;

        static Matrix() { 
            Point bounds = new Point(4, 4);
            double drawRatio = ((Form1.Canvas.Width * 1.0) / (Form1.Canvas.Height * 1.0)) *
                ((Form1.Canvas.Width * 1.0) / (Form1.Canvas.Height * 1.0));

            View.ZeroIt();
            View[0.Get2D(bounds)] = 1;
            View[5.Get2D(bounds)] = Math.Cos((0 - 18) * ToRads);
            View[6.Get2D(bounds)] = 0 - Math.Sin((0 - 18) * ToRads);
            View[9.Get2D(bounds)] = Math.Sin((0 - 18) * ToRads);
            View[10.Get2D(bounds)] = Math.Cos((0 - 18) * ToRads);
            View[15.Get2D(bounds)] = 1;
            Matrix trans = new Matrix(4, true);
            trans[14.Get2D(bounds)] = 1;
            View = trans * View;

            double aspect = (Form1.Canvas.Width / drawRatio) / Form1.Canvas.Height;
            Projection.SetIdentity();
            double yscale = 1.0 / Math.Tan(FoV_Radians);
            double xscale = yscale * aspect;

            Projection[0.Get2D(bounds)] = xscale;
            Projection[5.Get2D(bounds)] = yscale;
            Projection[10.Get2D(bounds)] = Program.FarZ / (Program.FarZ - Program.NearZ);
            Projection[11.Get2D(bounds)] = 1.0;
            Projection[14.Get2D(bounds)] = 0.0 - ((Program.FarZ * Program.NearZ) / (Program.FarZ - Program.NearZ));
            Projection[15.Get2D(bounds)] = 0.0;
        }

        public Matrix(int sqrSize, bool setIdentity = false) {
            Width = sqrSize;
            Height = sqrSize;
            Data = new double[Width, Height];
            if (setIdentity) {
                SetIdentity();
            }
        }

        public Matrix(int width, int height, bool setIdentity = false) { 
            Width = width;
            Height = height;
            Data = new double[Width, Height];
            if (setIdentity) {
                SetIdentity();
            }
        }

        public double this[Point pos] { 
            get => Data[pos.X, pos.Y];
            set => Data[pos.X, pos.Y] = value;
        }

        public double this[int x, int y] {
            get => Data[x, y];
            set => Data[x, y] = value;
        }

        public void ZeroIt() {
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    Data[x, y] = 0;
                }
            }
        }

        public void SetIdentity() { 
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    Data[x, y] = x == y ? 1 : 0;
                }
            }
        }

        public Matrix GetFastInverse() {
            Point bounds3 = new Point(3, 3);
            Point bounds4 = new Point(4, 4);
            Matrix temp = new Matrix(3, false);
            Matrix output = new Matrix(4, false);
            int i = 0, j = 0, getter = 0, setter = 0;
            for (int a = 0; a < 9; a++) {
                i = a / 3; 
                j = a % 3;
                getter = j * 4 + i;
                setter = i * 4 + j;
                temp[a.Get2D(bounds3)] = this[getter.Get2D(bounds4)];
                output[setter.Get2D(bounds4)] = this[getter.Get2D(bounds4)];
            }
            Vector vert = temp * new Vector(this[12.Get2D(bounds4)], this[13.Get2D(bounds4)], this[14.Get2D(bounds4)]);
            output[12.Get2D(bounds4)] = 0 - vert.X;
            output[13.Get2D(bounds4)] = 0 - vert.Y;
            output[14.Get2D(bounds4)] = 0 - vert.Z;
            output[15.Get2D(bounds4)] = 1;
            return output;
        }

        public void SetRow(int row, double[] values) {
            if (row >= 0 && row < Height) {
                for (int x = 0; x < Width && x < values.Length; x++) {
                    Data[x, row] = values[x];
                }
            }
        }

        public double[] GetRow(int row) {
            double[] data = new double[Height];
            if (row >= 0 && row < Width) {
                for (int x = 0; x < Width; x++) {
                    data[x] = Data[x, row];
                }
            }
            return data;
        }

        public void SetColumn(int column, double[] values) {
            if (column >= 0 && column < Width) {
                for (int y = 0; y < Height && y < values.Length; y++) { 
                    Data[column, y] = values[y];
                }
            }
        }

        public double[] GetColumn(int column) {
            double[] data = new double[Width];
            if (column >= 0 && column < Height) {
                for (int y = 0; y < Height; y++) {
                    data[y] = Data[column, y];
                }
            }
            return data;
        }

        public static Matrix operator *(double s, Matrix m) {
            return m * s;
        }
        
        public static Matrix operator *(Matrix m, double s) { 
            Matrix result = new Matrix(m.Width, m.Height);
            for (int x = 0; x < m.Width; x++) {
                for (int y = 0; y < m.Height; y++) { 
                    result[x, y] = m[x, y] * s;
                }
            }
            return result;
        }

        public static Matrix operator *(Matrix left, Matrix right) {
            int rA = left.Data.GetLength(0);
            int cA = left.Data.GetLength(1);
            int rB = right.Data.GetLength(0);
            int cB = right.Data.GetLength(1);

            if (cA != rB) {
                throw new Exception("ERROR: Incompatible matrices.");
            }
            double temp = 0;
            Matrix result = new Matrix(rA, cB);
            for (int i = 0; i < rA; i++) {
                for (int j = 0; j < cB; j++) {
                    temp = 0;
                    for (int k = 0; k < cA; k++) {
                        temp += left[i, k] * right[k, j];
                    }
                    result[i, j] = temp;
                }
            }
            return result;
        }

        public static Vector operator *(Matrix left, Vector right) {
            Vector result = new Vector(0, 0, 0, 0);
            for (int y = 0; y < left.Height && y < 4; y++) {
                for (int x = 0; x < left.Width && x < 4; x++) {
                    result[x] += left[x, y] * right[y];
                }
            }
            return result;
        }

        public static bool operator ==(Matrix left, Matrix right) {
            if (left.Width == right.Width && left.Height == right.Height) {
                for (int y = 0; y < left.Height; y++) {
                    for (int x = 0; x < left.Width; x++) {
                        if (left[x, y] != right[x, y]) {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public static bool operator !=(Matrix left, Matrix right) { 
            return !(left == right);
        }

    }
}
