// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    style: true,
    components: [
        '/components/pipeline-arguments-form/index'
    ],
    data() {
        return {
            pipeline: null,
            provider: null,
            trigger: null,
            external: null,
            mode: 'edit',
            newArgument: {
                key: null,
                value: null
            }
        };
    },
    async created() {
        if (!this.pipeline) {
            this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
        }
        if (this.trigger) {
            this.external = await this.getTriggerInfo(this.trigger.externalId);
            this.$parent.active = 'trigger-' + this.trigger.externalId;
        } else {
            this.mode = 'new';
            this.external = {
                name: 'GitLab MR',
                enabled: true,
                gitLabNamespace: null,
                gitLabProject: null,
                pomeloDevOpsProject: this.projectId,
                pomeloDevOpsPipeline: this.pipelineId,
                type: 'MergeRequest',
                arguments: {}
            };
            this.trigger = {
                name: 'GitLab MR',
                providerId: this.provider.id,
                pipelineId: this.pipelineId,
                enabled: true
            };
        }
    },
    computed: {
        arguments() {
            var ret = [];
            if (!this.external) {
                return ret;
            }
            var keys = Object.getOwnPropertyNames(this.external.arguments);
            for (var i = 0; i < keys.length; ++i) {
                ret.push({ key: keys[i], value: this.external.arguments[keys[i]] });
            }
            return ret;
        }
    },
    unmounted() {
        this.$parent.active = null;
    },
    watch: {
        deep: true,
        'trigger.enabled': function () {
            this.external.enabled = this.trigger.enabled;
        },
        'trigger.name': function () {
            this.external.name = this.trigger.name;
        }
    },
    methods: {
        async getTriggerInfo(externalId) {
            return await Pomelo.CQ.Get(`${this.provider.providerProxiedBaseUrl}/api/trigger/${externalId}`);
        },
        addArgument() {
            this.external.arguments[this.newArgument.key] = this.newArgument.value;
            this.newArgument = {
                key: null,
                value: null
            };
        },
        removeArgument(key) {
            delete this.external.arguments[key];
        },
        async createTrigger() {
            // 1. Create trigger in trigger service side
            var notify = notification.push(this.trigger.name, 'Creating trigger in trigger extension.');
            this.external.argumentsJson = JSON.stringify(this.external.arguments);
            var result = await Pomelo.CQ.Post(this.provider.providerProxiedBaseUrl + '/api/trigger', this.external);
            notify.time = 1;

            // 2. Callback
            this.trigger.externalId = result.id;
            this.$parent.onCreate(this.trigger);
        },
        async saveTrigger() {
            // 1. Save trigger in trigger service side
            var notify = notification.push(this.trigger.name, 'Saving trigger in trigger extension.');
            this.external.argumentsJson = JSON.stringify(this.external.arguments);
            var result = await Pomelo.CQ.Patch(this.provider.providerProxiedBaseUrl + '/api/trigger/' + this.external.id, this.external);
            notify.time = 1;

            // 2. Callback
            this.$parent.onSave(this.trigger);
        },
        async deleteTrigger() {
            if (confirm("Are you sure you want to delete this trigger?")) {
                // 1. Remove trigger in trigger service side
                var notify = notification.push(this.trigger.name, 'Removing trigger in trigger extension.');

                try {
                    await Pomelo.CQ.Delete(this.provider.providerProxiedBaseUrl + '/api/trigger/' + this.external.id, null, 'text');
                    notify.time = 1;
                } catch (ex) { console.warn(ex); }

                // 2. Callback
                this.$parent.onDelete(this.trigger);
            }
        }
    }
});