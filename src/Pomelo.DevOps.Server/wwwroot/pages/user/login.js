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
            providers: [],
            form: {
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

        await this.getProviders();
    },
    computed: {
        localProvider() {
            return this.providers.filter(x => x.mode == 'Local')[0];
        }
    },
    methods: {
        async submit() {
            var notify = notification.push('Signing in...', 'Validating account...');
            try {
                var result = (await Pomelo.CQ.Post(`/api/user/${this.localProvider.id}/${this.form.username}/session`, this.form)).data;
                notify.type = 'success';
                notify.message = 'Sign in succeeded';
                notify.time = 5;
                loginChecker.storeSession(result);
                window.location = this.redirect;
            } catch (ex) {
                console.warn(ex);
                notify.time = 10;
                notify.title = 'Sign in failed';
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