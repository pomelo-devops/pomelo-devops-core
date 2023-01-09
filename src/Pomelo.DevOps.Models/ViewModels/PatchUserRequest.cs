namespace Pomelo.DevOps.Models.ViewModels
{
    public class PatchUserRequest : User
    {
        public string RawPassword { get; set; }
    }
}
