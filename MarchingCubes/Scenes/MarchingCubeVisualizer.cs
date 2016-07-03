using Renderer;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// The visualizer provides a nice realtime visualization of the inner workings of the marching cube alogrithm while it is assembling the result.
	/// </summary>
	public class MarchingCubeVisualizer : MarchingCubeBaseScene
	{
		/// <summary>
		/// Creates a new instance of the visualizer.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="file"></param>
		public MarchingCubeVisualizer(IRenderContext renderContext, string file) : base(renderContext, file)
		{

		}

		/// <summary>
		/// Initializes the visualizer by creating a wireframe mesh.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			Initialized = true;
		}
	}
}