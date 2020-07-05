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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AsyncEnumerableExtensions.Tests
{
	public static class TestExtensions
	{
		public static async Task AssertEqualTo<T>(this IAsyncEnumerable<T> source, params T[] result)
		{
			var list = await source.ToListAsync();

			Assert.True(list.SequenceEqual(result), $"Result: {list.Join()}, Expected: {result.Join()}");
		}

		public static string Join<T>(this IEnumerable<T> source)
		{
			return string.Join(", ", source);
		}

	}
}