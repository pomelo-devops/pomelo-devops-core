// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            agents: [],
            pool: null
        };
    },
    created() {
        this.getAgentPool();
        this.getAgents();
    },
    mounted() {
        this.$root.ui.active = 'project-agent-pool';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getAgentPool() {
            this.pool = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentpool/${this.poolId}`)).data;
        },
        async getAgents() {
            this.agents = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentpool/${this.poolId}/agent`)).data;
        },
        async deleteAgent(agent) {
            if (confirm(`Are you sure you want to delete ${agent.address}?`)) {
                var notify = notification.push(agent.address, 'Deleting agent...');
                await Pomelo.CQ.Delete(`/api/project/${this.projectId}/agentpool/${agent.agentPoolId}/agent/${agent.id}`);
                notify.message = 'Agent pool has been deleted.';
                notify.time = 10;
                notify.type = 'success';
                this.getAgents();
            }
        }
    }
});