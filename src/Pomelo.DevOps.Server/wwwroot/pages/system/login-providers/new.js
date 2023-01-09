// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    mounted() {
        this.$parent.active = 'new';
    },
    unmounted() {
        this.$parent.active = null;
    },
    methods: {
        select(mode) {
            this.$parent.$containers[0].open(`/pages/system/login-providers/${mode}`);
        }
    }
});