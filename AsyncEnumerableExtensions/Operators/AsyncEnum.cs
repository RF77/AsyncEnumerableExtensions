// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
			return ToReplayQueueImpl().HandleInDispatcher(cancellationToken);

			IAsyncEnumerable<T> ToReplayQueueImpl()
			{
				var queue = new ReplayAsyncEnumerable<T>();

				FillQueue(source, queue, cancellationToken);

				return queue;
			}
		}

		/// <summary>
		/// If the caller runs in a dispatcher thread (one with a synchronization context) a stream running in another thread will continue in the dispatcher thread
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async IAsyncEnumerable<T> HandleInDispatcher<T>(this IAsyncEnumerable<T> source,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var item in source.WithCancellation(cancellationToken))
			{
				yield return item;
			}
		}


		//TODO: is not working!!!!!
		/// <summary>
		/// Stream will be handled in a worker or dispatcher thread (unknown)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		internal static async IAsyncEnumerable<T> HandleInThreadPoolThread<T>(this IAsyncEnumerable<T> source,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var stream = await Task.Run( () => RunInBackgroundThread(source, cancellationToken), cancellationToken);

			await foreach (var item in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
			{
				yield return item;
			}
		}

		private static async IAsyncEnumerable<T> RunInBackgroundThread<T>(IAsyncEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach(var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
			{
				yield return item;
			}
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
			return ToReplayQueueImpl().HandleInDispatcher(cancellationToken);

			IAsyncEnumerable<T> ToReplayQueueImpl()
			{
				var queue = new ReplayAsyncEnumerable<T>(maxSize);

				FillQueue(source, queue, cancellationToken);

				return queue;
			}
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
			return ToReplayQueueImpl().HandleInDispatcher(cancellationToken);

			IAsyncEnumerable<T> ToReplayQueueImpl()
			{
				var queue = new ReplayAsyncEnumerable<T>(maxSize, maxAge);

				FillQueue(source, queue, cancellationToken);

				return queue;
			}
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
			return ToReplayQueueImpl().HandleInDispatcher(cancellationToken);

			IAsyncEnumerable<T> ToReplayQueueImpl()
			{
				var queue = new ReplayAsyncEnumerable<T>(maxAge);

				FillQueue(source, queue, cancellationToken);

				return queue;
			}
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
			return ToMulticastQueueImpl().HandleInDispatcher(cancellationToken);

			IAsyncEnumerable<T> ToMulticastQueueImpl()
			{
				var queue = new MulticastAsyncEnumerable<T>();

				FillQueue(source, queue, cancellationToken);

				return queue;
			}
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

		/// <summary>
		/// Push received items continuously to the specified list.
		/// so far not optimized for performance! Consider to use a queue
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source">source async enumerable</param>
		/// <param name="list">Items are added to this list</param>
		/// <param name="maxSize">0 means unlimited (default) or specify max size for list</param>
		/// <returns></returns>
		public static IAsyncEnumerable<TSource> PushToList<TSource>(
			this IAsyncEnumerable<TSource> source, IList<TSource> list, int maxSize = 0)
		{
			return source.Do(i =>
			{
				list.Add(i);
				while (maxSize > 0 && list.Count > maxSize)
				{
					list.RemoveAt(0);
				}
			});
		}

		/// <summary>
		/// Push received items continuously to the specified list.
		/// so far not optimized for performance! Consider to use a queue
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source">source async enumerable</param>
		/// <param name="list">Items are added to this list</param>
		/// <param name="maxSizeFunc">func returning the max items for  list</param>
		/// <returns></returns>
		public static IAsyncEnumerable<TSource> PushToList<TSource>(
			this IAsyncEnumerable<TSource> source, IList<TSource> list, Func<int> maxSizeFunc)
		{
			return source.Do(i =>
			{
				list.Add(i);
				while (list.Count > maxSizeFunc())
				{
					list.RemoveAt(0);
				}
			});
		}


		/// <summary>
		/// Push received items continuously to the specified queue.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source"></param>
		/// <param name="queue">Items are enqueued to this queue</param>
		/// <param name="maxSize">0 means unlimited (default) or specify max size for queue</param>
		/// <returns></returns>
		public static IAsyncEnumerable<TSource> PushToQueue<TSource>(
			this IAsyncEnumerable<TSource> source, Queue<TSource> queue, int maxSize = 0)
		{
			return source.Do(i =>
			{
				queue.Enqueue(i);
				while (maxSize > 0 && queue.Count > maxSize)
				{
					queue.Dequeue();
				}
			});
		}

		/// <summary>
		/// Push received items continuously to the specified queue.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source"></param>
		/// <param name="queue">Items are enqueued to this queue</param>
		/// <param name="maxSizeFunc">func returning the max items for queue</param>
		/// <returns></returns>
		public static IAsyncEnumerable<TSource> PushToQueue<TSource>(
			this IAsyncEnumerable<TSource> source, Queue<TSource> queue, Func<int> maxSizeFunc)
		{
			return source.Do(i =>
			{
				queue.Enqueue(i);
				while (queue.Count > maxSizeFunc())
				{
					queue.Dequeue();
				}
			});
		}

	}
}
