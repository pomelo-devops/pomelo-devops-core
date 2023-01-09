// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/radio-button/index'
    ],
    data() {
        return {
            project: null
        };
    },
    async created() {
        this.getProject();
    },
    mounted() {
        this.$root.ui.active = 'project-settings';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        selectIcon() {
            document.querySelector('#file-org-icon').click();
        },
        onFileSelected() {
            var files = document.querySelector('#file-org-icon').files;
            if (!files.length) {
                return;
            }

            var file = files[0];
            if (!/image\/\w+/.test(file.type)) {
                alert("Invalid image");
                return;
            }

            var reader = new FileReader();
            var self = this;
            reader.onload = function () {
                self.project.iconBase64String = reader.result.substring(reader.result.indexOf('base64,') + 7);
            }
            reader.readAsDataURL(file);
        },
        async getProject() {
            this.project = (await Pomelo.CQ.Get(`/api/project/${this.projectId}`)).data;
        },
        async save() {
            var notify = notification.push('Edit Project', 'Saving...');
            try {
                console.log(this.project);
                var result = (await Pomelo.CQ.Patch(`/api/project/${this.projectId}`, this.project)).data;
                notify.message = `${this.project.name} has been saved`;
                notify.time = 10;
                notify.type = 'success';
                Pomelo.Redirect(`/project/${this.projectId}`);
                this.$root.data.defaultProject = result;
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});