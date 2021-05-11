using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;
using Nito.AsyncEx;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests
{
	/// <summary>
	///     Not real tests.. more to try things
	/// </summary>
	public class TryAndErrorTests : UnitTestBase
	{
		private const int NumberOfStreamItems = 5000;
		private const int NumberOfStreams = 10;

		public TryAndErrorTests(ITestOutputHelper output) : base(output)
		{
		}


		[Fact]
		public async Task Test1()
		{
			var stream = AsyncEnumerable.Range(0, 5).Delay(100);

			await WriteStreamToOutputAsync(stream);
		}

		private ReplayAsyncEnumerable<int> _queue = new ReplayAsyncEnumerable<int>();
		private AsyncLock _asyncLock = new AsyncLock();
		private ReplaySubject<int> _otherQueue = new ReplaySubject<int>();

		public async IAsyncEnumerable<IAsyncEnumerable<int>> GetStreams()
		{
			for (int i = 0; i < NumberOfStreams; i++)
			{
				yield return AsyncEnum.Range(0, NumberOfStreamItems);
				await Task.Delay(1);
			}

			await Task.CompletedTask;
		}


		[Fact]
		public async Task TestWithMergeConcurrently()
		{
			var streams = GetStreams();

			var sw = Stopwatch.StartNew();

			var list = await streams.MergeConcurrently().ToListAsync();

			sw.Stop();

			Assert.True(list.Count == NumberOfStreams * NumberOfStreamItems);

			Write(list.Count.ToString());
			
			Write(string.Join(", ", list.Take(500).Select(i => i.ToString())));

			Write($"{nameof(TestWithMergeConcurrently)}: Time: {sw.ElapsedMilliseconds}");
		}

		[Fact]
		public async Task TestWithMergeSequentially()
		{
			var streams = GetStreams();

			var sw = Stopwatch.StartNew();

			var list = await streams.MergeSequentially().ToListAsync();

			sw.Stop();

			Write($"{nameof(TestWithMergeConcurrently)}: Time: {sw.ElapsedMilliseconds}");
		}

		[Fact]
		public async Task Test2()
		{
			var stream = AsyncEnum.Range(0, 300000);
			var stream2 = AsyncEnum.Range(0, 300000);
			var streams = new[] {stream, stream2};

			var sw = Stopwatch.StartNew();

			//stream = stream.If(i => i % 2 == 0, i => i.Select(o => o * 2));
			//stream = stream.Select(i => i * 2).ToReplayQueue();

			await foreach (var item in stream)
			{
				lock (this)
				{
					_otherQueue.OnNext(item);
				}
				//using (await _asyncLock.LockAsync())
				{
					//await _queue.Next(item);
				}
			}

			await foreach (var item in stream2)
			{
				lock (this)
				{
					_otherQueue.OnNext(item);
				}
				//using (await _asyncLock.LockAsync())
				{
					//await _queue.Next(item);
				}
			}

			//await _queue.Complete();
			_otherQueue.OnCompleted();

			//stream = AsyncEnumerableEx.Merge(streams);
			//stream = streams.MergeSequential();

			var result = await _otherQueue.ToAsyncEnumerable().ToListAsync();

			sw.Stop();
		}
	}
}