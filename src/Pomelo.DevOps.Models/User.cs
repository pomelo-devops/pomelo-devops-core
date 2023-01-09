using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.DevOps.Models
{
    public enum UserRole
    {
        Collaborator,
        SystemAdmin
    }

    public class User
    {
        public Guid Id { get; set; }

        [MaxLength(64)]
        public string Username { get; set; }

        [MaxLength(64)]
        [ForeignKey(nameof(LoginProvider))]
        public string LoginProviderId { get; set; }

        public virtual LoginProvider LoginProvider { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }

        [MaxLength(64)]
        public string DisplayName { get; set; }

        public UserRole Role { get; set; }

        [MaxLength(64)]
        [Newtonsoft.Json.JsonIgnore]
        public byte[] Salt { get; set; }

        [MaxLength(64)]
        [Newtonsoft.Json.JsonIgnore]
        public byte[] PasswordHash { get; set; }

        [ForeignKey(nameof(Avatar))]
        public Guid? AvatarId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual Binary Avatar { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public virtual ICollection<PersonalAccessToken> PersonalAccessTokens { get; set; }
    }
}
