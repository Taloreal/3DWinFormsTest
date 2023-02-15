using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace _3DWinFormsTest {

    public class Vector {

        private bool HasChanged = true;
        private Point CachedScreenPoint;
        private Vector CachedScreenVector;
        private double[] Values = { 0.0, 0.0, 0.0, 0.0 };

        public double X { 
            get => Values[0]; 
            set { 
                Values[0] = value; 
                HasChanged = true;
            }
        }

        public double Y { 
            get => Values[1];
            set { 
                Values[1] = value; 
                HasChanged = true;
            }
        }

        public double Z { 
            get => Values[2];
            set {
                Values[2] = value;
                HasChanged = true;
            }
        }

        public double W { 
            get => Values[3];
            set {
                Values[3] = value;
                HasChanged = true;
            }
        }

        public Vector(Vector toCopy) { 
            X = toCopy.X;
            Y = toCopy.Y;
            Z = toCopy.Z;
            W = toCopy.W;
        }

        public Vector(double x = 0.0, double y = 0.0, double z = 0.0, double w = 0.0) {
            X = x; 
            Y = y; 
            Z = z; 
            W = w;
        }

        public double this[int ndx] { 
            get => Values[ndx];
            set => Values[ndx] = value;
        }

        public double GetLength() { 
            return Math.Sqrt((X*X) + (Y*Y) + (Z*Z));
        }

        public Vector GetNormalized() {
            return this / GetLength();
        }

        public static double operator *(Vector left, Vector right) { 
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);
        }

        public static Vector operator *(double s, Vector v) {
            return v * s;
        }

        public static Vector operator *(Vector vector, double scalar) { 
            return new Vector(vector.X * scalar, vector.Y * scalar, vector.Z * scalar, vector.W);
        }

        public static Vector operator /(Vector vector, double scalar) {
            return new Vector(vector.X / scalar, vector.Y / scalar, vector.Z / scalar, vector.W);
        }

        public static Vector operator +(Vector left, Vector right) { 
            return new Vector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector operator -(Vector left, Vector right) {
            Vector neg = right * -1;
            return left + neg;
        }

        public override string ToString() {
            return "Vector { x = " + X + ", y = " + Y + ", z = " + Z + ", w = " + W + " }";
        }

        public override bool Equals(object obj) {
            return obj is Vector && (obj as Vector) == this;
        }

        public static bool operator ==(Vector left, Vector right) { 
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;
        }

        public static bool operator !=(Vector left, Vector right) { 
            return !(left == right);
        }

        public Point ToScreenSpace() {
            if (HasChanged == true) {
                Vector coords = this.ToScreenVector();
                CachedScreenPoint = new Point((int)coords.X, (int)coords.Y);
                this.HasChanged = false;
            }
            return CachedScreenPoint;
        }

        public Vector ToScreenVector() {
            if (HasChanged == true) {
                Vector vector = new Vector(this);
                Point boundary = new Point(4, 4);
                Matrix camera = Matrix.View.GetFastInverse();
                vector = camera * vector;
                vector += new Vector(Matrix.Projection[3.Get2D(boundary)],
                    Matrix.Projection[7.Get2D(boundary)], Matrix.Projection[11.Get2D(boundary)]);
                vector = Matrix.Projection * vector;
                vector /= vector.W;
                vector.X = (vector.X + 1) / 2 * Form1.Canvas.Surface.Width;
                vector.Y = (1 - vector.Y) / 2 * Form1.Canvas.Surface.Height;
                CachedScreenVector = vector;
                this.HasChanged = false;
            }
            return CachedScreenVector;
        }

    }
}
