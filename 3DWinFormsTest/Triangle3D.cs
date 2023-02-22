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
            Point[] pts = GetScreenPoints();
            //Vector[] normal = { 
            //    TranslateVector(GetVector(Verticies[0])),
            //    TranslateVector(GetVector(Verticies[1])),
            //    TranslateVector(GetVector(Verticies[2])), };
            Vector[] asVerts = GetScreenPositions();

            int xStart = pts[0].X < pts[1].X ? pts[0].X : pts[1].X;
            xStart = xStart < pts[2].X ? xStart : pts[2].X;
            xStart = xStart.Clamp(0, Form1.Canvas.Width - 1);
            int xStop = pts[0].X > pts[1].X ? pts[0].X : pts[1].X;
            xStop = xStop > pts[2].X ? xStop : pts[2].X;
            xStop = xStop.Clamp(0, Form1.Canvas.Width - 1);
            int yStart = pts[0].Y < pts[1].Y ? pts[0].Y : pts[1].Y;
            yStart = yStart < pts[2].Y ? yStart : pts[2].Y;
            yStart = yStart.Clamp(0, Form1.Canvas.Height - 1);
            int yStop = pts[0].Y > pts[1].Y ? pts[0].Y : pts[1].Y;
            yStop = yStop > pts[2].Y ? yStop : pts[2].Y;
            yStop = yStop.Clamp(0, Form1.Canvas.Height - 1);

            Vector[] vectors = {
                TranslateVector(GetVector(Verticies[0])),
                TranslateVector(GetVector(Verticies[1])),
                TranslateVector(GetVector(Verticies[2])),
            };

            for (int y = yStart; y <= yStop; y++) {
                for (int x = xStart; x <= xStop; x++) {
                    Point pos = new Point(x, y);
                    //if (QuickZCheck(asVerts, pos) == false) { continue; }
                    //if (QuickZCheck(vectors, pos) == false) { continue; }
                    Vector p = new Vector(x, y, 0, 0);
                    Vector bary = GetBary(asVerts[0], asVerts[1], asVerts[2], p);
                    if (bary.X < 0 || bary.X > 1) { continue; }
                    if (bary.Y < 0 || bary.Y > 1) { continue; }
                    if (bary.Z < 0 || bary.Z > 1) { continue; }
                    ///double depth = GetZInterpolation(asVerts, bary);
                    double depth = GetZInterpolation(vectors, bary);
                    canvas.DrawPixel(new Point(x, y), clr, depth);
                }
            }
        }

        private bool QuickZCheck(Vector[] vectors, Point pos) { 
            double depth = Form1.Canvas.Depth[pos.X, pos.Y];
            return Math.Abs(vectors[0].Z) < depth || Math.Abs(vectors[1].Z) < depth || Math.Abs(vectors[2].Z) < depth;
        }

        private double GetZInterpolation(Vector[] vectors, Vector v) {
            return (vectors[0].Z * v.Z) + (vectors[1].Z * v.X) + (vectors[2].Z * v.Y);
        }

        //private double GetZInterpolation(Vector[] vectors, Vector v) {
        //    return ((1.0 / vectors[0].Z) * v.Z) + ((1.0 / vectors[1].Z) * v.X) + ((1.0 / vectors[2].Z) * v.Y);
        //}


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
            pivs[0] = TranslateVector(GetVector(Verticies[0])).ToScreenSpace();
            pivs[1] = TranslateVector(GetVector(Verticies[1])).ToScreenSpace();
            pivs[2] = TranslateVector(GetVector(Verticies[2])).ToScreenSpace();
            return pivs;
        }

        private Vector[] GetScreenPositions() {
            Vector[] verts = new Vector[3];
            verts[0] = TranslateVector(GetVector(Verticies[0])).ToScreenVector();
            verts[1] = TranslateVector(GetVector(Verticies[1])).ToScreenVector();
            verts[2] = TranslateVector(GetVector(Verticies[2])).ToScreenVector();
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
