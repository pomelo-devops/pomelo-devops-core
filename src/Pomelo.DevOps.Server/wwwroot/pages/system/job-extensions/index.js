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
            extensions: [],
            userId: window.localStorage.getItem('user'),
        };
    },
    created() {
        this.$root.ui.active = 'system-job-extensions';
        this.getProviders();
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    computed: {
        filteredExtensions() {
            return this.extensions.filter(x => x.id.indexOf(this.search.name) >= 0
                || x.name.indexOf(this.search.name) >= 0);
        }
    },
    watch: {
        deep: true
    },
    methods: {
        async getProviders() {
            this.extensions = (await Pomelo.CQ.Get(`/api/jobextension`)).data;
        },
        async remove(extension) {
            if (confirm(`Are you sure you want to remove ${extension.id}?`)) {
                var notify = notification.push('Remove extension', `Removing ${extension.id}...`);
                try {
                    await Pomelo.CQ.Delete(`/api/jobextension/${extension.id}`);
                    notify.message = `${extension.id} has been deleted`;
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