﻿using System.Collections.Generic;
using System.Dynamic;
using System.Web.Routing;

namespace ControlCentre {

    public static class MvcExtensions {

        public static ExpandoObject ToExpando(this object anonymousObject) {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }

    }
}