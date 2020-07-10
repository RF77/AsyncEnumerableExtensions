// Copyright (c) 2020 by RF77 (https://github.com/RF77)
// Licensed under the Apache 2.0 License.
// See LICENSE file in the project root for full license information.

using System;

namespace AsyncEnumerableExtensions.TimeOperators
{
	public interface ITimeStampItem<T>
	{
		DateTime TimeStamp { get; }
		T Content { get; }
	}
}