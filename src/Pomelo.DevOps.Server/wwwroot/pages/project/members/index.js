// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            search: {
                name: null,
                role: null
            },
            result: null,
            userId: window.localStorage.getItem('user'),
            paging: [1],
        };
    },
    created() {
        this.$root.ui.active = 'project-members';
        this.getMembers(1);
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    watch: {
        deep: true,
        'search.name': function () {
            this.getMembers(1);
        },
        'search.role': function () {
            this.getMembers(1);
        }
    },
    methods: {
        async getMembers(page) {
            this.result = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/member`, {
                name: this.search.name,
                role: this.search.role,
                p: page || 1
            }));
            this.updatePagination();
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
        async remove(member) {
            if (confirm(`Are you sure you want to remove ${member.user.displayName} from current project?`)) {
                var notify = notification.push('Remove member', `Removing ${member.user.displayName}...`);
                try {
                    await Pomelo.CQ.Delete(`/api/project/${this.projectId}/member/${member.userId}`);
                    notify.message = `${member.user.displayName} has been deleted`;
                    notify.type = 'success';
                    notify.time = 10;
                    this.getMembers(this.result.currentPage);
                } catch (ex) {
                    notify.message = ex.message;
                    notify.type = 'error';
                    notify.time = 10;
                }
            }
        }
    }
});