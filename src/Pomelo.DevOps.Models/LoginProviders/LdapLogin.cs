namespace Pomelo.DevOps.Models.LoginProviders
{
    [LoginProvider("Ldap")]
    public record LdapLogin : LoginProvider
    {
        public string UsernamePath { get; set; } = "sAMAccountName";

        public string EmailPath { get; set; } = "userPrincipalName";

        public string DisplayNamePath { get; set; } = "displayName";

        public string UserNamePlaceholder { get; set; } = "Company Account";

        public string Hint { get; set; } = "Please use your company email address as account name, for example: example@pomelo.cloud";

        public string LdapServer { get; set; }

        public int Port { get; set; } = 389;

        public string SearchBase { get; set; }

        public string LoginBackgroundUrl { get; set; }
    }
}
