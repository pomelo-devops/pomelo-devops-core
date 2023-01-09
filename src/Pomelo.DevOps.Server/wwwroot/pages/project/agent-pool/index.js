// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            pools: [],
            form: {
                name: null
            }
        };
    },
    created() {
        this.getAgentPools();
    },
    mounted() {
        this.$root.ui.active = 'project-agent-pool';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getAgentPools() {
            this.pools = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentpool`)).data;
        },
        async deleteAgentPool(pool) {
            if (confirm(`Are you sure you want to delete ${pool.name}? ${pool.agents.length} agent(s) will be also removed.`)) {
                var notify = notification.push(pool.name, 'Deleting agent pool...');
                await Pomelo.CQ.Delete(`/api/project/${this.projectId}/agentpool/${pool.id}`);
                notify.message = 'Agent pool has been deleted.';
                notify.time = 10;
                notify.type = 'success';
                this.getAgentPools();
            }
        },
        async createPool() {
            if (!this.form.name) {
                notification.push('Create Pool', 'The pool name could not be empty.', 'error', 10);
                return;
            }

            var notify = notification.push(this.form.name, 'Creating agent pool...');
            await Pomelo.CQ.Post(`/api/project/${this.projectId}/agentpool`, this.form);
            notify.message = 'Agent pool has been created.';
            notify.time = 10;
            notify.type = 'success';
            this.getAgentPools();
        }
    }
});