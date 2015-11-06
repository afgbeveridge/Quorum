#region License
//
// Copyright Tony Beveridge 2013-2015. All rights reserved. 
// This source code and any derivations can only be used with the express written permission of the author.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infra {

	public static class Assert {
		
		public static void True(bool condition, Func<string> msg) {
			if (!condition) Log(msg);
		}

		public static void False(bool condition, Func<string> msg) {
			if (condition) Log(msg);
		}

		private static void Log(Func<string> msg) {
			string message = msg();
			var ex = new ApplicationException(message);
			//Diagnostics.LogException(typeof(Assert), msg(), ex);
			throw ex;
		}

	}
}
