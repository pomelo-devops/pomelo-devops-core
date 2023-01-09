// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    methods: {
        select(trigger) {
            this.$parent.$containers[0].open(trigger.proxiedViewUrl, { provider: trigger, projectId: this.projectId, pipelineId: this.pipelineId });
        }
    }
});