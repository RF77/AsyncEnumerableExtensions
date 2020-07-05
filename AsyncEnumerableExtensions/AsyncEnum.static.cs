﻿// /*******************************************************************************
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

namespace System.Linq
{
	// Methods defined already in AsyncEnumerable and AsyncEnuemerableEx => provide it in AsyncEnum just for convenience too
	public static partial class AsyncEnum
	{

		/// <summary>
		/// Repeats the element indefinitely.
		/// </summary>
		/// <typeparam name="TResult">The type of the elements in the source sequence.</typeparam>
		/// <param name="element"> Element to repeat.</param>
		/// <returns>The async-enumerable sequence producing the element repeatedly and sequentially.</returns>
		public static IAsyncEnumerable<TResult> Repeat<TResult>(TResult element)
		{
			return AsyncEnumerableEx.Repeat(element);
		}

		/// <summary>
		/// Generates an async-enumerable sequence that repeats the given element the specified number of times.
		/// </summary>
		/// <typeparam name="TResult">The type of the element that will be repeated in the produced sequence.</typeparam>
		/// <param name="element">Element to repeat.</param>
		/// <param name="count">Number of times to repeat the element.</param>
		/// <returns>An async-enumerable sequence that repeats the given element the specified number of times.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="count" /> is less than zero.</exception>
		public static IAsyncEnumerable<TResult> Repeat<TResult>(
			TResult element,
			int count)
		{
			return AsyncEnumerable.Repeat(element, count);
		}
	}
}