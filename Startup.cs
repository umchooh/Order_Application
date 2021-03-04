using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Order_Application.Startup))]
namespace Order_Application
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
