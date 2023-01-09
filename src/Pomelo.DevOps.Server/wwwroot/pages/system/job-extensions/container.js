// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    data() {
        return {
            jobExtensionId: null
        };
    },
    async mounted() {
        var container = this.$container('#job-extension-manage');
        var extension = (await Pomelo.CQ.Get(`/api/jobextension/${this.jobExtensionId}`)).data;
        container.open(extension.proxiedManageUrl, { extension: this.extension });
    }
});