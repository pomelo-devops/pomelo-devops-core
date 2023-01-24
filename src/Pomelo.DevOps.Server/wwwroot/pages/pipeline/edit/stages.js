Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            stages: [],
            active: null
        };
    },
    mounted() {
        this.$root.ui.active = 'pipeline-stages';
        this.$container('#pipeline-stages-main');
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    created() {
        this.getStages();
    },
    methods: {
        async getStages() {
            this.stages = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram-stage`)).data;
        },
        editStage(stage) {
            this.$containers[0].open('/pages/pipeline/edit/diagram', { isStage: true, diagramStageId: stage.id });
        },
        createStage() {
            this.$containers[0].open('/pages/pipeline/edit/stages-create');
        }
    }
});