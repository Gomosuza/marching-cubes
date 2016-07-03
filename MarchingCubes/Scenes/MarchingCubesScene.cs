using MarchingCubes.Data;
using MarchingCubes.Extensions;
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
	public class MarchingCubesScene : SceneGraphEntity, ISceneGraphEntityInitializeProgressReporter
	{
		private ICamera _camera;
		private readonly IRenderContext _renderContext;
		private readonly string _dataPath;

		private Mesh _dataMesh;
		private Brush _solidColorBrush;
		private Pen _pen;
		private bool _firstUpdate;

		/// <summary>
		/// Creates a new marching cubes scene instance and tells the scene which data to load.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="dataPath"></param>
		public MarchingCubesScene(IRenderContext renderContext, string dataPath)
		{
			_renderContext = renderContext;
			_dataPath = dataPath;
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
			_camera = new FirstPersonCamera(_renderContext.GraphicsDevice, new Vector3(0, 100, 0));
			_camera.AddHorizontalRotation(MathHelper.ToRadians(90 + 45));
			_solidColorBrush = new SolidColorBrush(Color.Green);
			_pen = new SolidColorPen(Color.Black);

			var mriData = _renderContext.Content.LoadWithAttributeParser<ZippedMriData>(_dataPath);

			var meshBuilder = new TextureMeshDescriptionBuilder();

			// now that we know the min/max, find all values > avg and add cubes for now
			// first test, if we just generate a box per datapoint (6 sides * 6 vertices) we get out of memory exception
			// for now we just limit 
			var limit = 170;
			int lastProgress = 0;

			var bbox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
			var textureScale = Vector2.One;
			for (int z = 0; z < mriData.ZLength; z++)
			{
				for (int y = 0; y < mriData.YLength; y++)
				{
					var progress = (y + z * mriData.YLength) / (float)(mriData.YLength * mriData.ZLength);
					var newProgress = (int)(progress * 100);
					if (newProgress >= 100)
						newProgress = 99; // don't send 100 yet, send it only once at the end of the method
					if (newProgress != lastProgress)
					{
						var p = InitializeProgress;
						p?.Invoke(this, newProgress);
						lastProgress = newProgress;
					}
					for (int x = 0; x < mriData.XLength; x++)
					{
						var value = mriData[x, y, z];
						if (value > limit)
						{
							bbox.Min.X = x;
							bbox.Min.Y = y;
							bbox.Min.Z = z;
							bbox.Max.X = x + 1;
							bbox.Max.Y = z + 1;
							bbox.Max.Z = z + 1;
							meshBuilder.AddBox(bbox, textureScale);
						}
					}
				}
			}
			_dataMesh = _renderContext.MeshCreator.CreateMesh(meshBuilder);

			var visualizer = new MarchingCubeVisualizer(_renderContext, mriData, _camera);
			AddAsync(visualizer);

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
			var center = new Point(_renderContext.GraphicsDevice.Viewport.Width / 2, _renderContext.GraphicsDevice.Viewport.Height / 2);
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
			_camera.Move(movement);
		}

		private void CenterCursor()
		{
			Mouse.SetPosition(_renderContext.GraphicsDevice.Viewport.Width / 2, _renderContext.GraphicsDevice.Viewport.Height / 2);
		}

		/// <summary>
		/// Draws the marching cubes scene.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			_renderContext.DrawMesh(_dataMesh, Matrix.Identity, _camera.View, _camera.Projection, _solidColorBrush, _pen);

			base.Draw(gameTime);
		}
	}
}