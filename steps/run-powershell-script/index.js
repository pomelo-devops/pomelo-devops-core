// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var codeMirror = require('/assets/js/codemirror/lib/codemirror');
var powershell = require('/assets/js/codemirror/mode/powershell/powershell');
var sleep = require('/shared/sleep').sleep;

Page({
    style: [
        '/assets/js/codemirror/lib/codemirror.css',
        '/assets/js/codemirror/theme/monokai.css',
        'index.css'
    ],
    data() {
        return {
            step: null,
            package: null,
            editor: null
        }
    },
    watch: {
        deep: true,
        'step.method': async function() {
            await this.renderCodeMirror();
        }
    },
    created() {
        if (!this.step.arguments) {
            this.step.arguments = {};
        }
    
        if (!this.step.arguments.POWERSHELL_SCRIPT_PATH) {
            this.step.arguments.POWERSHELL_SCRIPT_PATH = null;
        }
    
        if (!this.step.arguments.POWERSHELL_SCRIPT_CONTENT) {
            this.step.arguments.POWERSHELL_SCRIPT_CONTENT = null;
        }
    },
    async mounted() {
        await this.renderCodeMirror();
    },
    unmounted() {
        this.removeCodeMirror();
    },
    methods: {
        async renderCodeMirror() {
            if (this.step.method == 'Execute Inline Powershell Script') {
                while(!document.querySelectorAll('.cm-outer > textarea').length) {
                    await sleep(10);
                }
                await sleep(200);
                this.editor = codeMirror.fromTextArea(
                    document.querySelector('#cm-powershell-content'), 
                    { 
                        lineNumbers: true, 
                        mode: 'powershell',
                        theme: 'monokai'
                    });
                var self = this;
                this.editor.on('change', function(editor, _) {
                    self.step.arguments.POWERSHELL_SCRIPT_CONTENT = editor.getValue();
                });
            } else {
                this.removeCodeMirror();
            }
        },
        removeCodeMirror() {
            if (this.editor) {
                this.editor.toTextArea();
                this.editor = null;
            }
        }
    }
});