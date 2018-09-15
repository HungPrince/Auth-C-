using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using OauthIdentityApp.Providers;
using Owin;
using Microsoft.Owin.Cors;
using System;
using System.Web.Http;

[assembly: OwinStartup(typeof(OauthIdentityApp.API.Startup))]
namespace OauthIdentityApp.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            ConfigureOauth(app);

            WebApiConfig.Register(config);
            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(config);
        }

        public void ConfigureOauth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAutheServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleResfreshTokenProvider()
            };

            //Token Generation 
            app.UseOAuthAuthorizationServer(OAutheServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }

  
}