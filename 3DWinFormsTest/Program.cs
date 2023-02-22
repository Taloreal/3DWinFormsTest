using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public static class Program {

        public static double FarZ = 10;
        public static double NearZ = 1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
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

        public static int Clamp(this int val, int min, int max) {
            int clamped = val < min ? min : val;
            clamped = clamped > max ? max : clamped;
            return clamped;
        }
    }
}
