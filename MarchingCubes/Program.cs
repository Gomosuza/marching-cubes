namespace MarchingCubes
{
	/// <summary>
	/// Program entry point.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			using (var g = new Game())
			{
				g.Run();
			}
		}
	}
}
