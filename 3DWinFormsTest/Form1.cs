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

        public bool RotateCube = true;
        public double CubeScale = 0.25;
        public DateTime LastRotation = DateTime.Now;
        public double RotationAngle = 5;
        public double RotationDelta = 1000 /*ms*/ / 30; /*target fps*/ 
        public Vector CubePosition = new Vector(0, 0.25, 1);

        public static Drawable Canvas = new Drawable(new Size(800, 600), 255u << 24);
        public Object3D Camera;
        public Object3D Grid;
        public Object3D Cube;

        public Object3D Axis;

        private Bitmap Image = null;
        private Bitmap Buffer = null;
        private bool QuitThreads = false;
        private Mutex FPSMutex = new Mutex();
        private Mutex QuitMutex = new Mutex();
        private Mutex ImageMutex = new Mutex();

        private double MovementSensitivity = 1;
        private double RotationSensitivity = 150;

        private double FPSCount = 0;
        private DateTime LastFrame = DateTime.Now;

        public Form1() {
            InitializeObjects();
            SetupGrid();
            SetupCube();
            SetupAxis();
            new Thread(() => DrawWorker()).Start();
            this.DoubleBuffered = true;
            InitializeComponent();
        }

        private void SetupAxis() {
            Axis.AddVector(new Vector(-.75, 0, 0)); // 0
            Axis.AddVector(new Vector(0.75, 0, 0)); // 1
            Axis.AddLine(new List<int>() { 0, 1 }, CubeOutlineColor);
            Axis.AddVector(new Vector(0, -.75, 0)); // 2
            Axis.AddVector(new Vector(0, 0.75, 0)); // 3
            Axis.AddLine(new List<int>() { 2, 3 }, CubeOutlineColor);
            Axis.AddVector(new Vector(0, 0, -.75)); // 4
            Axis.AddVector(new Vector(0, 0, 0.75)); // 5
            Axis.AddLine(new List<int>() { 4, 5 }, CubeOutlineColor);
        }

        private void InitializeObjects() {
            Camera = new Object3D(null, new Transform3D());
            Grid = new Object3D(Camera, new Transform3D());
            Cube = new Object3D(Camera, new Transform3D());
            Axis = new Object3D(Camera, new Transform3D());
            Camera.Transform.Position = new Vector(0, 0, 0, 0);
        }

        private void DrawWorker() {
            QuitMutex.WaitOne();
            while (QuitThreads == false) {
                QuitMutex.ReleaseMutex();

                // preprocessing
                Canvas.FillDrawable(BGroundColor);
                if (RotateCube == true) { 
                    if ((DateTime.Now - LastRotation).TotalMilliseconds > RotationDelta) {
                        Cube.Transform.Rotation.Y = (Cube.Transform.Rotation.Y + RotationAngle) % 360;
                        LastRotation = DateTime.Now;
                    }
                }

                // draw
                Camera.DrawObject(ref Canvas);
                Buffer = Canvas.ToBitmap();
                ImageMutex.WaitOne();
                Image = Buffer;
                ImageMutex.ReleaseMutex();

                FPSMutex.WaitOne();
                FPSCount = 1000 / (DateTime.Now - LastFrame).TotalMilliseconds;
                FPSMutex.ReleaseMutex();
                LastFrame = DateTime.Now;

                QuitMutex.WaitOne();
            }
            QuitMutex.ReleaseMutex();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            ImageMutex.WaitOne();
            this.BackgroundImage = Image;
            label1.Text = "Position: " + 
                Camera.Transform.Position.X + ", " +
                Camera.Transform.Position.Y + ", " +
                Camera.Transform.Position.Z;
            ImageMutex.ReleaseMutex();
            FPSMutex.WaitOne();
            this.Text = "Form1: " + ((int)FPSCount).ToString() + " fps";
            FPSMutex.ReleaseMutex();
        }

        public void SetupGrid() {
            //Grid.LinesVisible = false;
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
            //Grid.LinesVisible = false;
        }

        public void SetupCube() {

            // create verticies
            for (int i = 0; i < 8; i++) {
                int x = i % 4 < 2       ? -1 : 1;
                int y = i < 4           ? -1 : 1;
                int z = (i + 1) % 4 < 2 ? -1 : 1;
                Cube.AddVector(new Vector(x, y, z));
            }

            // create outline
            for (int i = 0; i < 2; i++) {
                Cube.AddLine(new List<int>() {
                    (i * 4) + 0, (i * 4) + 1, (i * 4) + 2, (i * 4) + 3, (i * 4) + 0,}, CubeOutlineColor);
                Cube.AddLine(new List<int>() { i + 0, i + 4 }, CubeOutlineColor);
                Cube.AddLine(new List<int>() { i + 2, i + 6 }, CubeOutlineColor);
            }

            // create triangles, math is dependent on verticies being in a predetermined order.
            for (int i = 0; i < 4; i++) {
                int rl_x = i < 2 ? (i % 2 == 0 ? 0 : 7) : (i % 2 == 0 ? 2 : 5);
                int fb_x = i < 2 ? (i % 2 == 0 ? 1 : 4) : (i % 2 == 0 ? 3 : 6);
                Cube.AddTriangle(i * 2,  ((i / 2) * 4) + 1,  ((i / 2) * 4) + 3,  i < 2 ? CubeBottomColor : CubeTopColor);
                Cube.AddTriangle(rl_x,   4 - ((i / 2) * 3),  3 * ((i / 2) + 1),  i < 2 ? CubeRightColor  : CubeLeftColor);
                Cube.AddTriangle(fb_x,   ((i / 2) * 2),      ((i / 2) * 2) + 5,  i < 2 ? CubeFrontColor  : CubeBackColor);
            }

            Cube.Transform.Position = CubePosition;
            //Cube.Transform.Rotation = new Vector(40, -40, 0);
            Cube.Transform.Scale = new Vector(CubeScale, CubeScale, CubeScale, 0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            QuitMutex.WaitOne();
            QuitThreads = true;
            QuitMutex.ReleaseMutex();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Camera != null) {
                Vector shiftPos = new Vector(0, 0, 0, 0);
                shiftPos.Z += e.KeyChar == 's' ? 0.01 : 0;
                shiftPos.Z += e.KeyChar == 'w' ? -0.01 : 0;
                shiftPos.X += e.KeyChar == 'a' ? 0.01 : 0;
                shiftPos.X += e.KeyChar == 'd' ? -0.01 : 0;
                Camera.Transform.Position += shiftPos;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e) {

        }
    }
}
