using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public struct DepthPoint {

        public double Depth;
        public Point Position;

        public DepthPoint(Point pos, double depth = 0) {
            Position = pos;
            Depth = depth;
        }
    }

    public class Line3D {

        public uint DefaultClr;
        public List<int> Verticies = new List<int>();
        public VectorGetter GetVector;
        public VectorTranslation TranslateVector;

        public Line3D(List<int> verts, VectorGetter getvert, VectorTranslation translation, uint defaultclr) { 
            if (getvert == null) {
                throw new NullReferenceException("ERROR: getvert must have a value.");
            }
            verts.ForEach(i => Verticies.Add(i));
            TranslateVector = translation;
            DefaultClr = defaultclr;
            GetVector = getvert;
        }

        public void DrawLine(ref Drawable canvas, uint clr = 0u) {
            if (clr == 0u) {
                clr = DefaultClr;
            }
            Vector start, stop;
            Point bounds = new Point(4, 4);
            for (int i = 0; i < Verticies.Count() - 1; i++) {
                start = TranslateVector(GetVector(Verticies[i]));
                stop = TranslateVector(GetVector(Verticies[i + 1]));
                DrawLine(ref canvas, 
                    new DepthPoint(Vector.ToScreenSpace(start), start.Z), 
                    new DepthPoint(Vector.ToScreenSpace(stop), stop.Z),
                    clr);
            }
        }

        public void DrawLine(ref Drawable canvas, DepthPoint start, DepthPoint end, uint clr = 0) {
            int deltaX = start.Position.X - end.Position.X;
            int deltaY = start.Position.Y - end.Position.Y;
            deltaX = deltaX < 0 ? deltaX * -1 : deltaX;
            deltaY = deltaY < 0 ? deltaY * -1 : deltaY;
            int total = deltaX;
            total = deltaY > total ? deltaY : total;
            double ratio = 0;
            double mindepth = start.Depth < end.Depth ? start.Depth : end.Depth;
            double maxdepth = start.Depth > end.Depth ? start.Depth : end.Depth;
            for (int i = 0; i < total; i++) {
                ratio = i / (double)total;
                Point position = new Point(
                    (int)((end.Position.X - start.Position.X) * ratio + start.Position.X + 0.5f),
                    (int)((end.Position.Y - start.Position.Y) * ratio + start.Position.Y + 0.5f));
                double depth = ((maxdepth - mindepth) * ratio) + mindepth;
                canvas.DrawPixel(position, clr, depth);
            }
        }

    }
}
