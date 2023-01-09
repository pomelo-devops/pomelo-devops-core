// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Pomelo.DevOps.Triggers.GitLab.Models.ViewModels
{
    public class WebHookBase
    {
        [JsonProperty(PropertyName = "object_kind")]
        public string ObjectKind { get; set; }
    }
}
