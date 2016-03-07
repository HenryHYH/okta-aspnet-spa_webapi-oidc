namespace Okta.Samples.OpenIdConnect.AspNet.Api.App_Start
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    public class JsonWebTokenValidationHandler : DelegatingHandler
    {
        public string SymmetricKey { get; set; }

        public string Audience { get; set; }

        public string Issuer { get; set; }

        public string JWTCertificatePublicKey { get; set; }

        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;

            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
            {
                // Fail if no Authorization header or more than one Authorization headers  
                // are found in the HTTP request  
                return false;
            }

            // Remove the bearer token scheme prefix and return the rest as ACS token  
            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

            return true;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string strToken;
            HttpResponseMessage errorResponse = null;

            if (TryRetrieveToken(request, out strToken))
            {
                try
                {
                    byte[] certBytes = Convert.FromBase64String(this.JWTCertificatePublicKey);
                    X509Certificate2 x509cert = new X509Certificate2(certBytes);
                    var x509SecurityKey = new X509SecurityKey(x509cert);

                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = new TokenValidationParameters()
                    {
                        //this is necessary to avoid a bug due to the presence of the "kid" parameter in the Okta JWT
                        IssuerSigningKeyResolver = (arbitrarily, declaring, these, parameters) =>
                        {
                            return x509SecurityKey;
                        },
                        ValidateAudience = true,
                        ValidAudience = this.Audience,
                        ValidateIssuer = true,
                        ValidIssuer = this.Issuer
                    };
                    SecurityToken secToken = new JwtSecurityToken();

                    Thread.CurrentPrincipal = tokenHandler.ValidateToken(strToken, validationParameters, out secToken);


                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = Thread.CurrentPrincipal;
                    }
                }
                //catch (JWT.SignatureVerificationException ex)
                //{
                //    errorResponse = request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex);
                //}
                //catch (JsonWebToken.TokenValidationException ex)
                //{
                //    errorResponse = request.CreateErrorResponse(HttpStatusCode.Unauthorized, ex);
                //}
                catch (Exception ex)
                {
                    errorResponse = request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }

            return errorResponse != null ?
                Task.FromResult(errorResponse) :
                base.SendAsync(request, cancellationToken);
        }
    }
}
