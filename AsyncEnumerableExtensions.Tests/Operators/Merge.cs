// /*******************************************************************************
//  * Copyright (c) 2020 by RF77 (https://github.com/RF77)
//  * All rights reserved. This program and the accompanying materials
//  * are made available under the terms of the Eclipse Public License v1.0
//  * which accompanies this distribution, and is available at
//  * http://www.eclipse.org/legal/epl-v10.html
//  *
//  * Contributors:
//  *    RF77 - initial API and implementation and/or initial documentation
//  *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests.Operators
{
	public class Merge : UnitTestBase
	{
		public Merge(ITestOutputHelper output) : base(output)
		{
		}

		private async IAsyncEnumerable<string> GetNumbersAsync(int amount, string prefix,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (var i = 0; i < amount; i++)
			{
				var val = $"{prefix}{i}";
				//Write($"yield {val}");
				yield return val;
				await Task.Delay(10, cancellationToken);
			}
		}

		private async IAsyncEnumerable<string> GetNumbersWithExceptionAsync(int amount, string prefix,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (var i = 0; i < amount; i++)
			{
				if (i + 2 == amount)
				{
					throw new InvalidOperationException();
				}
				var val = $"{prefix}{i}";
				//Write($"yield {val}");
				yield return val;
				await Task.Delay(10, cancellationToken);
			}
		}

		[Fact]
		public async Task MergeConcurrentlyOf4Sources()
		{
			var firstStream = AsyncEnum.Just(1);
			var secondStream = AsyncEnum.Just(2);
			var thirdStream = AsyncEnum.Just(3);
			var forthStream = AsyncEnum.Just(4);

			var list = await firstStream.MergeConcurrently(secondStream, thirdStream, forthStream).ToListAsync();

			Assert.True(list.Count == 4);
		}

		[Fact]
		public void MergeConcurrentlyOf4Sources_CalledInDispatcherThread()
		{
			AsyncContext.Run(async () =>
			{
				var threadId = Thread.CurrentThread.ManagedThreadId;
				var firstStream = AsyncEnum.Just(1);
				var secondStream = AsyncEnum.Just(2);
				var thirdStream = AsyncEnum.Just(3);
				var forthStream = AsyncEnum.Just(4);

				var list = await firstStream.MergeConcurrently(secondStream, thirdStream, forthStream)
					.Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId)).ToListAsync();

				Assert.True(list.Count == 4);
			});
		}

		[Fact]
		public async Task MergeConcurrentlyUntilFirstExceptionOfStreams()
		{
			var stream = StreamProducer().MergeConcurrentlyUntilFirstException();

			var list = await stream.ToListAsync();

			Assert.True(list.Count == 25);
			Assert.True(list.Last() == "E4");

			//await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task MergeConcurrentlyOfStreams()
		{
			var stream = StreamProducer().MergeConcurrently();

			var list = await stream.ToListAsync();

			Assert.True(list.Count == 25);
			Assert.True(list.Last() == "E4");

			//await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task MergeSequentiallyOfStreams()
		{
			var stream = StreamProducer().Merge();

			var list = await stream.ToListAsync();

			//Assert.True(list.Count == 25);
			//Assert.True(list.Last() == "E4");

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task MergeConcurrentlyOfStreamsAndCancel()
		{
			var cts = new CancellationTokenSource(100);

			var stream = StreamProducer().MergeConcurrentlyUntilFirstException();

			var list = await stream.ToListAsync(cts.Token);

			Assert.True(list.Count < 25);

			//await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task MergeConcurrentlyUntilFristExceptionOfStreamsAndThrowException()
		{
			var stream = StreamProducerWithException().MergeConcurrentlyUntilFirstException();

			await Assert.ThrowsAsync<InvalidOperationException>(async () => await stream.ToListAsync());

			//await WriteStreamToOutputAsync(stream);
		}


		[Fact]
		public async Task MergeConcurrentlyOfStreamsAndThrowException()
		{
			var stream = StreamProducerWithException().MergeConcurrently();

			await Assert.ThrowsAsync<AggregateException>(async () => await stream.ToListAsync());

			//await WriteStreamToOutputAsync(stream);
		}

		private async IAsyncEnumerable<IAsyncEnumerable<string>> StreamProducer([EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return GetNumbersAsync(5, ((char)('A' + (char) i)).ToString(), cancellationToken);
				await Task.Delay(30, cancellationToken);
			}
		}

		private async IAsyncEnumerable<IAsyncEnumerable<string>> StreamProducerWithException([EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return GetNumbersWithExceptionAsync(5, ((char)('A' + (char)i)).ToString(), cancellationToken);
				await Task.Delay(30, cancellationToken);
			}
		}
	}
}