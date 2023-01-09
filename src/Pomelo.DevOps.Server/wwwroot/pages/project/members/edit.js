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
            userId: null,
            user: null
        };
    },
    created() {
        this.getUser();
    },
    mounted() {
        this.$root.ui.active = 'project-members';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getUser() {
            this.user = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/member/${this.userId}`)).data;
        },
        async save() {
            var notify = notification.push('Edit Project Member', 'Saving...');
            try {
                var result = await Pomelo.CQ.Patch(`/api/project/${this.projectId}/member/${this.user.userId}`, this.user);
                notify.message = `${this.user.user.displayName} has been saved`;
                notify.time = 10;
                notify.type = 'success';
                Pomelo.Redirect(`/project/${this.projectId}/members`);
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});