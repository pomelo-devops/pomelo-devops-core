// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var randomString = require('/shared/random-string');

Page({
    style: true,
    components: [
        '/components/radio-button/index'
    ],
    data() {
        return {
            pipeline: null
        };
    },
    async mounted() {
        this.$root.ui.active = 'pipeline-consts';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        addConstant() {
            if (!this.pipeline.constants) {
                this.pipeline.constants = [];
            }

            this.pipeline.constants.push({
                key: 'KEY_' + randomString.rand(6).toUpperCase(),
                type: 'Plain',
                value: null
            });
        },
        removeConstant(arg) {
            var index = this.pipeline.constants.indexOf(arg);
            if (index == -1) {
                return;
            }

            if (confirm("Are you sure you want to remove this argument?")) {
                this.pipeline.constants.splice(index, 1);
            }
        }
    }
});