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

using System.Linq;
using System.Threading.Tasks;
using Time.AsyncEnumerable.Extensions.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests.Operators
{
    public class Merge : UnitTestBase
    {
        public Merge(ITestOutputHelper output) : base(output)
        {
        }        
        
        [Fact]
        public async Task MergeConcurrentlyOf4Sources()
        {
            var firstStream = AsyncEnum.Just(1);
            var secondStream = AsyncEnum.Just(2);
            var thirdStream = AsyncEnum.Just(3);
            var forthStream = AsyncEnum.Just(4);

            var list = await firstStream.MergeConcurrently(secondStream, thirdStream, forthStream).ToListAsync();

            Assert.True(list.Count == 4);
        }


    }
}
