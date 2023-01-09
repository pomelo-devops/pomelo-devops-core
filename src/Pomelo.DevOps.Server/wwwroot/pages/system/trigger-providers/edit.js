// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            provider: null
        };
    },
    created() {
        this.getProvider();
    },
    mounted() {
        this.$root.ui.active = 'system-trigger-providers';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getProvider() {
            this.provider = (await Pomelo.CQ.Get(`/api/triggerprovider/${this.triggerProviderId}`)).data;
        },
        async save() {
            var notify = notification.push('Edit Trigger Provider', 'Saving...');
            try {
                var result = await Pomelo.CQ.Patch(`/api/triggerprovider/${this.triggerProviderId}`, this.provider);
                notify.message = `${this.provider.name} has been saved`;
                notify.time = 10;
                notify.type = 'success';
                Pomelo.Redirect('/system/trigger-providers');
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});