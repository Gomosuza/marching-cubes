using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Extensions;
using System;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// A scene that displays a progress bar.
	/// </summary>
	public class LoadingProgressScene : SceneGraphEntity
	{
		private readonly IRenderContext _renderContext;
		private float _progressPercent;
		private Texture2D _pixel;
		private bool _removedFromParent;

		public LoadingProgressScene(IRenderContext renderContext)
		{
			_renderContext = renderContext;
		}

		public override void Initialize()
		{
			_pixel = new RenderTarget2D(_renderContext.GraphicsDevice, 1, 1);
			_pixel.SetData(new[] { Color.White });

			base.Initialize();
			Initialized = true;
		}

		/// <summary>
		/// When called will set the progressbar to the specific value.
		/// </summary>
		/// <param name="progress">A value in range 0-100.</param>
		public void SetProgress(int progress)
		{
			if (progress < 0 || progress > 100)
				throw new ArgumentOutOfRangeException(nameof(progress));

			_progressPercent = progress / 100f;
			if (progress == 100)
			{
				if (!_removedFromParent)
				{
					_removedFromParent = true;
					Parent.RemoveAsync(this);
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			var vp = _renderContext.GraphicsDevice.Viewport;
			const int margin = 10;
			var h = (int)(vp.Height / 10f);
			// full progress bar with margin to left + right
			var border = new Rectangle(margin, vp.Height / 2 - h / 2, vp.Width - 2 * margin, h);
			// progress fill state based on progressPercent, also 1 more pixel margin on each side
			var progress = new Rectangle(margin + 1, vp.Height / 2 - h / 2 + 1, (int)(_progressPercent * (vp.Width - 2 * margin)) - 2, h - 2);

			_renderContext.DrawTexture(_pixel, border, Color.Black);
			_renderContext.DrawTexture(_pixel, progress, Color.Green);
			base.Draw(gameTime);
		}
	}
}