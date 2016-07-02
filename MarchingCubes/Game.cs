using MarchingCubes.Extensions;
using MarchingCubes.SceneGraph;
using MarchingCubes.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Renderer;

namespace MarchingCubes
{
	public class Game : Microsoft.Xna.Framework.Game
	{
		private readonly GraphicsDeviceManager _graphicsDeviceManager;
		private IRenderContext _renderContext;

		public Game()
		{
			_graphicsDeviceManager = new GraphicsDeviceManager(this);
		}

		protected override void Initialize()
		{
			Content.RootDirectory = "Content";
			_renderContext = new DefaultRenderContext(_graphicsDeviceManager, Content);
			var root = new SceneGraphRoot();

			var scene = new MarchingCubesScene(_renderContext);

			root.AddAsyncWithLoadingScreen(scene, _renderContext);

			Components.Add(root);

			base.Initialize();
		}

		protected override void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
				return;
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			_renderContext.Attach();
			_renderContext.Clear(Color.White);

			base.Draw(gameTime);

			_renderContext.Detach();
		}
	}
}