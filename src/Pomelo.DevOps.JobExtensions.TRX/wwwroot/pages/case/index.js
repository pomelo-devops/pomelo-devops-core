// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: [
        '@(css)',
        '/assets/css/font-awesome.css'
    ],
    data() {
        return {
            trxSuites: [],
            simpleSuites: [],
            trxResults: {},
            simpleResults: {}
        };
    },
    created() {
        this.getTrxSuites();
        this.getSimpleSuites();
    },
    methods: {
        async getTrxSuites() {
            this.trxSuites = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/extensions/${this.jobExtensionId}/api/trx`)).data;
            var self = this;
            await Promise.all(this.trxSuites.map(x => Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/extensions/${this.jobExtensionId}/api/trx/${encodeURIComponent(x)}`).then(function (result) {
                self.trxResults[x] = result.data;
            })));
        },
        async getSimpleSuites() {
            this.simpleSuites = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/extensions/${this.jobExtensionId}/api/simple`)).data;
            var self = this;
            await Promise.all(this.simpleSuites.map(x => Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/job/${this.jobNumber}/extensions/${this.jobExtensionId}/api/simple/${encodeURIComponent(x)}`).then(function (result) {
                self.simpleResults[x] = result.data;
            })));
        }
    }
});