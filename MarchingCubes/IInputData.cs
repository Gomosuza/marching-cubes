namespace MarchingCubes
{
	/// <summary>
	/// The input data for the marching cubes algorithm.
	/// </summary>
	public interface IInputData
	{
		/// <summary>
		/// The data length in x direction.
		/// </summary>
		int XLength { get; }

		/// <summary>
		/// The data length in y direction.
		/// </summary>
		int YLength { get; }

		/// <summary>
		/// The data length in z direction.
		/// </summary>
		int ZLength { get; }

		/// <summary>
		/// Direct accessor to the value at the provided index.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		int this[int x, int y, int z] { get; }
	}
}