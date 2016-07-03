using MarchingCubes.RendererExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.Brushes;
using Renderer.Extensions;
using Renderer.Meshes;
using Renderer.Pens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MarchingCubes.Scenes
{
	/// <summary>
	/// The visualizer provides a nice realtime visualization of the inner workings of the marching cube alogrithm while it is assembling the result.
	/// This visualizer will slowly visualize the workings of the marching cubes algorithm by:
	/// - Selected cube by cube and checking if the cube intersects with the dataset
	/// - if so, creating the appropriate triangles and rendering before moving on
	/// 
	/// For improved visuals the generation occurs on a background thread and can thus be "live debugged" while the mesh is being built.
	/// The algorithm will also fast-skip over empty rows and slices.
	/// </summary>
	public class MarchingCubeVisualizer : MarchingCubeBaseScene
	{
		private BackgroundWorker _marchingCubesWorker;

		private DynamicMesh _mesh1;
		private DynamicMesh _mesh2;
		private Mesh _visualizerMesh;

		private Brush _brush;
		private Pen _pen;
		private SolidColorPen _visualizerAlgorithmWorking;

		/// <summary>
		/// Since new triangles are created on the background thread and we can't update a mesh that is attached to the graphicsdevice we hotswap between two meshes.
		/// This technically means 2x memory consumption, but the visualizer is not about performance, instead it slowly shows the user how the marching cubes algorithm works.
		/// </summary>
		private bool _mesh1IsBeingEdited;

		/// <summary>
		/// Since stepping one cube at a time makes no sense in empty rows, this parameter indicates how fast stepping should be when an entire row is empty (but at least one cube in the slice has values).
		/// </summary>
		private TimeSpan _steppingSpeedForEmptyRows;

		/// <summary>
		/// Since stepping one cube at a time makes no sense in empty slices, this parameter indicates how fast stepping should be when an entire slice is empty (this is the time it takes to skip the entire slice).
		/// </summary>
		private TimeSpan _steppingSpeedForEmptySlices;

		/// <summary>
		/// Stepping speed for rows that have at least one value.
		/// </summary>
		private TimeSpan _steppingSpeedForNonEmptyRows;

		private TimeSpan _lastStep;

		private int _currentCubeIndex;

		/// <summary>
		/// Holds all indices of slices that are fully empty.
		/// </summary>
		private bool[] _emptySliceIndices;

		/// <summary>
		/// Holds all indices of empty rows where at least one row is not empty within the same slice.
		/// </summary>
		private bool[,] _emptyRowIndices;

		private bool _animateCurrentCubeUntilBackgroundWorkerFinishes;

		/// <summary>
		/// Creates a new instance of the visualizer.
		/// </summary>
		/// <param name="renderContext"></param>
		/// <param name="file"></param>
		public MarchingCubeVisualizer(IRenderContext renderContext, string file) : base(renderContext, file)
		{

		}

		/// <summary>
		/// Initializes the visualizer by creating a wireframe mesh.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			_marchingCubesWorker = new BackgroundWorker();
			_marchingCubesWorker.DoWork += MarchCube;
			_marchingCubesWorker.RunWorkerCompleted += CubeMarched;

			_pen = new VertexColorPen(CullMode.None);
			_brush = new SolidColorBrush(Color.Green);
			_visualizerAlgorithmWorking = new SolidColorPen(Color.Red, CullMode.None);

			var cube = new LineMeshDescriptionBuilder();
			cube.AddBox(new BoundingBox(Vector3.Zero, Vector3.One), Color.AliceBlue);
			_visualizerMesh = RenderContext.MeshCreator.CreateMesh(cube);

			_steppingSpeedForNonEmptyRows = TimeSpan.FromSeconds(0.05f);
			_steppingSpeedForEmptyRows = TimeSpan.FromSeconds(0.05f);
			_steppingSpeedForEmptySlices = TimeSpan.FromSeconds(0.1f);

			_mesh1 = RenderContext.MeshCreator.CreateDynamicMesh(PrimitiveType.TriangleList, typeof(VertexPosition), VertexPosition.VertexDeclaration, DynamicMeshUsage.UpdateOften);
			_mesh2 = RenderContext.MeshCreator.CreateDynamicMesh(PrimitiveType.TriangleList, typeof(VertexPosition), VertexPosition.VertexDeclaration, DynamicMeshUsage.UpdateOften);
			// setup our indices so we don't have to recheck everytime we need this info

			// since we iterate x first then y then z, our slices are all z values
			_emptySliceIndices = new bool[InputData.ZLength];
			// each row is y,z value (and thus contains all x values)
			_emptyRowIndices = new bool[InputData.YLength, InputData.ZLength];
			for (int z = 0; z < InputData.ZLength; z++)
			{
				bool sliceIsEmpty = true;
				for (int y = 0; y < InputData.YLength; y++)
				{
					bool rowIsEmpty = true;
					for (int x = 0; x < InputData.XLength; x++)
					{
						if (InputData[x, y, z] > 0)
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
			Initialized = true;
		}

		private void CubeMarched(object sender, RunWorkerCompletedEventArgs e)
		{
			// TODO: currently throws because we set 0 vertices, implement MarchCube to actually output vertices for us
			return;
			// finished, update mesh
			var vertices = (VertexPosition[])e.Result;
			if (_mesh1IsBeingEdited)
			{
				_mesh1.Update(vertices);
			}
			else
			{
				_mesh2.Update(vertices);
			}
			// finished updating the mesh, let the rendering know
			_mesh1IsBeingEdited = !_mesh1IsBeingEdited;
			_animateCurrentCubeUntilBackgroundWorkerFinishes = false;
		}

		private static void MarchCube(object sender, DoWorkEventArgs e)
		{
			Vector3 position = (Vector3)e.Argument;
			var vertices = new List<VertexPosition>();
			e.Result = vertices.ToArray();
		}

		/// <summary>
		/// Updates the visualizer and advances the selected cube.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_animateCurrentCubeUntilBackgroundWorkerFinishes)
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
				skipCubeCount = InputData.XLength * InputData.YLength;
			}
			else if (_emptyRowIndices[(int)p.Y, (int)p.Z])
			{
				timeToCheck = _steppingSpeedForEmptyRows;
				// skip an entire row
				skipCubeCount = InputData.XLength;
			}
			else
			{
				timeToCheck = _steppingSpeedForNonEmptyRows;
				if (InputData[(int)p.X, (int)p.Y, (int)p.Z] > 0)
				{
					// hit a cube, fire of backgroundworker to modify the mesh
					_marchingCubesWorker.RunWorkerAsync(p);
					_animateCurrentCubeUntilBackgroundWorkerFinishes = true;
				}
			}
			if (gameTime.TotalGameTime - _lastStep > timeToCheck)
			{
				_currentCubeIndex += skipCubeCount;
				_lastStep = gameTime.TotalGameTime;
			}
		}

		/// <summary>
		/// Draws the visualizer, this includes the current cube being stepped as well as the mesh being modified.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			DrawMesh(_mesh1IsBeingEdited ? _mesh2 : _mesh1, _brush, _pen);

			var p = GetPositionFromIndex(_currentCubeIndex);
			var transform = Matrix.CreateTranslation(p);
			var pen = _pen;
			if (_emptySliceIndices[(int)p.Z])
			{
				// mark the entire slice
				transform *= Matrix.CreateScale(InputData.XLength, InputData.YLength, 1);
			}
			else if (_emptyRowIndices[(int)p.Y, (int)p.Z])
			{
				// mark the entire row
				transform *= Matrix.CreateScale(InputData.XLength, 1, 1);
			}
			else
			{
				if (InputData[(int)p.X, (int)p.Y, (int)p.Z] > 0)
				{
					// we hit a meshpoint where marching cubes algorithm will run, color it
					float alpha;
					if (_animateCurrentCubeUntilBackgroundWorkerFinishes)
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
					_visualizerAlgorithmWorking.Color = Color.Red * alpha;
					pen = _visualizerAlgorithmWorking;
				}
			}
			// update alpha value of pen

			DrawMesh(_visualizerMesh, _brush, pen, transform);

			base.Draw(gameTime);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Vector3 GetPositionFromIndex(int index)
		{
			var zSize = InputData.XLength * InputData.YLength;
			var z = index / zSize;
			var y = (index - z * zSize) / InputData.XLength;
			var x = index - z * zSize - y * InputData.XLength;
			return new Vector3(x, y, z);
		}
	}
}