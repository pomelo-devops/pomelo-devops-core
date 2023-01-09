// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            pool: null
        };
    },
    created() {
        this.getAgentPool();
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
        async savePool() {
            if (!this.pool.name) {
                notification.push('Save Pool', 'The pool name could not be empty.', 'error', 10);
                return;
            }

            var notify = notification.push(this.pool.name, 'Saving agent pool...');
            await Pomelo.CQ.Patch(`/api/project/${this.projectId}/agentpool/${this.poolId}`, this.pool);
            notify.message = 'Agent pool has been saved.';
            notify.time = 10;
            notify.type = 'success';
            this.getAgentPools();
        }
    }
});