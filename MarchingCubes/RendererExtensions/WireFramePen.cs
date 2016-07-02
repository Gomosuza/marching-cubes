using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Pens;

namespace MarchingCubes.RendererExtensions
{
	/// <summary>
	/// The wireframe pen will draw a wireframe that is visiblefrom all directions (CullMode.None).
	/// </summary>
	public class WireFramePen : Pen
	{
		private RasterizerState _rasterizer;
		private bool _isPrepared;
		private SamplerState _sampler;

		/// <summary>
		/// Creates a new wireframe pen with the specific colors.
		/// </summary>
		public WireFramePen() : base(Color.White)
		{
		}

		public override bool IsPrepared => _isPrepared && base.IsPrepared;

		public override void Prepare(IRenderContext renderContext)
		{
			base.Prepare(renderContext);
			_rasterizer = new RasterizerState
			{
				CullMode = CullMode.None,
				FillMode = FillMode.WireFrame,
				DepthBias = -0.1f,
				MultiSampleAntiAlias = true
			};

			// better filter helps for solid objects (esp. borders)
			_sampler = new SamplerState
			{
				Filter = TextureFilter.LinearMipPoint
			};

			_isPrepared = true;
		}

		public override void Configure(BasicEffect effect)
		{
			effect.VertexColorEnabled = true;
			effect.TextureEnabled = false;

			effect.GraphicsDevice.SamplerStates[0] = _sampler;
			// override the default rasterizer set by the rendercontext
			effect.GraphicsDevice.RasterizerState = _rasterizer;
		}
	}
}