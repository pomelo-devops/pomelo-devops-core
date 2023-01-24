var lifecycleManager = require('/assets/js/pomelo-workflow-pomelo-vue/lifecycleManager').lifecycleManager;

Page({
    layout: '/shared/devops',
    components: [
        '/assets/js/pomelo-workflow-pomelo-vue/index'
    ],
    style: true,
    data() {
        return {
            isStage: false, // Only for stage diagram
            diagramStageId: null, // Only for stage diagram
            pipeline: null,
            workflowVersion: null,
            versions: [],
            version: null,
            active: null
        };
    },
    created() {
        this.getWorkflowVersions();
        this.getWorkflowVersion();
    },
    computed: {
        innerActive() {
            var com = lifecycleManager.getById('pipeline-diagram-panel');
            if (!com) {
                return null;
            }

            return com.active;
        }
    },
    mounted() {
        if (this.isStage) {
            this.$parent.active = this.diagramStageId;
            this.$root.ui.active = 'pipeline-stages';
        } else {
            this.$root.ui.active = 'pipeline-diagram';
        }
    },
    unmounted() {
        this.$root.ui.active = null;
        if (this.isStage) {
            this.$parent.active = null;
        }
    },
    watch: {
        version() {
            this.getWorkflowVersion(this.version);
        }
    },
    methods: {
        getLifecycleManager() {
            return lifecycleManager;
        },
        settings() {
            if (this.active) {
                this.active = null;
                return;
            }

            if (!lifecycleManager.getById('pipeline-diagram-panel').active) {
                return;
            }

            this.active = lifecycleManager.getById('pipeline-diagram-panel').active;
        },
        discard() {
            window.location.reload();
        },
        async save() {
            if (!this.isStage) {
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
            } else {
                var notify = notification.push('Stage Diagram', 'Saving stage diagram...');
                try {
                    var version = (await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram-stage/${this.diagramStageId}/version`, this.workflowVersion.diagram)).data;
                    await this.getWorkflowVersion(version);
                    notify.type = 'success';
                    notify.message = 'Stage diagram saved';
                    notify.time = 10;
                } catch (ex) {
                    notify.type = 'error';
                    notify.message = ex.message;
                    notify.time = 10;
                }
            }
        },
        async getWorkflowVersion(version) {
            this.workflowVersion = null;
            if (!this.isStage) {
                this.workflowVersion = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram${((!version) ? '' : `/version/${version}`)}`)).data;
            } else {
                this.workflowVersion = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram-stage/${this.diagramStageId}${((!version) ? '' : `/version/${version}`)}`)).data;
            }
            this.version = this.workflowVersion.version;
        },
        async getWorkflowVersions() {
            if (!this.isStage) {
                this.versions = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram/version`)).data;
            } else {
                this.versions = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram-stage/${this.diagramStageId}/version`)).data;
            }
        },
        add(node, width, height) {
            console.log(lifecycleManager.getById('pipeline-diagram-panel'));
            lifecycleManager.getById('pipeline-diagram-panel').addNode = { key: node, width: width, height: height };
            lifecycleManager.getById('pipeline-diagram-panel').mode = 'add';
        }
    }
});