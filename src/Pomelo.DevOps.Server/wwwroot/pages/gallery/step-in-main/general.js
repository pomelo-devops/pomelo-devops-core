// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    components: [
        '/components/radio-button/index',
        '/components/input-number/index'
    ],
    data() {
        return {
            step: null,
            packages: null,
        };
    },
    mounted() {
        this.$parent.active = 'general';
    },
    unmounted() {
        this.$parent.active = null;
    },
    methods: {
        deleteStep() {
            if (confirm("Are you sure you want to delete this step?")) {
                var parent = this.$parent.$parent;
                var stage = parent.pipeline.stages.filter(x => x.steps.indexOf(this.step) >= 0)[0];
                stage.steps.splice(stage.steps.indexOf(this.step), 1);
                this.$parent.$parent.$containers[0].close();
            }
        }
    }
});