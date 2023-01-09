// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            fileName: null,
            json: null
        };
    },
    mounted() {
        this.$root.ui.active = 'system-widgets';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        selectFile() {
            document.querySelector('#widget-file-uploader').click();
        },
        async onFileSelected() {
            if (!document.querySelector('#widget-file-uploader').files.length) {
                return;
            }

            var fileName = document.querySelector('#widget-file-uploader').files[0].name;
            var extension = fileName.substr(Math.max(0, fileName.length - 5));
            if (extension.toLowerCase() != '.json') {
                notification.push('Format Error', 'Please upload JSON file', 'error', 10);
                return;
            }
            var json = await document.querySelector('#widget-file-uploader').files[0].text();
            try {
                this.json = JSON.parse(json);
            } catch (ex) {
                notification.push('Format Error', 'The file content is invalid', 'error', 10);
            }

            this.fileName = fileName;
        },
        async importWidget() {
            var notify = notification.push('Import Widget', 'Importing widget...');
            try {
                await Pomelo.CQ.Post(`/api/widget/${this.json.id}`, this.json);
                notify.type = 'success';
                notify.time = 10;
                notify.message = 'Widget imported';
                Pomelo.Redirect(`/system/widgets/edit?widgetId=${encodeURIComponent(this.json.id)}`);
            } catch (ex) {
                notify.type = 'error';
                notify.time = 10;
                notify.message = ex.message;
            }
        }
    }
});