using MarchingCubes.SceneGraph;
using System;

namespace MarchingCubes.Extensions
{
	/// <summary>
	/// Extensions for the <see cref="SceneGraphEntity"/>
	/// </summary>
	public static class SceneGraphEntityExtensions
	{
		/// <summary>
		/// Allows progress capturing of the provided entity.
		/// The progress reporter will be notified of each update when the initialize function is called.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="progressReporter"></param>
		public static void CaptureInitializeProgress(this ISceneGraphEntityInitializeProgressReporter entity, ProgressReporter progressReporter)
		{
			EventHandler<int> func = null;
			func = (s, p) =>
			{
				progressReporter.SetProgress(p);
				// once finished, remove the progress reporter
				if (p == 100)
					entity.InitializeProgress -= func;
			};
			entity.InitializeProgress += func;
		}
	}
}