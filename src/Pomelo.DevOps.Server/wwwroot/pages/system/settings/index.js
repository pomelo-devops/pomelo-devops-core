// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/radio-button/index',
        '/components/toggle-button/index',
        '/components/input-number/index',
    ],
    data() {
        return {
            policies: []
        };
    },
    async created() {
    },
    mounted() {
        this.$root.ui.active = 'system-settings';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async save() {
            var notify = notification.push('System Settings', 'Saving...');
            try {
                var result = await Pomelo.CQ.Patch(`/api/policy`, { policies: this.$root.policies });
                notify.message = `Settings are saved.`;
                notify.time = 10;
                notify.type = 'success';
                this.$root.getPolicies();
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        },
        parseOptions(extend) {
            return JSON.parse(extend);
        }
    }
});