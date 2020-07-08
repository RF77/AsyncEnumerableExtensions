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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AsyncEnumerableExtensions.Karnok;
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests
{
	public class UnicastEnumerableTests : UnitTestBase
	{

		[Fact]
		public async Task TestUniCastEnumWithCancellation()
		{
			var cts = new CancellationTokenSource(30);
			var stream = TestProducer();

			await WriteStreamToOutputAsync(stream, cts.Token);
		}

		private IAsyncEnumerable<int> TestProducer()
		{
			var queue = new UnicastAsyncEnumerable<int>();

			Handle();

			return queue;

			async void Handle()
			{
				try
				{
					for (int i = 0; i < 10; i++)
					{
						await Task.Delay(10);
						await queue.Next(i);
					}
				}
				finally
				{
					await queue.Complete();
				}

			}
		}

		public UnicastEnumerableTests(ITestOutputHelper output) : base(output)
		{
		}
	}
}