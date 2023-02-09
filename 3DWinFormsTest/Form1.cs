using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public partial class Form1 : Form {

        public uint BGroundColor    = 0xff000000;
        public uint GridColor       = 0xffffffff;
        public uint CubeColor       = 0xff00ff00;

        public double CubeScale = 0.25;
        public Vector CubePosition = new Vector(0, 0.25, 0.5);

        public static Drawable Canvas = new Drawable(new Size(800, 600), 255u << 24);
        public Object3D Grid = new Object3D(null, new Transform3D());
        public Object3D Cube = new Object3D(null, new Transform3D());

        public Form1() {
            SetupGrid();
            SetupCube();
            this.DoubleBuffered = true;
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            // preprocessing
            Canvas.FillDrawable(BGroundColor);
            Cube.Transform.Rotation.Y += 5;

            // draw
            Grid.DrawObject(ref Canvas);
            Cube.DrawObject(ref Canvas);

            this.BackgroundImage = Canvas.ToBitmap();
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

            Cube.AddLine(new List<int>() { 0, 1, 2, 3, 0 }, CubeColor);
            Cube.AddLine(new List<int>() { 4, 5, 6, 7, 4 }, CubeColor);
            Cube.AddLine(new List<int>() { 0, 4 }, CubeColor);
            Cube.AddLine(new List<int>() { 1, 5 }, CubeColor);
            Cube.AddLine(new List<int>() { 2, 6 }, CubeColor);
            Cube.AddLine(new List<int>() { 3, 7 }, CubeColor);

            //bottom
            Cube.AddTriangle(0, 1, 3, 0xff00ff00);
            Cube.AddTriangle(2, 1, 3, 0xff00ff00);
            //top
            Cube.AddTriangle(4, 5, 7, 0xffff0000);
            Cube.AddTriangle(6, 5, 7, 0xffff0000);
            //left
            Cube.AddTriangle(1, 5, 6, 0xffffff00);
            Cube.AddTriangle(2, 1, 6, 0xffffff00);
            //right
            Cube.AddTriangle(0, 4, 3, 0xffff00ff);
            Cube.AddTriangle(7, 4, 3, 0xffff00ff);
            //front
            Cube.AddTriangle(0, 1, 5, 0xff0000ff);
            Cube.AddTriangle(4, 0, 5, 0xff0000ff);
            //back
            Cube.AddTriangle(2, 3, 7, 0xff00ffff);
            Cube.AddTriangle(6, 2, 7, 0xff00ffff);

            Cube.Transform.Position = CubePosition;
            Cube.Transform.Scale = new Vector(CubeScale, CubeScale, CubeScale, 0);
        }
    }
}
