namespace Pomelo.DevOps.Models.ViewModels
{
    public class PostForkRequest
    {
        public string OriginalId { get; set; }

        public string Id { get; set; }

        public PipelineVisibility Visibility { get; set; }

        public string Name { get; set; }
    }
}
