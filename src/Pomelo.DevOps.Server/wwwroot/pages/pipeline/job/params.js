// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            job: null,
            variables: []
        };
    },
    async created() {
        await this.getJob();
        await this.getVariables();
    },
    mounted() {
        this.$root.ui.active = 'job-params';
    },
    unmounted() {
        this.$root.data.currentJob = null;
        this.$root.ui.active = null;
    },
    methods: {
        async getJob() {
            this.job = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}`)).data;
            this.$root.data.currentJob = this.job;
        },
        async getVariables() {
            this.variables = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/variable`)).data;
        }
    }
});