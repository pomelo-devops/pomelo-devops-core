// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/radio-button/index',
        '/components/user-selector/index'
    ],
    data() {
        return {
            form: {
                userId: null,
                role: 'Member'
            },
            userSelectorVisibile: false,
            selectedUser: null
        };
    },
    mounted() {
        this.$root.ui.active = 'project-members';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async add() {
            if (!this.form.userId) {
                notification.push('Add Project Member', 'You must select a user.', 'error');
                return;
            }

            var notify = notification.push('Add Project Member', 'Adding...');
            try {
                var result = await Pomelo.CQ.Post(`/api/project/${this.projectId}/member`, this.form);
                notify.message = `${this.selectedUser.displayName} has been added to project`;
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