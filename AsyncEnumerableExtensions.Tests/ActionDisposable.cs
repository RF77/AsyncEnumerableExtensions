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

namespace AsyncEnumerableExtensions.Tests
{
	public class ActionDisposable : IDisposable
	{
		private readonly Action _onDisposeAction;

		public ActionDisposable(Action onDisposeAction)
		{
			_onDisposeAction = onDisposeAction ?? throw new ArgumentNullException(nameof(onDisposeAction));
		}

		// Good enough for testing... 
		public void Dispose()
		{
			_onDisposeAction();
		}
	}
}