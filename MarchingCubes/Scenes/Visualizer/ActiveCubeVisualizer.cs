using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Brushes;
using Renderer.Extensions;
using Renderer.Meshes;
using Renderer.Pens;
using System;
using System.Runtime.CompilerServices;

namespace MarchingCubes.Scenes.Visualizer
{
	/// <summary>
	/// A helper class that will advance the cube position of the algorithm in a more user friendly way.
	/// Technically the marching cubes algorithm will check one cube at a time (hence the name).
	/// However given the input data format, they all have a lot of empty cells around the object.
	/// Visualizing to the user that the algorithm steps through a few hundret empty cells is not really interesting (and would take a few minutes to reach the actual cube if we step only every few ms).
	/// Instead this visualizer will step over entire slices (X*Y cubes) if the entire slice is empty and over empty rows (X).
	/// Only once a row is reached with at least one value will the algorithm actually march the cubes.
	/// </summary>
	public class ActiveCubeVisualizer : SceneGraphEntity
	{
		private readonly IRenderContext _renderContext;
		private readonly IInputData _inputData;
		private readonly VisualizerBackgroundWorker _backgroundWorker;
		private readonly ICamera _camera;
		private readonly Mesh _visualizerMesh;
		private readonly Mesh _visualizerLineMesh;

		private readonly Brush _brush;
		private readonly SolidColorPen _pen;

		/// <summary>
		/// Since stepping one cube at a time makes no sense in empty rows, this parameter indicates how fast stepping should be when an entire row is empty (but at least one cube in the slice has values).
		/// </summary>
		private readonly TimeSpan _steppingSpeedForEmptyRows;

		/// <summary>
		/// Since stepping one cube at a time makes no sense in empty slices, this parameter indicates how fast stepping should be when an entire slice is empty (this is the time it takes to skip the entire slice).
		/// </summary>
		private readonly TimeSpan _steppingSpeedForEmptySlices;

		/// <summary>
		/// Stepping speed for rows that have at least one value.
		/// </summary>
		private readonly TimeSpan _steppingSpeedForNonEmptyRows;

		private TimeSpan _lastStep;

		private int _currentCubeIndex;

		/// <summary>
		/// Holds all indices of slices that are fully empty.
		/// </summary>
		private readonly bool[] _emptySliceIndices;

		/// <summary>
		/// Holds all indices of empty rows where at least one row is not empty within the same slice.
		/// </summary>
		private readonly bool[,] _emptyRowIndices;

		/// <summary>
		/// Creates a new instance of the visualizer which will display a cube in the current working array.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="inputData"></param>
		/// <param name="backgroundWorker"></param>
		/// <param name="camera"></param>
		public ActiveCubeVisualizer(IRenderContext renderContext, IInputData inputData, VisualizerBackgroundWorker backgroundWorker, ICamera camera)
		{
			_renderContext = renderContext;
			_inputData = inputData;
			_backgroundWorker = backgroundWorker;
			_camera = camera;
			_brush = new SolidColorBrush(Color.AliceBlue);
			_pen = new SolidColorPen(Color.Red, CullMode.None);

			// we want to draw the outside surfaces of the currently worked on cube and its outline
			// currently this requires 2 meshes: one with the surfaces as triangles (for coloring) and one with only the outline as lines
			// if we used e.g. only the first mesh, the pen would also draw lines across the surfaces which we don't want
			// again: inefficient, but we don't care because it doesn't have to be efficient
			var cube = new TextureMeshDescriptionBuilder();
			var box = new BoundingBox(Vector3.Zero, Vector3.One);
			cube.AddRoom(box, Vector2.One);
			_visualizerMesh = renderContext.MeshCreator.CreateMesh(cube);
			var cube2 = new LineMeshDescriptionBuilder();
			cube2.AddBox(box, Color.Black);
			_visualizerLineMesh = renderContext.MeshCreator.CreateMesh(cube2);

			_steppingSpeedForNonEmptyRows = TimeSpan.FromSeconds(0.05f);
			_steppingSpeedForEmptyRows = TimeSpan.FromSeconds(0.05f);
			_steppingSpeedForEmptySlices = TimeSpan.FromSeconds(0.1f);

			// setup our indices so we don't have to recheck everytime we need this info

			// since we iterate x first then y then z, our slices are all z values
			_emptySliceIndices = new bool[_inputData.ZLength];
			// each row is y,z value (and thus contains all x values)
			_emptyRowIndices = new bool[_inputData.YLength, _inputData.ZLength];
			for (int z = 0; z < _inputData.ZLength; z++)
			{
				bool sliceIsEmpty = true;
				for (int y = 0; y < _inputData.YLength; y++)
				{
					bool rowIsEmpty = true;
					for (int x = 0; x < _inputData.XLength; x++)
					{
						if (_inputData[x, y, z] > 0)
						{
							sliceIsEmpty = rowIsEmpty = false;
						}
					}
					if (rowIsEmpty)
					{
						_emptyRowIndices[y, z] = true;
					}
				}
				if (sliceIsEmpty)
				{
					_emptySliceIndices[z] = true;
				}
			}
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
		/// Updates the visualizer which advances the cube if work on the previous one is finished.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			if (_backgroundWorker.IsBusy)
			{
				// no need to check position, time, etc.
				// current cube is generating mesh on the background thread

				return;
			}

			var p = GetPositionFromIndex(_currentCubeIndex);
			TimeSpan timeToCheck;
			int skipCubeCount = 1;
			if (_emptySliceIndices[(int)p.Z])
			{
				timeToCheck = _steppingSpeedForEmptySlices;
				// skip the slice entirely, its empty anyway
				skipCubeCount = _inputData.XLength * _inputData.YLength;
			}
			else if (_emptyRowIndices[(int)p.Y, (int)p.Z])
			{
				timeToCheck = _steppingSpeedForEmptyRows;
				// skip an entire row
				skipCubeCount = _inputData.XLength;
			}
			else
			{
				timeToCheck = _steppingSpeedForNonEmptyRows;
				if (_inputData[(int)p.X, (int)p.Y, (int)p.Z] > 0)
				{
					// hit a cube, fire of backgroundworker to modify the mesh
					_backgroundWorker.RunWorkerAsync(p);
				}
			}
			if (gameTime.TotalGameTime - _lastStep > timeToCheck)
			{
				_currentCubeIndex += skipCubeCount;
				_lastStep = gameTime.TotalGameTime;
			}
		}

		/// <summary>
		/// Draws the visualizer.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			var p = GetPositionFromIndex(_currentCubeIndex);
			var transform = Matrix.CreateTranslation(p);

			_pen.Color = Color.Black;
			if (_emptySliceIndices[(int)p.Z])
			{
				// mark the entire slice
				transform *= Matrix.CreateScale(_inputData.XLength, _inputData.YLength, 1);
			}
			else if (_emptyRowIndices[(int)p.Y, (int)p.Z])
			{
				// mark the entire row
				transform *= Matrix.CreateScale(_inputData.XLength, 1, 1);
			}
			else
			{
				if (_inputData[(int)p.X, (int)p.Y, (int)p.Z] > 0)
				{
					// we hit a meshpoint where marching cubes algorithm will run, color it
					float alpha;
					if (_backgroundWorker.IsBusy)
					{
						var second = gameTime.GetTotalElapsedSeconds();
						// pulsate for 2 seconds
						alpha = second % 1f;
						if ((int)second % 2 == 0)
							alpha = 1f - alpha;
					}
					else
					{
						alpha = 1f;
					}
					_pen.Color = Color.Red * alpha;
				}
			}
			_renderContext.DrawMesh(_visualizerMesh, transform, _camera.View, _camera.Projection, _brush);
			_renderContext.DrawMesh(_visualizerLineMesh, transform, _camera.View, _camera.Projection, null, _pen);
		}
	}
}