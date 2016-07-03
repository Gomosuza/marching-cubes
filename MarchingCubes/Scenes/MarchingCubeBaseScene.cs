using MarchingCubes.Data;
using MarchingCubes.Extensions;
using MarchingCubes.SceneGraph;
using Renderer;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// Base scene that contains code common to all marching cube scenes.
	/// </summary>
	public abstract class MarchingCubeBaseScene : SceneGraphEntity
	{
		private readonly string _dataPath;

		/// <summary>
		/// The input data that was loaded from the file.
		/// </summary>
		protected IInputData InputData;

		/// <summary>
		/// Creates a new instance of the base scene.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="dataPath"></param>
		protected MarchingCubeBaseScene(IRenderContext renderContext, string dataPath)
		{
			_dataPath = dataPath;
			RenderContext = renderContext;
		}

		/// <summary>
		/// The rendercontext that can be used to render with this class.
		/// </summary>
		public IRenderContext RenderContext { get; }

		/// <summary>
		/// Loads the input data from disk.
		/// </summary>
		public override void Initialize()
		{
			InputData = RenderContext.Content.LoadWithAttributeParser<ZippedMriData>(_dataPath);
		}
	}
}