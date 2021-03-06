﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using Quorum;
using System.Linq;
using Infra;
using System.Web.Mvc;
using ControlCentre.Models;

namespace ControlCentre.Filters {

    public class EnumParsingActionFilterAttribute : ActionFilterAttribute {

        public EnumParsingActionFilterAttribute() {
        }

        public string SourcePropertyName { get; set; }

        public string TargetPropertyName { get; set; }

        public Type EnumType { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var model = filterContext.ActionParameters.First().Value;
            if (model != null) {
                var val = model.GetType().GetProperty(SourcePropertyName).GetValue(model, null);
                var parsed = Enum.Parse(EnumType, val.ToString(), true);
                model.GetType().GetProperty(TargetPropertyName).SetValue(model, parsed);
            }
            base.OnActionExecuting(filterContext);
        }

    }
}