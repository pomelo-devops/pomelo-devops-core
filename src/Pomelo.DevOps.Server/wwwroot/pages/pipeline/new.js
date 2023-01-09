// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            projectId: null,
            form: {
                id: null,
                name: null
            }
        };
    },
    mounted() {
        this.$root.ui.active = 'project-pipeline';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async create() {
            if (!this.form.name) {
                notification.push('Error', 'The project name cannot be empty.', 'error');
                return;
            }

            if (!/^[a-zA-Z0-9-_]{1,}$/.test(this.form.id)) {
                notification.push('Error', 'Pipeline ID is invalid', 'error');
                return;
            }

            var result = await Pomelo.CQ.Get(`/api/misc/check-pipeline-id-available?pipelineId=${encodeURIComponent(this.form.id)}`);
            if (!result.data) {
                notification.push('Error', 'The pipeline ID is already taken.', 'error');
                return;
            }

            Pomelo.Redirect(`/project/${this.projectId}/pipeline/${this.form.id}/edit`);
        }
    }
});