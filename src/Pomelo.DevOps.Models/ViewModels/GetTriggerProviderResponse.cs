namespace Pomelo.DevOps.Models.ViewModels
{
    public record GetTriggerProviderResponse : TriggerProvider
    {
        public bool IsOnline { get; set; }
    }
}
