// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;
using AsyncEnumerableExtensions.Karnok.impl;
using Timer = AsyncEnumerableExtensions.Karnok.impl.Timer;

namespace System.Linq
{
	/// <summary>
	///     Factory and extension methods for working with <see cref="IAsyncEnumerable{T}" />s.
	/// </summary>
	public static partial class AsyncEnum
	{
		/// <summary>
		///     Consume the source async sequence by emitting items and terminal signals via
		///     the given <see cref="IAsyncConsumer{T}" /> consumer.
		/// </summary>
		/// <typeparam name="TSource">The element type.</typeparam>
		/// <param name="source">The source sequence to consume.</param>
		/// <param name="consumer">The push-awaitable consumer.</param>
		/// <param name="ct">The optional cancellation token to stop the consumption.</param>
		/// <returns>The task that is completed when the consumption terminates.</returns>
		public static ValueTask Consume<TSource>(this IAsyncEnumerable<TSource> source,
			IAsyncConsumer<TSource> consumer, CancellationToken ct = default)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(consumer, nameof(consumer));
			return ForEach.Consume(source, consumer, ct);
		}

		/// <summary>
		///     Wraps an array of items into an async sequence that emits its elements.
		/// </summary>
		/// <typeparam name="TValue">The element type.</typeparam>
		/// <param name="values">The params array of values to emit.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TValue> FromArray<TValue>(params TValue[] values)
		{
			RequireNonNull(values, nameof(values));
			return new FromArray<TValue>(values);
		}

		/// <summary>
		///     Signals a 0L and completes after the specified time delay.
		/// </summary>
		/// <param name="delay">The time delay before emitting 0L and completing.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<long> Timer(TimeSpan delay)
		{
			return new Timer(delay);
		}

		/// <summary>
		///     Signals a 0L and completes after the specified time delay, allowing external
		///     cancellation via the given CancellationToken.
		/// </summary>
		/// <param name="delay">The time delay before emitting 0L and completing.</param>
		/// <param name="token">The token to cancel the timer.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		/// <remarks>
		///     Note that the <see cref="CancellationToken" /> is shared across all instantiations of the
		///     async sequence and thus it is recommended this Timer is created in a deferred manner,
		///     such as <see cref="AsyncEnum.Defer{T}" />.
		/// </remarks>
		public static IAsyncEnumerable<long> Timer(TimeSpan delay, CancellationToken token)
		{
			return new TimerCancellable(delay, token);
		}

		/// <summary>
		///     Calls the specified synchronous handler when the async sequence is disposed via
		///     <see cref="IAsyncDisposable.DisposeAsync" />.
		/// </summary>
		/// <typeparam name="TSource">The element type.</typeparam>
		/// <param name="source">The source sequence to relay items of.</param>
		/// <param name="handler">The handler called when the sequence is disposed.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TSource> DoOnDispose<TSource>(this IAsyncEnumerable<TSource> source,
			Action handler)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(handler, nameof(handler));
			return new DoOnDispose<TSource>(source, handler);
		}

		/// <summary>
		///     Calls the specified asynchronous handler when the async sequence is disposed via
		///     <see cref="IAsyncDisposable.DisposeAsync" />.
		/// </summary>
		/// <typeparam name="TSource">The element type.</typeparam>
		/// <param name="source">The source sequence to relay items of.</param>
		/// <param name="handler">The async handler called when the sequence is disposed.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TSource> DoOnDispose<TSource>(this IAsyncEnumerable<TSource> source,
			Func<ValueTask> handler)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(handler, nameof(handler));
			return new DoOnDisposeAsync<TSource>(source, handler);
		}

		/// <summary>
		///     Signals the given Exception immediately.
		/// </summary>
		/// <typeparam name="TResult">The intended element type of the sequence.</typeparam>
		/// <param name="exception">The exception to signal immediately.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TResult> OnError<TResult>(Exception exception)
		{
			RequireNonNull(exception, nameof(exception));
			return new Error<TResult>(exception);
		}

		/// <summary>
		///     Creates a resource and an actual async sequence with the help of this resource
		///     to be relayed and cleaned up afterwards.
		/// </summary>
		/// <typeparam name="TSource">The element type of the async sequence.</typeparam>
		/// <typeparam name="TResource">The resource type.</typeparam>
		/// <param name="resourceProvider">The function that returns the resource to be used.</param>
		/// <param name="sourceProvider">
		///     The function that produces the actual async sequence for
		///     the generated resource.
		/// </param>
		/// <param name="resourceCleanup">
		///     The action to cleanup the resource after the generated
		///     async sequence terminated.
		/// </param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TSource> Using<TSource, TResource>(Func<TResource> resourceProvider,
			Func<TResource, IAsyncEnumerable<TSource>> sourceProvider, Action<TResource> resourceCleanup)
		{
			RequireNonNull(resourceProvider, nameof(resourceProvider));
			RequireNonNull(sourceProvider, nameof(sourceProvider));
			RequireNonNull(resourceCleanup, nameof(resourceCleanup));
			return new Using<TSource, TResource>(resourceProvider, sourceProvider, resourceCleanup);
		}

		/// <summary>
		///     Create a task and emit its result/error as an async sequence.
		/// </summary>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="func">The function that returns a task that will create an item.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TResult> FromTask<TResult>(Func<Task<TResult>> func)
		{
			RequireNonNull(func, nameof(func));
			return new FromTaskFunc<TResult>(func);
		}

		/// <summary>
		///     Wrap an existing task and emit its result/error when it terminates as
		///     an async sequence.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="task">The task to wrap.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> FromTask<T>(Task<T> task)
		{
			RequireNonNull(task, nameof(task));
			return new FromTask<T>(task);
		}

		/// <summary>
		///     Creates an async sequence which emits items generated by the handler asynchronous function
		///     via the <see cref="IAsyncEmitter{T}" /> provided. The sequence ends when
		///     the task completes or fails.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="handler">
		///     The function that receives the emitter to be used for emitting items
		///     and should return a task which when terminates, the resulting async sequence terminates.
		/// </param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> Create<T>(Func<IAsyncEmitter<T>, Task> handler)
		{
			RequireNonNull(handler, nameof(handler));
			return new CreateEmitter<T>(handler);
		}


		/// <summary>
		///     Wraps an <see cref="IObservable{T}" /> and turns it into an async sequence that
		///     buffers all items until they are requested by the consumer of the async sequence.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="source">The source observable sequence to turn into an async sequence.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> FromObservable<T>(IObservable<T> source)
		{
			RequireNonNull(source, nameof(source));
			return new FromObservable<T>(source);
		}

		/// <summary>
		///     Wraps an <see cref="IObservable{T}" /> and turns it into an async sequence that
		///     buffers all items until they are requested by the consumer of the async sequence.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="source">The source observable sequence to turn into an async sequence.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IObservable<T> source)
		{
			RequireNonNull(source, nameof(source));
			return new FromObservable<T>(source);
		}

		/// <summary>
		///     Wraps an <see cref="IEnumerable{T}" /> sequence into an async sequence.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="source">The source sequence to turn into an async sequence.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> FromEnumerable<T>(IEnumerable<T> source)
		{
			// ReSharper disable once PossibleMultipleEnumeration
			RequireNonNull(source, nameof(source));
			// ReSharper disable once PossibleMultipleEnumeration
			return new FromEnumerable<T>(source);
		}

		/// <summary>
		///     Creates an async sequence that emits the given preexisting value.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="item">The item to emit.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> Just<T>(T item)
		{
			return new Just<T>(item);
		}

		/// <summary>
		///     Combines an accumulator and the next item through a function to produce
		///     a new accumulator of which the last accumulator value
		///     is the result item.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="source">The source to reduce into a single value.</param>
		/// <param name="reducer">
		///     The function that takes the previous accumulator value (or the first item), the current item and
		///     should produce the new accumulator value.
		/// </param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> Reduce<T>(this IAsyncEnumerable<T> source, Func<T, T, T> reducer)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(reducer, nameof(reducer));
			return new Reduce<T>(source, reducer);
		}

		/// <summary>
		///     Combines an accumulator and the next item through a function to produce
		///     a new accumulator of which the last accumulator value
		///     is the result item.
		/// </summary>
		/// <typeparam name="TSource">The element type.</typeparam>
		/// <typeparam name="TResult">The accumulator and result type.</typeparam>
		/// <param name="source">The source to reduce into a single value.</param>
		/// <param name="initialSupplier">The function returning the initial accumulator value.</param>
		/// <param name="reducer">
		///     The function that takes the previous accumulator value (or the first item), the current item and
		///     should produce the new accumulator value.
		/// </param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TResult> Reduce<TSource, TResult>(this IAsyncEnumerable<TSource> source,
			Func<TResult> initialSupplier, Func<TResult, TSource, TResult> reducer)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(initialSupplier, nameof(initialSupplier));
			RequireNonNull(reducer, nameof(reducer));
			return new ReduceSeed<TSource, TResult>(source, initialSupplier, reducer);
		}

		/// <summary>
		///     Generates a collection and calls an action with
		///     the current item to be combined into it and then
		///     the collection is emitted as the final result.
		/// </summary>
		/// <typeparam name="TSource">The element type of the source async sequence.</typeparam>
		/// <typeparam name="TCollection">The collection and result type.</typeparam>
		/// <param name="source">The source async sequence.</param>
		/// <param name="collectionSupplier">The function that generates the collection.</param>
		/// <param name="collector">The action called with the collection and the current source item.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TCollection> Collect<TSource, TCollection>(this IAsyncEnumerable<TSource> source,
			Func<TCollection> collectionSupplier, Action<TCollection, TSource> collector)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(collectionSupplier, nameof(collectionSupplier));
			RequireNonNull(collector, nameof(collector));
			return new Collect<TSource, TCollection>(source, collectionSupplier, collector);
		}

		/// <summary>
		///     Produces an ever increasing number periodically.
		/// </summary>
		/// <param name="period">The initial and in-between time period for when to signal the next value.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<long> Interval(TimeSpan period)
		{
			return new Interval(0, long.MinValue, period, period);
		}

		/// <summary>
		///     Produces an ever increasing number after an initial delay, then periodically.
		/// </summary>
		/// <param name="initialDelay">The initial delay before the first item emitted.</param>
		/// <param name="period">The initial and in-between time period for when to signal the next value.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<long> Interval(TimeSpan initialDelay, TimeSpan period)
		{
			return new Interval(0, long.MinValue, initialDelay, period);
		}

		/// <summary>
		///     Produces a number from a range of numbers periodically.
		/// </summary>
		/// <param name="start">The initial value.</param>
		/// <param name="count">The number of values to produce.</param>
		/// <param name="period">The initial and in-between time period for when to signal the next value.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<long> Interval(long start, long count, TimeSpan period)
		{
			return new Interval(start, start + count, period, period);
		}

		/// <summary>
		///     Produces a number from a range of numbers, the first after an initial delay and the rest periodically.
		/// </summary>
		/// <param name="start">The initial value.</param>
		/// <param name="count">The number of values to produce.</param>
		/// <param name="initialDelay">The delay before the first value is emitted.</param>
		/// <param name="period">The initial and in-between time period for when to signal the next value.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<long> Interval(long start, long count, TimeSpan initialDelay, TimeSpan period)
		{
			return new Interval(start, start + count, initialDelay, period);
		}

		/// <summary>
		///     Resumes with another async sequence if the source sequence has no items.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="source">The source async sequence that could be empty.</param>
		/// <param name="other">The fallback async sequence if the source turns out to be empty.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> SwitchIfEmpty<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> other)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(other, nameof(other));
			return new SwitchIfEmpty<T>(source, other);
		}

		/// <summary>
		///     Periodically take the latest item from the source async sequence and relay it.
		/// </summary>
		/// <typeparam name="T">The element type of the async sequence.</typeparam>
		/// <param name="source">The source async sequence to sample.</param>
		/// <param name="period">The sampling period.</param>
		/// <param name="emitLast">Emit the very last item, even if the sampling period has not passed.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<T> Sample<T>(this IAsyncEnumerable<T> source, TimeSpan period,
			bool emitLast = false)
		{
			RequireNonNull(source, nameof(source));
			return new Sample<T>(source, period, emitLast);
		}

		/// <summary>
		///     Starts buffering up to the specified amount from the source async sequence
		///     and tries to keep this buffer filled in so with (75% limit) that a slow consumer can get
		///     the items of the faster producer much earlier.
		/// </summary>
		/// <typeparam name="T">The element type of the async sequences.</typeparam>
		/// <param name="source">The source to prefetch items of.</param>
		/// <param name="prefetch">The number of items to prefetch at most</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<T> Prefetch<T>(this IAsyncEnumerable<T> source, int prefetch)
		{
			return Prefetch(source, prefetch, prefetch - (prefetch >> 2)); // 75%
		}

		/// <summary>
		///     Starts buffering up to the specified amount from the source async sequence
		///     and tries to keep this buffer filled in so that a slow consumer can get
		///     the items of the faster producer much earlier.
		/// </summary>
		/// <typeparam name="T">The element type of the async sequences.</typeparam>
		/// <param name="source">The source to prefetch items of.</param>
		/// <param name="prefetch">The number of items to prefetch at most</param>
		/// <param name="limit">
		///     The number of consumed items after which more items should be
		///     pulled from the source.
		/// </param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<T> Prefetch<T>(this IAsyncEnumerable<T> source, int prefetch, int limit)
		{
			RequireNonNull(source, nameof(source));
			RequirePositive(prefetch, nameof(prefetch));
			RequirePositive(limit, nameof(limit));
			return new Prefetch<T>(source, prefetch, limit);
		}

		/// <summary>
		///     Emit the latest item if there were no newer items from the source async
		///     sequence within the given delay period.
		/// </summary>
		/// <typeparam name="T">The element type of the source.</typeparam>
		/// <param name="source">The source async sequence.</param>
		/// <param name="delay">The time to wait after each item.</param>
		/// <param name="emitLast">
		///     If true, the very last item is emitted upon completion if
		///     the delay has not yet passed for it.
		/// </param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<T> Debounce<T>(this IAsyncEnumerable<T> source, TimeSpan delay,
			bool emitLast = false)
		{
			RequireNonNull(source, nameof(source));
			return new Debounce<T>(source, delay, emitLast);
		}

		/// <summary>
		///     Shares and multicasts the source async sequence for the duration
		///     of a function and relays items from the returned async sequence.
		/// </summary>
		/// <typeparam name="TSource">The element type of the source.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="source">The source async sequence to multicast.</param>
		/// <param name="func">
		///     The function to transform the sequence without
		///     consuming it multiple times.
		/// </param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TResult> Publish<TSource, TResult>(this IAsyncEnumerable<TSource> source,
			Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(func, nameof(func));
			return new Publish<TSource, TResult>(source, func);
		}

		/// <summary>
		///     Merges all async sequences running at once into a single
		///     serialized async sequence. Exceptions are thrown at the end of all merged sources
		/// </summary>
		/// <typeparam name="TSource">The element type of the sources and result.</typeparam>
		/// <param name="sources">The params array of source async sequences.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> MergeConcurrently<TSource>(params IAsyncEnumerable<TSource>[] sources)
		{
			RequireNonNull(sources, nameof(sources));
			switch (sources.Length)
			{
				case 0:
					return Empty<TSource>();
				case 1:
					return sources[0];
				default:
					return new Merge<TSource>(sources);
			}
		}

		/// <summary>
		///     Merges all async sequences running at once into a single
		///     serialized async sequence. Exceptions are thrown at the end of all merged sources
		/// </summary>
		/// <typeparam name="TSource">The element type of the sources and result.</typeparam>
		/// <param name="sources">The params array of source async sequences.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> MergeConcurrently<TSource>(
			this IEnumerable<IAsyncEnumerable<TSource>> sources)
		{
			return MergeConcurrently(sources.ToArray());
		}

		/// <summary>
		///     Merges all async sequences running at once into a single
		///     serialized async sequence. Exceptions are thrown at the end of all merged sources
		/// </summary>
		public static IAsyncEnumerable<TSource> MergeConcurrently<TSource>(this IAsyncEnumerable<TSource> firstSource,
			IAsyncEnumerable<TSource> secondSource,
			params IAsyncEnumerable<TSource>[] moreSources)
		{
			var allSources = new IAsyncEnumerable<TSource>[moreSources.Length + 2];
			allSources[0] = firstSource;
			allSources[1] = secondSource;

			for (var i = 0; i < moreSources.Length; i++) allSources[i + 2] = moreSources[i];

			return MergeConcurrently(allSources);
		}


		/// <summary>
		///     Returns a shared instance of an empty async sequence.
		/// </summary>
		/// <typeparam name="T">The target element type.</typeparam>
		/// <returns>The shared empty IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> Empty<T>()
		{
			return AsyncEnumerableExtensions.Karnok.impl.Empty<T>.Instance;
		}

		/// <summary>
		///     Signals the given Exception immediately.
		/// </summary>
		/// <typeparam name="TResult">The intended element type of the sequence.</typeparam>
		/// <param name="exception">The exception to signal immediately.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TResult> Error<TResult>(Exception exception)
		{
			RequireNonNull(exception, nameof(exception));
			return new Error<TResult>(exception);
		}

		/// <summary>
		///     Returns a shared instance of an async sequence that never produces any items or terminates.
		/// </summary>
		/// <typeparam name="T">The target element type.</typeparam>
		/// <returns>The shared non-signaling IAsyncEnumerable instance.</returns>
		/// <remarks>
		///     Note that the async sequence API doesn't really support a never emitting source because
		///     such source never completes its MoveNextAsync and thus DisposeAsync can't be called.
		/// </remarks>
		public static IAsyncEnumerable<T> Never<T>()
		{
			return AsyncEnumerableExtensions.Karnok.impl.Never<T>.Instance;
		}

		/// <summary>
		///     Continue with another async sequence once the main sequence completes.
		/// </summary>
		/// <typeparam name="T">The shared element type of the async sequences.</typeparam>
		/// <param name="source">The main source async sequences.</param>
		/// <param name="other">The next async source sequence.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<T> ConcatWith<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> other)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(other, nameof(other));
			return new[] {source, other}.Concat();
		}

		/// <summary>
		///     Generates a range of integer values as an async sequence.
		/// </summary>
		/// <param name="start">The starting value.</param>
		/// <param name="count">The number of items to generate.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<int> Range(int start, int count)
		{
			return AsyncEnumerable.Range(start, count);
		}

		/// <summary>
		///     Maps the items of the source async sequence into async sequences and then
		///     merges the elements of those inner sequences into a one sequence.
		/// </summary>
		/// <typeparam name="TSource">The element type of the source async sequence.</typeparam>
		/// <typeparam name="TResult">The element type of the inner async sequences.</typeparam>
		/// <param name="source">The source that emits items to be mapped.</param>
		/// <param name="mapper">
		///     The function that takes an source item and should return
		///     an async sequence to be merged.
		/// </param>
		/// <param name="maxConcurrency">The maximum number of inner sequences to run at once.</param>
		/// <param name="prefetch">The number of items to prefetch from each inner async sequence.</param>
		/// <returns>The new IAsyncEnumerable instance.</returns>
		public static IAsyncEnumerable<TResult> FlatMap<TSource, TResult>(this IAsyncEnumerable<TSource> source,
			Func<TSource, IAsyncEnumerable<TResult>> mapper, int maxConcurrency = int.MaxValue, int prefetch = 32)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(mapper, nameof(mapper));
			RequirePositive(maxConcurrency, nameof(maxConcurrency));
			RequirePositive(prefetch, nameof(prefetch));
			return new FlatMap<TSource, TResult>(source, mapper, maxConcurrency, prefetch);
		}

		/// <summary>
		///     Shares and multicasts the source async sequence, caching some or
		///     all of its items, for the duration
		///     of a function and relays items from the returned async sequence.
		/// </summary>
		/// <typeparam name="TSource">The element type of the source.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="source">The source async sequence to multicast.</param>
		/// <param name="func">
		///     The function to transform the sequence without
		///     consuming it multiple times.
		/// </param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TResult> Replay<TSource, TResult>(this IAsyncEnumerable<TSource> source,
			Func<IAsyncEnumerable<TSource>, IAsyncEnumerable<TResult>> func)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(func, nameof(func));
			return new Replay<TSource, TResult>(source, func);
		}

		/// <summary>
		///     Switches to a newer source async sequence, disposing the old one,
		///     when the source async sequence produces
		///     a value and is mapped to an async sequence via a function.
		/// </summary>
		/// <typeparam name="TSource">The source value type.</typeparam>
		/// <typeparam name="TResult">The result value type.</typeparam>
		/// <param name="source">The source to map into async sequences and switch between them.</param>
		/// <param name="mapper">
		///     The function that receives the source item and should
		///     return an async sequence to be run.
		/// </param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TResult> SwitchMap<TSource, TResult>(this IAsyncEnumerable<TSource> source,
			Func<TSource, IAsyncEnumerable<TResult>> mapper)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(mapper, nameof(mapper));
			return new SwitchMap<TSource, TResult>(source, mapper);
		}

		/// <summary>
		///     Switches to a newer source async sequence, disposing the old one,
		///     when the source async sequence produces a new inner async sequence.
		/// </summary>
		/// <typeparam name="TSource">The source value type.</typeparam>
		/// <param name="sources">The async sequence of async sequences to switch between.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> Switch<TSource>(
			this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources)
		{
			RequireNonNull(sources, nameof(sources));
			return sources.SwitchMap(v => v);
		}


		/// <summary>
		///     Runs multiple async sequences mapped from the upstream source into
		///     inner async sequences via a function at once and relays items from each,
		///     one after the previous source terminated.
		/// </summary>
		/// <typeparam name="TSource">The element type of the source.</typeparam>
		/// <typeparam name="TResult">The result type.</typeparam>
		/// <param name="source">The source async sequence to map and concatenate eagerly.</param>
		/// <param name="mapper">
		///     The function that receives an upstream item and should
		///     return an async sequence to be consumed once the previous async sequence terminated.
		/// </param>
		/// <param name="maxConcurrency">The maximum number of inner async sources to run at once.</param>
		/// <param name="prefetch">The number of items to prefetch from each inner source.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TResult> ConcatMapEager<TSource, TResult>(this IAsyncEnumerable<TSource> source,
			Func<TSource, IAsyncEnumerable<TResult>> mapper, int maxConcurrency = 32, int prefetch = 32)
		{
			RequireNonNull(source, nameof(source));
			RequireNonNull(mapper, nameof(mapper));
			RequirePositive(maxConcurrency, nameof(maxConcurrency));
			RequirePositive(prefetch, nameof(prefetch));
			return new ConcatMapEager<TSource, TResult>(source, mapper, maxConcurrency, prefetch);
		}

		/// <summary>
		///     Runs multiple async sources at once and relays items, in order, from each
		///     one after the previous async source terminates.
		/// </summary>
		/// <typeparam name="TSource">The element type of the sources and result.</typeparam>
		/// <param name="sources">The params array of sources to run at once but concatenate in order.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> ConcatEager<TSource>(params IAsyncEnumerable<TSource>[] sources)
		{
			return FromArray(sources).ConcatMapEager(v => v);
		}

		/// <summary>
		///     Runs multiple async sources at once and relays items, in order, from each
		///     one after the previous async source terminates.
		/// </summary>
		/// <typeparam name="TSource">The element type of the sources and result.</typeparam>
		/// <param name="maxConcurrency">The maximum number of inner async sources to run at once.</param>
		/// <param name="sources">The params array of sources to run at once but concatenate in order.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> ConcatEager<TSource>(int maxConcurrency,
			params IAsyncEnumerable<TSource>[] sources)
		{
			return FromArray(sources).ConcatMapEager(v => v, maxConcurrency);
		}

		/// <summary>
		///     Runs multiple async sources at once and relays items, in order, from each
		///     one after the previous async source terminates.
		/// </summary>
		/// <typeparam name="TSource">The element type of the sources and result.</typeparam>
		/// <param name="maxConcurrency">The maximum number of inner async sources to run at once.</param>
		/// <param name="prefetch">The number of items to prefetch from each inner source.</param>
		/// <param name="sources">The params array of sources to run at once but concatenate in order.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> ConcatEager<TSource>(int maxConcurrency, int prefetch,
			params IAsyncEnumerable<TSource>[] sources)
		{
			return FromArray(sources).ConcatMapEager(v => v, maxConcurrency, prefetch);
		}

		/// <summary>
		///     Runs multiple async sources at once and relays items, in order, from each
		///     one after the previous async source terminates.
		/// </summary>
		/// <typeparam name="TSource">The element type of the sources and result.</typeparam>
		/// <param name="maxConcurrency">The maximum number of inner async sources to run at once.</param>
		/// <param name="prefetch">The number of items to prefetch from each inner source.</param>
		/// <param name="sources">The params array of sources to run at once but concatenate in order.</param>
		/// <returns>The new IAsyncEnumerable sequence.</returns>
		public static IAsyncEnumerable<TSource> ConcatEager<TSource>(
			this IAsyncEnumerable<IAsyncEnumerable<TSource>> sources, int maxConcurrency = 32, int prefetch = 32)
		{
			return sources.ConcatMapEager(v => v, maxConcurrency, prefetch);
		}

		#region - ValidationHelper -

		/// <summary>
		///     Checks if the argument is null and throws.
		/// </summary>
		/// <typeparam name="TValue">The class type.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <param name="argumentName">The argument name for the ArgumentNullException</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value" /> is null.</exception>
		/// <returns>The value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
		internal static void RequireNonNull<TValue>(TValue value, string argumentName) where TValue : class
		{
			if (value == null) throw new ArgumentNullException(argumentName);
		}

		/// <summary>
		///     Check if the value is positive.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="argumentName">The argument name for the ArgumentNullException</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value" /> is null.</exception>
		/// <returns>The value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequirePositive(int value, string argumentName)
		{
			if (value <= 0) throw new ArgumentOutOfRangeException(argumentName, value, "must be positive");
		}

		/// <summary>
		///     Check if the value is positive.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="argumentName">The argument name for the ArgumentNullException</param>
		/// <exception cref="ArgumentNullException">If <paramref name="value" /> is null.</exception>
		/// <returns>The value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequirePositive(long value, string argumentName)
		{
			if (value <= 0L) throw new ArgumentOutOfRangeException(argumentName, value, "must be positive");
		}

		#endregion - ValidationHelper -
	}
}