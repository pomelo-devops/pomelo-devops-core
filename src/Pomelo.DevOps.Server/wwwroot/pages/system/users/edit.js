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
            username: null,
            providerId: null,
            user: null
        };
    },
    created() {
        this.getUser();
    },
    mounted() {
        this.$root.ui.active = 'system-users';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getUser() {
            this.user = (await Pomelo.CQ.Get(`/api/user/${this.providerId}/${this.username}`)).data;
            this.user.rawPassword = null;
        },
        async save() {
            var notify = notification.push('Edit User', 'Saving...');
            try {
                this.user = (await Pomelo.CQ.Patch(`/api/user/${this.providerId}/${this.username}`, this.user)).data;
                this.user.rawPassword = null;
                notify.message = `${this.user.displayName} has been saved.`;
                notify.type = 'success';
                notify.time = 10;
            } catch (ex) {
                notify.message = ex.message;
                notify.type = 'error';
                notify.time = 10;
            }
        }
    }
});