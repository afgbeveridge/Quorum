﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;

namespace Infra {

    public static class DBC {
		
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
