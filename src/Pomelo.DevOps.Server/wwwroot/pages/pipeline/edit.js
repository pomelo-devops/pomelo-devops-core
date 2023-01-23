// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            projectId: null,
            pipelineId: null,
            func: null,
            pipeline: {
                id: null,
                name: null,
                timeout: -1,
                stages: [],
                arguments: [],
                hashmap: [],
                triggers: [],
                visibility: 'Private',
                name: 'New Pipeline',
                isNew: true
            }
        };
    },
    created() {
        // TODO: Fetch pipeline;
        this.pipeline.id = this.pipelineId;
        this.$root.data.currentPipeline = this.pipeline;
    },
    async mounted() {
        var pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        if (pipeline) {
            this.pipeline = pipeline;
        }

        if (this.func == null) {
            Pomelo.Redirect(`/project/${this.projectId}/pipeline/${this.pipelineId}/edit/${this.pipeline.type == 'Linear' ? 'playbook' : 'diagram'}`, { pipeline: this.pipeline });
            return;
        }

        if (this.func) {
            var container = this.$container('#pipeline-edit-main');
            container.open(`/pages/pipeline/edit/${this.func}`, { pipeline: this.pipeline });
        }
    },
    unmounted() {
        this.$root.data.currentPipeline = null;
    },
    watch: {
        pipeline() {
            this.$root.data.currentPipeline = this.pipeline;
        }
    },
    methods: {
        discard() {
            window.location.reload();
        },
        async save(withoutCheck) {
            // Try invoke onSave in sub views
            if (this.$containers.length) {
                if (this.$containers[0].active && this.$containers[0].active.onSave) {
                    var result = this.$containers[0].active.onSave();
                    if (result instanceof Promise) {
                        await result;
                    }
                }
            }

            withoutCheck = withoutCheck || false;
            var notify = notification.push('Saving Pipeline', `Saving pipeline '${this.pipeline.name || this.pipeline.id}'`, 'info', -1);

            // Check step values
            var checkSucceeded = true;
            if (!withoutCheck) {
                for (let i = 0; i < this.pipeline.stages.length; ++i) {
                    var stage = this.pipeline.stages[i];
                    if (!stage.agentPoolId) {
                        notification.push(stage.name, 'The agent pool is not specified.', 'warn', 10);
                    }

                    for (let j = 0; j < this.pipeline.stages[i].steps.length; ++j) {
                        var step = this.pipeline.stages[i].steps[j];
                        try {
                            var checker = require(`/api/gallery/${step.stepId}/version/${step.version}/checker`);
                            if (!checker.check) {
                                continue;
                            }
                            var result = checker.check(step);
                            if (result instanceof Promise) {
                                result = await result;
                            }

                            if (typeof result == 'string') {
                                result = [result];
                            }

                            if (!result instanceof Array) {
                                continue;
                            }

                            for (var k = 0; k < result.length; ++k) {
                                checkSucceeded = false;
                                notify.time = 10;
                                notify.message = 'One or more step field(s) validated failed';
                                notify.type = 'error';
                                notification.push(step.name, result[k], 'error', 10);
                                step.error = true;
                            }
                        } catch (ex) { }
                    }
                }
            }

            if (!checkSucceeded) {
                return;
            }

            // Generate computed fields
            for (let i = 0; i < this.pipeline.stages.length; ++i) {
                for (let j = 0; j < this.pipeline.stages[i].steps.length; ++j) {
                    this.pipeline.stages[i].steps[j].argumentsString = JSON.stringify(this.pipeline.stages[i].steps[j].arguments);
                }
            }

            this.$root.data.currentPipeline.isNew = false;

            try {
                await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}`, this.pipeline);
                notify.message = `Pipeline '${this.pipeline.name || this.pipeline.id}' have been saved.`
                notify.time = 10;
                notify.type = 'success';
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});