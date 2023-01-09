// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    data() {
        return {
            triggerProviderId: null
        };
    },
    async mounted() {
        var container = this.$container('#trigger-provider-manage');
        var provider = (await Pomelo.CQ.Get(`/api/triggerprovider/${this.triggerProviderId}`)).data;
        container.open(provider.proxiedManageUrl, { provider: this.provider });
    }
});