using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public enum PolicyType
    { 
        Text,
        Number,
        Toggle,
        Radio
    }

    public class Policy
    {
        [MaxLength(256)]
        public string Key { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public PolicyType Type { get; set; }

        [MaxLength(256)]
        public string ValueJson { get; set; }

        [NotMapped]
        public object Value { get; set; }

        public string Description { get; set; }

        public string Extended { get; set; }

        public int Order { get; set; }

        #region Consts
        public const string DevOpsName = "DEVOPS_NAME";
        public const string DevOpsIcon = "DEVOPS_ICON";
        public const string GalleryName = "GALLERY_NAME";
        public const string GalleryIcon = "GALLERY_ICON";
        public const string AllowUserCreateProject = "ALLOW_USER_CREATE_PROJECT";
        #endregion
    }
}
