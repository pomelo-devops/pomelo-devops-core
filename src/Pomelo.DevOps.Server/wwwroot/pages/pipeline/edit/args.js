// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var randomString = require('/shared/random-string');
var sleep = require('/shared/sleep').sleep;
var yield = require('/shared/sleep').yield;
var dragula = require('/assets/js/dragula/dist/dragula');

Page({
    style: [
        '@(css)',
        '/assets/js/dragula/dist/dragula.css'
    ],
    data() {
        return {
            pipeline: null
        };
    },
    async mounted() {
        this.$root.ui.active = 'pipeline-args';
        while (!document.querySelectorAll('#pipeline-argument-rows').length) {
            await sleep(50);
        }
        this.bindDragula();
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        bindDragula() {
            var self = this;
            dragula([document.getElementById('pipeline-argument-rows')], {})
                .on('drag', function (el) {
                    el.className += ' el-drag-ex-1';
                }).on('drop', function (el) {
                    el.className = el.className.replace('el-drag-ex-1', '');
                    self.reorderArguments();
                }).on('cancel', function (el) {
                    el.className = el.className.replace('el-drag-ex-1', '');
                });
        },
        addArgument() {
            if (!this.pipeline.arguments) {
                this.pipeline.arguments = [];
            }

            this.pipeline.arguments.push({
                argument: 'NEW_' + randomString.rand(6).toUpperCase(),
                description: null,
                default: null,
                priority: 999,
                validateRegex: null,
                options: null,
                type: 'Text',
                numberStep: null,
                numberMin: null,
                numberMax: null
            });
        },
        removeArgument(arg) {
            var index = this.pipeline.arguments.indexOf(arg);
            if (index == -1) {
                return;
            }

            if (confirm("Are you sure you want to remove this argument?")) {
                this.pipeline.arguments.splice(index, 1);
            }
        },
        async reorderArguments() {
            var doms = document.querySelectorAll('#pipeline-argument-rows .pipeline-argument-row');
            console.log(doms.length);
            var ret = [];
            for (var i = 0; i < doms.length; ++i) {
                var arg = this.pipeline.arguments.filter(x => x.argument == doms[i].getAttribute('data-arg'))[0];
                arg.priority = i;
                ret.push(arg);
            }
            this.pipeline.arguments = [];
            await yield();
            this.pipeline.arguments = ret;
            console.log(ret);
        }
    }
});