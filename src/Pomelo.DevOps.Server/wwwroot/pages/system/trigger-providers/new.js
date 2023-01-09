// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            form: {
                providerEntryUrl: null,
                token: null
            }
        };
    },
    mounted() {
        this.$root.ui.active = 'system-trigger-providers';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async add() {
            if (!this.form.providerEntryUrl) {
                notification.push('Add Trigger Provider', 'Provider Metadata JSON URL can not be empty', 'error');
                return;
            }

            var notify = notification.push('Add Trigger Provider', 'Connecting...');
            try {
                var result = await Pomelo.CQ.Post(`/api/triggerprovider`, this.form);
                notify.message = `The trigger provider has been added`;
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