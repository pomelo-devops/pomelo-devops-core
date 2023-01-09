// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var yield = require('/shared/sleep').yield;

Page({
    style: true,
    components: [
        '/components/radio-button/index',
        '/components/agent-pool-selector/index',
        '/components/input-number/index'
    ],
    data() {
        return {
            projectId: null,
            stage: null,
            showAgentSelector: false,
            agentPool: null
        };
    },
    async created() {
        await this.updateAgentPool();
    },
    watch: {
        deep: true,
        'stage.agentPoolId': async function () {
            await this.updateAgentPool();
        }
    },
    methods: {
        async updateAgentPool() {
            if (this.stage && this.stage.agentPoolId) {
                var pool = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentPool/${this.stage.agentPoolId}`)).data;
                pool.agents = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentPool/${this.stage.agentPoolId}/agent`)).data;
                this.agentPool = pool;
            } else {
                this.agentPool = null;
            }
        },
        async deleteStage() {
            if (confirm('Please confirm you want to delete this stage')) {
                this.$parent.pipeline.stages.splice(parent.pipeline.stages.indexOf(this.stage), 1);
                await yield();
                this.$parent.reorderStages();
            }
        }
    },
    mounted() {
        this.$parent.active = 'stage-' + this.stage.temp;
    },
    unmounted() {
        this.$parent.active = null;
    }
});