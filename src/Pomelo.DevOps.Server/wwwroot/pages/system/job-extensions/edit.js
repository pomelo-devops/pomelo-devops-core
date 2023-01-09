// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            extension: null
        };
    },
    created() {
        this.getProvider();
    },
    mounted() {
        this.$root.ui.active = 'system-job-extensions';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getProvider() {
            this.extension = (await Pomelo.CQ.Get(`/api/jobextension/${this.jobExtensionId}`)).data;
        },
        async save() {
            var notify = notification.push('Edit Job Extension', 'Saving...');
            try {
                var result = await Pomelo.CQ.Patch(`/api/jobextension/${this.jobExtensionId}`, this.extension);
                notify.message = `${this.extension.name} has been saved`;
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