// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/gallery',
    style: true,
    data() {
        return {
            name: null,
            token: window.localStorage.token
        }
    },
    watch: {
        name() {
            if (!this.name) {
                return;
            }

            var ext = this.name.substr(this.name.length - 4, 4);
            if (ext != '.zip' && ext != '.pdo') {
                notification.push('Upload Package', 'Package file must end with .pdo or .zip extension name', 'error', 10);
                this.name = null;
            }
        }
    },
    methods: {
        selectFile() {
            document.querySelector('#file-upload').click();
        },
        onFileSelectorChanged(e) {
            var fileName = e.target.value;
            var index = fileName.lastIndexOf('/');
            if (index == -1) {
                index = fileName.lastIndexOf('\\');
            }
            if (index == -1) {
                this.name = fileName;
                return;
            }

            this.name = fileName.substr(index + 1);
        }
    }
});