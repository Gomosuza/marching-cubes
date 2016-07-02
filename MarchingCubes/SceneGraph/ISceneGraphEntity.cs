using Microsoft.Xna.Framework;

namespace MarchingCubes.SceneGraph
{
	public interface ISceneGraphEntity : IGameComponent, IUpdateable, IDrawable
	{
		/// <summary>
		/// Gets the parent entity of the current one.
		/// </summary>
		ISceneGraphEntity Parent { get; }

		/// <summary>
		/// Internal, sets the <see cref="Parent"/> property to the provided value.
		/// </summary>
		/// <param name="parent"></param>
		void SetParent(ISceneGraphEntity parent);

		bool Initialized { get; }
	}
}