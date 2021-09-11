#pragma warning disable IDE1006 // Naming Styles

using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZargoEngine.Editor;
using ZargoEngine.Helper;
using ZargoEngine.Mathmatics;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    using Bmatrix = BulletSharp.Math.Matrix;
    
    public unsafe class Transform : Component
    {
        public Vector3 right   => Vector3.Transform(Vector3.UnitX, Quaternion.Conjugate(Translation.ExtractRotation()));
        public Vector3 up      => Vector3.Transform(Vector3.UnitY, Quaternion.Conjugate(Translation.ExtractRotation()));
        public Vector3 forward => Vector3.Transform(-Vector3.UnitZ, Quaternion.Conjugate(Translation.ExtractRotation()));

        // comining
        public Vector3 localPosition = Vector3.Zero;

        public Vector3 position = Vector3.Zero;

        // fpr serialization
        public Vector3 _eulerAngles = Vector3.Zero;

        public Vector3 eulerAngles;

        public Quaternion rotation;

        public Vector3 scale = Vector3.One;

        public Matrix4 Translation;

        public Transform parent;
        public List<Transform> childs = new List<Transform>();

        public ref Matrix4 GetTranslation()
        {
            return ref Translation;
        }

        public float Distance(Vector3 other)
        {
            return MathF.Sqrt(MathF.Pow(position.X - other.X, 2) +
                              MathF.Pow(position.Y - other.Y, 2) +
                              MathF.Pow(position.Z - other.Z, 2));
        }

        public float Distance(Transform other)
        {
            return MathF.Sqrt(MathF.Pow(position.X - other.position.X, 2) +
                              MathF.Pow(position.Y - other.position.Y, 2) +
                              MathF.Pow(position.Z - other.position.Z, 2));
        }

        public override void DrawWindow()
        {
            ImGui.TextColored(Color4.Orange.ToSystem(), name);

            GUI.Vector3Field(ref position, "Position", () => SetPosition(position, true));
            GUI.Vector3Field(ref scale, "Scale", () => SetScale(scale, true));
            GUI.Vector3Field(ref _eulerAngles, "Euler Angles", () => SetEuler(_eulerAngles, true));

            ImGui.Separator();
        }

        public Transform(GameObject gameObject, Vector3 position = new Vector3(), Vector3 eulerAngles = new Vector3()) : base(gameObject)
        {
            SetPosition(position, false);
            SetScale(scale, false);
            SetEuler(eulerAngles, true);

            name = gameObject.name + "'s" + " Transform";
            new Line(position, position + up * 10);
        }

        public void UpdateTranslation(bool notify = true)
        {
            if (notify) OnTransformChanged?.Invoke(ref Translation);


            Translation = Matrix4.Identity * Matrix4.CreateFromQuaternion(rotation)*
                          Matrix4.CreateScale(scale) *
                          Matrix4.CreateTranslation(position);

            if (parent != null) {
                Translation += parent.Translation;
            }

            for (short i = 0; i < childs.Count; i++)
            {
                childs[i].UpdateTranslation();
            }

            // when object moves wee need to calculate shadow
            // avoiding calculation of every children
            if (childs.Count == 0 || parent == null) {
                Renderer3D.UpdateShadows();
            }
        }

        public void SetEuler(in Vector3 value, bool notify)
        {
            _eulerAngles = value; // this because _eulerangles must be non zero
            eulerAngles = value.V3DegreToRadian();

            Quaternion.FromEulerAngles(eulerAngles, out rotation);

            if (notify) UpdateTranslation();

            OnRotationChanged?.Invoke(ref rotation);
        }

        public void SetPosition(in float x, in float y, in float z, bool notify)
        {
            position = new Vector3(x, y, z);
            if (notify) UpdateTranslation();
            OnPositionChanged?.Invoke(ref position);
        }
        
        public void SetPosition(in Vector3 value, bool notify)
        {
            position = value;
            if (notify) UpdateTranslation();
            OnPositionChanged?.Invoke(ref position);
        }
        
        public void SetScale(in Vector3 value, bool notify)
        {
            scale = value;
            if (notify) UpdateTranslation();
            OnScaleChanged?.Invoke(ref scale);
        }

        public void SetQuaterion(in Quaternion value, bool notify, bool notifyTranslationEvent = true)
        {
            rotation = value;
            Quaternion.ToEulerAngles(rotation, out eulerAngles);
            _eulerAngles = eulerAngles;

            if (notify) UpdateTranslation(notifyTranslationEvent);

            OnRotationChanged?.Invoke(ref rotation);
        }

        public void SetMatrix(Bmatrix bmatrixPtr)
        {
            Translation = Unsafe.As<Bmatrix, Matrix4>(ref bmatrixPtr);

            SetScale(Translation.ExtractScale(), false);
            SetPosition(Translation.ExtractTranslation(), false);
            SetQuaterion(Translation.ExtractRotation(), true, false);
        }

        public void SetMatrix(Matrix4 matrixPtr)
        {
            SetScale(matrixPtr.ExtractScale(), false);
            SetPosition(matrixPtr.ExtractTranslation(), false);
            SetQuaterion(matrixPtr.ExtractRotation(), true, false);
        }

        // events
        public delegate void TransformChangedEvent([In] ref Matrix4 transform);
        public delegate void RotationChangedEvent([In] ref Quaternion rotation);
        public delegate void PositionChangedEvent([In] ref Vector3 position);
        public delegate void ScaleChangedEvent([In] ref Vector3 scale);

        public event TransformChangedEvent OnTransformChanged;
        public event PositionChangedEvent OnPositionChanged;
        public event RotationChangedEvent OnRotationChanged;
        public event ScaleChangedEvent OnScaleChanged;

        // shortcuts: instead of transform.gameobject.something transform.something
        public T AddComponent<T>(T component) where T : Component => gameObject.AddComponent(component);
        public T GetComponent<T>() where T : Component => gameObject.GetComponent<T>();
        public bool TryGetComponent<T>(out T value) where T : Component => gameObject.TryGetComponent(out value);
        public bool HasComponent<T>() => gameObject.HasComponent<T>();
    }
}
