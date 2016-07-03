using MarchingCubes.Threading;
using System;

namespace MarchingCubes.SceneGraph
{
	/// <summary>
	/// The root implementation of a scene graph.
	/// It allows to add entities to itself  (as children) that in turn will be rendered and updated.
	/// The scene graph is itself a <see cref="SceneGraphEntity"/> and can be added to other scene graphs.
	/// Circular references are not checked and will result in a <see cref="StackOverflowException"/>.
	/// The scene graph root handles loading of all its child entities (if a non-initialized entity is added, initialize is called in a background thread
	/// and the entitiy will only be added to the update/draw collection once finished).
	/// </summary>
	public class SceneGraphRoot : SceneGraphEntity
	{
		private readonly GenericBackgroundWorker<SceneGraphEntity> _sceneGraphObjectInitializer;

		/// <summary>
		/// Creates a new scenegraph root.
		/// </summary>
		public SceneGraphRoot()
		{
			_sceneGraphObjectInitializer = new GenericBackgroundWorker<SceneGraphEntity>(e => e.Initialize());
			Initialized = true;
			FindAndSetRootNode();
		}

		/// <summary>
		/// Scenegraph internal helper that calls initialize on the child in a background thread and runs the action after initialization.
		/// This function returns immediately, the actual initialization of the child is thus deferred.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="action"></param>
		internal void InitializeInBackground(SceneGraphEntity child, Action<SceneGraphEntity> action)
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