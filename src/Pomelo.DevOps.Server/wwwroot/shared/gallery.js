// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var loginChecker = require('/shared/login-checker');
var moment = require('/assets/js/moment/moment.js');

Layout({
    style: [
        '@(css)',
        '/assets/css/font-awesome.css'
    ],
    components: [
        '/components/notification/index'
    ],
    data() {
        return {
            name: null,
            ui: {
                menu: false,
                login: false,
                search: null
            },
            policies: []
        };
    },
    async created() {
        this.ui.login = await loginChecker.check();
        if (this.name) {
            this.ui.search = this.name;
        }
        this.getPolicies();
    },
    computed: {
        'currentUserName'() {
            if (this.ui.login) {
                return window.localStorage.getItem('user');
            } else {
                return null;
            }
        }
    },
    methods: {
        searchPackage() {
            if (this.ui.search) {
                Pomelo.Redirect(`/gallery?name=${encodeURIComponent(this.ui.search)}`);
            } else {
                Pomelo.Redirect(`/gallery`);
            }
        },
        moment(str) {
            return moment(str);
        },
        signOut() {
            loginChecker.signOut();
        },
        signIn() {
            Pomelo.Redirect(`/login?redirect=${encodeURIComponent(window.location.pathname + window.location.search)}`);
        },
        async getPolicies() {
            this.policies = (await Pomelo.CQ.Get('/api/policy')).data;
            document.querySelector('title').innerHTML = this.getPolicyValue('GALLERY_NAME');
        },
        getPolicyValue(key) {
            var policy = this.policies.filter(x => x.key == key)[0];
            if (!policy) {
                return null;
            } else {
                return policy.value;
            }
        },
    }
});