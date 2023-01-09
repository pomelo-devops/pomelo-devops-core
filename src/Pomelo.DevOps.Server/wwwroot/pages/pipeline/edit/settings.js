// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    components: [
        '/components/radio-button/index'
    ],
    data() {
        return {
            pipeline: null
        };
    },
    mounted() {
        this.$root.ui.active = 'pipeline-settings';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async deletePipeline() {
            if (confirm('Are you sure you want to delete this pipeline?')) {
                await Pomelo.CQ.Delete(`/api/project/${this.projectId}/pipeline/${this.pipelineId}`);
                Pomelo.Redirect(`/project/${this.projectId}`);
            }
        }
    }
});