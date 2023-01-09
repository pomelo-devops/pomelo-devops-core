// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            search: {
                name: null,
                role: null,
                provider: null
            },
            result: null,
            paging: [1],
            providers: [],
            roleMapping: {
                'SystemAdmin': 'System Admin',
                'Collaborator': 'Collaborator'
            }
        };
    },
    created() {
        this.$root.ui.active = 'system-users';
        this.getUsers(1);
        this.getProviders();
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    watch: {
        deep: true,
        'search.name': function () {
            this.getUsers(1);
        },
        'search.role': function () {
            this.getUsers(1);
        },
        'search.provider': function () {
            this.getUsers(1);
        }
    },
    computed: {
        localProvider() {
            return this.providers.filter(x => x.mode == 'Local')[0];
        }
    },
    methods: {
        async getUsers(page) {
            this.result = (await Pomelo.CQ.Get(`/api/user`, {
                name: this.search.name,
                role: this.search.role,
                provider: this.search.provider,
                p: page || 1
            }));
            this.updatePagination();
        },
        getProviderById(id) {
            return this.providers.filter(x => x.id == id)[0];
        },
        getProviderName(id) {
            var provider = this.providers.filter(x => x.id == id)[0];
            return provider ? provider.name : id;
        },
        async getProviders() {
            this.providers = (await Pomelo.CQ.Get(`/api/loginprovider`)).data;
        },
        updatePagination() {
            if (!this.result) {
                return;
            }

            var current = this.result.currentPage;
            var begin = current - 2;
            var end = current + 2;
            var max = this.result.totalPages;
            begin = Math.max(1, begin);
            end = Math.min(end, max);
            this.paging = this.generateArray(begin, end);
        },
        generateArray(begin, end) {
            var arr = [];
            if (begin > end) {
                return arr;
            }

            for (var i = begin; i <= end; ++i) {
                arr.push(i);
            }

            return arr;
        },
        async remove(user) {
            if (confirm(`Are you sure you want to remove ${user.displayName}? All of the resources like pipeline, packages which belongs to the account will also be removed.`)) {
                var notify = notification.push('Delete User', `Deleting ${user.displayName}...`);
                try {
                    await Pomelo.CQ.Delete(`/api/user/${user.loginProviderId}/${user.username}`);
                    notify.message = `${user.displayName} has been deleted`;
                    notify.type = 'success';
                    notify.time = 10;
                    this.getUsers(this.result.currentPage);
                } catch (ex) {
                    notify.message = ex.message;
                    notify.type = 'error';
                    notify.time = 10;
                }
            }
        }
    }
});