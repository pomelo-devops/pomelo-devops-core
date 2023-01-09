// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Pomelo.DevOps.Agent
{
    public class Config
    {
        public string Server { get; set; }
        public string PersonalAccessToken { get; set; }
        public Guid? AgentId { get; set; }
        public int? Identifier { get; set; }
        public string ProjectId { get; set; }
        public long AgentPoolId { get; set; }
        public bool RemoteControl { get; set; } = true;
        public IList<string> GalleryFeeds { get; set; } = new List<string>();
    }
}
