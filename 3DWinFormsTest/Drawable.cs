using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace _3DWinFormsTest {

    public class Drawable {

        private uint[,] _Canvas;
        private double[,] _Depth;
        public uint[,] Canvas => _Canvas;
        public double[,] Depth => _Depth;

        public int Width => _Canvas.GetLength(0);
        public int Height => _Canvas.GetLength(1);
        public Rectangle Surface => new Rectangle(0, 0, Width, Height);

        public Drawable(Size size, uint clr) {
            _Canvas = new uint[size.Width, size.Height];
            _Depth = new double[size.Width, size.Height];
            FillDrawable(clr);
        }

        public Drawable(ref uint[,] src, Rectangle copyArea, Size size) { 
            _Canvas = new uint[size.Width, size.Height];
            _Depth = new double[size.Width, size.Height];
            Blit(ref src, ref _Canvas, copyArea, Surface, false);
        }

        public void FillDrawable(uint clr) {
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    _Canvas[x, y] = clr;
                    _Depth[x, y] = Program.FarZ;
                }
            }
        }

        public void DrawPixel(Point pos, uint clr, double depth = 0.0) {
            if (pos.X < 0 || pos.X >= Surface.Width) { return; }
            if (pos.Y < 0 || pos.Y >= Surface.Height) { return; }
            if (depth > 0) {
                if (depth < _Depth[pos.X, pos.Y]) {
                    _Canvas[pos.X, pos.Y] = clr;
                    _Depth[pos.X, pos.Y] = depth;
                }
            }
            else {
                _Canvas[pos.X, pos.Y] = clr;
            }
        }

        public void Blit(ref uint[,] src, Rectangle copyArea, Rectangle destArea) {
            Blit(ref src, ref _Canvas, copyArea, destArea, true);
        }

        public void Blit(Drawable src, Rectangle copyArea, Rectangle destArea) {
            Blit(ref src._Canvas, ref _Canvas, copyArea, destArea, false);
        }

        public static uint GetPartialColor(uint clr, uint toggles, int shift) {
            uint num = clr & toggles;
            num = shift == 0 ? num : (shift < 0 ? num << (shift * -1) : num >> shift);
            return num;
        }

        public static uint BlendColors(uint bottom, uint top) {
            int alpha = (int)GetPartialColor(top, 0xff000000, 24);
            uint result = alpha == 0 ? bottom : (alpha == 255 ? top : 0);
            if (alpha != 0 && alpha != 255) {
                int src = 0, dst = 0, shift = 0;
                float ratio = alpha / 255.0f;
                for (int i = 0; i < 4; i++) {
                    shift = i * 8;
                    src = (int)GetPartialColor(top, 255u << shift, shift);
                    dst = (int)GetPartialColor(bottom, 255u << shift, shift);
                    dst = (int)((src - dst) * ratio) + dst;
                    result += ((uint)dst << shift);
                }
            }
            return result;
        }

        public static uint InvertColor(uint clr) {
            return ((clr & 0xff000000) >> 24) + 
                ((clr & 0x00ff0000) >> 8) + 
                ((clr & 0x0000ff00) << 8) + 
                ((clr & 0x000000ff) << 24);
        }

        public static void Blit(ref uint[,] src, ref uint[,] dest, Rectangle copyArea, Rectangle destArea, bool flipBits) {
            double xscale = destArea.Width / (copyArea.Width + 0.0);
            double yscale = destArea.Height / (copyArea.Height + 0.0);
            xscale = xscale > 1.0 ? 1.0 : xscale;
            yscale = yscale > 1.0 ? 1.0 : yscale;
            int area = copyArea.Width * copyArea.Height;
            int sndx = 0, dndx = 0;
            Point bookmark = new Point();
            for (int i = 0; i < area; i++) {
                bookmark = i.Get2D(copyArea.Size);
                Point stleft = new Point(copyArea.Left, copyArea.Top);
                bookmark = bookmark.Add(stleft);
                sndx = bookmark.Get1D(src.GetSize());
                if (sndx >= 0) { 
                    Point dtleft = new Point(destArea.Left, destArea.Top);
                    uint clr = flipBits ? InvertColor(src[bookmark.X, bookmark.Y]) : src[bookmark.X, bookmark.Y];
                    bookmark.X = (int)(((bookmark.X - stleft.X + 0.0) * xscale) + dtleft.X);
                    bookmark.Y = (int)(((bookmark.Y - stleft.Y + 0.0) * yscale) + dtleft.Y);
                    dndx = bookmark.Get1D(dest.GetSize());
                    if (dndx >= 0) {
                        dest[bookmark.X, bookmark.Y] = BlendColors(dest[bookmark.X, bookmark.Y], clr);
                    }
                }
            }
        }

        public Bitmap ToBitmap() {
            Bitmap img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            BitmapData data = img.LockBits(new Rectangle(0, 0, Width, Height), 
                ImageLockMode.WriteOnly, img.PixelFormat);
            int stride = data.Stride;
            IntPtr ptr = data.Scan0;
            int bytes = Math.Abs(stride) * Height;
            byte[] buffer = new byte[bytes];
            int offset = 0;
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) { 
                    offset = y * stride + x * 4;
                    uint pixel = _Canvas[x, y];
                    buffer[offset + 0] = (byte)(pixel & 0xff);
                    buffer[offset + 1] = (byte)((pixel >> 8) & 0xff);
                    buffer[offset + 2] = (byte)((pixel >> 16) & 0xff);
                    buffer[offset + 3] = (byte)((pixel >> 24) & 0xff);
                }
            }
            Marshal.Copy(buffer, 0, ptr, bytes);
            img.UnlockBits(data);
            return img;
        }
    } 
}
