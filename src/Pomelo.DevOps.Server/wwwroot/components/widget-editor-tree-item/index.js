// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var $ = require('/assets/js/jquery/dist/jquery');
var yield = require('/shared/sleep').yield;
var rand = require('/shared/random-string').rand;

function getBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
    });
}

Component('widget-editor-tree-item', {
    style: true,
    props: ['modelValue', 'level'],
    data() {
        return {
            id: rand(),
            toggled: false,
            editing: false,
            fileSelectorSwitch: true
        };
    },
    methods: {
        getParentContainer() {
            var parent = this.$parent.modelValue || this.$parent.widget;
            var children = parent.children || parent.items;
            return children;
        },
        setActive(item, e) {
            if (e) {
                if ($(e.target).hasClass('widget-editor-tree-folder-actions')
                    || $(e.target).parents('.widget-editor-tree-folder-actions').length) {
                    return;
                }
            }

            this.$root.active = item;
        },
        toggle(e) {
            if ($(e.target).hasClass('widget-editor-tree-folder-actions')
                || $(e.target).parents('.widget-editor-tree-folder-actions').length) {
                return;
            }

            this.toggled = !!!this.toggled;
        },
        deleteItem(item) {
            if (!confirm("Are you sure you want to remove this item?")) {
                return;
            }

            this.getParentContainer().splice(this.getParentContainer().indexOf(item), 1);
        },
        selectFile() {
            document.querySelector('#file-' + this.id).click();
        },
        getAvailableName(type) {
            var name = 'New';
            if (type == 'Directory') {
                name += 'Folder';
            } else {
                name += 'File';
            }

            if (!this.modelValue.children.some(x => x.name == name)) {
                return name;
            }

            var i = 1;
            while (true) {
                var _name = name + i;
                if (!this.modelValue.children.some(x => x.name == _name)) {
                    return _name;
                }
                ++i;
            }
        },
        async onFileSelected() {
            if (this.modelValue.type != 'Directory') {
                return;
            }

            if (!document.querySelector('#file-' + this.id).files.length) {
                return;
            }

            var file = document.querySelector('#file-' + this.id).files[0];
            var name = file.name;

            if (this.modelValue.children && this.modelValue.children.some(x => x.name == name && x.type == 'File')) {
                notification.push('File Upload', `${name} is already existed`, 'error');
                return;
            }

            var base64 = await getBase64(file);
            base64 = base64.substr(base64.indexOf(',') + 1);
            if (!this.modelValue.children) {
                this.modelValue.children = [];
            }

            var item = {
                name: name,
                type: 'File',
                content: base64
            };
            this.modelValue.children.push(item);

            notification.push('File Upload', `${name} has been uploaded`, 'success');
            this.toggled = true;

            this.$root.sortWidgetFiles(this.modelValue);
            item = this.modelValue.children.filter(x => x.name == name)[0];
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
            this.modelValue.children.push(item);
            this.toggled = true;
            this.$root.sortWidgetFiles(this.modelValue);
            item = this.modelValue.children.filter(x => x.name == name)[0];
            this.setActive(item);
        },
        newFolder() {
            var name = this.getAvailableName('Directory');
            var item = {
                name: name,
                type: 'Directory',
                children: []
            };
            this.modelValue.children.push(item);
            this.toggled = true;
            this.$root.sortWidgetFiles(this.modelValue);
            item = this.modelValue.children.filter(x => x.name == name)[0];
            this.setActive(item);
        },
        async edit() {
            this.editing = true;
            await yield();
            $(`#edit-${this.id}`).focus();
        },
        onBlur() {
            this.editing = false;
        }
    }
});