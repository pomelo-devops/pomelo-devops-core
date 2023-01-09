// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/radio-button/index'
    ],
    data() {
        return {
            userId: null,
            user: null
        };
    },
    async created() {
        this.getUser();
        this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        this.$root.data.currentPipeline = this.pipeline;
    },
    mounted() {
        this.$root.ui.active = 'pipeline-accesses';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getUser() {
            this.user = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/access/${this.userId}`)).data;
        },
        async save() {
            var notify = notification.push('Edit Pipeline Access', 'Saving...');
            try {
                var result = await Pomelo.CQ.Patch(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/access/${this.user.userId}`, this.user);
                notify.message = `${this.user.user.displayName}'s access has been saved`;
                notify.time = 10;
                notify.type = 'success';
                Pomelo.Redirect(`/project/${this.projectId}/pipeline/${this.pipelineId}/access`);
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});