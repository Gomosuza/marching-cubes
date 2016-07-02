namespace MarchingCubes.Data
{
	/// <summary>
	/// Container class that reads a .zip file containing mri data.
	/// The zip file must contain 2 entries: one with .info and one with .raw extension.
	/// The .info file may contain empty lines and lines starting with ; (both are ignored).
	/// The .info file must contain one line in the format: x*y*z where x,y and z are all valid ints.
	/// Finally the .raw file must contain as many bites as x*y*z sums up to.
	/// </summary>
	[ContentLoader(typeof(ZippedMriDataContentLoader))]
	public class ZippedMriData : IInputData
	{
		/// <summary>
		/// Creates a new instance holding the provided data.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public ZippedMriData(byte[] buffer, int x, int y, int z)
		{

		}
	}
}