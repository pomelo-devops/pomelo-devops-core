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
            redirect: null,
            provider: null,
            login: {
                username: null,
                password: null
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

        await this.getProvider();
    },
    methods: {
        async getProvider() {
            this.provider = (await Pomelo.CQ.Get(`/api/loginprovider/${this.id}`)).data;
        },
        async signIn() {
            var notify = notification.push('Signing in...', 'Validating account...');
            try {
                var result = (await Pomelo.CQ.Post(`/api/ldap/${this.id}`, this.login)).data;
                notify.type = 'success';
                notify.message = 'Sign in succeeded';
                notify.time = 5;
                loginChecker.storeSession(result);
                window.location = this.redirect;
            } catch (ex) {
                notify.time = 10;
                notify.title = 'Sign in failed';
                notify.message = ex.Message;
                notify.type = 'error'
                return;
            }
        }
    }
});