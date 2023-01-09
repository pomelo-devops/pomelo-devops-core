namespace Pomelo.DevOps.Models.LoginProviders
{
    [LoginProvider("Local")]
    public record LocalLogin : LoginProvider
    {
        public bool AllowRegister { get; set; }
    }
}
