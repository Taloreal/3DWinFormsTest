using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DWinFormsTest {

    public class Transform3D {

        public Vector Position = new Vector(0, 0, 0, 0);
        public Vector Rotation = new Vector(0, 0, 0, 0);
        public Vector Scale = new Vector(1, 1, 1, 0);

        public Vector Rotate(Vector original) {
            Vector rot = Rotation * Matrix.ToRads;
            Vector rotated = new Vector(original.X, original.Y, original.Z, original.W);
            rotated -= Position;
            if (Rotation.X != 0) {
                Matrix xrot = new Matrix(3, true);
                xrot.SetRow(1, new double[] { 0.0, Math.Cos(rot.X), -1.0 * Math.Sin(rot.X) });
                xrot.SetRow(2, new double[] { 0.0, Math.Sin(rot.X), Math.Cos(rot.X) });
                rotated = xrot * rotated;
            }
            if (Rotation.Y != 0) {
                Matrix yrot = new Matrix(3, true);
                yrot.SetRow(0, new double[] { Math.Cos(rot.Y), 0.0, Math.Sin(rot.Y) });
                yrot.SetRow(2, new double[] { -1.0 * Math.Sin(rot.Y), 0.0, Math.Cos(rot.Y) });
                rotated = yrot * rotated;
            }
            if (Rotation.Z != 0) {
                Matrix zrot = new Matrix(3, true);
                zrot.SetRow(0, new double[] { Math.Cos(rot.Z), -1.0 * Math.Sin(rot.Z), 0.0 });
                zrot.SetRow(1, new double[] { Math.Sin(rot.Z), Math.Cos(rot.Z), 0.0 });
                rotated = zrot * rotated;
            }
            rotated += Position;
            return rotated;
        }

    }
}
