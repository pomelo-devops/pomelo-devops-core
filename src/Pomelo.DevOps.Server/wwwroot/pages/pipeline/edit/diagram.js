var lifecycleManager = require('/assets/js/pomelo-workflow-pomelo-vue/lifecycleManager').lifecycleManager;

Page({
    layout: '/shared/devops',
    components: [
        '/assets/js/pomelo-workflow-pomelo-vue/index'
    ],
    style: true,
    data() {
        return {
            pipeline: null,
            workflowVersion: null
        };
    },
    async created() {
        await this.getWorkflowVersion();
    },
    mounted() {
        this.$root.ui.active = 'pipeline-diagram';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        discard() {
            window.location.reload();
        },
        async save() {
            var notify = notification.push('Pipeline Diagram', 'Saving pipeline diagram...');
            try {
                var version = (await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram`, this.workflowVersion.diagram)).data;
                await this.getWorkflowVersion(version);
                notify.type = 'success';
                notify.message = 'Pipeline diagram saved';
                notify.time = 10;
            } catch (ex) {
                notify.type = 'error';
                notify.message = ex.message;
                notify.time = 10;
            }
        },
        async getWorkflowVersion(version) {
            this.workflowVersion = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram${((!version) ? '' : `/version/${version}`)}`)).data;
        },
        add(node, width, height) {
            console.log(lifecycleManager.getById('pipeline-diagram-panel'));
            lifecycleManager.getById('pipeline-diagram-panel').addNode = { key: node, width: width, height: height };
            lifecycleManager.getById('pipeline-diagram-panel').mode = 'add';
        }
    }
});