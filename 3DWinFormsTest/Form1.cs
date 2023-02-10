using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public partial class Form1 : Form {

        public uint BGroundColor        = 0xff000000;
        public uint GridColor           = 0xffffffff;
        
        public uint CubeOutlineColor    = 0xff00ff00;
        public uint CubeTopColor        = 0xffff0000;
        public uint CubeBottomColor     = 0xff00ff00;
        public uint CubeLeftColor       = 0xffffff00;
        public uint CubeRightColor      = 0xffff00ff;
        public uint CubeFrontColor      = 0xff0000ff;
        public uint CubeBackColor       = 0xff00ffff;

        public double CubeScale = 0.25;
        public DateTime LastRotation = DateTime.Now;
        public double RotationAngle = 5;
        public double RotationDelta = 1000 /*ms*/ / 30; /*target fps*/ 
        public Vector CubePosition = new Vector(0, 0.25, 0.5);

        public static Drawable Canvas = new Drawable(new Size(800, 600), 255u << 24);
        public Object3D Grid = new Object3D(null, new Transform3D());
        public Object3D Cube = new Object3D(null, new Transform3D());

        private Bitmap Image = null;
        private Bitmap Buffer = null;
        private bool QuitThreads = false;
        private Mutex FPSMutex = new Mutex();
        private Mutex QuitMutex = new Mutex();
        private Mutex ImageMutex = new Mutex();

        private double FPSCount = 0;
        private DateTime LastFrame = DateTime.Now;

        public Form1() {
            SetupGrid();
            SetupCube();
            new Thread(() => DrawWorker()).Start();
            this.DoubleBuffered = true;
            InitializeComponent();
        }

        private void DrawWorker() {
            QuitMutex.WaitOne();
            while (QuitThreads == false) {
                QuitMutex.ReleaseMutex();

                // preprocessing
                Canvas.FillDrawable(BGroundColor);
                if ((DateTime.Now - LastRotation).TotalMilliseconds > RotationDelta) {
                    Cube.Transform.Rotation.Y = (Cube.Transform.Rotation.Y + RotationAngle) % 360;
                    LastRotation = DateTime.Now;
                }

                // draw
                Grid.DrawObject(ref Canvas);
                Cube.DrawObject(ref Canvas);
                Buffer = Canvas.ToBitmap();
                ImageMutex.WaitOne();
                Image = Buffer;
                ImageMutex.ReleaseMutex();

                FPSMutex.WaitOne();
                FPSCount = 1000 / (DateTime.Now - LastRotation).TotalMilliseconds;
                FPSMutex.ReleaseMutex();
                LastFrame = DateTime.Now;

                QuitMutex.WaitOne();
            }
            QuitMutex.ReleaseMutex();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            ImageMutex.WaitOne();
            this.BackgroundImage = Image;
            ImageMutex.ReleaseMutex();
            FPSMutex.WaitOne();
            this.Text = "Form1: " + ((int)FPSCount).ToString() + " fps";
            FPSMutex.ReleaseMutex();
        }

        public void SetupGrid() {
            for (double x = -0.5; x <= 0.5; x += 0.1) {
                Grid.AddVector(new Vector(-.5, 0, Math.Round(x, 3))); // 0 - 10
            }
            for (double x = -0.5; x <= 0.5; x += 0.1) {
                Grid.AddVector(new Vector(.5, 0, Math.Round(x, 3))); // 11 - 21
            }
            for (double x = -0.4; x <= 0.4; x += 0.1) {
                Grid.AddVector(new Vector(Math.Round(x, 3), 0, -.5)); // 22 - 30
            }
            for (double x = -0.4; x <= 0.4; x += 0.1) {
                Grid.AddVector(new Vector(Math.Round(x, 3), 0, .5)); // 31 - 39
            }
            for (int i = 0; i < 11; i++) {
                Grid.AddLine(new List<int>() { i, 11 + i }, GridColor);
            }
            Grid.AddLine(new List<int>() { 0, 10 }, GridColor);
            for (int i = 0; i < 9; i++) {
                Grid.AddLine(new List<int>() { 22 + i, 31 + i }, GridColor);
            }
            Grid.AddLine(new List<int>() { 11, 21 }, GridColor);
        }

        public void SetupCube() {
            Cube.AddVector(new Vector(-1, -1, -1)); 
            Cube.AddVector(new Vector(-1, -1,  1)); 
            Cube.AddVector(new Vector( 1, -1,  1)); 
            Cube.AddVector(new Vector( 1, -1, -1)); 
            Cube.AddVector(new Vector(-1,  1, -1)); 
            Cube.AddVector(new Vector(-1,  1,  1)); 
            Cube.AddVector(new Vector( 1,  1,  1)); 
            Cube.AddVector(new Vector( 1,  1, -1)); 

            Cube.AddLine(new List<int>() { 0, 1, 2, 3, 0 }, CubeOutlineColor);
            Cube.AddLine(new List<int>() { 4, 5, 6, 7, 4 }, CubeOutlineColor);
            Cube.AddLine(new List<int>() { 0, 4 }, CubeOutlineColor);
            Cube.AddLine(new List<int>() { 1, 5 }, CubeOutlineColor);
            Cube.AddLine(new List<int>() { 2, 6 }, CubeOutlineColor);
            Cube.AddLine(new List<int>() { 3, 7 }, CubeOutlineColor);

            //bottom
            Cube.AddTriangle(0, 1, 3, CubeBottomColor);
            Cube.AddTriangle(2, 1, 3, CubeBottomColor);
            //top
            Cube.AddTriangle(4, 5, 7, CubeTopColor);
            Cube.AddTriangle(6, 5, 7, CubeTopColor);
            //left
            Cube.AddTriangle(5, 1, 6, CubeLeftColor);
            Cube.AddTriangle(2, 1, 6, CubeLeftColor);
            //right
            Cube.AddTriangle(0, 4, 3, CubeRightColor);
            Cube.AddTriangle(7, 4, 3, CubeRightColor);
            //front
            Cube.AddTriangle(0, 1, 5, CubeFrontColor);
            Cube.AddTriangle(4, 0, 5, CubeFrontColor);
            //back
            Cube.AddTriangle(2, 3, 7, CubeBackColor);
            Cube.AddTriangle(6, 2, 7, CubeBackColor);

            Cube.Transform.Position = CubePosition;
            Cube.Transform.Scale = new Vector(CubeScale, CubeScale, CubeScale, 0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            QuitMutex.WaitOne();
            QuitThreads = true;
            QuitMutex.ReleaseMutex();
        }
    }
}
