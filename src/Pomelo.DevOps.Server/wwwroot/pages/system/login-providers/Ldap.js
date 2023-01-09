// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    components: [
        '/components/input-number/index',
        '/components/toggle-button/index',
        '/components/radio-button/index'
    ],
    data() {
        return {
            mode: 'new',
            provider: {
                id: null,
                mode: 'Ldap',
                priority: 1,
                name: 'New Provider',
                enabled: true,
                iconUrl: '/assets/img/oauth2.svg',
                usernamePath: 'sAMAccountName',
                emailPath: 'userPrincipalName',
                displayNamePath: 'displayName',
                userNamePlaceholder: null,
                hint: null,
                ldapServer: null,
                port: 389,
                searchBase: 'DC=example,DC=com',
                loginBackgroundUrl: '/assets/img/ldap-sidebar.png'
            }
        };
    },
    created() {
        this.provider.userNamePlaceholder = this.$root.localization.sr('SYSTEM_LOGIN_PROVIDER_LDAP_USERNAME_INPUT_PLACEHOLDER_TEXT_DEFAULT');
        this.provider.hint = this.$root.localization.sr('SYSTEM_LOGIN_PROVIDER_LDAP_LOGIN_HINT_DEFAULT');
    },
    mounted() {
        if (this.mode == 'new') {
            this.$parent.active = 'new';
        } else {
            this.$parent.active = this.provider.id;
        }
    },
    unmounted() {
        this.$parent.active = null;
    },
    methods: {
        async create() {
            if (!this.provider.id) {
                notification.push('LDAP Authentication', 'You must specify an ID for the provider.', 'error', 10);
                return;
            }

            if (this.$parent.providers.some(x => x.id == this.provider.id)) {
                notification.push('LDAP Authentication', `${this.provider.id} is already existed`, 'error', 10);
                return;
            }

            if (!/^[a-zA-Z0-9-_]{1,}$/.test(this.provider.id)) {
                notification.push('Error', 'Invalid provider ID', 'error');
                return;
            }

            if (!this.provider.name) {
                notification.push('LDAP Authentication', 'You must specify a name for the provider.', 'error', 10);
                return;
            }

            var notify = notification.push('LDAP Authentication', 'Creating provider...');
            try {
                var result = await Pomelo.CQ.Post(`/api/loginprovider/${this.provider.id}`, this.provider);
            } catch (ex) {
                notify.message = ex.message;
                notify.type = 'error';
                notify.time = 10;
                return;
            }
            notify.message = `${this.provider.name} has been created.`;
            notify.type = 'success';
            notify.time = 10;
            this.$parent.getProviders();
            this.$parent.$containers[0].close();
        },
        async save() {
            if (!this.provider.name) {
                notification.push('LDAP Authentication', 'You must specify a name for the provider.', 'error', 10);
                return;
            }

            var notify = notification.push('LDAP Authentication', 'Saving provider...');
            try {
                var result = await Pomelo.CQ.Post(`/api/loginprovider/${this.provider.id}`, this.provider);
            } catch (ex) {
                notify.message = ex.message;
                notify.type = 'error';
                notify.time = 10;
                return;
            }
            notify.message = `${this.provider.name} has been saved.`;
            notify.type = 'success';
            notify.time = 10;
            this.$parent.$containers[0].close();
        },
        async remove() {
            if (!confirm('Are you sure you want to delete this provider?')) {
                return;
            }

            var notify = notification.push('LDAP Authentication', 'Deleting provider...');
            try {
                var result = await Pomelo.CQ.Delete(`/api/loginprovider/${this.provider.id}`);
            } catch (ex) {
                notify.message = ex.message;
                notify.type = 'error';
                notify.time = 10;
                return;
            }
            notify.message = `${this.provider.name} has been deleted.`;
            notify.type = 'success';
            notify.time = 10;
            this.$parent.getProviders();
            this.$parent.$containers[0].close();
        }
    }
});