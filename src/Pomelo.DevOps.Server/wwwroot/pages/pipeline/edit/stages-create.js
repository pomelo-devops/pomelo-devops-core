Page({
    style: true,
    data() {
        return {
            form: {
                name: null,
                description: null,
                active: null
            }
        };
    },
    mounted() {
        this.$root.ui.active = 'pipeline-stages';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    created() {
        this.form.name = this.$root.localization.sr('PIPELINE_EDIT_PLAYBOOK_ADD_STAGE_DEFAULT');
    },
    methods: {
        async create() {
            if (!this.form.name) {
                notification.push('Create Stage Definition', 'Name cannot be empty', 'error');
                return;
            }

            var notify = notification.push('Create Stage Definition', 'Creating...');
            try {
                var result = await Pomelo.CQ.Post(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram-stage`, this.form);
                notify.type = 'success';
                notify.message = 'Stage definition has been created';
                notify.time = 10;

                this.$parent.getStages();
                this.$parent.editStage(result.data);
            } catch (ex) {
                notify.type = 'error';
                notify.message = ex.message;
                notify.time = 10;
            }
        }
    }
});