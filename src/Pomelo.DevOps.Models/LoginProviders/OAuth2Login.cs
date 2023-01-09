using Newtonsoft.Json;

namespace Pomelo.DevOps.Models.LoginProviders
{
    public class OAuth2LoginRedirection
    { 
        public string Endpoint { get; set; }

        public string ClientId { get; set; }

        public bool Restrict { get; set; } = false;

        public string Scope { get; set; }
    }

    public class OAuth2LoginAccessToken
    { 
        public string Endpoint { get; set; }

        public string AccessTokenPath { get; set; }

        public string TokenTypePath { get; set; }
    }

    public class OAuth2LoginUserInfo
    { 
        public string Endpoint { get; set; }

        public string UsernamePath { get; set; }

        public string EmailPath { get; set; }

        public string DisplayNamePath { get; set; }
    }

    [LoginProvider("OAuth2")]
    public record OAuth2Login : LoginProvider
    {
        public string ServerBaseUrl { get; set; }

        public OAuth2LoginRedirection Redirection { get; set; }

        public OAuth2LoginAccessToken AccessToken { get; set; }

        public OAuth2LoginUserInfo UserInfo { get; set; }
    }
}
