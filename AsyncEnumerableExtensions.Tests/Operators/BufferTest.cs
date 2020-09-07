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
using Xunit;
using Xunit.Abstractions;

namespace AsyncEnumerableExtensions.Tests.Operators
{
	public class BufferTest : UnitTestBase
	{
		public BufferTest(ITestOutputHelper output) : base(output)
		{
		}

		private async IAsyncEnumerable<string> GetNumbersAsync(int amount, string prefix,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (var i = 0; i < amount; i++)
			{
				var val = $"{prefix}{i}";
				//Write($"yield {val}");
				yield return val;
				await Task.Delay(100, cancellationToken);
			}
		}

		private async IAsyncEnumerable<string> GetNumbersWithExceptionAsync(int amount, string prefix,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (var i = 0; i < amount; i++)
			{
				if (i + 2 == amount)
				{
					throw new InvalidOperationException();
				}
				var val = $"{prefix}{i}";
				//Write($"yield {val}");
				yield return val;
				await Task.Delay(100, cancellationToken);
			}
		}

		[Fact]
		public async Task BufferWhereMaxSizeWillOccur()
		{
			CancellationTokenSource cts = new CancellationTokenSource();

			await Assert.ThrowsAsync<TaskCanceledException>(async () =>
			{
				await foreach (var buffer in GetNumbersAsync(100, "A", cts.Token).Buffer(3, TimeSpan.FromSeconds(2)))
				{
					Assert.True(buffer.Count == 3);
					cts.Cancel();
				}
			});
		}


		[Fact]
		public async Task BufferWhereMaxTimeWillOccur()
		{
			CancellationTokenSource cts = new CancellationTokenSource();

			await Assert.ThrowsAsync<TaskCanceledException>(async () =>
			{
				await foreach (var buffer in GetNumbersAsync(100, "A", cts.Token).Buffer(40, TimeSpan.FromMilliseconds(500)))
				{
					Assert.True(buffer.Count > 1);
					Assert.True(buffer.Count < 40);
					cts.Cancel();
				}
			});
		}

		[Fact]
		public async Task BufferWhereMaxSizeWillOccurConsumeAll()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var all = new List<string>();

			var numOfBuffers = 0;

			await foreach (var buffer in GetNumbersAsync(9, "A", cts.Token).Buffer(4, TimeSpan.FromMilliseconds(1000)))
			{
				all.AddRange(buffer);
				numOfBuffers++;
			}

			Assert.True(all.Count == 9);
			Assert.True(numOfBuffers == 3);
		}


		[Fact]
		public async Task BufferWhereMaxTimeWillOccurConsumeAll()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			var all = new List<string>();

			var numOfBuffers = 0;

			await foreach (var buffer in GetNumbersAsync(9, "A", cts.Token).Buffer(6, TimeSpan.FromMilliseconds(300)))
			{
				all.AddRange(buffer);
				numOfBuffers++;
			}

			Assert.True(all.Count == 9);
			Assert.True(numOfBuffers >= 2);
		}

	}
}