// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            form: {
                username: null,
                password: null,
                confirm: null,
                displayName: null,
                email: null,
                loginProviderId: null
            },
            providers: []
        };
    },
    mounted() {
        this.$root.ui.active = 'project-pipeline';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    computed: {
        localProvider() {
            return this.providers.filter(x => x.mode == 'Local')[0];
        }
    },
    methods: {
        async getProviders() {
            this.providers = (await Pomelo.CQ.Get('/api/loginprovider')).data;
            this.$forceUpdate();
            this.form.loginProviderId = this.localProvider.id;
        },
        async create() {
            console.log(this.form);
            if (!this.form.username) {
                notification.push('Create Local User', 'Username can not be empty.', 'error');
                return;
            }

            if (!this.form.password) {
                notification.push('Create Local User', 'Password can not be empty.', 'error');
                return;
            }

            if (this.form.password != this.form.confirm) {
                notification.push('Create Local User', 'The passwords you entered are inconsistent.', 'error');
                return;
            }

            if (!this.form.email) {
                notification.push('Create Local User', 'Email can not be empty.', 'error');
                return;
            }

            if (!this.form.displayName) {
                notification.push('Create Local User', 'Display name can not be empty.', 'error');
                return;
            }

            var notify = notification.push('Create Local User', 'Creating...');
            try {
                var result = await Pomelo.CQ.Put(`/api/user/${this.localProvider}/${this.form.username}`, this.form);
                notify.message = 'User has been created';
                notify.time = 10;
                notify.type = 'success';
                Pomelo.Redirect('/system/users');
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        }
    }
});