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
    
        if (!this.step.arguments.PUBLISH_TRX_SUITE) {
            this.step.arguments.PUBLISH_TRX_SUITE = 'Test Suite'
        }
    
        if (!this.step.arguments.PUBLISH_TRX_PATH) {
            this.step.arguments.PUBLISH_TRX_PATH = ''
        }
    
        if (!this.step.arguments.PUBLISH_FOLDER_PATH) {
            this.step.arguments.PUBLISH_FOLDER_PATH = ''
        }
    }
});
