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
                mode: 'Local',
                priority: 1,
                name: 'New Provider',
                enabled: true,
                allowRegister: false
            }
        };
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
                notification.push('Local Authentication', 'You must specify an ID for the provider.', 'error', 10);
                return;
            }

            if (!/^[a-zA-Z0-9-_]{1,}$/.test(this.provider.id)) {
                notification.push('Error', 'Invalid provider ID', 'error');
                return;
            }

            if (this.$parent.providers.some(x => x.id == this.provider.id)) {
                notification.push('Local Authentication', `${this.provider.id} is already existed`, 'error', 10);
                return;
            }

            if (!this.provider.name) {
                notification.push('Local Authentication', 'You must specify a name for the provider.', 'error', 10);
                return;
            }

            var notify = notification.push('Local Authentication', 'Creating provider...');
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
                notification.push('Local Authentication', 'You must specify a name for the provider.', 'error', 10);
                return;
            }

            var notify = notification.push('Local Authentication', 'Saving provider...');
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

            var notify = notification.push('Local Authentication', 'Deleting provider...');
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