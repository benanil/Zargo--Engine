

using OpenTK.Mathematics;
using System;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.Rendering
{
    using SysVec4 = System.Numerics.Vector4;

    public class PlayerCamera : Companent, ICamera
    {
        private Matrix4 ProjectionMatrix;
        private Matrix4 ViewMatrix;

        public float fov = 90;
        public float FarPlane = 5000;
        public float NearPlane = 0.1f;

        private static readonly Line[] lines = new Line[] {
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One),
            new Line(Vector3.Zero, Vector3.Zero, SysVec4.One)
        };


        public PlayerCamera(GameObject go) : base(go)
        {
            name = "Player Camera";
            SceneManager.currentScene.playerCamera = this;
            go.transform.OnTransformChanged += delegate (ref Matrix4 matrix) 
            {
                ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(Mathmatic.Deg2Rad * fov, AspectRatio(), NearPlane, FarPlane);
                ViewMatrix = Matrix4.LookAt(transform.position, transform.position + transform.forward, Vector3.UnitY);
            };

            for (byte i = 0; i < lines.Length; i++)
            {
                lines[i].ConnectTransform(transform);
            }

            DebugViewArea();
        }

        private float currentX, currentY;

        public override void Update()
        {
            currentX += Input.MouseX();
            currentY += Input.MouseY();

            transform.position += transform.forward * Input.Vertical() * Time.DeltaTime * 2;
            transform.position += transform.right   * Input.Horizontal() * Time.DeltaTime * 2;

            transform.eulerAngles.X = Mathmatic.LerpAngle(transform.eulerAngles.X, currentX, Time.DeltaTime);
            transform.eulerAngles.Y = Mathmatic.LerpAngle(transform.eulerAngles.X, currentY, Time.DeltaTime);
            transform.UpdateTranslation();
        }

        public override void OnValidate()
        {
            base.OnValidate();
            DebugViewArea();
        }

        public void DebugViewArea()
        {
            // Vector3 left  = Vector3.Lerp(transform.forward, -transform.right, Mathmatic.Min(fov, 179) / 360);
            // Vector3 right = Vector3.Lerp(transform.forward,  transform.right, Mathmatic.Min(fov, 179) / 360);
            // Vector3 up = Vector3.Lerp(transform.forward, transform.up, Mathmatic.Min(fov, 179) / (360 * AspectRatio()));
            // 
            // Vector3 rightUp   = ( up + right) * FarPlane;
            // Vector3 leftUp    = ( up + left ) * FarPlane;
            // Vector3 rightDown = (-up + right) * FarPlane;
            // Vector3 leftDown  = (-up + left ) * FarPlane;
            // 
            // // frustum
            // lines[0].Invalidate(Vector3.Zero, rightUp);
            // lines[1].Invalidate(Vector3.Zero, leftUp);
            // lines[2].Invalidate(Vector3.Zero, rightDown);
            // lines[3].Invalidate(Vector3.Zero, leftDown);
            // // near plane
            // lines[0].Invalidate(left  + up * NearPlane, right + up * NearPlane);
            // lines[1].Invalidate(right + up * NearPlane, right - up * NearPlane);
            // lines[2].Invalidate(right - up * NearPlane, left  - up * NearPlane);
            // lines[3].Invalidate(left  - up * NearPlane, left  + up * NearPlane);
            // // far plane
            // lines[0].Invalidate(leftUp   , rightUp);
            // lines[1].Invalidate(rightUp  , rightDown);
            // lines[2].Invalidate(rightDown, leftDown);
            // lines[3].Invalidate(leftDown , leftUp);
            for (int i = 0; i < lines.Length; i++)
            {
                float t = i * (i / (float)lines.Length);
                t *= MathF.PI;
                lines[i].Invalidate(Vector3.Zero, new Vector3(0, MathF.Sin(t), MathF.Cos(t)) * 15);
            }
        }

        public ref Matrix4 GetProjectionMatrix() => ref ProjectionMatrix;
        public ref Matrix4 GetViewMatrix() => ref ViewMatrix;
        public ref Vector3 GetPosition() => ref transform.position;
        public Vector3 GetForward() => transform.forward;
        public Vector3 GetRight() => transform.right;
        public Vector3 GetUp() => transform.up;
        private float AspectRatio() => Camera.SceneCamera.AspectRatio;
    }
}
