// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Xunit;

namespace AsyncEnumerableExtensions.Tests.Karnok
{
    public class ErrorTest
    {
        [Fact]
        public async void Calls()
        {
            var en = AsyncEnum.Error<int>(new InvalidOperationException()).GetAsyncEnumerator(default);

            try
            {
                await en.MoveNextAsync();
                Assert.False(true, "Should have thrown");
            }
            catch (InvalidOperationException)
            {
                // expected;
            }
            
            Assert.Equal(0, en.Current);
            Assert.False(await en.MoveNextAsync());

            await en.DisposeAsync();
        }
    }
}
