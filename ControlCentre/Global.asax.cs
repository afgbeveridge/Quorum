#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Infra;

namespace ControlCentre {
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Local
            Quorum.ActiveDisposition.Shared = true;
            Builder.ConfigureInjections();
            LogFacade.Instance.Adapter = new NLogLogger().Configure(new LoggingOptions { RequireConsoleSink = false });
        }
    }
}
