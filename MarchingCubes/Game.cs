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
			var scene = new MarchingCubesScene(_graphicsDeviceManager, Content, Window);
			Components.Add(scene);

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