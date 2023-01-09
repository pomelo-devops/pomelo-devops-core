// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var pipelineClient = require('/shared/pipeline-client');

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            active: null,
            projectId: null,
            pipelineId: null,
            func: null,
            pipeline: null,
            providers: [],
            triggers: []
        }
    },
    async created() {
        this.getProviders();
        this.getTriggers();
        this.pipeline = await pipelineClient.getPipeline(this.projectId, this.pipelineId);
    },
    mounted() {
        this.$container('#trigger-main');
        this.$root.ui.active = 'pipeline-triggers';
    },
    unmounted() {
        this.$root.ui.active = null;
        this.$root.data.currentPipeline = null;
    },
    watch: {
        pipeline() {
            this.$root.data.currentPipeline = this.pipeline;
        }
    },
    methods: {
        async getTriggers() {
            this.triggers = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/trigger`)).data;
        },
        getProviderById(providerId) {
            var ret = this.providers.filter(x => x.id == providerId);
            if (!ret.length) {
                return null;
            }

            return ret[0];
        },
        async getProviders() {
            this.providers = (await Pomelo.CQ.Get(`/api/triggerProvider`)).data;
        },
        addTrigger() {
            this.$containers[0].open('/pages/pipeline/trigger/list', { triggers: this.providers });
        },
        editTrigger(trigger) {
            var provider = this.getProviderById(trigger.providerId);
            this.$containers[0].open(provider.proxiedViewUrl, {
                trigger: JSON.parse(JSON.stringify(trigger)),
                provider: provider,
                pipeline: this.pipeline
            });
        },
        async onCreate(trigger) {
            var notify = notification.push(trigger.name, 'Creating trigger...', 'info', -1);
            trigger = (await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/trigger`, trigger)).data;
            this.triggers.push(trigger);
            console.log(trigger);
            notify.message = `'${trigger.name}' have been created.`
            notify.time = 10;
            notify.type = 'success';
            this.$containers[0].close();
            this.editTrigger(trigger);
        },
        async onSave(trigger) {
            var index = this.triggers.filter(x => x.externalId == trigger.externalId);
            if (!index.length) {
                return;
            }
            index = this.triggers.indexOf(index[0]);
            this.triggers[index] = trigger;
            var notify = notification.push(trigger.name, 'Saving trigger...', 'info', -1);
            await Pomelo.CQ.Patch(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/trigger/${trigger.id}`, trigger);
            notify.message = `'${trigger.name}' have been saved.`
            notify.time = 10;
            notify.type = 'success';
            this.$containers[0].close();
            await this.getTriggers();
            this.editTrigger(trigger);
        },
        async onDelete(trigger) {
            var index = this.triggers.filter(x => x.externalId == trigger.externalId);
            if (!index.length) {
                return;
            }
            index = this.triggers.indexOf(index[0]);
            this.triggers.splice(index, 1);
            var notify = notification.push(trigger.name, 'Deleting trigger...', 'info', -1);
            await Pomelo.CQ.Delete(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/trigger/${trigger.id}`);
            notify.message = `'${trigger.name}' have been deleted.`
            notify.time = 10;
            notify.type = 'success';
            this.$containers[0].close();
            this.getTriggers();
        }
    }
});