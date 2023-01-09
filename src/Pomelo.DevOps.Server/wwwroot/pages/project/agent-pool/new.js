// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            form: {
                extensionEntryUrl: null,
                token: null
            }
        };
    },
    mounted() {
        this.$root.ui.active = 'system-job-extensions';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async add() {
            if (!this.form.extensionEntryUrl) {
                notification.push('Add Job Extension', 'Extension Metadata JSON URL can not be empty', 'error');
                return;
            }

            var notify = notification.push('Add Job Extension', 'Connecting...');
            try {
                var result = await Pomelo.CQ.Post(`/api/jobextension`, this.form);
                notify.message = `The job extension has been added`;
                notify.time = 10;
                notify.type = 'success';
                Pomelo.Redirect('/system/job-extensions');
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});