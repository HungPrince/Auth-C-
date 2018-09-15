using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using OauthIdentityApp.Entities;
using OauthIdentityApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static OauthIdentityApp.Models.Models;

namespace OauthIdentityApp.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowOrigin = context.OwinContext.Get<string>(("as:clientAllowOrigin"));
            if (allowOrigin == null)
            {
                allowOrigin = "*";
            }
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowOrigin });

            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (AuthRepository _repo = new AuthRepository())
            {
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            var props = new AuthenticationProperties(new Dictionary<string, string>
            {
                {
                    "as:client_id", (context.ClientId != null) ? context.ClientId : null
                },
                {
                    "userName", context.UserName
                }
            });

            var ticket = new AuthenticationTicket(identity, props);

            context.Validated(ticket);

        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecrect = string.Empty;
            Client client = null;

            if (!context.TryGetBasicCredentials(out clientId, out clientSecrect))
            {
                context.TryGetFormCredentials(out clientId, out clientSecrect);
            }

            if (context.ClientId == null)
            {
                context.Validated();
                return Task.FromResult<object>(null);
            }

            using (AuthRepository _repo = new AuthRepository())
            {
                client = _repo.FindClient(context.ClientId);
            }

            if (client == null)
            {
                context.SetError("Invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                return Task.FromResult<object>(null);
            }

            if (client.ApplicationType == ApplicationTyes.NativeConfidential)
            {
                if (string.IsNullOrWhiteSpace(clientSecrect))
                {
                    context.SetError("Client '{0}' is not registered in the system.", context.ClientId);
                    return Task.FromResult<object>(null);
                }
                else
                {
                    if (client.Secret != Helper.GetHash(clientSecrect))
                    {
                        context.SetError("invalid_clientId", "Client secrect is invalid");
                        return Task.FromResult<object>(null);
                    }
                }
            }

            if (!client.Active)
            {
                context.SetError("invalid_clientid", "Client is inactive");
                return Task.FromResult<object>(null);
            }

            context.OwinContext.Set<string>("as:clientAllowOrigin", client.AllowedOrigin);
            context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

            context.Validated();
            return Task.FromResult<object>(null);
        }
    }
}