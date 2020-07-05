// Copyright (c) Atos
// Licensed under MIT License.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Interactive.Async.Karnok;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    /// <summary>
    /// New Extensions
    /// </summary>
    public static partial class AsyncEnumerableEx
    {
        /// <summary>
        /// Source stream will be cached for multiple enumerations of later consumers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var queue =  new ReplayAsyncEnumerable<T>();

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
        public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, int maxSize, CancellationToken cancellationToken = default)
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
        public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, int maxSize, TimeSpan maxAge, CancellationToken cancellationToken = default)
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
        public static IAsyncEnumerable<T> ToReplayQueue<T>(this IAsyncEnumerable<T> source, TimeSpan maxAge, CancellationToken cancellationToken = default)
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
        public static IAsyncEnumerable<T> ToMulticastQueue<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var queue = new MulticastAsyncEnumerable<T>();

            FillQueue(source, queue, cancellationToken);

            return queue;
        }

        private static async void FillQueue<T>(IAsyncEnumerable<T> source, IAsyncConsumer<T> queue, CancellationToken cancellationToken = default)
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

    }
}
