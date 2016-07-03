using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Renderer;
using Renderer.Brushes;
using Renderer.Extensions;
using Renderer.Meshes;
using Renderer.Pens;
using System;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// The main scene that will show the result of the marching cube algorithm.
	/// </summary>
	public class MarchingCubesScene : MarchingCubeBaseScene, ISceneGraphEntityInitializeProgressReporter
	{
		private ICamera _camera;

		private Mesh _dataMesh;
		private Brush _solidColorBrush;
		private Pen _pen;
		private bool _firstUpdate;

		/// <summary>
		/// Creates a new marching cubes scene instance and tells the scene which data to load.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="dataPath"></param>
		public MarchingCubesScene(IRenderContext renderContext, string dataPath) : base(renderContext, dataPath)
		{
		}

		/// <summary>
		/// The progress reporter which will be fired with values between 0-100.
		/// If this is implemented, <see cref="ISceneGraphEntityInitializeProgressReporter.InitializeProgress"/> must be called at least once with a value of 100 (progress completed).
		/// </summary>
		public event EventHandler<int> InitializeProgress;

		/// <summary>
		/// Initializes the marching cubes scene by loading the dataset from disk and creating a mesh for it.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			_camera = new FirstPersonCamera(RenderContext.GraphicsDevice, new Vector3(0, 100, 0));
			_camera.AddHorizontalRotation(MathHelper.ToRadians(90 + 45));
			_solidColorBrush = new SolidColorBrush(Color.Green);
			_pen = new SolidColorPen(Color.Black);

			var triangleBuilder = new TriangleMeshDescriptionBuilder();
			// isolevel defines which points are inside/outside the structure
			var isolevel = 128;
			int lastProgress = 0;

			// for each point we will at all 8 points forming a cube, we will simply take the index + 1 in each direction, thus our iteration counters must be reduced by 1 to prevent out of bound exception
			int xlen = InputData.XLength - 1;
			int ylen = InputData.YLength - 1;
			int zlen = InputData.ZLength - 1;

			var mcAlgo = new MarchingCubesAlgorithm();
			var box = new BoundingBox();
			for (int x = 0; x < xlen; x++)
			{
				for (int y = 0; y < ylen; y++)
				{
					// report progress here as one dimension by itself would progress really fast (too small increments)
					var progress = (y + x * ylen) / (float)(ylen * xlen);
					var newProgress = (int)(progress * 100);
					if (newProgress >= 100)
						newProgress = 99; // don't send 100 yet, send it only once at the end of the method
					if (newProgress != lastProgress)
					{
						var p = InitializeProgress;
						p?.Invoke(this, newProgress);
						lastProgress = newProgress;
					}
					for (int z = 0; z < zlen; z++)
					{
						box.Min.X = x;
						box.Min.Y = y;
						box.Min.Z = z;
						box.Max.X = x + 1;
						box.Max.Y = y + 1;
						box.Max.Z = z + 1;
						var vertices = mcAlgo.Polygonize(InputData, isolevel, box);
						if (vertices != null && vertices.Count > 0)
							triangleBuilder.Vertices.AddRange(vertices);
					}
				}
			}
			_dataMesh = RenderContext.MeshCreator.CreateMesh(triangleBuilder);

			_firstUpdate = true;

			var i = InitializeProgress;
			i?.Invoke(this, 100);
			InitializeProgress = null;
			Initialized = true;
		}

		/// <summary>
		/// Updates the marching cubes scene.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			_camera.Update(gameTime);

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

			_camera.AddHorizontalRotation(diff.X * t * factor);
			_camera.AddVerticalRotation(diff.Y * t * factor);

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
			_camera.Move(movement);
		}

		private void CenterCursor()
		{
			Mouse.SetPosition(RenderContext.GraphicsDevice.Viewport.Width / 2, RenderContext.GraphicsDevice.Viewport.Height / 2);
		}

		/// <summary>
		/// Draws the marching cubes scene.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			RenderContext.DrawMesh(_dataMesh, Matrix.Identity, _camera.View, _camera.Projection, _solidColorBrush, _pen);

			base.Draw(gameTime);
		}
	}
}