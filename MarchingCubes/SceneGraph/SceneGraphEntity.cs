using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MarchingCubes.SceneGraph
{
	/// <summary>
	/// Base implementation of a scene graph entity which can contain children.
	/// </summary>
	public abstract class SceneGraphEntity : ISceneGraphEntity
	{
		private readonly List<ISceneGraphEntity> _sceneGraphEntities;
		private readonly List<ISceneGraphEntity> _sceneGraphEntitiesToBeAdded, _sceneGraphEntitiesToBeRemoved;

		private SceneGraph _root;
		private bool _parentSet;

		public int UpdateOrder => 0;

		protected SceneGraphEntity()
		{
			Visible = Enabled = true;
			_sceneGraphEntities = new List<ISceneGraphEntity>();
			_sceneGraphEntitiesToBeRemoved = new List<ISceneGraphEntity>();
			_sceneGraphEntitiesToBeAdded = new List<ISceneGraphEntity>();
		}

		public int DrawOrder => 0;

		/// <summary>
		/// Gets or sets whether the current node is visible or not.
		/// </summary>
		public bool Visible { get; set; }

		/// <summary>
		/// Gets or sets whether the current node is updated or not.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Returns the parent of the current entity.
		/// This property will be set automatically when the entity is added to a scene graph.
		/// </summary>
		public ISceneGraphEntity Parent { get; private set; }

		/// <summary>
		/// Indicates whether this entity was already initialized.
		/// </summary>
		public bool Initialized { get; protected set; }

		public event EventHandler<EventArgs> EnabledChanged;

		public event EventHandler<EventArgs> UpdateOrderChanged;

		public event EventHandler<EventArgs> DrawOrderChanged;

		public event EventHandler<EventArgs> VisibleChanged;

		/// <summary>
		/// Sets the parent of the current entity.
		/// Will also navigate up the tree of the parent to find the root.
		/// </summary>
		/// <param name="parent"></param>
		public void SetParent(ISceneGraphEntity parent)
		{
			if (_parentSet)
				throw new NotSupportedException("parent has already been set for the current node.");

			Parent = parent;
			FindAndSetRootNode();
			_parentSet = true;
		}

		protected void FindAndSetRootNode()
		{
			ISceneGraphEntity root = this;
			while (!(root is SceneGraph))
			{
				root = Parent;
				while (root is SceneGraph && root.Parent != null)
				{
					// keep going, we support nested scene graphs
					root = root.Parent;
				}
			}
			if (root == null)
			{
				throw new NotSupportedException("Could not find the root of the provided parent.");
			}

			_root = (SceneGraph)root;
		}

		public virtual void Initialize()
		{

		}

		public virtual void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;
			// add/remove entities
			UpdateCollection();

			foreach (var e in _sceneGraphEntities)
			{
				if (e.Enabled)
					e.Update(gameTime);
			}
		}

		/// <summary>
		/// Call to alter the <see cref="_sceneGraphEntities"/> by adding entities from <see cref="_sceneGraphEntitiesToBeAdded"/>
		/// and removing entitites that are listed in <see cref="_sceneGraphEntitiesToBeRemoved"/>.
		/// This method is not thread safe.
		/// </summary>
		private void UpdateCollection()
		{
			// check before using the lock, in most updates it will be empty, so no need to apply the hefty lock
			if (_sceneGraphEntitiesToBeAdded.Count > 0)
			{
				// at least one entitity to be added, lock and update
				lock (_sceneGraphEntitiesToBeAdded)
				{
					// no need to check count of list again as no one (except for us) takes items out of this list (and we are in a lock)

					// copy entire list to our actual list
					_sceneGraphEntities.AddRange(_sceneGraphEntitiesToBeAdded);
					// clear the temp collection
					_sceneGraphEntitiesToBeAdded.Clear();
				}
			}

			// same for entities to be removed
			if (_sceneGraphEntitiesToBeRemoved.Count > 0)
			{
				lock (_sceneGraphEntitiesToBeRemoved)
				{
					// no need to check count of list again as no one (except for us) takes items out of this list (and we are in a lock)
					foreach (var e in _sceneGraphEntitiesToBeRemoved)
					{
						_sceneGraphEntities.Remove(e);
					}
					_sceneGraphEntitiesToBeRemoved.Clear();
				}
			}
		}

		public virtual void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			foreach (var e in _sceneGraphEntities)
			{
				if (e.Visible)
					e.Draw(gameTime);
			}
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
		public virtual void AddScheduled(ISceneGraphEntity child)
		{
			child.SetParent(this);
			// note that the child is not added directly but rather to another list, see UpdateCollection method for reason
			if (child.Initialized)
			{
				// already initialized, add right away
				lock (_sceneGraphEntitiesToBeAdded)
				{
					_sceneGraphEntitiesToBeAdded.Add(child);
				}
			}
			else
			{
				// need to initialize first, add when callback is executed
				// the initialize background worker guarantees us the same order as we called it, so if no entity is initialized yet they are all scheduled on the worker thread in order
				// and end up being added in the same order
				_root.InitializeInBackground(child, e =>
				{
					lock (_sceneGraphEntitiesToBeAdded)
					{
						_sceneGraphEntitiesToBeAdded.Add(e);
					}
				});
			}
		}

		/// <summary>
		/// Removes the entity from the scenegraph.
		/// Note that the item is not removed immediately but rather the next time update is called.
		/// This method is threadsafe.
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public void RemoveScheduled(ISceneGraphEntity child)
		{
			// note that the child is not removed directly but rather scheduled in another list, see UpdateCollection method for reason
			_sceneGraphEntitiesToBeRemoved.Add(child);
		}
	}
}