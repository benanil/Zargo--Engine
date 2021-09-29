using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using System;
using ZargoEngine.SaveLoad;

#nullable disable warnings
namespace ZargoEngine.Rendering
{
    
    public class Camera : ICamera
    {
        // please don't create other instance of camera
        private static Camera instance;
        public static ICamera main
        {
            get
            {
                if (SceneManager.currentScene != null && SceneManager.currentScene.isPlaying && SceneManager.currentScene.playerCamera != null)
                {
                    return SceneManager.currentScene.playerCamera;
                }
                return instance;
            }
        }
        
        /// <summary>level editors camera</summary>
        public static Camera SceneCamera => instance;

        // Those vectors are directions pointing outwards from the camera to define how it rotated
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;
        public Vector3 GetForward() => _front;
        public Vector3 GetRight() => Right;
        public Vector3 GetUp() => Up;
        public ref Vector3 GetPosition() => ref Position;

        // Rotation around the X axis (radians)
        private float _pitch;

        // Rotation around the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2; // Without this you would be started rotated 90 degrees right

        // The field of view of the camera (radians)
        private float _fov = MathHelper.PiOver2;


        /// <summary>dont call it we are need to call it only once when engine starts</summary>
        public Camera(Vector3 position, float aspectRatio,Vector3 front)
        {
            instance = this;
            _front = front;
            Position = position;
            AspectRatio = aspectRatio;
            UpdateVectors();
        }

        public void SetRotation(Vector3 position,Vector3 front, Vector3 up)
        {
            _front = front;
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
            Position = position;
            UpdateVectors();
        }

        // The position of the camera
        public Vector3 Position;

        private float _AspectRatio;
        public float AspectRatio 
        {
            get => _AspectRatio;
            set
            {
                _AspectRatio = value;
                UpdateVectors();
            }
        }

        public Vector3 Front
        {
            get => _front;
            set
            {
                _front = value;
                UpdateVectors();
            }
        }

        public Vector3 Up
        {
            get => _up;
            set
            {
                _up = value;
                UpdateVectors();
            }
        }

        public Vector3 Right
        {
            get => _right;
            set
            {
                _right = value;
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view, this has been discussed more in depth in a
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 160f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 ViewMatrix;

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public ref Matrix4 GetViewMatrix() => ref ViewMatrix;

        public Matrix4 projectionMatrix;

        // Get the projection matrix using the same method we have used up until this point
        public ref Matrix4 GetProjectionMatrix() => ref projectionMatrix;

        private Vector3 lastPosition;
        public Vector3 velocity;

        public Matrix4 Transformation;

        public Quaternion rotation;

        public void UpdateVectors()
        {
            // First the front matrix is calculated using some basic trigonometry
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results
            _front = Vector3.Normalize(_front);

            rotation = Quaternion.FromMatrix(new Matrix3(-Front, Up, -Right));

            // Calculate both the right and the up vector using cross product
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));

            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fov, MathF.Min(MathF.Max(AspectRatio,0.2f),3), 0.01f, 10000f);
            ViewMatrix = Matrix4.LookAt(Position, Position + _front, _up);

            // Handle Audio Listenner for 3d audio stuff
            // not working for now
            {
                velocity = (lastPosition - Position) * 3;

                AL.Listener(ALListener3f.Position, Position.X, Position.Y, Position.Z);
                AL.Listener(ALListener3f.Velocity, ref velocity);

                Transformation = Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(rotation);

                // oriantation of the listenner
                float[] ori = {
                    Front.X, Front.Y , Front.Z,
                    Up.X   , Up.Y    , Up.Z
                };
                AL.Listener(ALListenerfv.Orientation, ori);
                
                lastPosition = Position;
            }
        }
    }
}