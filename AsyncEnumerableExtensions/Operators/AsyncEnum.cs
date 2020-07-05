// Copyright (c) Atos
// Licensed under MIT License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;

namespace System.Linq
{
	/// <summary>
	/// New Extensions
	/// </summary>
	public static partial class AsyncEnum
	{
		/// <summary>
		/// Source stream will be cached for multiple enumerations of later consumers
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source,
			CancellationToken cancellationToken = default)
		{
			var queue = new ReplayAsyncEnumerable<T>();

			FillQueue(source, queue, cancellationToken);

			return queue;
		}

		/// <summary>
		/// Source stream will be cached for multiple enumerations of later consumers
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="maxSize">max Size of Queue, otherwise unbounded</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, int maxSize,
			CancellationToken cancellationToken = default)
		{
			var queue = new ReplayAsyncEnumerable<T>(maxSize);

			FillQueue(source, queue, cancellationToken);

			return queue;
		}

		/// <summary>
		/// Source stream will be cached for multiple enumerations of later consumers
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="maxSize">max Size of Queue, otherwise unbounded</param>
		/// <param name="maxAge">buffer items are no older than</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, int maxSize,
			TimeSpan maxAge, CancellationToken cancellationToken = default)
		{
			var queue = new ReplayAsyncEnumerable<T>(maxSize, maxAge);

			FillQueue(source, queue, cancellationToken);

			return queue;
		}

		/// <summary>
		/// Source stream will be cached for multiple enumerations of later consumers
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="maxAge">buffer items are no older than</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, TimeSpan maxAge,
			CancellationToken cancellationToken = default)
		{
			var queue = new ReplayAsyncEnumerable<T>(maxAge);

			FillQueue(source, queue, cancellationToken);

			return queue;
		}

		/// <summary>
		/// Kind of Multiplexer to multiple consumers (multiple enumeration) to provide the the same results
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static IAsyncEnumerable<T> ToMulticastQueue<T>(this IAsyncEnumerable<T> source,
			CancellationToken cancellationToken = default)
		{
			var queue = new MulticastAsyncEnumerable<T>();

			FillQueue(source, queue, cancellationToken);

			return queue;
		}

		private static async void FillQueue<T>(IAsyncEnumerable<T> source, IAsyncConsumer<T> queue,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await foreach (var item in source.WithCancellation(cancellationToken))
				{
					await queue.Next(item);
				}
			}
			catch (Exception e)
			{
				await queue.Error(e);
			}
			finally
			{
				await queue.Complete();
			}
		}

		/// <summary>
		/// Merges elements from all of the specified async-enumerable sequences into a single async-enumerable sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
		/// <param name="sources">Observable sequences.</param>
		/// <returns>The async-enumerable sequence that merges the elements of the async-enumerable sequences.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="sources"/> is null.</exception>
		public static IAsyncEnumerable<TSource> MergeConcurrentlyUntilFirstException<TSource>(
			params IAsyncEnumerable<TSource>[] sources)
		{
			return AsyncEnumerableEx.Merge(sources);
		}

		/// <summary>
		/// Merges elements from all async-enumerable sequences in the given enumerable sequence into a single async-enumerable sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
		/// <param name="sources">Enumerable sequence of async-enumerable sequences.</param>
		/// <returns>The async-enumerable sequence that merges the elements of the async-enumerable sequences.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="sources"/> is null.</exception>
		public static IAsyncEnumerable<TSource> MergeSequential<TSource>(
			this IEnumerable<IAsyncEnumerable<TSource>> sources)
		{
			return sources.Merge();
		}

	}
}
