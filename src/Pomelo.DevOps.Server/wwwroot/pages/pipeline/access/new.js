// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/radio-button/index',
        '/components/project-member-selector/index'
    ],
    data() {
        return {
            form: {
                userId: null,
                accessType: 'Reader'
            },
            userSelectorVisibile: false,
            selectedUser: null
        };
    },
    async created() {
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
        async add() {
            if (!this.form.userId) {
                notification.push('Add Pipeline Access', 'You must select a user.', 'error');
                return;
            }

            var notify = notification.push('Add Pipeline Access', 'Adding...');
            try {
                var result = await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/access`, this.form);
                notify.message = `${this.selectedUser.displayName}'s access has been created`;
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