using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OktaMvcSample.Startup))]
namespace OktaMvcSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
