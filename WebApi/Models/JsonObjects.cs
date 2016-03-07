using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Okta.Samples.OpenIdConnect.AspNet.Api.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonWebKeys
    {
        [JsonProperty("keys")]
        public List<JsonWebKey> Keys { get; set; }
    }

    public class JsonWebKey
    {
        [JsonProperty("e")]
        public string Exponent { get; set; }

        [JsonProperty("kty")]
        public string KeyType { get; set; }

        [JsonProperty("use")]
        public string PublicKeyUse { get; set; }

        [JsonProperty("kid")]
        public string KeyID { get; set; }

        [JsonProperty("x5c")]
        public List<string> X509CertificateChain { get; set; }

        [JsonProperty("x5t")]
        public string X509CertificateThumbprint { get; set; }

        [JsonProperty("n")]
        public string Modulus { get; set; }


    }
}