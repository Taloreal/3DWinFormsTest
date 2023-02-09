using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DWinFormsTest {

    public delegate Vector VectorGetter(int index);
    public delegate Vector VectorTranslation(Vector vector);

    public class Object3D {

        public Transform3D Transform { get; private set; }

        private List<Line3D> _Lines;
        public Line3D[] Lines => _Lines.ToArray();

        private List<Vector> _Verticies;
        public Vector[] Verticies => _Verticies.ToArray();

        private List<Triangle3D> _Triangles;
        public Triangle3D[] Triangles => _Triangles.ToArray();

        public Object3D Parent;
        private List<Object3D> _Children;
        public Object3D[] Children => _Children.ToArray();

        public Object3D(Object3D parent, Transform3D transform) {
            Parent = parent;
            Transform = transform;
            _Lines = new List<Line3D>();
            _Verticies = new List<Vector>();
            _Triangles = new List<Triangle3D>();
            _Children = new List<Object3D>();
            if (parent != null ) {
                Parent._Children.Add(this);
            }
        }

        public void AddVector(Vector vect) {
            if (_Verticies.All(v => v != vect)) { 
                _Verticies.Add(vect);
            }
        }

        public void RemoveVector(int index) { 
            if (index < 0 || index >= _Verticies.Count) {
                return;
            }
            _Triangles.RemoveAll(t => t.Verticies.Contains(index));
            _Triangles.ForEach(t => {
                for (int i = 0; i < 3; i++) {
                    if (t.Verticies[i] > index) {
                        t.Verticies[i] -= 1;
                    }
                }
            });
            _Lines.RemoveAll(l => l.Verticies.Contains(index));
            _Lines.ForEach(l => {
                for (int i = 0; i < l.Verticies.Count; i++) {
                    if (l.Verticies[i] > index) {
                        l.Verticies[i] -= 1;
                    }
                }
            });
            _Verticies.RemoveAt(index);
        }

        public void AddTriangle(int v1, int v2, int v3, uint clr = 0) {
            if (v1 < 0 || v2 < 0 || v3 < 0) { return; }
            if (v1 >= _Verticies.Count() || v2 >= _Verticies.Count() || v3 >= _Verticies.Count()) {
                return;
            }

            _Triangles.Add(new Triangle3D(v1, v2, v3, GetVerticy, TranslateVerticy, clr));
        }

        public void AddLine(List<int> verticies, uint clr) {
            verticies.RemoveAll(i => i < 0 || i >= _Verticies.Count());
            _Lines.Add(new Line3D(verticies, GetVerticy, TranslateVerticy, clr));
        }

        private Vector GetVerticy(int index) { 
            if (index < 0 || index >= _Verticies.Count()) {
                return null;
            }
            return _Verticies[index];
        }

        private Vector TranslateVerticy(Vector vector) {
            Vector after = Transform.Rotate(vector);
            after.X *= Transform.Scale.X;
            after.Y *= Transform.Scale.Y;
            after.Z *= Transform.Scale.Z;
            after += Transform.Position;
            return after;
        }

        public void DrawObject(ref Drawable canvas) {
            for (int i = 0; i < _Lines.Count; i++) {
                _Lines[i].DrawLine(ref canvas);
            }
            for (int i = 0; i < _Triangles.Count; i++) {
                _Triangles[i].DrawFilled(ref canvas);
            }
        }

    }
}
