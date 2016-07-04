using MarchingCubes.Scenes.Visualizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Renderer;
using Renderer.Brushes;
using Renderer.Extensions;
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
		private bool _paused;
		private Texture2D _pixel;
		private KeyboardState _lastKeyboardState;
		private MouseState _lastMouse;
		private ActiveCubeVisualizer _visualizer;

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

			_pixel = new Texture2D(RenderContext.GraphicsDevice, 1, 1);
			_pixel.SetData(new[] { Color.White });
			// isolevel defines which points are inside/outside the structure
			var isolevel = 128;
			_marchingCubesWorker = new VisualizerBackgroundWorker(RenderContext, InputData, isolevel);
			_visualizer = new ActiveCubeVisualizer(RenderContext, InputData, _marchingCubesWorker, Camera);
			_visualizer.Initialize();
			AddAsync(_visualizer);
			Initialized = true;
		}

		/// <summary>
		/// Updates the visualizer.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			var kb = Keyboard.GetState();
			var mouse = Mouse.GetState();
			// allow the user to pause the algorithm to inspect the elements
			if (kb.IsKeyDown(Keys.Space) && _lastKeyboardState.IsKeyUp(Keys.Space))
			{
				_paused = !_paused;
				_visualizer.SetPause(_paused);
			}
			if (mouse.RightButton == ButtonState.Pressed && _lastMouse.RightButton == ButtonState.Released)
			{
				ToggleCameraAttach();
			}

			if ((kb.IsKeyDown(Keys.Enter) && _lastKeyboardState.IsKeyUp(Keys.Enter)) ||
				(kb.IsKeyDown(Keys.E) && _lastKeyboardState.IsKeyUp(Keys.E)))
			{
				_visualizer.Step();
			}

			_lastKeyboardState = kb;
			_lastMouse = mouse;
			base.Update(gameTime);
		}

		/// <summary>
		/// Draws the visualizer, this includes the current cube being stepped as well as the mesh being modified.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			DrawMesh(_marchingCubesWorker.GetMeshToDraw(), _finsihedMeshBrush, _finishedMeshPen);

			base.Draw(gameTime);

			if (_paused)
			{
				// draw border to indicate pause
				RenderContext.DrawTexture(_pixel, new Rectangle(0, 0, RenderContext.GraphicsDevice.Viewport.Width, 1), Color.Orange);
				RenderContext.DrawTexture(_pixel, new Rectangle(0, 0, 1, RenderContext.GraphicsDevice.Viewport.Height), Color.Orange);
				RenderContext.DrawTexture(_pixel, new Rectangle(RenderContext.GraphicsDevice.Viewport.Width - 1, 0, RenderContext.GraphicsDevice.Viewport.Width, 1), Color.Orange);
				RenderContext.DrawTexture(_pixel, new Rectangle(0, RenderContext.GraphicsDevice.Viewport.Height - 1, 1, RenderContext.GraphicsDevice.Viewport.Height), Color.Orange);
			}
		}
	}
}