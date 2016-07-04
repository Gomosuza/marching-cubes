using MarchingCubes.RendererExtensions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
		private int _cubesPerTick;
		private MouseState _lastMouse;

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

			_cubesPerTick = 1;
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

			var m = Mouse.GetState();
			var delta = m.ScrollWheelValue - _lastMouse.ScrollWheelValue;
			if (delta > 0)
			{
				_cubesPerTick *= 2;
			}
			else if (delta < 0)
			{
				_cubesPerTick /= 2;
			}
			if (_cubesPerTick < 1)
				_cubesPerTick = 1;
			_lastMouse = m;

			if (_backgroundWorker.IsBusy)
				return;
			var totalLength = _inputData.XLength * _inputData.YLength * _inputData.ZLength;
			if (_currentCubeIndex < totalLength)
			{
				_backgroundWorker.RunWorkerAsync(_cubesPerTick);
				// posibility that our index overflows (e.g. we are 1 before buffer end and want to add 10 more
				// this is ok because we only use the index to check if we are still in the buffer (and only then would we add further data)
				_currentCubeIndex += _cubesPerTick;
			}
		}

		/// <summary>
		/// Call to manually step once.
		/// </summary>
		public void Step()
		{
			if (_backgroundWorker.IsBusy)
				return;
			if (_currentCubeIndex < _inputData.XLength * _inputData.YLength * _inputData.ZLength)
			{
				_backgroundWorker.RunWorkerAsync(1);
				_currentCubeIndex++;
			}
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
		/// Draws the visualizer.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			var length = _inputData.XLength * _inputData.YLength * _inputData.ZLength;
			if (_currentCubeIndex < length)
			{
				var idx = _backgroundWorker.CubeIndexToArrayIndex(_currentCubeIndex);
				if (idx != -1)
				{
					var p = GetPositionFromIndex(idx);
					var transform = Matrix.CreateTranslation(p);

					_renderContext.DrawMesh(_visualizerMesh, transform, _camera.View, _camera.Projection, _brush);

					_renderContext.DrawMesh(_visualizerLineMesh, transform, _camera.View, _camera.Projection, null, _pen);
				}
			}
		}
	}
}