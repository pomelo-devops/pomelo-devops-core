// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');
var duration = require('/shared/duration');

Page({
    layout: '/shared/devops',
    style: [
        '@(css)',
        '/assets/css/font-awesome.css'
    ],
    data() {
        return {
            projectId: null,
            pipelineId: null,
            pipeline: null,
            labels: [],
            search: {
                name: null,
                label: null
            },
            result: null,
            paging: [1]
        };
    },
    created() {
        this.getPipeline();
        this.$root.ui.active = 'pipeline-jobs';
        this.getLabels();
        this.getJobs();
    },
    unmounted() {
        this.$root.ui.active = null;
        this.$root.data.currentPipeline = null;
    },
    watch: {
        deep: true,
        'search.name': function () {
            this.getJobs(1);
        },
        'search.label': function () {
            this.getJobs(1);
        }
    },
    methods: {
        async getPipeline() {
            this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
            this.$root.data.currentPipeline = this.pipeline;
        },
        async getLabels() {
            this.labels = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/labels`)).data;
        },
        async getJobs(page) {
            this.result = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job`, {
                name: this.search.name,
                labels: this.search.label,
                p: page || 1
            }));
            this.updatePagination();
        },
        duration(begin, end) {
            return duration.calc(begin, end);
        },
        toJob(jobNumber) {
            Pomelo.Redirect(`/project/${this.projectId}/pipeline/${this.pipelineId}/job/${jobNumber}`);
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
    }
});