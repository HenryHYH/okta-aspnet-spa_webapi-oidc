using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Configuration;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Jwt;

[assembly: OwinStartup(typeof(Okta.Samples.OpenIDConnect.AspNet.Api.Startup))]

namespace Okta.Samples.OpenIDConnect.AspNet.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var clientID = WebConfigurationManager.AppSettings["okta:ClientId"];
            var issuer = WebConfigurationManager.AppSettings["okta:TenantUrl"];

            TokenValidationParameters tvps = new TokenValidationParameters
            {
                ValidAudience = clientID,
            };

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(tvps,
                new OpenIdConnectCachingSecurityTokenProvider(issuer + "/.well-known/openid-configuration"))
            });
        }
    }
}
