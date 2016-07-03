using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarchingCubes.SceneGraph
{
	/// <summary>
	/// Base implementation of a scene graph entity which can contain children.
	/// </summary>
	public abstract class SceneGraphEntity : IGameComponent, IDrawable, IUpdateable
	{
		private readonly List<SceneGraphEntity> _sceneGraphEntities;
		private readonly List<SceneGraphEntity> _sceneGraphEntitiesToBeAdded;
		private readonly List<SceneGraphEntity> _sceneGraphEntitiesToBeRemoved;

		private SceneGraphRoot _root;
		private bool _parentSet;
		private string _name;

		/// <summary>
		/// Creates a new scenegraph entity.
		/// </summary>
		protected SceneGraphEntity()
		{
			Visible = Enabled = true;
			_sceneGraphEntities = new List<SceneGraphEntity>();
			_sceneGraphEntitiesToBeRemoved = new List<SceneGraphEntity>();
			_sceneGraphEntitiesToBeAdded = new List<SceneGraphEntity>();
		}

		/// <summary>
		/// Not used.
		/// </summary>
		[Obsolete("Monogame artefact, will never be used")]
		public int UpdateOrder => 0;

		/// <summary>
		/// Not used.
		/// </summary>
		[Obsolete("Monogame artefact, will never be used")]
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
		/// The name of the current scenegraph entity.
		/// Mainly used for debugging purposes. If not set will return type name.
		/// </summary>
		public string Name
		{
			get { return _name ?? GetType().FullName; }
			set { _name = value; }
		}

		/// <summary>
		/// Returns the parent of the current entity.
		/// This property will be set automatically when the entity is added to a scene graph.
		/// </summary>
		public SceneGraphEntity Parent { get; private set; }

		/// <summary>
		/// Indicates whether this entity was already initialized.
		/// </summary>
		public bool Initialized { get; protected set; }

		/// <summary>
		/// Not used, do not hook.
		/// </summary>
		[Obsolete("Monogame artefact, will never be called")]
		public event EventHandler<EventArgs> EnabledChanged;

		/// <summary>
		/// Not used, do not hook.
		/// </summary>
		[Obsolete("Monogame artefact, will never be called")]
		public event EventHandler<EventArgs> UpdateOrderChanged;

		/// <summary>
		/// Not used, do not hook.
		/// </summary>
		[Obsolete("Monogame artefact, will never be called")]
		public event EventHandler<EventArgs> DrawOrderChanged;

		/// <summary>
		/// Not used, do not hook.
		/// </summary>
		[Obsolete("Monogame artefact, will never be called")]
		public event EventHandler<EventArgs> VisibleChanged;

		/// <summary>
		/// Sets the parent of the child entity.
		/// Will also navigate up the tree of the parent to find the root.
		/// If a null parent is provided the function will "unset" the parent.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="parent">If null, will unset the parent, otherwise will set it. Note that a parent can only be set if it has not yet been set before.</param>
		private static void SetParent(SceneGraphEntity child, SceneGraphEntity parent)
		{
			if (parent != null && child._parentSet)
				throw new NotSupportedException("parent has already been set for the provided node. Remove the node from the scene graph that it is attached to and then try to add it again.");

			if (parent != null)
			{
				child.Parent = parent;
				child.FindAndSetRootNode();
				child._parentSet = true;
			}
			else
			{
				child.Parent = null;
				child._root = null;
				child._parentSet = false;
			}
		}

		/// <summary>
		/// Looks for the scenegraph this node is attached to by going up the parent chain until a scene graph is found.
		/// </summary>
		protected void FindAndSetRootNode()
		{
			SceneGraphEntity root = this;
			while (!(root is SceneGraphRoot))
			{
				root = root.Parent;
				// don't look for the ultimate root, just look for the next highest root node
				// this allows each root to run its own scheduler for all its children
				//while (root is SceneGraph && root.Parent != null)
				//{
				//	// keep going, we support nested scene graphs
				//	root = root.Parent;
				//}
			}
			if (root == null)
			{
				throw new NotSupportedException("Could not find the root of the provided parent.");
			}

			_root = (SceneGraphRoot)root;
		}

		/// <summary>
		/// Initializes the current entity. This should only be called if <see cref="Initialized"/> is false.
		/// If a non-initialized entity is added to a scenegraph, the scenegraph will automatically initialize the entity
		/// on a background thread before adding it to the update/render loop.
		/// </summary>
		public virtual void Initialize()
		{
			Initialized = true;
		}

		/// <summary>
		/// Updates the scenegraph entity and all its children given that it is <see cref="Enabled"/>.
		/// </summary>
		/// <param name="gameTime"></param>
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
						SetParent(e, null);
						_sceneGraphEntities.Remove(e);
					}
					_sceneGraphEntitiesToBeRemoved.Clear();
				}
			}
		}

		/// <summary>
		/// Renderes the scenegraph entity and all its children, given that it is <see cref="Visible"/>.
		/// </summary>
		/// <param name="gameTime"></param>
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
		/// The initialize thread is executed on the first parent scenegraph root that is found.
		/// Due to this, entities are not guaranteed to be added in same order as method was called (e.g. a not yet initialized entity is added first
		/// and needs 3s to initialize on the background thread. In the meantime entities that are already Initialized can be added directly).
		/// To prevent this issue, never mix initialized and non-initialized entities. Both categories are guaranteed to be added in order, UNLESS they are mixed.
		/// This method is threadsafe.
		/// </summary>
		/// <param name="child"></param>
		public virtual void AddAsync(SceneGraphEntity child)
		{
			SetParent(child, this);
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
		public void RemoveAsync(SceneGraphEntity child)
		{
			// note that the child is not removed directly but rather scheduled in another list, see UpdateCollection method for reason
			_sceneGraphEntitiesToBeRemoved.Add(child);
		}

		/// <summary>
		/// Due to the way the Add/Remove scheduled methods work it is not possible to call entity.RemoveScheduled(a) and entity.AddScheduled(a) within the same update.
		/// Either one waits at least one update between the 2 calls, or uses this method which will set the child to the new parent.
		/// This method is threadsafe.
		/// </summary>
		/// <param name="newParent"></param>
		public void ChangeParent(SceneGraphEntity newParent)
		{
			if (newParent == null)
				throw new ArgumentNullException(nameof(newParent), "if you want to remove the entity from a scenegraph, call RemoveScheduled");

			if (!Initialized)
				throw new NotSupportedException("Changing parent is only possible if the entity was already Initialized");

			// even though initialized is set to true, it will take one more update before the entity is actually added to the scenegraph
			// assert that said update has occured, otherwise parent is not properly set
			if (!Parent.IsRegistered(this))
				throw new NotSupportedException("it takes at least one frame after the entity has been Initialized before the entity is actually registered with the parent.");

			if (Parent == null)
				throw new NotSupportedException("The current entity must have a parent, otherwise ChangingParent is invalid.");

			// allow override of parent because we are switching parent
			_parentSet = false;
			SetParent(this, newParent);
		}

		/// <summary>
		/// Call to find out if the specific entity is already registered as a child of the current node.
		/// Note that the *Scheduled functions will not execute directly and it may take a frame (or two) for an entity to actually be registered.
		/// This method is threadsafe.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="searchEntireTree">Defaults to false. If false will only check the direct childrend of this entity. If true will search the entire tree until all leafes are reached.</param>
		/// <returns>True if the entity is registered, false otherwise.</returns>
		public bool IsRegistered(SceneGraphEntity entity, bool searchEntireTree = false)
		{
			// create a copy of the list as this function might be called on a different thread and each update can change the source collection
			var entities = _sceneGraphEntities.ToList();
			if (entities.Contains(entity))
				return true;
			if (searchEntireTree)
			{
				foreach (var e in entities)
				{
					if (e.IsRegistered(entity, true))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets all direct children of the current scenegraph node.
		/// </summary>
		/// <param name="thisNodeOnly">Defaults to true. If true will only return the direct children. If false, will also return all children of children.</param>
		/// <returns></returns>
		public IReadOnlyList<SceneGraphEntity> GetChildScenes(bool thisNodeOnly = true)
		{
			if (thisNodeOnly)
				return _sceneGraphEntities.ToList();

			var current = _sceneGraphEntities.ToList();
			foreach (var e in current)
			{
				current.AddRange(e.GetChildScenes(false));
			}
			return current;
		}
	}
}