using MarchingCubes.SceneGraph;
using MarchingCubes.Scenes;
using Renderer;
using System;

namespace MarchingCubes.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="SceneGraphRoot"/>.
	/// </summary>
	public static class SceneGraphRootExtensions
	{
		/// <summary>
		/// When called will add a temporary loading scene to the <see cref="SceneGraphRoot"/>
		/// and returns the progress reporter that must be called to animate the progress scene.
		/// The progress scene will automatically remove itself when the progress reporter is called with progress 100.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="renderContext"></param>
		/// <returns>The progress reporter that will visualize the progress scene.</returns>
		public static ProgressReporter AddLoadingScene(this SceneGraphRoot root, IRenderContext renderContext)
		{
			var progressScene = new LoadingProgressScene(renderContext);
			var progress = new ProgressReporter();
			EventHandler<int> func = null;
			func = (s, p) =>
			{
				progressScene.SetProgress(p);
				if (p == 100)
					((ProgressReporter)s).ProgressReported -= func;
			};
			progress.ProgressReported += func;
			// add the progress reporter which is listening in
			root.AddAsync(progressScene);

			return progress;
		}
	}
}