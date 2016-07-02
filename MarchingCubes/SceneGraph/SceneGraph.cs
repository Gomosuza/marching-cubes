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

		/// <summary>
		/// Adds the entity the scenegraph.
		/// Note that the item is not added directly but rather the next time update is called.
		/// Entities that are not yet initialized are also further delayed (until initialize has executed on a background thread).
		/// Due to this, entities are not guaranteed to be added in same order as method was called (e.g. a not yet initialized entity is added first
		/// and needs 3s to initialize on the background thread. In the meantime entities that are already Initialized can be added directly).
		/// To prevent this issue, never mix initialized and non-initialized entities. Both categories are guaranteed to be added in order, UNLESS they are mixed.
		/// This method is threadsafe.
		/// </summary>
		/// <param name="child"></param>
		public override void AddScheduled(ISceneGraphEntity child)
		{
			// explicitely call find root node on a scenegraph, this will set properly set root either to itself or (if it is a nested scene graph) to the parent scene graph
			FindAndSetRootNode();
			base.AddScheduled(child);
		}
	}
}