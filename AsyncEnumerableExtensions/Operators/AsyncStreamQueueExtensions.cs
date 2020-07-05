using System.Collections.Generic;
using System.Threading;
using AsyncEnumerableExtensions.Queue;

namespace System.Linq
{
	public static class AsyncStreamQueueExtensions
	{
		public static IAsyncEnumerable<TResult> ForEachAsAsyncEnumerable<TResult, TSource>(
			this IEnumerable<TSource> source,
			Func<TSource, IAsyncEnumerable<TResult>> function,
			int maxConcurrentTasks,
			CancellationToken cancellationToken = default)
		{
			var queue = new AsyncStreamQueue<TResult, TSource>();
			return queue.ExecuteParallelAsync(source.Select(sourceItem => new AsyncStreamQueueItem<TResult, TSource>(sourceItem, function)), maxConcurrentTasks, cancellationToken);
		}

		public static IAsyncEnumerable<TResult> ForEachAsAsyncEnumerable<TResult, TSource>(
			this IAsyncEnumerable<TSource> source,
			Func<TSource, IAsyncEnumerable<TResult>> function,
			int maxConcurrentTasks,
			CancellationToken cancellationToken = default)
		{
			var queue = new AsyncStreamQueue<TResult, TSource>();
			return queue.ExecuteParallelAsync(source.Select(sourceItem => new AsyncStreamQueueItem<TResult, TSource>(sourceItem, function)), maxConcurrentTasks, cancellationToken);
		}
	}
}