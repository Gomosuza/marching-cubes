using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Meshes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
			_marchingCubesAlgorithm = new MarchingCubesAlgorithm();
		}

		/// <summary>
		/// Returns whether the worker is currently working on a cell or not.
		/// </summary>
		public bool IsBusy { get; private set; }

		private void CubeMarched(object sender, RunWorkerCompletedEventArgs e)
		{
			// finished, update mesh
			//but only if vertices where added, if our total count is still 0, API will throw as we can't update a vertex buffer to 0 vertices
			// as the API wouldn't know which vertex type to use (since none was provided)
			if (_vertices.Count > 0)
			{

				// Since new triangles are created on the background thread and we can't update a mesh that is attached to the graphicsdevice we hotswap between two meshes.
				// This technically means 3x memory consumption (mesh1, mesh2, original vertex list), but the visualizer is not about performance, instead it slowly shows the user how the marching cubes algorithm works.
				if (_mesh1IsBeingEdited)
				{
					_mesh1.Update(_vertices.ToArray());
				}
				else
				{
					_mesh2.Update(_vertices.ToArray());
				}
				// finished updating the mesh, let the rendering know
				_mesh1IsBeingEdited = !_mesh1IsBeingEdited;
			}
			IsBusy = false;
		}

		private void MarchCube(object sender, DoWorkEventArgs e)
		{
			var position = (Vector3)e.Argument;
			var vertices = _marchingCubesAlgorithm.Polygonize(_inputData, _isoLevel, new BoundingBox(position, position + Vector3.One));
			if (vertices != null && vertices.Count > 0)
				_vertices.AddRange(vertices);
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
		/// When called will run the algorithm on the specific location.
		/// </summary>
		/// <param name="position"></param>
		public void RunWorkerAsync(Vector3 position)
		{
			if (IsBusy)
				throw new NotSupportedException("Worker is busy");
			IsBusy = true;
			_backgroundWorker.RunWorkerAsync(position);
		}
	}
}