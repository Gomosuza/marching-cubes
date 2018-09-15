using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Brushes;

namespace MarchingCubes
{
    /// <summary>
    /// Polygonal lighting brush.
    /// Allows easily distinguishing non parallel surfaces in a 3D view.
    /// </summary>
    public class NormalBrush : Brush
    {
        private SamplerState _sampler;
        private bool _isPrepared;

        /// <inheritdoc />
        public override bool IsPrepared => _isPrepared;

        /// <inheritdoc />
        public override void Configure(BasicEffect effect)
        {
            // really ugly way of getting the model lit up (green color only), but does the job for now
            effect.FogEnabled = false;
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = false;

            // primitive color
            effect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 2.0f;
            effect.Alpha = 1.0f;

            effect.LightingEnabled = true;
            if (effect.LightingEnabled)
            {
                // x direction
                effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
                effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1, -1, 0));
                // points from the light to the origin of the scene
                effect.DirectionalLight0.SpecularColor = Vector3.One;

                effect.DirectionalLight1.Enabled = true;
                // y direction
                effect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
                effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1, -1, 0));
                effect.DirectionalLight1.SpecularColor = Vector3.One;
            }
            effect.GraphicsDevice.SamplerStates[0] = _sampler;
        }

        /// <inheritdoc />
        public override void Prepare(IRenderContext renderContext)
        {
            _sampler = new SamplerState
            {
                Filter = TextureFilter.LinearMipPoint
            };
            _isPrepared = true;
        }
    }
}
