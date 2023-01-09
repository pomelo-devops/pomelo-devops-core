// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            projectId: null,
            data: {
                pipelines: []
            },
            views: {
                pipelines: null
            },
            search: {
                type: null,
                text: null
            }
        };
    },
    created() {
        this.getPipelines();
    },
    mounted() {
        this.$root.ui.active = 'project-pipeline';
    },
    computed: {
        pipelines() {
            var ret = this.data.pipelines;
            if (this.search.type) {
                ret = ret.filter(x => x.visibility.toLowerCase() == this.search.type);
            }
            if (this.search.text) {
                ret = ret.filter(x => x.id.indexOf(this.search.text) >= 0 || x.name.indexOf(this.search.text) >= 0);
            }
            return ret;
        }
    },
    methods: {
        async getPipelines() {
            var result = await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline`, {});
            for (var i = 0; i < result.data.length; ++i) {
                result.data[i].lastStatus = null;
                if (result.data[i].jobs.length) {
                    var lastJob = result.data[i].jobs[0];
                    if (lastJob.status == 'Succeeded') {
                        result.data[i].lastStatus = 'succeeded';
                    } else if (lastJob.status == 'Failed') {
                        result.data[i].lastStatus = 'failed';
                    }

                    result.data[i].maxDuration = 0;
                    for (var j = 0; j < result.data[i].jobs.length; ++j) {
                        var job = result.data[i].jobs[j];
                        if (!job.startedAt) {
                            job.startedAt = new Date().toISOString();
                        }
                        if (!job.finishedAt) {
                            job.finishedAt = new Date().toISOString();
                        }
                        if (!job.triggeredAt) {
                            job.triggeredAt = new Date().toISOString();
                        }
                        if (job.status == 'Pending' || job.status == 'Waiting') {
                            job.duration = new Date().getTime() - new Date(job.triggeredAt).getTime();
                        } else {
                            job.duration = new Date(job.finishedAt).getTime() - new Date(job.startedAt).getTime();
                        }
                        result.data[i].maxDuration = Math.max(result.data[i].maxDuration, job.duration);
                    }
                    if (result.data[i].maxDuration == 0) {
                        result.data[i].maxDuration = 1;
                    }
                }
            }
            this.data.pipelines = result.data;
        }
    },
    unmounted() {
        this.$root.ui.active = null;
    }
});