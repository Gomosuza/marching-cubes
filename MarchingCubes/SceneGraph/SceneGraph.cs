using MarchingCubes.Threading;
using System;

namespace MarchingCubes.SceneGraph
{
	/// <summary>
	/// The root implementation of a scene graph.
	/// It allows to add entities to itself  (as children) that in turn will be rendered and updated.
	/// The scene graph is itself a <see cref="SceneGraphEntity"/> and can be added to other scene graphs.
	/// Circular references are not checked and will result in a <see cref="StackOverflowException"/>.
	/// </summary>
	public class SceneGraph : SceneGraphEntity
	{
		private readonly GenericBackgroundWorker<ISceneGraphEntity> _sceneGraphObjectInitializer;

		public SceneGraph()
		{
			_sceneGraphObjectInitializer = new GenericBackgroundWorker<ISceneGraphEntity>(e => e.Initialize());
			Initialized = true;
			FindAndSetRootNode();
		}

		/// <summary>
		/// Scenegraph internal helper that calls initialize on the child in a background thread and runs the action after initialization.
		/// This function returns immediately, the actual initialization of the child is thus deferred.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="action"></param>
		internal void InitializeInBackground(ISceneGraphEntity child, Action<ISceneGraphEntity> action)
		{
			if (child == null)
				throw new ArgumentNullException(nameof(child));
			if (child.Initialized)
			{
				// no need to call initialize again, just call the function right away
				action(child);
			}
			_sceneGraphObjectInitializer.QueueItem(child, action);
		}
	}
}