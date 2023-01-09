// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client.js');

Component('pipeline-arguments-form', {
    style: true,
    components: [
        '/components/radio-button/index',
        '/components/input-number/index',
        '/components/toggle-button/index'
    ],
    props: [ 'modelValue', 'projectId', 'pipelineId' ],
    data() {
        return {
            pipeline: null
        };
    },
    async created() {
        this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        for (var i = 0; i < this.pipeline.arguments.length; ++i) {
            var arg = this.pipeline.arguments[i];
            if (!this.modelValue[arg.argument]) {
                this.modelValue[arg.argument] = arg.default;
            }
        }
    }
});