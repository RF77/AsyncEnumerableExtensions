// Copyright (c) 2021 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests.Operators
{
	public class PushTests : UnitTestBase
	{
		public PushTests(ITestOutputHelper output) : base(output)
		{
			
		}

		[Fact]
		public async Task PushToUnlimitedList()
		{
			var asyncEnum = AsyncEnum.Range(0, 10);
			var list = new List<int>();
			await asyncEnum.PushToList(list).LastAsync();

			Assert.Equal(10, list.Count);
		}

		[Fact]
		public async Task PushToLimitedList()
		{
			var asyncEnum = AsyncEnum.Range(0, 10);
			var list = new List<int>();
			await asyncEnum.PushToList(list, 5).LastAsync();

			Assert.Equal(5, list.Count);
			Assert.Equal(5, list.First());
			Assert.Equal(9, list.Last());
		}

		[Fact]
		public async Task PushToLimitedListWithFunc()
		{
			var asyncEnum = new ReplayAsyncEnumerable<int>();
			var list = new List<int>();
			var max = 5;
			var task = asyncEnum.PushToList(list, () => max).LastAsync();
			
			Assert.Empty(list);

			for (int i = 0; i < 7; i++)
			{
				await asyncEnum.Next(i);
			}

			await Task.Delay(500);

			Assert.Equal(5, list.Count);

			max = 2;

			for (int i = 0; i < 4; i++)
			{
				await asyncEnum.Next(i);
			}

			await asyncEnum.Complete();

			await task;
			Assert.Equal(2, list.Count);
		}

		[Fact]
		public async Task PushToUnlimitedQueue()
		{
			var asyncEnum = AsyncEnum.Range(0, 10);
			var queue = new Queue<int>();
			await asyncEnum.PushToQueue(queue).LastAsync();

			Assert.Equal(10, queue.Count);
		}

		[Fact]
		public async Task PushToLimitedQueue()
		{
			var asyncEnum = AsyncEnum.Range(0, 10);
			var queue = new Queue<int>();
			await asyncEnum.PushToQueue(queue, 5).LastAsync();

			Assert.Equal(5, queue.Count);
			Assert.Equal(5, queue.Dequeue());
		}
	}
}