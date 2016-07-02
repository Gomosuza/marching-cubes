using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Renderer;
using Renderer.Brushes;
using Renderer.Extensions;
using Renderer.Meshes;
using Renderer.Pens;
using System.Runtime.InteropServices;

namespace MarchingCubes
{
	/// <summary>
	/// The main scene that will show the result of the marching cube algorithm.
	/// </summary>
	public class MarchingCubesScene : SceneGraphEntity
	{
		private readonly GameWindow _window;
		private readonly GraphicsDevice _device;
		private ICamera _camera;
		private readonly IRenderContext _renderContext;

		private Mesh _cameraTestMesh;
		private Brush _solidColorBrush;
		private Pen _pen;
		private bool _firstUpdate;

		public MarchingCubesScene(IGraphicsDeviceService graphicsDeviceService, ContentManager content, GameWindow window)
		{
			_window = window;
			_device = graphicsDeviceService.GraphicsDevice;
			_renderContext = new DefaultRenderContext(graphicsDeviceService, content);
		}

		public override void Initialize()
		{
			_camera = new FirstPersonCamera(_device, new Vector3(0, 0, 50));
			_solidColorBrush = new SolidColorBrush(Color.Green);
			_pen = new Pen(Color.Black);

			var desc = new TextureMeshDescriptionBuilder();
			desc.AddBox(new BoundingBox(-Vector3.One, Vector3.One), Vector2.One);
			_cameraTestMesh = _renderContext.MeshCreator.CreateMesh(desc);
			_firstUpdate = true;
		}

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
				return;
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
			// the monogame implementation (Mouse.SetPosition) doesn't seem to play too nice (sometimes input seems slugish, possibly because mouse events are skipped)
			var rp = _window.ClientBounds;
			SetCursorPos(rp.X + _device.Viewport.Width / 2, rp.Y + _device.Viewport.Height / 2);
		}

		[DllImport("User32.dll")]
		private static extern bool SetCursorPos(int x, int y);

		public override void Draw(GameTime gameTime)
		{
			_renderContext.Attach();
			_renderContext.Clear(Color.White);

			_renderContext.DrawMesh(_cameraTestMesh, Matrix.Identity, _camera.View, _camera.Projection, _solidColorBrush, _pen);
			_renderContext.Detach();

			base.Draw(gameTime);
		}
	}
}