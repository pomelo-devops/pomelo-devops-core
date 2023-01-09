// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    mounted() {
        this.$root.ui.active = 'system-job-extensions';
    },
    unmounted() {
        this.$root.ui.active = null;
    }
});