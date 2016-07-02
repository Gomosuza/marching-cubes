using FluentAssertions;
using MarchingCubes.SceneGraph;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;
using System.Threading;

namespace MarchingCubes.Tests
{
	[TestFixture]
	public class SceneGraphTests
	{
		public class MockEntity : SceneGraphEntity
		{
			private readonly int _sleepInInit;

			public MockEntity(int sleepInInit)
			{
				_sleepInInit = sleepInInit;
			}

			public override void Initialize()
			{
				if (_sleepInInit > 0)
					Thread.Sleep(_sleepInInit);
				Initialized = true;
			}
		}

		[Test]
		public void ChildParentRelationBetweenEntityAndGraphShouldExist()
		{
			var root = new SceneGraph.SceneGraph();
			root.Parent.Should().BeNull();
			var node = new MockEntity(0);

			node.Parent.Should().BeNull();

			node.Initialize();
			node.Initialized.Should().BeTrue();

			root.AddScheduled(node);
			node.Parent.Should().Be(root, "because add scheduled added the node right away as it was already initialized");

			const int sleep = 1000;
			var lazyNode = new MockEntity(sleep);
			lazyNode.Initialized.Should().BeFalse();
			var before = DateTime.Now;
			root.AddScheduled(lazyNode);
			lazyNode.Initialized.Should().BeFalse();
			lazyNode.Parent.Should().Be(root);
			// wait longer than init would take
			while (DateTime.Now < before + TimeSpan.FromMilliseconds(sleep + 1000))
			{
				Thread.Sleep(10);
			}
			// assert that it was infact executed
			lazyNode.Initialized.Should().BeTrue();
		}

		[Test]
		public void TestSceneGraphNesting()
		{
			var root1 = new SceneGraph.SceneGraph();
			var root2 = new SceneGraph.SceneGraph();
			root1.Initialize();
			root2.Initialize();

			root1.AddScheduled(root2);

			root2.Parent.Should().Be(root1);
		}

		[Test]
		public void TestChangingSceneGraphForEntity()
		{
			var entity = new MockEntity(0);
			entity.Initialize();

			var root1 = new SceneGraph.SceneGraph();
			var root2 = new SceneGraph.SceneGraph();

			root1.AddScheduled(entity);
			entity.Parent.Should().Be(root1);

			new Action(() => root1.AddScheduled(entity)).ShouldThrow<NotSupportedException>();

			root1.RemoveScheduled(entity);
			entity.Parent.Should().Be(root1, "because RemoveScheduled will only be called in the next update");
			root1.Update(new GameTime());
			entity.Parent.Should().BeNull("because update now removed the entity from the scene");

			root2.AddScheduled(entity);
			entity.Parent.Should().Be(root2);

			root1.IsRegistered(entity).Should().BeFalse("because it will only be registered in the update call");
			new Action(() => entity.ChangeParent(root1)).ShouldThrow<NotSupportedException>();
			// call update so entity is actually registered with the root2
			root2.Update(new GameTime());

			// only now is the alternative method possible
			entity.ChangeParent(root1);
			entity.Parent.Should().Be(root1);
		}
	}
}