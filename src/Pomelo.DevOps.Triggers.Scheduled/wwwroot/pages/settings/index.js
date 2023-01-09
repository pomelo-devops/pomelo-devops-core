// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    data() {
        return {
            config: null
        };
    },
    created() {
        this.getConfig();
    },
    methods: {
        async getConfig() {
            this.config = (await Pomelo.CQ.Get(`/api/triggerprovider/${this.triggerProviderId}/api/settings`));
        },
        async save() {
            var notify = notification.push('GitLab MR Settings', 'Saving...');
            try {
                this.config = await Pomelo.CQ.Patch(`/api/triggerprovider/${this.triggerProviderId}/api/settings`, this.config);
                notify.message = `Settings have been saved.`;
                notify.type = 'success';
                notify.time = 10;
            } catch (ex) {
                notify.message = ex.message;
                notify.type = 'error';
                notify.time = 10;
            }
        }
    }
});