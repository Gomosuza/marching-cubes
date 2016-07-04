using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Brushes;
using Renderer.Meshes;
using Renderer.Pens;
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

		private int _currentCubeIndex;

		/// <summary>
		/// Holds all indices of slices that are fully empty.
		/// </summary>
		private readonly bool[] _emptySliceIndices;

		/// <summary>
		/// Holds all indices of empty rows where at least one row is not empty within the same slice.
		/// </summary>
		private readonly bool[,] _emptyRowIndices;

		private bool _paused;

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
			if (_paused)
			{
				return;
			}

			FindNextCube();
		}

		/// <summary>
		/// Call to manually step once
		/// </summary>
		public void Step()
		{
			FindNextCube();
		}

		/// <summary>
		/// Sets the pause state. If paused, the visualizer will no longer advance based on time.
		/// </summary>
		/// <param name="pause"></param>
		public void SetPause(bool pause)
		{
			_paused = pause;
		}

		/// <summary>
		/// Checks how many steps need to be made based on the passed time.
		/// </summary>
		private void FindNextCube()
		{
			// can't add more work when worker is already busy
			var length = _inputData.XLength * _inputData.YLength * _inputData.ZLength;
			while (_currentCubeIndex < length && !_backgroundWorker.IsBusy)
			{
				var p = GetPositionFromIndex(_currentCubeIndex);
				int skipCubeCount;
				if (_emptySliceIndices[(int)p.Z])
				{
					// skip the slice entirely, its empty anyway
					skipCubeCount = _inputData.XLength * _inputData.YLength;
				}
				else if (_emptyRowIndices[(int)p.Y, (int)p.Z])
				{
					// skip an entire row
					skipCubeCount = _inputData.XLength;
				}
				else
				{
					skipCubeCount = 1;
					if (_inputData[(int)p.X, (int)p.Y, (int)p.Z] > 0)
					{
						// hit a cube, fire of backgroundworker to modify the mesh
						// we can't step anymore as our backgroundworker has no queue, it can only work on 1 item at a time
						_backgroundWorker.RunWorkerAsync(p);
					}
				}
				_currentCubeIndex += skipCubeCount;
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
			_renderContext.DrawMesh(_visualizerMesh, transform, _camera.View, _camera.Projection, _brush);

			_renderContext.DrawMesh(_visualizerLineMesh, transform, _camera.View, _camera.Projection, null, _pen);
		}
	}
}