using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public static class Program {

        public static double FarZ = 10;
        public static double NearZ = 0.1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {


            //Matrix m1 = new Matrix(3);
            //m1.SetRow(0, new double[] { 2, 4, 1 });
            //m1.SetRow(1, new double[] { 2, 3, 9 });
            //m1.SetRow(2, new double[] { 3, 1, 8 });

            //Matrix m2 = new Matrix(3);
            //m2.SetRow(0, new double[] { 1, 2, 3 });
            //m2.SetRow(1, new double[] { 3, 6, 1 });
            //m2.SetRow(2, new double[] { 2, 4, 7 });
            //
            //Vector vec = new Vector(3, 7, 5);
            //Vector result = m1 * vec;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static int Get1D(this Point pos, Point bounds) {
            return (pos.Y >= bounds.Y || pos.X >= bounds.X || pos.Y < 0 || pos.X < 0) ? 
                -1 : pos.Y * bounds.X + pos.X;
        }

        public static int Get1D(this Point pos, Size bounds) {
            return pos.Get1D(new Point(bounds.Width, bounds.Height));
        }

        public static Point Get2D(this int ndx, Point bounds) {
            int limit = bounds.X * bounds.Y;
            return new Point(
                (ndx < 0 || ndx >= limit) ? -1 : ndx % bounds.X,
                (ndx < 0 || ndx >= limit) ? -1 : ndx / bounds.X);
        }

        public static Point Get2D(this int ndx, Size bounds) {
            return ndx.Get2D(new Point(bounds.Width, bounds.Height));
        }

        public static Point Add(this Point og, Point offset) {
            return new Point(og.X + offset.X, og.Y + offset.Y);
        }

        public static Point Minus(this Point og, Point offset) {
            return new Point(og.X - offset.X, og.Y - offset.Y);
        }

        public static Point Multiply(this Point og, double scalar) {
            return new Point((int)(og.X * scalar), (int)(og.Y * scalar));
        }

        public static Point Divide(this Point og, double scalar) {
            return new Point((int)(og.X / scalar), (int)(og.Y / scalar));
        }

        public static Point GetSize<T>(this T[,] arr) {
            return new Point(arr.GetLength(0), arr.GetLength(1));
        }
    }
}
