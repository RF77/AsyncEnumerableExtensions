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
	public class IfOperatorTests : UnitTestBase
	{
		private static int _counter;

		[Fact]
		public async Task TestWithIf()
		{
			var stream = GetNumbersAsync(20);

			stream = stream.If(i => i % 2 == 0, AddNumbers);

			await WriteStreamToOutputAsync(stream);
		}

		[Fact]
		public async Task TestWithIfAndCancellation()
		{
			var cts = new CancellationTokenSource(100);

			var stream = GetNumbersAsync(20);

			stream = stream.If(i => i % 2 == 0, AddNumbers);

			await WriteStreamToOutputAsync(stream, cts.Token);
		}

		[Fact]
		public async Task TestWithIfAndError()
		{

			var stream = GetNumbersWithErrorAsync(20);

			stream = stream.If(i => i % 2 == 0, AddNumbers);

			await WriteStreamToOutputAsync(stream);
		}

		private async IAsyncEnumerable<int> GetNumbersAsync(int amount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			for (var i = 0; i < amount; i++)
			{
				Write($"yield {_counter}");
				yield return _counter++;
				await Task.Delay(10, cancellationToken);
				//if (cancellationToken.IsCancellationRequested)
				//{
				//	yield break;
				//}
			}
		}

		private async IAsyncEnumerable<int> GetNumbersWithErrorAsync(int amount)
		{
			for (var i = 0; i < amount; i++)
			{
				if (i == 10)
				{
					throw new InvalidOperationException();
				}
				yield return _counter++;
				await Task.Delay(10);
			}
		}

		private async IAsyncEnumerable<int> AddNumbers(IAsyncEnumerable<int> numbers)
		{
			await foreach (var ab in numbers.Buffer(2))
			{
				if (ab.Count == 2)
				{
					yield return ab[0] + ab[1];
				}
				else
				{
					yield break;
				}
			}
		}

		public IfOperatorTests(ITestOutputHelper output) : base(output)
		{
		}
	}
}