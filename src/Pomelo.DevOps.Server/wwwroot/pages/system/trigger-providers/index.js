// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            search: {
                name: ''
            },
            providers: [],
            userId: window.localStorage.getItem('user'),
        };
    },
    created() {
        this.$root.ui.active = 'system-trigger-providers';
        this.getProviders();
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    computed: {
        filteredProviders() {
            return this.providers.filter(x => x.id.indexOf(this.search.name) >= 0
                || x.name.indexOf(this.search.name) >= 0);
        }
    },
    watch: {
        deep: true
    },
    methods: {
        async getProviders() {
            this.providers = (await Pomelo.CQ.Get(`/api/triggerprovider`)).data;
        },
        async remove(provider) {
            if (confirm(`Are you sure you want to remove ${provider.id}?`)) {
                var notify = notification.push('Remove provider', `Removing ${provider.id}...`);
                try {
                    await Pomelo.CQ.Delete(`/api/triggerprovider/${provider.id}`);
                    notify.message = `${provider.id} has been deleted`;
                    notify.type = 'success';
                    notify.time = 10;
                    this.getProviders();
                } catch (ex) {
                    notify.message = ex.message;
                    notify.type = 'error';
                    notify.time = 10;
                }
            }
        }
    }
});