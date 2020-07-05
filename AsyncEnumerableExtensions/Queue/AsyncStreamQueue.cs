// Projekt Achat
// Copyright (c) 2020 Atos AG
// Author: Fux, Rolf

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
	public class AsyncStreamQueue<TStreamItem, TSourceItem>
	{
		public IAsyncEnumerable<TStreamItem> ExecuteParallelAsync(IEnumerable<AsyncStreamQueueItem<TStreamItem, TSourceItem>> inputItems, int maxConcurrentTasks, CancellationToken cancellationToken)
		{
			var throttler = new SemaphoreSlim(maxConcurrentTasks);

			IEnumerable<IAsyncEnumerable<TStreamItem>> streams = inputItems.Select(
				inputItem => ExecuteItem(throttler, inputItem, cancellationToken));

			return streams.MergeConcurrently();
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public IAsyncEnumerable<TStreamItem> ExecuteParallelAsync(IAsyncEnumerable<AsyncStreamQueueItem<TStreamItem, TSourceItem>> inputItems, int maxConcurrentTasks, CancellationToken cancellationToken)
		{
			var throttler = new SemaphoreSlim(maxConcurrentTasks);

			var multicastStream = new MulticastAsyncEnumerable<TStreamItem>();

			bool allStreamsStarted = false;

			HashSet<IAsyncEnumerable<TStreamItem>> runningStreams = new HashSet<IAsyncEnumerable<TStreamItem>>();

			HandleInputStreams();

			return multicastStream;

			async void HandleInputStreams()
			{
				try
				{
					await foreach (AsyncStreamQueueItem<TStreamItem, TSourceItem> inputItem in inputItems.WithCancellation(cancellationToken))
					{
						var stream = ExecuteItem(throttler, inputItem, cancellationToken).Do(async i => await multicastStream.Next(i), async i => await multicastStream.Error(i));
#pragma warning disable 4014
						stream.DoOnDispose(async () => await RemoveStreamAndCloseMainStreamIfAllFinishedAsync(stream));
#pragma warning restore 4014
						runningStreams.Add(stream);
					}

					allStreamsStarted = true;
					await RemoveStreamAndCloseMainStreamIfAllFinishedAsync(null);
				}
				catch (Exception e)
				{
					await multicastStream.Error(e);
				}
			}

			async Task RemoveStreamAndCloseMainStreamIfAllFinishedAsync(IAsyncEnumerable<TStreamItem> stream)
			{
				if (stream != null)
				{
					runningStreams.Remove(stream);
				}

				if (allStreamsStarted && !runningStreams.Any())
				{
					await multicastStream.Complete();
				}
			}
		}

		private async IAsyncEnumerable<TStreamItem> ExecuteItem(SemaphoreSlim throttler, AsyncStreamQueueItem<TStreamItem, TSourceItem> item, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			await throttler.WaitAsync(cancellationToken);
			try
			{
				IAsyncEnumerable<TStreamItem> stream = item.StreamProducer(item.Source, cancellationToken);
				await foreach (TStreamItem streamItem in stream.WithCancellation(cancellationToken))
				{
					yield return streamItem;
				}
			}
			finally
			{
				throttler.Release();
			}
		}
	}
}