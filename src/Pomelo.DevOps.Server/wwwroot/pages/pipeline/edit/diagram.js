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
        async getWorkflowVersion() {
            this.workflowVersion = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram`)).data;
        },
        add(node, width, height) {
            console.log(lifecycleManager.getById('pipeline-diagram-panel'));
            lifecycleManager.getById('pipeline-diagram-panel').addNode = { key: node, width: width, height: height };
            lifecycleManager.getById('pipeline-diagram-panel').mode = 'add';
        }
    }
});