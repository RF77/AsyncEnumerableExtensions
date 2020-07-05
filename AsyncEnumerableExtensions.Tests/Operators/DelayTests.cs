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
using Xunit;
using Xunit.Abstractions;

namespace Time.AsyncEnumerable.Extensions.Tests
{
	public class DelayTests : UnitTestBase
	{
		[Fact]
		public async Task NormalDelay()
		{
			using (MustTakeLongerThan(180))
			{
				await System.Linq.AsyncEnumerable.Range(0, 2).Delay(100).AssertEqualTo(0, 1);
			}
		}

		public DelayTests(ITestOutputHelper output) : base(output)
		{
		}
	}
}