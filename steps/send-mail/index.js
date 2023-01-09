// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    components: [
        '/components/toggle-button/index',
        '/components/radio-button/index',
        '/components/codemirror/index',
    ],
    style: true,
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
    
        if (!this.step.arguments.SEND_EMAIL_BODY_TYPE) {
            this.step.arguments.SEND_EMAIL_BODY_TYPE = 'Plain Text'
        }
    
        if (!this.step.arguments.SEND_EMAIL_SMTP_PORT) {
            this.step.arguments.SEND_EMAIL_SMTP_PORT = 25;
        }
    
        if (!this.step.arguments.SEND_EMAIL_DOMAIN) {
            this.step.arguments.SEND_EMAIL_DOMAIN = 'pomelo.cloud'
        }
    
        if (!this.step.arguments.SEND_EMAIL_USE_SSL) {
            this.step.arguments.SEND_EMAIL_USE_SSL = false;
        }
    }
});
