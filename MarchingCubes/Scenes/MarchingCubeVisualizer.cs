using MarchingCubes.Scenes.Visualizer;
using Microsoft.Xna.Framework;
using Renderer;
using Renderer.Brushes;
using Renderer.Pens;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// The visualizer provides a nice realtime visualization of the inner workings of the marching cube alogrithm while it is assembling the result.
	/// This visualizer will slowly visualize the workings of the marching cubes algorithm by:
	/// - Selected cube by cube and checking if the cube intersects with the dataset
	/// - if so, creating the appropriate triangles and rendering before moving on
	/// 
	/// For improved visuals the generation occurs on a background thread and can thus be "live debugged" while the mesh is being built.
	/// The algorithm will also fast-skip over empty rows and slices.
	/// </summary>
	public class MarchingCubeVisualizer : MarchingCubeBaseScene
	{
		private VisualizerBackgroundWorker _marchingCubesWorker;
		private Brush _finsihedMeshBrush;
		private Pen _finishedMeshPen;

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

			_finishedMeshPen = new SolidColorPen(Color.Black);
			_finsihedMeshBrush = new SolidColorBrush(Color.Green);

			// isolevel defines which points are inside/outside the structure
			var isolevel = 128;
			_marchingCubesWorker = new VisualizerBackgroundWorker(RenderContext, InputData, isolevel);
			var visualizer = new ActiveCubeVisualizer(RenderContext, InputData, _marchingCubesWorker, Camera);
			visualizer.Initialize();
			AddAsync(visualizer);
			Initialized = true;
		}

		/// <summary>
		/// Draws the visualizer, this includes the current cube being stepped as well as the mesh being modified.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			DrawMesh(_marchingCubesWorker.GetMeshToDraw(), _finsihedMeshBrush, _finishedMeshPen);

			base.Draw(gameTime);
		}
	}
}