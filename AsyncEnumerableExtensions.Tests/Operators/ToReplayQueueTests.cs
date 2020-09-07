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
    public class ToReplayQueueTests : UnitTestBase
    {
        private static int _counter;
        private DateTime _startTime;
        private DateTime _lastTimeStamp;

        public ToReplayQueueTests(ITestOutputHelper output):base(output)
        {
            _startTime = DateTime.Now;
            _lastTimeStamp = _startTime;
        }

        private async IAsyncEnumerable<int> GetNumbers(int amount, [EnumeratorCancellation] CancellationToken token = default)
        {
            for (var i = 0; i < amount; i++)
            {
                Write($"yield {_counter}");
                if (token.IsCancellationRequested)
                {
                    Write($"GetNumbers Cancel");
                    yield break;
                }
                yield return _counter++;
                await Task.Delay(10);
            }
            Write($"GetNumbers finished");
        }

        private async IAsyncEnumerable<int> GetNumbersWithException(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                Write($"yield {_counter}");
                yield return _counter++;
                await Task.Delay(10);
                if (i == amount - 2)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        
        [Fact]
        public async Task MultipleEnumerations()
        {
            var stream = GetNumbersWithException(5);
            var replayedStream = stream.ToReplayQueue();

            WriteStreamToOutput(replayedStream);

            bool catchedException = false;

            try
            {
                var list = await replayedStream.ToListAsync();
            }
            catch (Exception e)
            {
                Write($"ToListAsync catched {e.GetType().Name} Exception");
                catchedException = true;
            }

            Assert.True(catchedException);
        }

        [Fact]
        public async Task EnumerationWithException()
        {
            var stream = GetNumbers(3);
            var replayedStream = stream.ToReplayQueue();

            WriteStreamToOutput(replayedStream);
            var task1 = replayedStream.ToListAsync();
            var task2 = replayedStream.ToListAsync();

            await Task.WhenAll(task1.AsTask(), task2.AsTask());

            var list1 = task1.Result;
            var list2 = task2.Result;

            Assert.True(list1.SequenceEqual(list2));
        }

        [Fact]
        public async Task EnumerationWithCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource(50);
            var cancellationTokenSource2 = new CancellationTokenSource(50);
            var cancellationTokenSource3 = new CancellationTokenSource(80);
            var stream = GetNumbers(20, cancellationTokenSource3.Token);
            var replayedStream = stream.ToReplayQueue(cancellationTokenSource.Token);

            WriteStreamToOutput(replayedStream, cancellationTokenSource2.Token);

            var list = await replayedStream.ToListAsync();
            Write($"List content is: {string.Join(", ", list)}");

            Write("Finished");
        }

        [Fact]
        public void EnumerationWithCancellationOfConsumersOnly()
        {
            AsyncContext.Run(async () =>
            {
                var weakQueue = await EnumerationWithCancellationOfConsumersOnlyMethod();

                for (int i = 0; i < 10; i++)
                {
                    GC.Collect();
                    await Task.Delay(20);
                }

                var isAlive = weakQueue.TryGetTarget(out var test);

                Write($"Queue is alive = {isAlive}");
            });

            Write("Finished");
        }

        private async Task<WeakReference<IAsyncEnumerable<int>>> EnumerationWithCancellationOfConsumersOnlyMethod()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationTokenSource2 = new CancellationTokenSource(120);
            var cancellationTokenSource3 = new CancellationTokenSource(50);
            var cancellationTokenSource4 = new CancellationTokenSource(80);
            var cancellationTokenSource5 = new CancellationTokenSource();
            var stream = GetNumbers(10, cancellationTokenSource.Token);
            var replayedStream = stream.ToReplayQueue(cancellationTokenSource2.Token);

            WriteStreamToOutput(replayedStream, cancellationTokenSource3.Token);
            WriteStreamToOutput(replayedStream, cancellationTokenSource4.Token);

            var list = await replayedStream.ToListAsync(cancellationTokenSource5.Token);
            Write($"List content is: {string.Join(", ", list)}");

            await Task.CompletedTask;

            return new WeakReference<IAsyncEnumerable<int>>(replayedStream);
        }

        [Fact]
        public async Task EnumerationWithTwoLists()
        {
            var stream = GetNumbers(20);
            var replayedStream = stream.ToReplayQueue();

            var list1 = await replayedStream.ToListAsync();
            var list2 = await replayedStream.ToListAsync();

            Assert.True(list1.SequenceEqual(list2));
        }

        [Fact]
        public async Task MulticastEnumeration2WithToLists()
        {
            var stream = GetNumbers(20);
            var replayedStream = stream.ToMulticastQueue();

            var task1 = replayedStream.ToListAsync();
            await Task.Delay(60);

            var cts = new CancellationTokenSource(60);
            var list2 = await replayedStream.ToListAsync(cts.Token);
            var list1 = await task1;

            Assert.False(list1.SequenceEqual(list2));
            Assert.False(list1.First() == list2.First());
            Assert.False(list1.Last() == list2.Last());

            var listHash = new HashSet<int>(list1);
            Assert.True(list2.All(i => listHash.Contains(i)));
        }

        [Fact]
        public async Task ReplayQueueWithMaxSize_EnumerateTo2Lists()
        {
            var amount = 25;
            var stream = GetNumbers(amount);
            var replayedStream = stream.ToReplayQueue(10);

            await Task.Delay(100);

            var task1 = replayedStream.ToListAsync();
            await Task.Delay(150);

            var cts = new CancellationTokenSource(40);
            var list2 = await replayedStream.ToListAsync(cts.Token);
            var list1 = await task1;

            Assert.False(list1.SequenceEqual(list2), "list1.SequenceEqual(list2)");
            Assert.False(list1.First() == list2.First(), "list1.First() == list2.First()");
            Assert.False(list1.Last() == list2.Last(), "list1.Last() == list2.Last()");

            var listHash = new HashSet<int>(list1);
            Assert.True(list2.All(i => listHash.Contains(i)), "list2.All(i => listHash.Contains(i))");
            Assert.True(list1.Count == amount);
        }

        [Fact]
        public async Task ReplayQueueWithMaxAge_EnumerateTo2Lists()
        {
            var amount = 25;
            var stream = GetNumbers(amount);
            var replayedStream = stream.ToReplayQueue(TimeSpan.FromMilliseconds(100));

            await Task.Delay(100);

            var task1 = replayedStream.ToListAsync();
            await Task.Delay(150);

            var cts = new CancellationTokenSource(40);
            var list2 = await replayedStream.ToListAsync(cts.Token);
            var list1 = await task1;

            Assert.False(list1.SequenceEqual(list2), "list1.SequenceEqual(list2)");
            Assert.False(list1.First() == list2.First(), "list1.First() == list2.First()");
            Assert.False(list1.Last() == list2.Last(), "list1.Last() == list2.Last()");

            var listHash = new HashSet<int>(list1);
            Assert.True(list2.All(i => listHash.Contains(i)), "list2.All(i => listHash.Contains(i))");
        }

        [Fact]
        public void ReplayQueueWithDispatcher_ReplayQueueIsStillInDispatcherThread()
        {
	        AsyncContext.Run(async () =>
	        {
		        var threadId = Thread.CurrentThread.ManagedThreadId;
		        var stream = GetNumbers(100);
		        stream = stream.Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var replayedStream = stream.ToReplayQueue().HandleInDispatcher().Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var list = await replayedStream.ToListAsync();

                Assert.True(list.Count == 100);
	        });
        }

        [Fact]
        public void HandleInDispatcher_ReplayQueueIsStillInDispatcherThread()
        {
	        AsyncContext.Run(async () =>
	        {
		        var threadId = Thread.CurrentThread.ManagedThreadId;
		        var stream = GetNumbers(100);
		        stream = stream.Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var replayedStream = stream.ToReplayQueue().HandleInDispatcher().Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var list = await replayedStream.ToListAsync();

		        Assert.True(list.Count == 100);
	        });
        }

        //[Fact]
        //public void HandleInThreadpoolThread()
        //{
	       // AsyncContext.Run(async () =>
	       // {
		      //  var threadId = Thread.CurrentThread.ManagedThreadId;
		      //  var stream = GetNumbers(100);
		      //  stream = stream.Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		      //  var replayedStream = stream.HandleInThreadpoolThread().Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId != threadId));
		      //  var list = await replayedStream.ToListAsync();

		      //  Assert.True(list.Count == 100);
	       // });
        //}

        [Fact]
        public void ReplayQueueWithTimeWithDispatcher_ReplayQueueIsStillInDispatcherThread()
        {
	        AsyncContext.Run(async () =>
	        {
		        var threadId = Thread.CurrentThread.ManagedThreadId;
		        var stream = GetNumbers(100);
		        stream = stream.Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var replayedStream = stream.ToReplayQueue(TimeSpan.FromMilliseconds(100)).Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var list = await replayedStream.ToListAsync();

		        Assert.True(list.Count == 100);
	        });
        }

        [Fact]
        public void MulticastQueueWithDispatcher_ReplayQueueIsStillInDispatcherThread()
        {
	        AsyncContext.Run(async () =>
	        {
		        var threadId = Thread.CurrentThread.ManagedThreadId;
		        var stream = GetNumbers(100);
		        stream = stream.Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var replayedStream = stream.ToMulticastQueue().Do(i => Assert.True(Thread.CurrentThread.ManagedThreadId == threadId));
		        var list = await replayedStream.ToListAsync();

		        Assert.True(list.Count > 90);
	        });
        }



        [Fact]
        public async Task ReplayQueueWithMaxAgeAndMaxItems_EnumerateTo2Lists()
        {
            var amount = 25;
            var stream = GetNumbers(amount);
            var replayedStream = stream.ToReplayQueue(6, TimeSpan.FromMilliseconds(100));

            await Task.Delay(100);

            var task1 = replayedStream.ToListAsync();
            await Task.Delay(150);

            var cts = new CancellationTokenSource(40);
            var list2 = await replayedStream.ToListAsync(cts.Token);
            var list1 = await task1;

            Assert.False(list1.SequenceEqual(list2), "list1.SequenceEqual(list2)");
            Assert.False(list1.First() == list2.First(), "list1.First() == list2.First()");
            Assert.False(list1.Last() == list2.Last(), "list1.Last() == list2.Last()");

            var listHash = new HashSet<int>(list1);
            Assert.True(list2.All(i => listHash.Contains(i)), "list2.All(i => listHash.Contains(i))");
        }

        [Fact]
        public async Task SimpleEnumerationWithCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource(50);
            var stream = GetNumbers(20, cancellationTokenSource.Token);

            await WriteStreamToOutputAsync(stream);

            Write("Finished");
        }

        [Fact]
        public async Task LastTest()
        {
            await foreach (var number in GetNumbers(20))
            {
                Write($"Consume slowly: {number}");
                await Task.Delay(50);
            }

            Write("Finished");
        }

        [Fact]
        public async Task SimpleEnumerationWithDelay()
        {
            var stream = GetNumbers(10);

            Write("Wait now for 100ms");

            await Task.Delay(100);

            await WriteStreamToOutputAsync(stream);

            //var list = await stream.ToListAsync(cancellationTokenSource3.Token);

            Write("Finished");
        }
    }

    //public static class MyExtensions
    //{
	   // public static IAsyncEnumerable<T> HandleOnDispatcher<T>(this IAsyncEnumerable<T> source)
	   // {

	   // }
    //}
}
