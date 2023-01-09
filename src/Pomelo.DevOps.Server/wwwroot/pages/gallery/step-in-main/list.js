// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    data() {
        return {
            paging: [1],
            result: null,
            search: {
                name: null,
                platform: null,
                _text: null,
                page: 1
            },
            stage: null,
            step: {
                stepId: null,
                version: null,
                method: null,
                name: null,
                arguments: {},
                order: null,
                timeout: -1,
                condition: 'RequirePreviousTaskSuccess',
                temp: null,
                retry: 0,
                errorHandlingMode: 'Normal'
            }
        }
    },
    created() {
        this.getPackages();
    },
    mounted() {
        this.$parent.active = 'gallery-' + this.stage.temp;
    },
    unmounted() {
        this.$parent.active = null;
    },
    watch: {
        deep: true,
        'search.platform': function () {
            this.getPackages();
        },
        'search.name': function () {
            this.getPackages();
        }
    },
    methods: {
        async getPackages(p) {
            this.result = await Pomelo.CQ.Get(`/api/gallery`, {
                platform: this.search.platform,
                name: this.search.name,
                page: p || 1
            });
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
        async importStep(step) {
            this.stage.toggled = true;
            this.step.stepId = step.id;
            this.step.version = step.version;
            this.step.method = step.methods ? step.methods.split(',').map(x => x.trim())[0] : null;
            this.step.name = step.name;
            this.step.order = this.stage.steps.length + 1;
            this.step.temp = new Date().getTime();
            this.stage.steps.push(JSON.parse(JSON.stringify(this.step)));
            this.$parent.rebindDragForSteps(this.stage);
        }
    }
});