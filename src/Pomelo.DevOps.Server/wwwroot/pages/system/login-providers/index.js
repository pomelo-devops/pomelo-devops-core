// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            providers: [],
            active: null
        };
    },
    created() {
        this.getProviders();
    },
    mounted() {
        this.$root.ui.active = 'system-login-providers';
        this.$container('#login-provider-container');
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getProviders() {
            this.providers = (await Pomelo.CQ.Get(`/api/loginprovider`, { includeInvisible: true })).data;
        },
        addProvider() {
            this.$containers[0].open('/pages/system/login-providers/new');
        },
        editProvider(provider) {
            this.$containers[0].open(`/pages/system/login-providers/${provider.mode}`, { provider: provider, mode: 'edit' });
        }
    }
});