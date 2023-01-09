// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var mimetypes = require('/assets/js/mimetypes/src/mimetypes');

function getBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
    });
}

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/widget-editor-tree-item/index',
        '/components/codemirror/index'
    ],
    data() {
        return {
            widgetId: null,
            widget: null,
            active: null,
            text: '',
            mode: null,
            settings: true
        };
    },
    watch: {
        deep: true,
        'active.name': function () {
            this.mode = this.getMode();
        },
        active() {
            if (!this.active) {
                return;
            }

            this.settings = false;
            this.text = atob(this.active.content);
            this.mode = this.getMode();
        },
        text() {
            if (!this.active) {
                return;
            }

            this.active.content = btoa(this.text);
        }
    },
    computed: {
        mimeType() {
            return mimetypes.detectMimeType(this.getExtension());
        }
    },
    created() {
        this.getWidget();
    },
    mounted() {
        this.$root.ui.active = 'system-widgets';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        getExtension() {
            if (!this.active) {
                return null;
            }

            var splited = this.active.name.split('.');
            if (splited.length < 2) {
                return null;
            }

            return splited[splited.length - 1].toLowerCase();
        },
        async getWidget() {
            var widget = (await Pomelo.CQ.Get(`/api/widget/${this.widgetId}`)).data;
            this.sortWidgetFiles(widget, 'items');
            this.widget = widget;
        },
        sortWidgetFiles(item, field = 'children') {
            if (!item[field] || !item[field].length) {
                return;
            }

            item[field].sort(function (a, b) {
                var ret = a.type.localeCompare(b.type);
                if (ret != 0) {
                    return ret;
                }
                return a.name.localeCompare(b.name);
            });

            var self = this;
            item[field].forEach(function (inner) {
                self.sortWidgetFiles(inner);
            });
        },
        async save() {
            var notify = notification.push('Edit Widget', 'Saving...');
            try {
                var result = await Pomelo.CQ.Patch(`/api/widget/${this.widgetId}`, this.widget);
                notify.message = `${this.widget.name} has been saved`;
                notify.time = 10;
                notify.type = 'success';
            } catch (ex) {
                notify.message = ex.message;
                notify.time = 10;
                notify.type = 'error';
            }
        },
        getMode() {
            switch (this.getExtension()) {
                case 'js':
                case 'ts':
                case 'json':
                    return 'javascript';
                case 'css':
                case 'scss':
                case 'less':
                    return 'css';
                case 'sass':
                    return 'sass';
                case 'html':
                case 'htm':
                    return 'htmlmixed';
                case 'md':
                    return 'markdown';
                case 'xml':
                    return 'xml';
                default:
                    return null;
            }
        },
        refresh() {
            window.location.reload();
        },
        selectFile() {
            document.querySelector('#file-' + this.widget.id).click();
        },
        getAvailableName(type) {
            var name = 'New';
            if (type == 'Directory') {
                name += 'Folder';
            } else {
                name += 'File';
            }

            if (!this.widget.items.some(x => x.name == name)) {
                return name;
            }

            var i = 1;
            while (true) {
                var _name = name + i;
                if (!this.widget.items.some(x => x.name == _name)) {
                    return _name;
                }
                ++i;
            }
        },
        async onFileSelected() {
            if (!document.querySelector('#file-' + this.widget.id).files.length) {
                return;
            }

            var file = document.querySelector('#file-' + this.widget.id).files[0];
            var name = file.name;

            if (this.widget.items && this.widget.items.some(x => x.name == name && x.type == 'File')) {
                notification.push('File Upload', `${name} is already existed`, 'error');
                return;
            }

            var base64 = await getBase64(file);
            base64 = base64.substr(base64.indexOf(',') + 1);
            if (!this.widget.items) {
                this.widget.items = [];
            }

            var item = {
                name: name,
                type: 'File',
                content: base64
            };
            this.widget.items.push(item);

            notification.push('File Upload', `${name} has been uploaded`, 'success');
            this.toggled = true;

            this.sortWidgetFiles(this.widget, 'items');
            item = this.widget.items.filter(x => x.name == name)[0];
            this.setActive(item);

            $('#file-' + this.id).val(null);
            this.fileSelectorSwitch = false;
            await yield();
            this.fileSelectorSwitch = true;
        },
        newFile() {
            var name = this.getAvailableName('File');
            var item = {
                name: name,
                type: 'File',
                content: ''
            };
            this.widget.items.push(item);
            this.toggled = true;
            this.sortWidgetFiles(this.widget, 'items');
            item = this.widget.items.filter(x => x.name == name)[0];
            this.active = item;
        },
        newFolder() {
            var name = this.getAvailableName('Directory');
            var item = {
                name: name,
                type: 'Directory',
                children: []
            };
            this.widget.items.push(item);
            this.toggled = true;
            this.sortWidgetFiles(this.widget, 'items');
            item = this.widget.items.filter(x => x.name == name)[0];
            this.active = item;
        },
    }
});