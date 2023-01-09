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
        this.$root.ui.active = this.jobExtensionId;
        var container = this.$container('#job-extension-container');
        var extension = (await Pomelo.CQ.Get(`/api/jobextension/${this.jobExtensionId}`)).data;
        container.open(`/api/jobextension/${this.jobExtensionId}${extension.viewUrl}`, { extension: extension });
    },
    unmounted() {
        this.$root.ui.active = null;
        this.$root.data.currentJob = null;
    },
    async created() {
        await this.getJob();
    },
    methods: {
        async getJob() {
            this.job = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}`)).data;
            this.$root.data.currentJob = this.job;
        }
    }
});