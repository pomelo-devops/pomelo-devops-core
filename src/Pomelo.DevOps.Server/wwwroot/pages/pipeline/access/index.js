// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            search: {
                name: null,
                accessType: null
            },
            result: null,
            userId: window.localStorage.getItem('user'),
            paging: [1],
            pipeline: null
        };
    },
    async created() {
        this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        this.$root.data.currentPipeline = this.pipeline;
        this.$root.ui.active = 'pipeline-accesses';
        this.getAccesses(1);
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    watch: {
        deep: true,
        'search.name': function () {
            this.getAccesses(1);
        },
        'search.accessType': function () {
            this.getAccesses(1);
        }
    },
    methods: {
        async getAccesses(page) {
            this.result = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/access`, {
                name: this.search.name,
                type: this.search.accessType,
                p: page || 1
            }));
            this.updatePagination();
        },
        updatePagination() {
            if (!this.result) {
                return;
            }

            var current = this.result.currentPage;
            var begin = current - 2;
            var end = current + 2;
            var max = this.result.totalPages;
            begin = Math.max(1, begin);
            end = Math.min(end, max);
            this.paging = this.generateArray(begin, end);
        },
        generateArray(begin, end) {
            var arr = [];
            if (begin > end) {
                return arr;
            }

            for (var i = begin; i <= end; ++i) {
                arr.push(i);
            }

            return arr;
        },
        async remove(access) {
            if (confirm(`Are you sure you want to remove ${access.user.displayName}'s access' from current pipeline?`)) {
                var notify = notification.push('Remove access', `Removing ${access.user.displayName}...`);
                try {
                    await Pomelo.CQ.Delete(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/access/${access.userId}`);
                    notify.message = `${access.user.displayName}'s access has been deleted.`;
                    notify.type = 'success';
                    notify.time = 10;
                    this.getAccesses(this.result.currentPage);
                } catch (ex) {
                    notify.message = ex.message;
                    notify.type = 'error';
                    notify.time = 10;
                }
            }
        }
    }
});