using MarchingCubes.Extensions;
using MarchingCubes.SceneGraph;
using MarchingCubes.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Renderer;

namespace MarchingCubes
{
	/// <summary>
	/// Monogame implementation, only used to setup the scenegraph.
	/// </summary>
	public class Game : Microsoft.Xna.Framework.Game
	{
		private readonly GraphicsDeviceManager _graphicsDeviceManager;
		private IRenderContext _renderContext;

		/// <summary>
		/// Default ctor.
		/// </summary>
		public Game()
		{
			_graphicsDeviceManager = new GraphicsDeviceManager(this);
		}

		/// <summary>
		/// Sets up scenegraph and loads marching cube data.
		/// </summary>
		protected override void Initialize()
		{
			Window.Title = "Marching cubes demo - Esc to quit";
			Content.RootDirectory = "Content";
			_renderContext = new DefaultRenderContext(_graphicsDeviceManager, Content);
			var root = new SceneGraphRoot();

			var scene = new MarchingCubesScene(_renderContext, "mri.zip");

			root.AddAsyncWithLoadingScreen(scene, _renderContext);

			Components.Add(root);

			base.Initialize();
		}

		/// <summary>
		/// Game update.
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			// easy way for user to quit
			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
				return;
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// Game draw.
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Draw(GameTime gameTime)
		{
			_renderContext.Attach();
			_renderContext.Clear(Color.White);

			base.Draw(gameTime);

			_renderContext.Detach();
		}
	}
}