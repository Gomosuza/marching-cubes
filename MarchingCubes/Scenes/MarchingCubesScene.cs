using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Renderer;
using Renderer.Brushes;
using Renderer.Meshes;
using Renderer.Pens;
using System;

namespace MarchingCubes.Scenes
{
    /// <summary>
    /// The main scene that will show the result of the marching cube algorithm.
    /// </summary>
    public class MarchingCubesScene : MarchingCubeBaseScene, ISceneGraphEntityInitializeProgressReporter
    {
        private const int _isolevel = 128;

        private Mesh _dataMesh;
        private Brush _brush;
        private Pen _pen;

        /// <summary>
        /// Creates a new marching cubes scene instance and tells the scene which data to load.
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="dataPath"></param>
        public MarchingCubesScene(IRenderContext renderContext, string dataPath) : base(renderContext, dataPath)
        {
        }

        /// <summary>
        /// The progress reporter which will be fired with values between 0-100.
        /// If this is implemented, <see cref="ISceneGraphEntityInitializeProgressReporter.InitializeProgress"/> must be called at least once with a value of 100 (progress completed).
        /// </summary>
        public event EventHandler<int> InitializeProgress;

        /// <summary>
        /// Initializes the marching cubes scene by loading the dataset from disk and creating a mesh for it.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _brush = new NormalBrush();
            _pen = new SolidColorPen(Color.Black);

            var triangleBuilder = new TriangleMeshDescriptionBuilder();
            // isolevel defines which points are inside/outside the structure
            int lastProgress = 0;

            // for each point we will at all 8 points forming a cube, we will simply take the index + 1 in each direction, thus our iteration counters must be reduced by 1 to prevent out of bound exception
            int xlen = InputData.XLength - 1;
            int ylen = InputData.YLength - 1;
            int zlen = InputData.ZLength - 1;

            var mcAlgo = new MarchingCubesAlgorithm(false);
            var box = new BoundingBox();
            for (int x = 0; x < xlen; x++)
            {
                for (int y = 0; y < ylen; y++)
                {
                    // report progress here as one dimension by itself would progress really fast (too small increments)
                    var progress = (y + x * ylen) / (float)(ylen * xlen);
                    var newProgress = (int)(progress * 100);
                    if (newProgress >= 100)
                        newProgress = 99; // don't send 100 yet, send it only once at the end of the method
                    if (newProgress != lastProgress)
                    {
                        var p = InitializeProgress;
                        p?.Invoke(this, newProgress);
                        lastProgress = newProgress;
                    }
                    for (int z = 0; z < zlen; z++)
                    {
                        box.Min.X = x;
                        box.Min.Y = y;
                        box.Min.Z = z;
                        box.Max.X = x + 1;
                        box.Max.Y = y + 1;
                        box.Max.Z = z + 1;
                        var vertices = mcAlgo.Polygonize(InputData, _isolevel, box);
                        if (vertices != null && vertices.Count > 0)
                            triangleBuilder.Vertices.AddRange(vertices);
                    }
                }
            }
            _dataMesh = RenderContext.MeshCreator.CreateMesh(triangleBuilder);

            var i = InitializeProgress;
            i?.Invoke(this, 100);
            InitializeProgress = null;
            Initialized = true;
        }

        /// <summary>
        /// Draws the marching cubes scene.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            DrawMesh(_dataMesh, _brush, null);

            base.Draw(gameTime);
        }
    }
}
