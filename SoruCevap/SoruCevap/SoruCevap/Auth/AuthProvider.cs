﻿using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SoruCevap.Auth
{
    public class AuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" }); // Farklı domainlerden istek sorunu yaşamamak için

            //Burada kendi authentication yöntemimizi belirleyebiliriz.Veritabanı bağlantısı vs...
            var uyeServis = new UyeService();
            var uye = uyeServis.UyeOturumAc(context.UserName, context.Password);
            List<string> uyeYetkileri = new List<string>();

            if (uye != null)
            {
                string yetki = "";
                if (uye.rol == "admin")
                {
                    yetki = "Admin";

                }
                else
                {
                    yetki = "Uye";
                }
                uyeYetkileri.Add(yetki);

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, yetki));
                identity.AddClaim(new Claim(ClaimTypes.PrimarySid, uye.userId.ToString()));

                AuthenticationProperties propert = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { "uyeId", uye.userId.ToString() },
                    { "uyeEmail", uye.email },
                    { "uyeIsim", uye.ad + " " + uye.soyad},
                    { "uyeYetkileri",Newtonsoft.Json.JsonConvert.SerializeObject(uyeYetkileri) }

               });
                AuthenticationTicket ticket = new AuthenticationTicket(identity, propert);


                context.Validated(ticket);
            }
            else
            {
                context.SetError("Geçersiz istek", "Hatalı kullanıcı bilgisi");
            }



        }
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }
}
