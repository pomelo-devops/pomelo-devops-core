// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var dragula = require('/assets/js/dragula/dist/dragula.js');
var $ = require('/assets/js/jquery/dist/jquery.min.js');
var randomString = require('/shared/random-string.js');
var yield = require('/shared/sleep.js').yield;

Page({
    style: [
        '@(css)',
        '/assets/js/dragula/dist/dragula.css'
    ],
    data() {
        return {
            pipeline: null,
            active: null,
            toggled: {}
        };
    },
    mounted() {
        this.$root.ui.active = 'pipeline-playbook';
        this.$container('#pipeline-edit-playbook-container');
        this.rebindDrag();
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        editStage(stage) {
            this.$containers[0].open('/pages/pipeline/edit/stage', { stage: stage });
        },
        createStage() {
            var stage = {
                name: this.$root.localization.sr('PIPELINE_EDIT_PLAYBOOK_ADD_STAGE_DEFAULT'),
                isolationLevel: 'Sequential',
                agentCount: 1,
                timeout: -1,
                condition: 'RequirePreviousTaskSuccess',
                agentPoolId: null,
                temp: randomString.rand(),
                steps: [],
                order: this.pipeline.stages.length ? (this.pipeline.stages[this.pipeline.stages.length - 1].order + 1) : 1
            };
            this.pipeline.stages.push(stage);
            this.editStage(stage);
            this.reorderStages();
        },
        createStep(stage) {
            this.active = 'add-' + stage.temp;
            this.$containers[0].open('/pages/gallery/step-in-main/list', { stage: stage });
        },
        rebindDrag() {
            var self = this;
            dragula([document.getElementById('stages')], { invalid: (el, handle) => el.classList.contains('pipeline-playbook-stage-steps') })
                .on('drag', function (el) {
                    self.collapseAll();
                    el.className += ' el-drag-ex-1';
                }).on('drop', function (el) {
                    el.className = el.className.replace('el-drag-ex-1', '');
                    self.reorderStages();
                }).on('cancel', function (el) {
                    el.className = el.className.replace('el-drag-ex-1', '');
                });
        },
        async rebindDragForSteps(stage, firstBind) {
            var self = this;
            if (!firstBind) {
                var tmp = stage.steps;
                stage.steps = null;
                this.$forceUpdate();
                while ($('.pipeline-playbook-stage[data-id="' + stage.temp + '"] .pipeline-playbook-stage-steps').length) {
                    await yield();
                }
                stage.steps = tmp;
                while (!$('.pipeline-playbook-stage[data-id="' + stage.temp + '"] .pipeline-playbook-stage-steps').length) {
                    await yield();
                }
            }
            var doms = document.getElementsByClassName('pipeline-playbook-stage-steps');
            for (var i = 0; i < doms.length; ++i) {
                if (!$(doms[i]).parents('.pipeline-playbook-stage[data-id="' + stage.temp + '"]').length) {
                    continue;
                }
                dragula([doms[i]])
                    .on('drag', function (el) {
                        el.className += ' el-drag-ex-1';
                    }).on('drop', function (el) {
                        el.className = el.className.replace('el-drag-ex-1', '');
                        var stageTemp = $(el).parents('.pipeline-playbook-stage').attr('data-id');
                        var stage = self.pipeline.stages.filter(x => x.temp == stageTemp)[0];
                        self.reorderSteps(stage);
                    }).on('cancel', function (el) {
                        el.className = el.className.replace('el-drag-ex-1', '');
                    });
            }
        },
        async reorderStages() {
            while ($('.gu-transit').length) {
                await yield();
            }
            var doms = $('.pipeline-playbook-stage');
            var dic = {};
            var cur = 0;
            for (var i = 0; i < doms.length; ++i) {
                var dom = $(doms[i]);
                var id = dom.attr('data-id');
                var stage = this.pipeline.stages.filter(x => x.temp == id)[0];
                if (!dic[stage.order.toString()]) {
                    dic[stage.order.toString()] = ++cur;
                }
                stage.order = dic[stage.order.toString()];
            }

            this.pipeline.stages.sort((a, b) => a.order - b.order);
            var tmp = this.pipeline.stages;
            this.pipeline.stages = [];
            this.$forceUpdate();
            while ($('.pipeline-playbook-stage').length) {
                await yield();
            }
            this.pipeline.stages = tmp;
            while (!$('.pipeline-playbook-stage').length) {
                await yield();
            }
            for (var i = 0; i < this.pipeline.stages.length; ++i) {
                this.rebindDragForSteps(this.pipeline.stages[i], true);
            }
        },
        async reorderSteps(stage) {
            while ($('.gu-transit').length) {
                await yield();
            }
            var doms = $('.pipeline-playbook-stage[data-id="' + stage.temp + '"] .pipeline-playbook-stage-step');
            for (var i = 0; i < doms.length; ++i) {
                var dom = $(doms[i]);
                var id = dom.attr('data-id');
                var step = stage.steps.filter(x => x.temp == id)[0];
                step.order = i + 1;
            }

            stage.steps.sort((a, b) => a.order - b.order);
            var tmp = stage.steps;
            stage.steps = [];
            this.$forceUpdate();
            while ($('.pipeline-playbook-stage[data-id="' + stage.temp + '"] .pipeline-playbook-stage-step').length) {
                await yield();
            }
            stage.steps = tmp;
        },
        editStep(step) {
            this.$containers[0].open('/pages/gallery/step-in-main/index', { step: step });
        },
        collapseAll() {
            for (var i = 0; i < this.pipeline.stages.length; ++i) {
                this.pipeline.stages[i].toggled = false;
            }
        }
    }
});