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
		private readonly byte[] _buffer;

		/// <summary>
		/// Creates a new instance holding the provided data.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="xLength"></param>
		/// <param name="yLength"></param>
		/// <param name="zLength"></param>
		public ZippedMriData(byte[] buffer, int xLength, int yLength, int zLength)
		{
			_buffer = buffer;
			XLength = xLength;
			YLength = yLength;
			ZLength = zLength;

			int min = int.MaxValue, max = int.MinValue, points = 0;
			for (int z = 0; z < ZLength; z++)
			{
				for (int y = 0; y < YLength; y++)
				{
					for (int x = 0; x < XLength; x++)
					{
						var value = this[x, y, z];
						if (value > 0)
							points++;
						if (value > max)
							max = value;
						if (value < min)
							min = value;
					}
				}
			}
			MinValue = min;
			MaxValue = max;
			DataPoints = points;
		}

		/// <summary>
		/// The data length in x direction.
		/// </summary>
		public int XLength { get; }

		/// <summary>
		/// The data length in y direction.
		/// </summary>
		public int YLength { get; }

		/// <summary>
		/// The data length in z direction.
		/// </summary>
		public int ZLength { get; }

		/// <summary>
		/// Direct accessor to the value at the provided index.
		/// y * sizeX
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		public int this[int x, int y, int z] => _buffer[x + XLength * (y + z * YLength)];

		/// <summary>
		/// Returns the minimum value of the entire data set.
		/// This is usually 0 as at least one datapoint will be empty.
		/// </summary>
		public int MinValue { get; }

		/// <summary>
		/// Returns the maximum value found within the entire data set.
		/// </summary>
		public int MaxValue { get; }

		/// <summary>
		/// Returns the number of data points found that where greater than 0.
		/// </summary>
		public int DataPoints { get; }
	}
}