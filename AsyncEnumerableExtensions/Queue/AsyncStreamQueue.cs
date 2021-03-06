﻿// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;

namespace AsyncEnumerableExtensions.Queue
{
	public class AsyncStreamQueue<TResult, TSource>
	{
		public IAsyncEnumerable<TResult> ExecuteParallelAsync(IEnumerable<AsyncStreamQueueItem<TResult, TSource>> inputItems, int maxConcurrentTasks, CancellationToken cancellationToken)
		{
			var throttler = new SemaphoreSlim(maxConcurrentTasks);

			IEnumerable<IAsyncEnumerable<TResult>> streams = inputItems.Select(
				inputItem => ExecuteItem(throttler, inputItem, cancellationToken));

			return streams.MergeConcurrently();
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public IAsyncEnumerable<TResult> ExecuteParallelAsync(IAsyncEnumerable<AsyncStreamQueueItem<TResult, TSource>> inputItems, int maxConcurrentTasks, CancellationToken cancellationToken)
		{
			var throttler = new SemaphoreSlim(maxConcurrentTasks);
			var nextLock = new SemaphoreSlim(1,1);

			var multicastStream = new MulticastAsyncEnumerable<TResult>();

			HashSet<Task> runningStreams = new HashSet<Task>();

			HandleInputStreams();

			return multicastStream;

			async void HandleInputStreams()
			{
				try
				{
					await foreach (AsyncStreamQueueItem<TResult, TSource> inputItem in inputItems.WithCancellation(cancellationToken))
					{
						var stream = ExecuteItem(throttler, inputItem, cancellationToken);
						lock (runningStreams)
						{
							runningStreams.Add(ConsumeSubStream(stream));
						}
					}

					await Task.WhenAll(runningStreams);

					await nextLock.WaitAsync(cancellationToken);
					try
					{
						await multicastStream.Complete();
					}
					finally
					{
						nextLock.Release();
					}
				}
				catch (Exception e)
				{
					await multicastStream.Error(e);
				}
			}

			async Task ConsumeSubStream(IAsyncEnumerable<TResult> source)
			{
				try
				{
					await foreach (var item in source.WithCancellation(cancellationToken))
					{
						await nextLock.WaitAsync(cancellationToken);
						try
						{
							await multicastStream.Next(item);
						}
						finally
						{
							nextLock.Release();
						}
					}
				}
				catch (Exception e)
				{
					await multicastStream.Error(e);
				}
			}
		}

		private async IAsyncEnumerable<TResult> ExecuteItem(SemaphoreSlim throttler, AsyncStreamQueueItem<TResult, TSource> item, [EnumeratorCancellation] CancellationToken cancellationToken=default)
		{
			try
			{
				bool canContinue = true;
				try
				{
					await throttler.WaitAsync(cancellationToken);
				}
				catch (OperationCanceledException)
				{
					canContinue = false;
				}

				if (canContinue)
				{
					IAsyncEnumerable<TResult> stream = item.StreamProducer(item.Source);
					await foreach (TResult streamItem in stream.WithCancellation(cancellationToken))
					{
						yield return streamItem;
					}
				}
			}
			
			finally
			{
				throttler.Release();
			}
		}
	}
}