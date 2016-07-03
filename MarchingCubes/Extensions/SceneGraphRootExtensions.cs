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
		/// When called will add a temporary loading scene to the <see cref="SceneGraphRoot"/>.
		/// The progress scene will automatically update remove itself when the Initialize function of the scene is finished.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="scene">The actual scene that has heavy loading in its Initialize function.</param>
		/// <param name="renderContext"></param>
		public static void AddAsyncWithLoadingScreen<TScene>(this SceneGraphRoot root, TScene scene, IRenderContext renderContext) where TScene : SceneGraphEntity, ISceneGraphEntityInitializeProgressReporter
		{
			var progressScene = new LoadingProgressScene(renderContext);
			EventHandler<int> func = null;
			func = (s, p) =>
			{
				progressScene.SetProgress(p);
				// remove once completed
				if (p == 100)
					scene.InitializeProgress -= func;
			};

			scene.InitializeProgress += func;
			// add the progress reporter which is listening in
			root.AddAsync(progressScene);
			root.AddAsync(scene);
		}
	}
}