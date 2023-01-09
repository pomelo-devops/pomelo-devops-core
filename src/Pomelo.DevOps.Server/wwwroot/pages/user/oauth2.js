// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var randomString = require('/shared/random-string');
var loginChecker = require('/shared/login-checker');

Page({
    data() {
        return {
            code: null,
            id: null,
            provider: null,
            redirect: null
        }
    },
    async created() {
        if (!this.redirect) {
            this.redirect = this.redirect || '/';
        }

        await this.getLoginProvider();

        if (window.localStorage.getItem('token')) {
            window.location = this.redirect;
        }

        if (this.code) {
            try {
                var result = (await Pomelo.CQ.Post(`/api/oauth2/${this.id}/code?redirect=${window.location.origin + window.location.pathname}`, { code: this.code })).data;
                loginChecker.storeSession(result);
                window.location = this.redirect;
            } catch (ex) {
                console.error(ex);
                window.location = `/login`;
                return;
            }
        } else {
            var callback = `${window.location.origin}/login/OAuth2/${this.id}`
            if (!this.provider.redirection.restrict) {
                callback = callback + '?redirect=' + encodeURIComponent(this.redirect);
            }
            window.location = `${this.provider.serverBaseUrl}${this.provider.redirection.endpoint}?client_id=${encodeURIComponent(this.provider.redirection.clientId)}&response_type=code&state=${randomString.rand()}&scope=${this.provider.redirection.scope || ''}&redirect_uri=${encodeURIComponent(callback)}`
        }
    },
    methods: {
        async getLoginProvider() {
            this.provider = (await Pomelo.CQ.Get(`/api/loginprovider/${this.id}`)).data;
            if (this.provider.mode != 'OAuth2') {
                window.location = `/login`;
                return;
            }
        }
    }
});