using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.Meshes;
using System.Collections.Generic;

namespace MarchingCubes.RendererExtensions
{
    /// <summary>
    /// Mesh builder that creates colored lines only.
    /// Lines may have different colors.
    /// </summary>
    public class LineMeshDescriptionBuilder : IMeshDescription<VertexPositionColor>
    {
        /// <summary>
        /// The primitive type in use.
        /// </summary>
        public PrimitiveType PrimitiveType => PrimitiveType.LineList;

        /// <summary>
        /// The total vertex count.
        /// </summary>
        public int VertexCount => Vertices.Count;

        /// <summary>
        /// The actual vertices.
        /// </summary>
        public List<VertexPositionColor> Vertices { get; }

        /// <summary>
        /// Creates a new instance that creates line meshes. This instance always uses <see cref="Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList"/>.
        /// </summary>
        public LineMeshDescriptionBuilder()
        {
            Vertices = new List<VertexPositionColor>();
        }

        /// <summary>
        /// Adds a new line to the list.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <param name="color"></param>
        public void AddLine(Vector3 position, Vector3 target, Color color)
        {
            Vertices.Add(new VertexPositionColor(position, color));
            Vertices.Add(new VertexPositionColor(target, color));
        }

        /// <summary>
        /// Adds lines of the outer surface of the box to the vertex list.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="color"></param>
        public void AddBox(BoundingBox box, Color color)
        {
            // bottom
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Max.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Max.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), color));

            // connector pieces bottom to top
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Min.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Max.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Min.Y, box.Max.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Max.Z), color));

            // top
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Max.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Max.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Max.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Max.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Min.Z), color));

            Vertices.Add(new VertexPositionColor(new Vector3(box.Max.X, box.Max.Y, box.Min.Z), color));
            Vertices.Add(new VertexPositionColor(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), color));

        }
    }
}