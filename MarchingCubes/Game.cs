using MarchingCubes.Extensions;
using MarchingCubes.SceneGraph;
using MarchingCubes.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Renderer;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MarchingCubes
{
	/// <summary>
	/// Monogame implementation, only used to setup the scenegraph.
	/// </summary>
	public class Game : Microsoft.Xna.Framework.Game
	{
		private readonly GraphicsDeviceManager _graphicsDeviceManager;
		private IRenderContext _renderContext;

		private string _selectedAsset;
		private string[] _availableAssets;
		private int _assetIndex;
		private bool _reloadScene;
		private int _sceneType;
		private KeyboardState _lastKeyboardState;

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
			Content.RootDirectory = "Content";

			_availableAssets = Directory.GetFiles(Content.RootDirectory, "*.zip").Select(a => a.Substring(Content.RootDirectory.Length + 1)).ToArray();
			_selectedAsset = _availableAssets[0];

			_renderContext = new DefaultRenderContext(_graphicsDeviceManager, Content);

			Components.Add(new SceneGraphRoot());
			_reloadScene = true;
			_sceneType = 2;

			UpdateTitle();

			base.Initialize();
		}

		private void UpdateTitle()
		{
			Window.Title = $"Marching cubes demo - Esc to quit; F1 to show help; current asset: {_selectedAsset}";
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
			var kb = Keyboard.GetState();
			if (kb.IsKeyDown(Keys.Escape))
			{
				Exit();
				return;
			}
			if (kb.IsKeyDown(Keys.F1))
			{
				var message = new[]
				{
					"Keybindings:",
					"F5 - starts the marching cube visualizer",
					"F6 - loads the finsihed marching cubes result",
					"F7 - cycles through and changes the currently loaded asset (this reloads the current mode)"
				};
				MessageBox.Show(string.Join(Environment.NewLine, message));
			}
			var root = Components.Get<SceneGraphRoot>();
			var children = root.GetChildScenes();
			// we only ever have once scene, check that it is in the node. if not, we don't allow switching yet (otherwise we could get 2 scenes loading at the same time
			// or if we just started the app, in which case no scene is rendered
			var justStartedApplication = children.Count == 0;
			var currentNode = children.OfType<MarchingCubeBaseScene>().FirstOrDefault();
			if (currentNode != null || justStartedApplication)
			{
				if ((kb.IsKeyDown(Keys.F5) && _lastKeyboardState.IsKeyUp(Keys.F5)) || (_sceneType == 1 && _reloadScene))
				{
					_sceneType = 1;
					_reloadScene = false;

					var scene = new MarchingCubeVisualizer(_renderContext, _selectedAsset);
					root.AddAsync(scene);

					// remove the old scene so we don't render 2 at a time
					if (currentNode != null)
						root.RemoveAsync(currentNode);
				}
				else if ((kb.IsKeyDown(Keys.F6) && _lastKeyboardState.IsKeyUp(Keys.F6)) || (_sceneType == 2 && _reloadScene))
				{
					_sceneType = 2;
					_reloadScene = false;

					var scene = new MarchingCubesScene(_renderContext, _selectedAsset);
					root.AddAsyncWithLoadingScreen(scene, _renderContext);

					// remove the old scene so we don't render 2 at a time
					if (currentNode != null)
						root.RemoveAsync(currentNode);
				}
				if (kb.IsKeyDown(Keys.F7) && _lastKeyboardState.IsKeyUp(Keys.F7))
				{
					_assetIndex = (_assetIndex + 1) % _availableAssets.Length;
					_selectedAsset = _availableAssets[_assetIndex];
					UpdateTitle();
					_reloadScene = true;

					// remove the old scene so we don't render 2 at a time
					if (currentNode != null)
						root.RemoveAsync(currentNode);
				}
			}
			_lastKeyboardState = kb;
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