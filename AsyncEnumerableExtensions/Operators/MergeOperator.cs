// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;
using AsyncEnumerableExtensions.Karnok.impl;

namespace System.Linq
{
	public static class MergeOperator
	{
		/// <summary>
		/// Merges elements from all inner async-enumerable sequences concurrently into a single async-enumerable sequence.
		/// Enumeration stops on first thrown Exception of any inner streams.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the source sequences.</typeparam>
		/// <param name="sources">Observable sequence of inner async-enumerable sequences.</param>
		/// <returns>The async-enumerable sequence that merges the elements of the inner sequences.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="sources" /> is null.</exception>
		public static IAsyncEnumerable<T> MergeConcurrentlyUntilFirstException<T>(this IAsyncEnumerable<IAsyncEnumerable<T>> sources)
		{
			var queue = new UnicastAsyncEnumerable<T>();
			var tasks = new List<Task>();

			StreamSources();
			
			return queue;

			async void StreamSources()
			{
				try
				{
					await foreach (var stream in sources)
					{
						tasks.Add(StreamSource(stream));
					}

					// After the last stream arrived, we have to wait until all of those streams have completely finished with streaming
					await Task.WhenAll(tasks);
				}
				catch (OperationCanceledException)
				{
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

			async Task StreamSource(IAsyncEnumerable<T> source)
			{
				try
				{
					await foreach (var item in source)
					{
						await queue.Next(item);
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					await queue.Error(e);
				}
			}
		}

		/// <summary>
		/// Merges elements from all inner async-enumerable sequences concurrently into a single async-enumerable sequence.
		/// All thrown Exception are thrown at the end of the enumeration of all streams. Multiple Exceptions are thrown as AggregateException.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the source sequences.</typeparam>
		/// <param name="sources">Observable sequence of inner async-enumerable sequences.</param>
		/// <param name="cancellationToken">Cancellation Token to cancel the enumeration</param>
		/// <returns>The async-enumerable sequence that merges the elements of the inner sequences.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="sources" /> is null.</exception>
		public static IAsyncEnumerable<T> MergeConcurrently<T>(this IAsyncEnumerable<IAsyncEnumerable<T>> sources, CancellationToken cancellationToken = default)
		{
			var queue = new UnicastAsyncEnumerable<T>();
			var tasks = new List<Task>();
			Exception thrownException = null;

			StreamSources();

			return queue;

			async void StreamSources()
			{
				try
				{
					await foreach (var stream in sources.WithCancellation(cancellationToken))
					{
						tasks.Add(StreamSource(stream));
					}

					// After the last stream arrived, we have to wait until all of those streams have completely finished with streaming
					await Task.WhenAll(tasks);

					if (thrownException != null)
					{
						await queue.Error(thrownException);
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					ExceptionHelper.AddException(ref thrownException, e);
					await queue.Error(thrownException);
				}
				finally
				{
					await queue.Complete();
				}
			}

			async Task StreamSource(IAsyncEnumerable<T> source)
			{
				try
				{
					await foreach (var item in source.WithCancellation(cancellationToken))
					{
						await queue.Next(item);
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					ExceptionHelper.AddException(ref thrownException, e); ;
				}
			}
		}

		/// <summary>
		/// Merges elements from all inner async-enumerable sequences sequentially into a single async-enumerable sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
		/// <param name="sources">Observable sequence of inner async-enumerable sequences.</param>
		/// <returns>The async-enumerable sequence that merges the elements of the inner sequences.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="sources" /> is null.</exception>
		public static IAsyncEnumerable<T> MergeSequentially<T>(this IAsyncEnumerable<IAsyncEnumerable<T>> sources)
		{
			return sources.Merge();
		}
	}
}