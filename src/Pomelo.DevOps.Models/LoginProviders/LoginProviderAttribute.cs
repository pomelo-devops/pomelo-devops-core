using System;

namespace Pomelo.DevOps.Models.LoginProviders
{
    public class LoginProviderAttribute : Attribute
    {
        public string Mode { get; private set; }

        public LoginProviderAttribute(string mode) 
        {
            this.Mode = mode;
        }
    }
}
