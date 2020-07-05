// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Xunit;

namespace AsyncEnumerableExtensions.Tests.Karnok
{
    public class IntervalTest
    {
        [Fact]
        public async void Normal()
        {
            await AsyncEnum.Interval(TimeSpan.FromMilliseconds(100))
                .Take(5)
                .AssertResult(0, 1, 2, 3, 4);
        }

        [Fact]
        public async void Normal_initial()
        {
            await AsyncEnum.Interval(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100))
                .Take(5)
                .AssertResult(0, 1, 2, 3, 4);
        }

        [Fact]
        public async void Range()
        {
            await AsyncEnum.Interval(1, 5, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(100))
                .AssertResult(1, 2, 3, 4, 5);
        }
    }
}
