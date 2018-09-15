using Microsoft.Xna.Framework.Graphics;
using Renderer.Meshes;
using System.Collections.Generic;

namespace MarchingCubes.RendererExtensions
{
    /// <summary>
    /// A triangle mesh description builder for <see cref="VertexPositionNormal"/> meshes.
    /// </summary>
    public class TriangleMeshDescriptionBuilder : IMeshDescription<VertexPositionNormal>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TriangleMeshDescriptionBuilder"/>.
        /// </summary>
        public TriangleMeshDescriptionBuilder()
        {
            Vertices = new List<VertexPositionNormal>();
        }

        /// <summary>
        /// The primitive type in use, this will always be <see cref="Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList"/>.
        /// </summary>
        public PrimitiveType PrimitiveType => PrimitiveType.TriangleList;

        /// <summary>
        /// The total vertex count.
        /// </summary>
        public int VertexCount => Vertices.Count;

        /// <summary>
        /// The actual vertices.
        /// </summary>
        public List<VertexPositionNormal> Vertices { get; }
    }
}
