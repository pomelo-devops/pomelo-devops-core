// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var loginChecker = require('/shared/login-checker');

Page({
    style: true,
    components: [
        '/components/notification/index'
    ],
    data() {
        return {
            succeeded: false,
            providers: [],
            form: {
                username: null,
                password: null,
                confirm: null,
                email: null
            }
        };
    },
    async created() {
        if (!this.redirect) {
            this.redirect = this.redirect || '/';
        }

        if (await loginChecker.check()) {
            Pomelo.Redirect(this.redirect);
            return;
        }

        await this.getProviders();
    },
    computed: {
        localProvider() {
            return this.providers.filter(x => x.mode == 'Local')[0];
        }
    },
    methods: {
        async submit() {
            if (this.form.password != this.form.confirm) {
                notification.push('Create Account', 'Password confirm and password are inconsistent', 'error', 10);
                return;
            }

            if (!this.form.username) {
                notification.push('Create Account', 'Username cannot be empty', 'error', 10);
                return;
            }

            if (!this.form.email || !/^[^@]+@[^@]+\.[^@]+$/.test(this.form.email)) {
                notification.push('Create Account', 'Invalid email address', 'error', 10);
                return;
            }

            if (!this.form.password) {
                notification.push('Create Account', 'Password cannot be empty', 'error', 10);
                return;
            }

            var notify = notification.push('Create Account', 'Creating account...');
            try {
                var result = (await Pomelo.CQ.Put(`/api/user/${this.localProvider.id}/${this.form.username}`, this.form)).data;
                notify.type = 'success';
                notify.message = 'Account created';
                notify.time = 5;
                this.succeeded = true;

            } catch (ex) {
                console.warn(ex);
                notify.time = 10;
                notify.title = 'Create Account';
                notify.message = ex.message;
                notify.type = 'error'
                return;
            }
        },
        async getProviders() {
            this.providers = (await Pomelo.CQ.Get(`/api/loginprovider`)).data;
            if (this.providers.length == 1 && this.providers[0].mode != 'Local') {
                Pomelo.Redirect(`/login/${this.providers[0].mode}/${this.providers[0].id}`);
                return;
            }
        }
    }
});