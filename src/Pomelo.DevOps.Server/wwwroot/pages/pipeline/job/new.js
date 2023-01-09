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
            pipelineId: null,
            projectId: null,
            pipeline: null,
            job: {
                name: null,
                description: null,
                triggerType: 'Manual',
                triggerName: window.localStorage.userDisplayName,
                arguments: {}
            }
        };
    },
    async created() {
        this.job.name = this.$root.localization.sr('PIPELINE_NEW_JOB_JOB_NAME_DEFAULT');
        this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        this.$root.data.currentPipeline = this.pipeline;
    },
    unmounted() {
        this.$root.data.currentPipeline = null;
    },
    methods: {
        async startJob() {
            var notify = notification.push(this.job.name, 'Starting new job...');
            var result = (await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job`, this.job)).data;
            notify.time = 10;
            notify.message = `Job '${this.job.name}' started.`;
            notify.type = 'success';
            Pomelo.Redirect(`/project/${this.projectId}/pipeline/${this.pipelineId}/job/${result.number}`);
        }
    }
});