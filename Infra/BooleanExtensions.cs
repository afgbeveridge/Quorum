﻿using System;

namespace Infra {
    public static class BooleanExtensions {

        public static bool IfTrue(this bool val, Action a) {
            if (val) a();
            return val;
        }

        public static bool IfFalse(this bool val, Action a) {
            if (!val) a();
            return val;
        }

    }
}