using MarchingCubes.Data;
using MarchingCubes.Extensions;
using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Renderer;
using Renderer.Brushes;
using Renderer.Extensions;
using Renderer.Meshes;
using Renderer.Pens;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// Base scene that contains code common to all marching cube scenes.
	/// </summary>
	public abstract class MarchingCubeBaseScene : SceneGraphEntity
	{
		private readonly string _dataPath;
		private Mesh _mesh;
		private Pen _pen;
		private bool _firstUpdate;

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
		/// The camera in control of this scene.
		/// </summary>
		public ICamera Camera { get; private set; }

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

			Camera = new FirstPersonCamera(RenderContext.GraphicsDevice, new Vector3(-100, 100, -100));
			Camera.AddHorizontalRotation(MathHelper.ToRadians(90 + 45));

			var builder = new LineMeshDescriptionBuilder();
			var bbox = new BoundingBox(Vector3.Zero, new Vector3(InputData.XLength, InputData.YLength, InputData.ZLength));
			builder.AddBox(bbox, Color.Black);
			_mesh = RenderContext.MeshCreator.CreateMesh(builder);

			_pen = new VertexColorPen(CullMode.None);
			_firstUpdate = true;
		}

		/// <summary>
		/// Updates the marching cubes scene.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			Camera.Update(gameTime);

			HandleInput(gameTime);
			base.Update(gameTime);
		}

		private void HandleInput(GameTime gameTime)
		{
			if (_firstUpdate)
			{
				_firstUpdate = false;
				CenterCursor();
			}
			var mouseState = Mouse.GetState();
			var center = new Point(RenderContext.GraphicsDevice.Viewport.Width / 2, RenderContext.GraphicsDevice.Viewport.Height / 2);
			var diff = mouseState.Position - center;

			var t = gameTime.GetElapsedSeconds();
			const float factor = 0.4f;

			Camera.AddHorizontalRotation(diff.X * t * factor);
			Camera.AddVerticalRotation(diff.Y * t * factor);

			CenterCursor();

			var keyboardState = Keyboard.GetState();

			var movement = Vector3.Zero;
			if (keyboardState.IsKeyDown(Keys.W))
			{
				movement += -Vector3.UnitZ;
			}
			if (keyboardState.IsKeyDown(Keys.A))
			{
				movement += -Vector3.UnitX;
			}
			if (keyboardState.IsKeyDown(Keys.S))
			{
				movement += Vector3.UnitZ;
			}
			if (keyboardState.IsKeyDown(Keys.D))
			{
				movement += Vector3.UnitX;
			}
			if (keyboardState.IsKeyDown(Keys.LeftShift))
				movement *= 4f;
			if (keyboardState.IsKeyDown(Keys.LeftControl))
				movement /= 4f;
			Camera.Move(movement);
		}

		private void CenterCursor()
		{
			Mouse.SetPosition(RenderContext.GraphicsDevice.Viewport.Width / 2, RenderContext.GraphicsDevice.Viewport.Height / 2);
		}

		/// <summary>
		/// Draws the base scene which includes the bounding box around the entire loaded dataset.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			DrawMesh(_mesh, null, _pen);

			base.Draw(gameTime);
		}

		/// <summary>
		/// Draws a mesh with the provided brush and pen using the current camera.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="brush"></param>
		/// <param name="pen"></param>
		/// <param name="world">Optional world matrix that can be applied to transform the object.</param>
		protected void DrawMesh(Mesh mesh, Brush brush, Pen pen, Matrix? world = null)
		{
			RenderContext.DrawMesh(mesh, world ?? Matrix.Identity, Camera.View, Camera.Projection, brush, pen);
		}
	}
}