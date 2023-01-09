// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    data() {
        return {
            step: null,
            package: null
        };
    },
    created() {
        if (!this.step.arguments) {
            this.step.arguments = {};
        }
    }
});