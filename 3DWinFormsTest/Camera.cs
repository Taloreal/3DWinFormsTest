using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DWinFormsTest {

    public class Camera {

        public const double FoV = 90;
        public const double FarZ = 10.0;
        public const double NearZ = 0.1;

        public const double ToDegs = 180.0 / Math.PI;
        public const double ToRads = Math.PI / 180.0;
        public const double Aspect = 1920.0 / 1080.0;

        public static Matrix ClipMatrix = new Matrix(4, true);

        static Camera() {
            SetupClipMatrix();
        }

        private static void SetupClipMatrix() {
            double f = 1.0 / Math.Tan((FoV * ToRads) * 0.5);
            ClipMatrix[0, 0] = f * Aspect;
            ClipMatrix[1, 1] = f;
            ClipMatrix[2, 2] = (FarZ) / (FarZ - NearZ);
            ClipMatrix[3, 2] = 1;
            ClipMatrix[2, 3] = 0 - ((FarZ / NearZ) / (FarZ - NearZ));
            ClipMatrix[3, 3] = 0;
        }
    }
}
