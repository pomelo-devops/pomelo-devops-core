// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Pomelo.DevOps.Triggers.GitLab.Models
{
    public class TriggerHistory
    {
        [MaxLength(128)]
        public string NamespaceProject { get; set; }

        [MaxLength(128)]
        public string CommitHash { get; set; }

        public TriggerType Type { get; set; }
    }
}
