using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MarchingCubes.Data
{
	/// <summary>
	/// Content loader implementation for a zip mri file.
	/// </summary>
	public class ZippedMriDataContentLoader : IContentLoader<ZippedMriData>
	{
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