using System.Collections.Generic;
using System.Threading;
using AsyncEnumerableExtensions.Queue;

namespace System.Linq
{
	public static class AsyncStreamQueueExtensions
	{
		/// <summary>
		/// Based on an Enumerable Source a max of defined Calls the functions are made concurrently, but one stream for all returns
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source">Enumeration as input for the functions</param>
		/// <param name="function">Function to produce a part of the result stream</param>
		/// <param name="maxConcurrentTasks">Defines how many of the functions are executed concurrently</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>A stream containing the results of all calls</returns>
		public static IAsyncEnumerable<TResult> ForEachAsAsyncEnumerable<TResult, TSource>(
			this IEnumerable<TSource> source,
			Func<TSource, IAsyncEnumerable<TResult>> function,
			int maxConcurrentTasks,
			CancellationToken cancellationToken = default)
		{
			var queue = new AsyncStreamQueue<TResult, TSource>();
			return queue.ExecuteParallelAsync(source.Select(sourceItem => new AsyncStreamQueueItem<TResult, TSource>(sourceItem, function)), maxConcurrentTasks, cancellationToken);
		}

		/// <summary>
		/// Based on an async stream source a max of defined Calls the functions are made concurrently, but one stream for all returns
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source">Enumeration as input for the functions</param>
		/// <param name="function">Function to produce a part of the result stream</param>
		/// <param name="maxConcurrentTasks">Defines how many of the functions are executed concurrently</param>
		/// <param name="cancellationToken">Cancellation Token</param>
		/// <returns>A stream containing the results of all calls</returns>
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