// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Interactive.Async.Karnok;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AsyncEnumerableExtensions.Tests.Karnok
{
    public class MergeTest
    {
        [Fact]
        public async void Empty()
        {
            await AsyncEnum.MergeConcurrently<int>(
                )
                .AssertResult();
        }

        [Fact]
        public async void Solo()
        {
            await AsyncEnum.MergeConcurrently(
                    AsyncEnumerable.Range(1, 5)
                )
                .AssertResult(1, 2, 3, 4, 5);
        }

        [Fact]
        public async void Normal()
        {
            await AsyncEnum.MergeConcurrently(
                    AsyncEnumerable.Range(1, 5),
                    AsyncEnumerable.Range(6, 5)
                )
                .AssertResultSet(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Fact]
        public async void Normal_Uneven_1()
        {
            await AsyncEnum.MergeConcurrently(
                    AsyncEnumerable.Range(1, 5),
                    AsyncEnumerable.Range(6, 4)
                )
                .AssertResultSet(1, 2, 3, 4, 5, 6, 7, 8, 9);
        }

        [Fact]
        public async void Normal_Uneven_2()
        {
            await AsyncEnum.MergeConcurrently(
                    AsyncEnumerable.Range(1, 4),
                    AsyncEnumerable.Range(6, 5)
                )
                .AssertResultSet(1, 2, 3, 4, 6, 7, 8, 9, 10);
        }

        [Fact]
        public async void Error()
        {
            await AsyncEnum.MergeConcurrently(
                    AsyncEnumerable.Range(1, 5),
                    AsyncEnum.Error<int>(new InvalidOperationException())
                )
                .AssertFailure(typeof(InvalidOperationException), 1, 2, 3, 4, 5);
        }

        [Fact]
        public async void Push()
        {
            for (var i = 0; i < 10; i++)
            {
                var push = new MulticastAsyncEnumerable<int>();

                var en = AsyncEnum.MergeConcurrently(
                        push.Where(v => v % 2 == 0), 
                        push.Where(v => v % 2 != 0)
                    )
                    .ToListAsync();

                var t = Task.Run(async () =>
                {
                    for (var j = 0; j < 100_000; j++)
                    {
                        await push.Next(j);
                    }
                    await push.Complete();
                });

                var list = await en;

                await t;

                var set = new HashSet<int>(list);

                Assert.Equal(100_000, set.Count);
            }
        }

        [Fact]
        public async void Multicast_Merge()
        {
            for (var i = 0; i < 100000; i++)
            {
                await AsyncEnumerable.Range(1, 5)
                    .Publish(a => a.Take(3).MergeConcurrently(a.Skip(3)))
                    .AssertResultSet(1, 2, 3, 4, 5);
            }
        }


        [Fact]
        public async void Async_Normal()
        {
            await
                AsyncEnum.FromArray(
                    AsyncEnumerable.Range(1, 3),
                    AsyncEnumerable.Empty<int>(),
                    AsyncEnum.FromArray(4, 5, 6, 7),
                    AsyncEnumerable.Empty<int>(),
                    AsyncEnum.Just(8),
                    AsyncEnum.FromEnumerable(new[] { 9, 10 }),
                    AsyncEnumerable.Empty<int>()
                )
                .Merge()
                .AssertResultSet(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }

        [Fact]
        public async void Take()
        {
            await AsyncEnum.MergeConcurrently(
                    AsyncEnum.Timer(TimeSpan.FromMilliseconds(100)),
                    AsyncEnum.Timer(TimeSpan.FromMilliseconds(200))
                )
                .Take(1)
                .AssertResult(0L);
        }
    }
}
