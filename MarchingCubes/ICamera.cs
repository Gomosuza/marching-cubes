using Microsoft.Xna.Framework;

namespace MarchingCubes
{
	/// <summary>
	/// The interface for the camera, describing what portion of the level is being rendered.
	/// Responsible for transforming level to screen coordinates and vice-verca.
	/// </summary>
	public interface ICamera
	{
		/// <summary>
		/// The projection matrix that represents the current camera.
		/// </summary>
		Matrix Projection { get; }

		/// <summary>
		/// The view matrix that represents the current camera.
		/// </summary>
		Matrix View { get; }

		/// <summary>
		/// Returns the camera position.
		/// </summary>
		/// <returns></returns>
		Vector3 GetPosition();

		/// <summary>
		/// Updates the cameras internal values.
		/// </summary>
		/// <param name="dt"></param>
		void Update(GameTime dt);

		/// <summary>
		/// Rotates the camera horizontally (y axis).
		/// </summary>
		/// <param name="a"></param>
		void AddHorizontalRotation(float a);

		/// <summary>
		/// Rotates the camera vertically.
		/// </summary>
		/// <param name="a"></param>
		void AddVerticalRotation(float a);

		/// <summary>
		/// Moves the camera by the provided amount.
		/// Note that this is in relation to the rotation.
		/// </summary>
		/// <param name="movement"></param>
		void Move(Vector3 movement);
	}
}