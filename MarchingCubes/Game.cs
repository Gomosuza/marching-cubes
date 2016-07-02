using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MarchingCubes
{
	public class Game : Microsoft.Xna.Framework.Game
	{
		private readonly GraphicsDeviceManager _graphicsDeviceManager;

		public Game()
		{
			_graphicsDeviceManager = new GraphicsDeviceManager(this);
		}

		protected override void Initialize()
		{
			Content.RootDirectory = "Content";
			var graph = new SceneGraph.SceneGraph();

			var scene = new MarchingCubesScene(_graphicsDeviceManager, Content, Window);
			graph.AddScheduled(scene);

			Components.Add(graph);

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
	}
}