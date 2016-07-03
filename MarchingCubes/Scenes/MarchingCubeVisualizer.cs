using MarchingCubes.RendererExtensions;
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
	public class MarchingCubeVisualizer : MarchingCubeBaseScene
	{
		private Mesh _mesh;
		private Pen _pen;
		private ICamera _camera;

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
			_camera = new FirstPersonCamera(RenderContext.GraphicsDevice, new Vector3(0, 100, 0));
			_camera.AddHorizontalRotation(MathHelper.ToRadians(90 + 45));

			var builder = new LineMeshDescriptionBuilder();
			var bbox = new BoundingBox(Vector3.Zero, new Vector3(InputData.XLength, InputData.YLength, InputData.ZLength));
			builder.AddBox(bbox, Color.Black);
			_mesh = RenderContext.MeshCreator.CreateMesh(builder);

			_pen = new VertexColorPen(CullMode.None);

			Initialized = true;
		}

		/// <summary>
		/// Draws the marching cubes visualizer.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			RenderContext.DrawMesh(_mesh, Matrix.Identity, _camera.View, _camera.Projection, null, _pen);
			base.Draw(gameTime);
		}
	}
}