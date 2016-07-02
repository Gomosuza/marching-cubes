using Microsoft.Xna.Framework;

namespace MarchingCubes.SceneGraph
{
	public interface ISceneGraphEntity : IGameComponent, IUpdateable, IDrawable
	{
		/// <summary>
		/// Gets the name of the current <see cref="ISceneGraphEntity"/>.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the parent entity of the current one.
		/// </summary>
		ISceneGraphEntity Parent { get; }

		bool Initialized { get; }

		/// <summary>
		/// Call to find out if the specific entity is already registered as a child of the current node.
		/// Note that the *Scheduled functions will not execute directly and it may take a frame (or two) for an entity to actually be registered.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="searchEntireTree">Defaults to false. If false will only check the direct childrend of this entity. If true will search the entire tree until all leafes are reached.</param>
		/// <returns>True if the entity is registered, false otherwise.</returns>
		bool IsRegistered(ISceneGraphEntity entity, bool searchEntireTree = false);
	}
}