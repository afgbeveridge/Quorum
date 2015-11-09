using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ControlCentre.Startup))]
namespace ControlCentre
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
