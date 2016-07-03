using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Meshes;
using Renderer.Pens;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// The visualizer provides a nice realtime visualization of the inner workings of the marching cube alogrithm while it is assembling the result.
	/// </summary>
	public class MarchingCubeVisualizer : SceneGraphEntity
	{
		private readonly IRenderContext _renderContext;
		private readonly IInputData _inputData;
		private readonly ICamera _camera;
		private Mesh _mesh;
		private Pen _pen;

		/// <summary>
		/// Creates a new instance of the visualizer.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="inputData"></param>
		/// <param name="camera"></param>
		public MarchingCubeVisualizer(IRenderContext renderContext, IInputData inputData, ICamera camera)
		{
			_renderContext = renderContext;
			_inputData = inputData;
			_camera = camera;
		}

		/// <summary>
		/// Initializes the visualizer by creating a wireframe mesh.
		/// </summary>
		public override void Initialize()
		{
			var builder = new LineMeshDescriptionBuilder();
			var bbox = new BoundingBox(Vector3.Zero, new Vector3(_inputData.XLength, _inputData.YLength, _inputData.ZLength));
			builder.AddBox(bbox, Color.Black);
			_mesh = _renderContext.MeshCreator.CreateMesh(builder);

			_pen = new VertexColorPen(CullMode.None);

			Initialized = true;
		}

		/// <summary>
		/// Draws the marching cubes visualizer.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			_renderContext.DrawMesh(_mesh, Matrix.Identity, _camera.View, _camera.Projection, null, _pen);
			base.Draw(gameTime);
		}
	}
}