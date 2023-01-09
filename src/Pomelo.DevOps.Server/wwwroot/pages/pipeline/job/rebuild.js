// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/pipeline-arguments-form/index'
    ],
    data() {
        return {
            originalJob: null,
            pipelineId: null,
            projectId: null,
            pipeline: null,
            job: {
                name: 'Rebuild',
                description: null,
                triggerType: 'Manual',
                triggerName: window.localStorage.userDisplayName,
                arguments: {}
            }
        };
    },
    async created() {
        this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        this.getJob();
        this.getVariables();
        this.$root.data.currentPipeline = this.pipeline;
        this.$root.ui.active = 'job-rebuild';
    },
    unmounted() {
        this.$root.data.currentJob = null;
        this.$root.ui.active = null;
    },
    methods: {
        async startJob() {
            var notify = notification.push(this.job.name, 'Starting new job...');
            var result = (await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job`, this.job)).data;
            notify.time = 10;
            notify.message = `Job '${this.job.name}' started.`;
            notify.type = 'success';
            Pomelo.Redirect(`/project/${this.projectId}/pipeline/${this.pipelineId}/job/${result.number}`);
        },
        async getJob() {
            this.originalJob = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}`)).data;
            this.job.name = `Rebuild #${this.originalJob.number} ${this.originalJob.name}`
            this.$root.data.currentJob = this.originalJob;
        },
        async getVariables() {
            var variables = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/variable`)).data;
            for (var i = 0; i < variables.length; ++i) {
                this.job.arguments[variables[i].name] = variables[i].value;
            }
        }
    }
});