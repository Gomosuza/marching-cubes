using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VoxelGen
{
	public class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length != 5)
			{
				PrintUsage();
				return;
			}
			int x, y, z;
			VoxelType type;
			string path = args[4];

			if (!int.TryParse(args[0], out x) ||
				!int.TryParse(args[1], out y) ||
				!int.TryParse(args[2], out z) ||
				!Enum.TryParse(args[3], true, out type))
			{
				Console.WriteLine("Unable to parse values.");
				Console.WriteLine();
				PrintUsage();
				return;
			}
			if (x < 10 || y < 10 || z < 10)
			{
				Console.WriteLine("Dimensions cannot be smaller than 10");
				Console.WriteLine();
				PrintUsage();
			}
			Generate(x, y, z, type, path);
		}

		private static void Generate(int x, int y, int z, VoxelType type, string path)
		{
			var buffer = new byte[x * y * z];
			switch (type)
			{
				case VoxelType.Sphere:
					GenerateSphere(buffer, x, y, z);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
			var rawFile = Path.GetTempFileName();
			var infoFile = Path.GetTempFileName();
			File.WriteAllText(infoFile, $"{x}x{y}x{z}");
			File.WriteAllBytes(rawFile, buffer);
			if (File.Exists(path))
				File.Delete(path);
			using (var zip = ZipFile.Open(path, ZipArchiveMode.Create))
			{
				zip.CreateEntryFromFile(infoFile, $"{type}.info");
				zip.CreateEntryFromFile(rawFile, $"{type}.raw");
			}
		}

		private static void GenerateSphere(byte[] buffer, int xlen, int ylen, int zlen)
		{
			var min = Math.Min(xlen, Math.Min(ylen, zlen));
			// prefer 10% margin
			var prefferedRadius = min * 0.9f / 2f;
			var centerX = xlen / 2f;
			var centerY = ylen / 2f;
			var centerZ = zlen / 2f;
			var radiusSquared = prefferedRadius * prefferedRadius;
			for (int x = 0; x < xlen; x++)
			{
				for (int y = 0; y < ylen; y++)
				{
					for (int z = 0; z < zlen; z++)
					{
						// only surface needs sample points
						var d = DistanceSquared(x, y, z, centerX, centerY, centerZ);
						var dist = (d - radiusSquared) / radiusSquared;
						if (dist < 0.1f)
						{
							buffer[Index(x, y, z, xlen, ylen)] = 255;
						}
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float DistanceSquared(int x, int y, int z, float centerX, float centerY, float centerZ)
		{
			return (x - centerX) * (x - centerX) +
					(y - centerY) * (y - centerY) +
					(z - centerZ) * (z - centerZ);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Index(int x, int y, int z, int xlen, int ylen)
		{
			return x + xlen * (y + z * ylen);
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: x y z <type> <file_out>");
			Console.WriteLine("x y and z are the dimensions of the resulting datagrid");
			var enumValues = Enum.GetValues(typeof(VoxelType)).OfType<VoxelType>().ToList();
			Console.WriteLine("<type> is one of the following: " + string.Join(", ", enumValues));
			Console.WriteLine("<file_out> is the full filepath where the output file will be written. Existing files will be overriden");
		}
	}
}
