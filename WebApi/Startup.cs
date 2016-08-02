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
            app.Use((context, next) =>
            {
                PrintCurrentIntegratedPipelineStage(context, "Middleware 1");
                return next.Invoke();
            });

            var clientID = WebConfigurationManager.AppSettings["okta:ClientId"];
            var oauthIssuer = WebConfigurationManager.AppSettings["okta:OAuth_Issuer"];
            var oidcIssuer = WebConfigurationManager.AppSettings["okta:OIDC_Issuer"];
            var IDorAccess = WebConfigurationManager.AppSettings["okta:IDorAccessToken"];

            var issuer = oidcIssuer;

            if (IDorAccess == "access")
            {
                issuer = oauthIssuer;
            }

            TokenValidationParameters tvps = new TokenValidationParameters
            {
                ValidAudience = clientID,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidateIssuer = true
            };

            //app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            //{
            //    TokenValidationParameters = tvps,
            //    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
            //    {
            //        new OpenIdConnectCachingSecurityTokenProvider(oidcIssuer + "/.well-known/openid-configuration")
            //    }
            //});

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(tvps,
                new OpenIdConnectCachingSecurityTokenProvider(oidcIssuer + "/.well-known/openid-configuration")),
            });


            //app.Use((context, next) =>
            //{
            //    PrintCurrentIntegratedPipelineStage(context, "2nd MW");
            //    return next.Invoke();
            //});
            //app.Run(context =>
            //{
            //    PrintCurrentIntegratedPipelineStage(context, "3rd MW");
            //    return context.Response.WriteAsync("Hello world");
            //});

        }

        private void PrintCurrentIntegratedPipelineStage(IOwinContext context, string msg)
        {
            var currentIntegratedpipelineStage = System.Web.HttpContext.Current.CurrentNotification;
            context.Get<System.IO.TextWriter>("host.TraceOutput").WriteLine(
                "Current IIS event: " + currentIntegratedpipelineStage
                + " Msg: " + msg);
        }
    }


}
