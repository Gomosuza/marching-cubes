using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MarchingCubes
{
    /// <summary>
    /// Basic camera implementation.
    /// </summary>
    public sealed class FirstPersonCamera : ICamera
    {
        private const float NearZ = 0.5f;

        private readonly GraphicsDevice _device;

        private bool _dirty;
        private float _leftRightLeftRightRotation;
        private float _upDownRotation;

        /// <summary>
        /// Creates a new instance of the camera.
        /// The camera will face negative Z direction by default.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="initialPosition">The starting point of the camera.</param>
        /// <param name="farZ">The far clipping plane.</param>
        public FirstPersonCamera(GraphicsDevice device, Vector3 initialPosition, float farZ = 1000)
        {
            _device = device;
            FarZ = farZ;

            Position = initialPosition;
            _dirty = true;
        }

        /// <summary>
        /// The far plane value that this camera uses.
        /// </summary>
        public float FarZ { get; }

        /// <summary>
        /// The horizontal rotation around the Y axis.
        /// </summary>
        public float LeftRightRotation
        {
            get { return _leftRightLeftRightRotation; }
            private set
            {
                _leftRightLeftRightRotation = value % MathHelper.TwoPi;
                _dirty = true;
            }
        }

        /// <summary>
        /// The position of the camera in the world.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <see cref="ICamera.Projection"/>
        public Matrix Projection { get; private set; }

        /// <summary>
        /// The vertical rotation around the X axis.
        /// </summary>
        public float UpDownRotation
        {
            get { return _upDownRotation; }
            private set
            {
                // restrict movement around Y axis
                _upDownRotation = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
                _dirty = true;
            }
        }

        /// <see cref="ICamera.View"/>
        public Matrix View { get; private set; }

        /// <summary>
        /// Adds the value to the horizontal rotation (if positive will make the player look right, if negative will make him look left).
        /// </summary>
        /// <param name="value"></param>
        public void AddHorizontalRotation(float value)
        {
            LeftRightRotation -= value;

            _dirty = true;

            Update();
        }

        /// <summary>
        /// Adds the value to the vertical rotation (if positive will make the player look upwards, if negative will make him look downwards).
        /// The rotation is automatically clamped at 0 and <see cref="MathHelper.Pi"/>.
        /// </summary>
        /// <param name="value"></param>
        public void AddVerticalRotation(float value)
        {
            UpDownRotation -= value;

            _dirty = true;

            Update();
        }

        /// <summary>
        /// Returns the current position of the camera.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPosition()
        {
            return Position;
        }

        /// <summary>
        /// Moves the camera by the specified amount.
        /// </summary>
        /// <param name="movement"></param>
        public void Move(Vector3 movement)
        {
            var cameraRotation = Matrix.CreateRotationX(_upDownRotation) * Matrix.CreateRotationY(_leftRightLeftRightRotation);
            var rotatedVector = Vector3.Transform(movement, cameraRotation);
            Position += rotatedVector;
            _dirty = true;
        }


        /// <summary>
        /// Moves the camera immediately to the specific position.
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {
            Position = position;
            _dirty = true;
        }

        /// <summary>
        /// Sets both rotation components.
        /// </summary>
        /// <param name="horizontalRotation">The rotation around the Y axis.</param>
        /// <param name="verticalRotation">The rotation around the X axis.</param>
        public void SetRotation(float horizontalRotation, float verticalRotation)
        {
            UpDownRotation = horizontalRotation;
            LeftRightRotation = verticalRotation;

            Update();
        }

        /// <summary>
        /// Updates the cameras internal values.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(GameTime dt) => Update();

        private void Update()
        {
            if (!_dirty)
            {
                return;
            }

            var rotation = Matrix.CreateRotationX(_upDownRotation) * Matrix.CreateRotationY(_leftRightLeftRightRotation);

            var rotated = Vector3.Transform(Vector3.UnitZ * -1, rotation);
            var final = Position + rotated;

            var up = Vector3.Transform(Vector3.Up, rotation);
            View = Matrix.CreateLookAt(Position, final, up);

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _device.Viewport.AspectRatio, NearZ, FarZ);

            _dirty = false;
        }
    }
}