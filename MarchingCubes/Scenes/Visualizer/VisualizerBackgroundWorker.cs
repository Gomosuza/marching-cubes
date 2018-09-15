using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Meshes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MarchingCubes.Scenes.Visualizer
{
    /// <summary>
    /// The background worker will create the meshes on a background thread and exposes the finished mesh.
    /// </summary>
    public class VisualizerBackgroundWorker
    {
        private readonly BackgroundWorker _backgroundWorker;

        private readonly DynamicMesh _mesh1;
        private readonly DynamicMesh _mesh2;
        private bool _mesh1IsBeingEdited;

        private readonly List<VertexPosition> _vertices;
        private readonly MarchingCubesAlgorithm _marchingCubesAlgorithm;
        private readonly IInputData _inputData;
        private readonly int _isoLevel;

        private readonly int[] _filledDataIndices;
        private readonly int[] _primitiveCountPerDataPoint;
        /// <summary>
        /// Number of cubes that are marched at once at a minimum.
        /// Each cube may not necesarily generate vertices (if its outside the dataset) but many cubes will generate more than 1 primitive.
        /// </summary>
        private const int VertexGenerationMinBatchSize = 1024 * 1024;
        private int _lookupTableWriteIndex;
        private int _lookupTableReadIndex;
        private int _primitiveCount;

        /// <summary>
        /// Creates a new instance of the visualizer
        /// </summary>
        /// <param name="renderContext"></param>
        /// <param name="inputData"></param>
        /// <param name="isoLevel"></param>
        public VisualizerBackgroundWorker(IRenderContext renderContext, IInputData inputData, int isoLevel)
        {
            _inputData = inputData;
            _isoLevel = isoLevel;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += MarchCube;
            _backgroundWorker.RunWorkerCompleted += CubeMarched;

            _mesh1 = renderContext.MeshCreator.CreateDynamicMesh(PrimitiveType.TriangleList, typeof(VertexPosition), VertexPosition.VertexDeclaration, DynamicMeshUsage.UpdateOften);
            _mesh2 = renderContext.MeshCreator.CreateDynamicMesh(PrimitiveType.TriangleList, typeof(VertexPosition), VertexPosition.VertexDeclaration, DynamicMeshUsage.UpdateOften);
            _vertices = new List<VertexPosition>();
            _marchingCubesAlgorithm = new MarchingCubesAlgorithm(false);

            // setup our indices so we don't have to recheck everytime we need this info
            // technically the marching cube algorithm will run over all cubes, but we will skip all that have 0 as a value

            // precalculate all the indices where data exists
            var data = new List<int>();
            for (int z = 0; z < _inputData.ZLength - 1; z++)
            {
                for (int y = 0; y < _inputData.YLength - 1; y++)
                {
                    for (int x = 0; x < _inputData.XLength - 1; x++)
                    {
                        if (_inputData[x, y, z] > 0)
                        {
                            var index = x + _inputData.XLength * (y + z * _inputData.YLength);
                            data.Add(index);
                        }
                    }
                }
            }
            _filledDataIndices = data.ToArray();
            _primitiveCountPerDataPoint = new int[_filledDataIndices.Length];
        }

        /// <summary>
        /// Returns whether the worker is currently working on a cell or not.
        /// </summary>
        public bool IsBusy { get; private set; }

        private void CubeMarched(object sender, RunWorkerCompletedEventArgs e)
        {
            // Since new triangles are created on the background thread and we can't update a mesh that is attached to the graphicsdevice we hotswap between two meshes.
            // This technically means 2x memory consumption (mesh1, mesh2), but the visualizer is not about performance, instead it slowly shows the user how the marching cubes algorithm works
            var meshToEdit = _mesh1IsBeingEdited ? _mesh1 : _mesh2;

            if (meshToEdit.Primitives < _primitiveCount)
            {
                // the mesh is old still old and now the existing primitives are no longer sufficient, patch it with our latest vertex array
                meshToEdit.Update(_vertices.ToArray());
            }
            // explicitly set the primitive count
            // most of the time we update the mesh with 1000+ primitives but want to draw only 1 more than previously
            // e.g. update1: patch to 1000 vertices, draw 1 primitive, update2: no patch, draw 2 primitives, update3: no patch, draw 3 primitives, ...
            if (meshToEdit.PrimitiveRange != _primitiveCount)
                meshToEdit.UpdatePrimitiveRange(0, _primitiveCount);

            // finished updating the mesh, let the rendering know
            _mesh1IsBeingEdited = !_mesh1IsBeingEdited;
            IsBusy = false;
        }

        private void MarchCube(object sender, DoWorkEventArgs e)
        {
            var count = (int)e.Argument;
            // ideally we would just march the number of provided cubes and everything just works
            // in real life this usually means a lot of calls with count = 1 or other small values in each update
            // this forces the vertexbuffer to move vertices from CPU to GPU RAM everytime we add a few new vertices
            // (each time a new vertexbuffer from the perspective of the GPU when in reality it is the same buffer + ~3 vertices)
            // this not only causes stalls in the pipeline but is also generally very inefficient (and the GPU can't keep up with the changing data, thus eventually throws out of memory exception)

            // instead we add a batchsize of a fixed minimum (e.g. 1000) and only display the requested count ( e.g. 1) to the user
            // upon next request, we simply change the indexpointer as long as we still have enough pregenerated data (in this example for the next 999 calls with count = 1 each)
            // only once we run out the generated vertices will a new batch be generated

            // this drastically reduces the transfer times and prevents the out of memory exceptions

            if (_primitiveCount + count >= _vertices.Count / 3)
            {
                int refCount = count;
                // at least one new vertex must be generated to show all requested to the user
                // generate a new batch, but make sure not to overflow our data

                // make sure we generated no less than our batchsize at any time
                if (refCount < VertexGenerationMinBatchSize)
                    refCount = VertexGenerationMinBatchSize;
                if (_lookupTableWriteIndex + refCount > _filledDataIndices.Length)
                    refCount = _filledDataIndices.Length - _lookupTableWriteIndex;

                for (int i = 0; i < refCount; i++)
                {
                    // our dataset contains all datapoints, even all the empty cubes
                    // we built a lookup table for all the cubes with values, so access it
                    var lookupIndex = _lookupTableWriteIndex + i;
                    var indexInDataset = _filledDataIndices[lookupIndex];
                    // now we have the actual index for the next cube with values in the dataset

                    // polygonize it
                    var p = GetPositionFromIndex(indexInDataset);
                    var vertices = _marchingCubesAlgorithm.Polygonize(_inputData, _isoLevel, new BoundingBox(p, p + Vector3.One));
                    if (vertices != null && vertices.Count > 0)
                    {
                        _vertices.AddRange(vertices);
                        // list is always multiple of 3 (3 vertices form a triangle)
                        // add number of newly generated primitives/triangles
                        _primitiveCountPerDataPoint[lookupIndex] = vertices.Count / 3;
                    }
                }
                // update the index
                _lookupTableWriteIndex += refCount;
            }
            // now the vertex array is large enough, update the meshpointer and return
            var lookupTableReadIndex = _lookupTableReadIndex;
            for (int i = 0; i < count; i++)
            {
                var lookup = lookupTableReadIndex + i;
                if (lookup >= _primitiveCountPerDataPoint.Length)
                    break; // end of dataset
                var primitives = _primitiveCountPerDataPoint[lookup];
                _primitiveCount += primitives;
            }
            _lookupTableReadIndex += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 GetPositionFromIndex(int index)
        {
            var zSize = _inputData.XLength * _inputData.YLength;
            var z = index / zSize;
            var y = (index - z * zSize) / _inputData.XLength;
            var x = index - z * zSize - y * _inputData.XLength;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Returns the entire mesh that the background worker has already finished and that can be drawn.
        /// </summary>
        /// <returns></returns>
        public Mesh GetMeshToDraw()
        {
            return _mesh1IsBeingEdited ? _mesh2 : _mesh1;
        }

        /// <summary>
        /// When called will run the algorithm on the next n cubes with data and add them to the mesh.
        /// The algorithm will only run on cubes with a value, but this does not guarantee that triangles will be generated (some cubes have values but will not generate polygons due to their position in the grid).
        /// <param name="n">The number of cubes to march. Minimum is 1, each marched cube is a cube that already contains data, however it may not necessarily generate primitives.</param>
        /// </summary>
        public void RunWorkerAsync(int n)
        {
            if (n <= 0)
                throw new ArgumentOutOfRangeException(nameof(n));
            if (IsBusy)
                throw new NotSupportedException("Worker is busy");
            IsBusy = true;
            _backgroundWorker.RunWorkerAsync(n);
        }

        /// <summary>
        /// Returns the index of the n-th element in the source array that has a datavalue greater 0.
        /// -1 if overflowed.
        /// </summary>
        /// <param name="currentCubeIndex"></param>
        /// <returns></returns>
        public int CubeIndexToArrayIndex(int currentCubeIndex)
        {
            if (currentCubeIndex >= _filledDataIndices.Length)
                return -1;
            return _filledDataIndices[currentCubeIndex];
        }
    }
}