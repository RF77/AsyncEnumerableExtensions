// Copyright (c) David Karnok & Contributors.
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System.Linq;
using Xunit;

namespace AsyncEnumerableExtensions.Tests.Karnok
{
    public class NeverTest
    {
        /*
        [Fact(Skip = "The task of MoveNextAsync never completes and thus DisposeAsync won't either")]
        public async void Never()
        {
            await AsyncEnum.Never<int>()
                .Timeout(TimeSpan.FromMilliseconds(100))
                .AssertFailure(typeof(TimeoutException));
        }
        */

        [Fact]
        public void Normal()
        {
            var en = AsyncEnum.Never<int>().GetAsyncEnumerator(default);

            Assert.Equal(0, AsyncEnum.Never<int>().GetAsyncEnumerator(default).Current);

            // no await as the test would never end otherwise

            en.MoveNextAsync();
            en.DisposeAsync();
        }
    }
}
