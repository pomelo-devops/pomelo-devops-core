// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            search: {
                name: null
            },
            result: null,
            paging: [1]
        };
    },
    created() {
        this.$root.ui.active = 'system-widgets';
        this.getWidgets();
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    watch: {
        deep: true,
        'search.name': function () {
            this.getWidgets();
        }
    },
    methods: {
        async getWidgets(page) {
            this.result = (await Pomelo.CQ.Get(`/api/widget`, {
                name: this.search.name,
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
        async remove(widget) {
            if (confirm(`Are you sure you want to remove ${widget.name}?`)) {
                var notify = notification.push('Remove Widget', `Removing ${widget.name}...`);
                try {
                    await Pomelo.CQ.Delete(`/api/widget/${widget.id}`);
                    notify.message = `${widget.name} has been deleted`;
                    notify.type = 'success';
                    notify.time = 10;
                    this.getWidgets(this.result.currentPage);
                } catch (ex) {
                    notify.message = ex.message;
                    notify.type = 'error';
                    notify.time = 10;
                }
            }
        }
    }
});