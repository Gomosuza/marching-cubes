using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MarchingCubes.Data
{
	/// <summary>
	/// Content loader implementation for a zip mri file.
	/// Expected file format:
	/// .zip file with at least 2 entries ending in .info and .raw
	/// The first .info file found is parsed:
	/// It must contain one line with 100x200x300 (3 valid integers seperated by x) defining the x,y and z dimension of the actual data.
	/// Empty lines and lines starting with ; are ignored.
	/// The first .raw file found is parsed:
	/// Its length must equal to the sum of the x,y and z values from the info file and each byte will be parsed into an integer.
	/// </summary>
	public class ZippedMriDataContentLoader : IContentLoader<ZippedMriData>
	{
		/// <summary>
		/// Loads an mri zip file.
		/// See class header for file spec.
		/// </summary>
		/// <param name="filepath">The path from where to load.</param>
		/// <returns>The loaded file type</returns>
		/// <exception cref="FileLoadException">Thrown when the file is missing, or wrong format.</exception>
		public ZippedMriData Load(string filepath)
		{
			using (var zip = ZipFile.Open(filepath, ZipArchiveMode.Read))
			{
				var info = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".info", StringComparison.InvariantCultureIgnoreCase));
				var raw = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".raw", StringComparison.InvariantCultureIgnoreCase));
				if (info == null || raw == null)
					throw new FileLoadException("mri file must contain .info and .raw entries");

				int x = 0, y = 0, z = 0;
				var validInfo = false;
				using (var infoStream = info.Open())
				using (var reader = new StreamReader(infoStream))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
							continue;
						if (!line.Contains("x"))
							throw new FileLoadException("info file must have one line with format 100x100x100, where the 3 digits represent the dimension (width, height and length) of the .raw data");
						var dims = line.Split('x');
						if (dims.Length != 3)
							throw new FileLoadException($"info dimension must be 3D, found: {dims.Length} dimension(s)");

						if (!int.TryParse(dims[0], out x) ||
							!int.TryParse(dims[1], out y) ||
							!int.TryParse(dims[2], out z))
						{
							throw new FileLoadException("Dimensions not valid integers!");
						}
						validInfo = true;
						break;
					}
				}
				if (!validInfo)
					throw new FileLoadException("unable to read .info file");
				var buffer = new byte[x * y * z];
				using (var rawStream = raw.Open())
				{
					var read = rawStream.Read(buffer, 0, buffer.Length);
					if (read != buffer.Length)
						throw new FileLoadException($".raw data length does not match info file, Found: {rawStream.Length} bytes in .raw, but .info said to expect: {x}*{y}*{z}={buffer.Length}");
				}
				return new ZippedMriData(buffer, x, y, z);
			}
		}
	}
}