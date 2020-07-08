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
using AsyncEnumerableExtensions.Karnok;

namespace System.Linq
{
	public static class IfOperator
	{
		public static async IAsyncEnumerable<T> If<T>(this IAsyncEnumerable<T> source, Func<T, bool> ifSelector,
			Func<IAsyncEnumerable<T>, IAsyncEnumerable<T>> ifFunction, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var ifSourceStream = new UnicastAsyncEnumerable<T>();
			var elseResultStream = new UnicastAsyncEnumerable<T>();
			var ifResultStream = ifFunction(ifSourceStream);

			HandleStream();

			var result = ifResultStream.MergeConcurrently(elseResultStream);
			try
			{
				await foreach (var item in result.WithCancellation(cancellationToken))
				{
					yield return item;
				}
			}
			finally
			{
				await ifSourceStream.Complete();
				await elseResultStream.Complete();
			}
			

			async void HandleStream()
			{
				try
				{
					await foreach (var item in source.WithCancellation(cancellationToken))
					{
						if (ifSelector(item))
						{
							await ifSourceStream.Next(item);
						}
						else
						{
							await elseResultStream.Next(item);
						}
					}
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception e)
				{
					await elseResultStream.Error(e);
				}
				finally
				{
					await ifSourceStream.Complete();
					await elseResultStream.Complete();
				}
			}
		}
	}
}