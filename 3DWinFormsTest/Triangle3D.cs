using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public class Triangle3D {

        public uint DefaultClr;
        public int[] Verticies { get; private set; } = new int[3];
        private VectorGetter GetVector;
        private VectorTranslation TranslateVector;

        public Triangle3D(int v1, int v2, int v3, VectorGetter getVector, VectorTranslation translation, uint defaultClr = 0u) {
            Verticies[0] = v1;
            Verticies[1] = v2;
            Verticies[2] = v3;
            GetVector = getVector;
            DefaultClr = defaultClr;
            TranslateVector = translation;
        }

        public void DrawOutline(ref Drawable canvas) {
            List<int> verts = new List<int>(4);
            verts.AddRange(Verticies);
            verts.Add(Verticies[0]);
            Line3D outline = new Line3D(verts, GetVector, TranslateVector, DefaultClr);
            outline.DrawLine(ref canvas);
        }

        public void DrawFilled(ref Drawable canvas, uint clr = 0) {
            if (clr == 0) { 
                clr = DefaultClr;
            }
            Point[] pivots = GetScreenPoints();
            Vector[] asVerts = GetScreenPositions();

            int xStart = pivots[0].X < pivots[1].X ? pivots[0].X : pivots[1].X;
            xStart = xStart < pivots[2].X ? xStart : pivots[2].X;
            int xStop = pivots[0].X > pivots[1].X ? pivots[0].X : pivots[1].X;
            xStop = xStop > pivots[2].X ? xStop : pivots[2].X;
            int yStart = pivots[0].Y < pivots[1].Y ? pivots[0].Y : pivots[1].Y;
            yStart = yStart < pivots[2].Y ? yStart : pivots[2].Y;
            int yStop = pivots[0].Y > pivots[1].Y ? pivots[0].Y : pivots[1].Y;
            yStop = yStop > pivots[2].Y ? yStop : pivots[2].Y;

            for (int y = yStart; y <= yStop; y++) {
                for (int x = xStart; x <= xStop; x++) {
                    Vector p = new Vector(x, y, 0, 0);
                    Vector bary = GetBary(asVerts[0], asVerts[1], asVerts[2], p);
                    if (bary.X < 0 || bary.X > 1) { continue; }
                    if (bary.Y < 0 || bary.Y > 1) { continue; }
                    if (bary.Z < 0 || bary.Z > 1) { continue; }
                    double depth = GetZInterpolation(bary);
                    canvas.DrawPixel(new Point(x, y), clr, depth);
                }
            }
        }

        private double GetZInterpolation(Vector p) {
            return (TranslateVector(GetVector(Verticies[0])).Z * p.Z) + 
                (TranslateVector(GetVector(Verticies[1])).Z * p.X) + 
                (TranslateVector(GetVector(Verticies[2])).Z * p.Y);
        }

        private Vector GetBary(Vector a, Vector b, Vector c, Vector p) {
            double beta = ImplicitLineEq(b, a, c);
            double gamma = ImplicitLineEq(c, b, a);
            double alpha = ImplicitLineEq(a, c, b);

            double B = ImplicitLineEq(p, a, c);
            double Y = ImplicitLineEq(p, b, a);
            double A = ImplicitLineEq(p, c, b);

            return new Vector(B / beta, Y / gamma, A / alpha, 0);
        }

        private double ImplicitLineEq(Vector b, Vector a, Vector p) {
            return ((a.Y - b.Y) * p.X) +
                ((b.X - a.X) * p.Y) +
                ((a.X * b.Y) - (a.Y * b.X));
        }

        private Point[] GetScreenPoints() {
            Point[] pivs = new Point[3];
            pivs[0] = Vector.ToScreenSpace(TranslateVector(GetVector(Verticies[0])));
            pivs[1] = Vector.ToScreenSpace(TranslateVector(GetVector(Verticies[1])));
            pivs[2] = Vector.ToScreenSpace(TranslateVector(GetVector(Verticies[2])));
            return pivs;
        }

        private Vector[] GetScreenPositions() {
            Vector[] verts = new Vector[3];
            verts[0] = Vector.ToScreenVector(TranslateVector(GetVector(Verticies[0])));
            verts[1] = Vector.ToScreenVector(TranslateVector(GetVector(Verticies[1])));
            verts[2] = Vector.ToScreenVector(TranslateVector(GetVector(Verticies[2])));
            return verts;
        }

        public static bool operator ==(Triangle3D left, Triangle3D right) {
            int same = 0;
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (left.GetVector(left.Verticies[i]) == right.GetVector(right.Verticies[j])) {
                        same += 1;
                        break;
                    }
                }
            }
            return same == 3;
        }

        public static bool operator !=(Triangle3D left, Triangle3D right) { 
            return !(left == right);
        }
    }
}
