// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var loginChecker = require('/shared/login-checker.js');

Page({
    style: true,
    components: [
        '/components/notification/index'
    ],
    data() {
        return {
            form: {
                name: null,
                id: null,
                iconBase64String: null
            },
            localization: null
        };
    },
    async created() {
        if (!await loginChecker.check()) {
            Pomelo.Redirect('/login');
            return;
        }

        this.localization = require('/shared/localization').create()
        this.localization.addLocale('/shared/localization/en-US', ['en', 'en-US', 'en-GB', 'en-AU', 'en-CA'], true);
        this.localization.addLocale('/shared/localization/zh-CN', ['zh', 'zh-CN']);
        this.localization.setLocale();
    },
    methods: {
        selectIcon() {
            document.querySelector('#file-org-icon').click();
        },
        onFileSelected() {
            var files = document.querySelector('#file-org-icon').files;
            if (!files.length) {
                return;
            }

            var file = files[0];
            if (!/image\/\w+/.test(file.type)) {
                alert("Invalid image");
                return;
            }

            var reader = new FileReader();
            var self = this;
            reader.onload = function () {
                self.form.iconBase64String = reader.result.substring(reader.result.indexOf('base64,') + 7);
            }
            reader.readAsDataURL(file);
        },
        async create() {
            if (!this.form.name) {
                notification.push('Error', 'Please input project name', 'error');
                return;
            }

            if (!this.form.id) {
                notification.push('Error', 'Please input project ID', 'error');
                return;
            }

            if (!/^[a-zA-Z0-9-_]{1,}$/.test(this.form.id)) {
                notification.push('Error', 'Project ID is invalid', 'error');
                return;
            }

            var result = await Pomelo.CQ.Put(`/api/project/${this.form.id}`, this.form);
            if (result.code == 200) {
                notification.push('Succeeded', result.message, 'success');
                Pomelo.Redirect(`/project/${this.form.id}`);
            } else {
                notification.push('Error', result.message, 'error');
            }
        }
    }
});