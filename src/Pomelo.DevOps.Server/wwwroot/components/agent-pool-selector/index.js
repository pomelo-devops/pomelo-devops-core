// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('agent-pool-selector', {
    style: true,
    props: ['title', 'modelValue', 'visible', 'projectId'],
    data() {
        return {
            selected: null,
            search: null,
            pools: []
        };
    },
    created() {
        var self = this;
        Pomelo.CQ.CreateView(`/api/project/${this.projectId}/agentpool`, {})
            .fetch(function (result) {
                self.pools = result.data;
            });
    },
    computed: {
        filteredPools() {
            if (this.search) {
                return this.pools.filter(x => x.name.indexOf(this.search) >= 0 || x.id.indexOf(this.search) >= 0);
            } else {
                return this.pools;
            }
        }
    },
    methods: {
        select(id) {
            this.selected = id;
        },
        close() {
            this.$emit('update:visible', false);
            this.visible = false;
        },
        callback() {
            this.$emit('update:modelValue', this.selected);
            this.$emit('update:visible', false);
        }
    }
});