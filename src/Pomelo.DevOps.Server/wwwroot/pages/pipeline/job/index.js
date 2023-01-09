// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var duration = require('/shared/duration');

Page({
    layout: '/shared/devops',
    style: [
        '@(css)',
        '/assets/css/font-awesome.css'
    ],
    data() {
        return {
            job: null,
            view: null,
            toggles: [],
            active: null,
            logs: [],
            timestamp: null
        };
    },
    async created() {
        this.$root.ui.active = 'job-overview';
        this.getJob();
        await this.$root.startSignalR();
        var self = this;
        this.$root.signalr.invoke('join', `pipeline-${this.pipelineId}-job-${this.jobNumber}`);
        this.$root.signalr.on('jobchanged', function () {
            console.log('job changed');
            self.view.refresh();
        });
    },
    async unmounted() {
        this.$root.data.currentJob = null;
        this.$root.ui.active = null;

        try { this.$root.signalr.invoke('quit', `pipeline-${this.pipelineId}-job-${this.jobNumber}`); } catch (ex) { }
        try { this.$root.off('jobchanged'); } catch (ex) { }
        
        if (this.active) {
            try { this.$root.signalr.invoke('quit', `pipeline-${this.pipelineId}-job-${this.job.number}-log-${this.active}`); } catch (ex) { console.warn(ex) }
            try { this.$root.signalr.off('logreceived'); } catch (ex) { console.warn(ex); }
        }
        await this.$root.stopSignalR();
    },
    watch: {
        async active(newData, oldData) {
            if (oldData) {
                try { this.$root.signalr.invoke('quit', `pipeline-${this.pipelineId}-job-${this.job.number}-log-${oldData}`); } catch (ex) { console.warn(ex) }
                try { this.$root.signalr.off('logreceived'); } catch (ex) { console.warn(ex) }
            }
            var self = this;
            this.logs = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/log/${newData}`)).data;
            console.log(`pipeline-${this.pipelineId}-job-${this.job.number}-log-${newData}`);
            this.$root.signalr.invoke('join', `pipeline-${this.pipelineId}-job-${this.job.number}-log-${newData}`);
            this.$root.signalr.on('logreceived', function (text, level, time) {
                console.log('log received');
                self.logs.push({ time: time, text: text, level: level });
            });
        }
    },
    methods: {
        getJob() {
            var self = this;
            this.view = Pomelo.CQ.CreateView(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}`)
            this.view.fetch(function (result) {
                if (self.timestamp && result.timestamp <= self.timestamp) {
                    return;
                }
                self.job = result.data;
                self.$root.data.currentJob = self.job;
                if (self.toggles.length < self.job.stages.length) {
                    var delta = self.job.stages.length - self.toggles.length;
                    for (var i = 0; i < delta; ++i) {
                        self.toggles.push(false);
                    }
                }
            });
        },
        openLogs(logSet) {
            this.active = logSet;
        },
        duration(begin, end) {
            return duration.calc(begin, end);
        },
        async abort() {
            if (confirm("Are you sure you want to abort this job?")) {
                await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/abort`);
            }
        }
    }
});