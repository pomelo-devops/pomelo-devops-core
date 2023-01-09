// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var codeMirror = require('/assets/js/codemirror/lib/codemirror');
var randomString = require('/shared/random-string');
var sleep = require('/shared/sleep').sleep;
var yield = require('/shared/sleep').yield;

if (!window.cmcache) {
    window.cmcache = {};
}

function getEditor(id) {
    return window.cmcache[id];
}

function setEditor(id, editor) {
    window.cmcache[id] = editor;
}

function removeEditor(id) {
    delete window.cmcache[id];
}

Component('codemirror', {
    style: [
        '/assets/js/codemirror/lib/codemirror.css',
        '/assets/js/codemirror/theme/monokai.css',
        '@(css)'
    ],
    props: [ 'modelValue', 'mode', 'disableLineNumber' ],
    data() {
        return {
            id: randomString.rand()
        };
    },
    watch: {
        modelValue() {
            var editor = getEditor(this.id);
            if (!editor) {
                return;
            }

            if (editor && editor.getValue() != this.modelValue) {
                editor.setValue(this.modelValue);
            }
        },
        mode() {
            require(`/assets/js/codemirror/mode/${this.mode}/${this.mode}`);
            var editor = getEditor(this.id);
            editor.setOption('mode', this.mode);
        }
    },
    created() {
        require(`/assets/js/codemirror/mode/${this.mode}/${this.mode}`);
    },
    async mounted() {
        await this.createEditor();
    },
    unmounted() {
        this.removeEditor();
    },
    methods: {
        async createEditor() {
            while (!document.querySelector('#codemirror-textarea-' + this.id))
            {
                await yield();
            }

            var editor = codeMirror.fromTextArea(
                document.querySelector('#codemirror-textarea-' + this.id),
                {
                    lineNumbers: this.disableLineNumber ? false : true,
                    mode: this.mode,
                    theme: 'monokai'
                });
            if (this.modelValue) {
                editor.setValue(this.modelValue || '');
            }
            setEditor(this.id, editor);
            await yield();
            editor.refresh();
            var self = this;
            editor.on('change', function (_, _) {
                self.$emit('update:modelValue', editor.getValue());
            });
        },
        removeEditor() {
            var editor = getEditor(this.id);
            if (editor) {
                editor.toTextArea();
            }
            removeEditor(this.id);
        }
    }
});