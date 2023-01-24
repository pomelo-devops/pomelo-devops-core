// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    data() {
        return {
            step: null,
            shape: null,
            packages: null,
            active: null,
            fromDiagram: false
        };
    },
    async mounted() {
        this.$container('#step-main');
        if (this.step) {
            this.$parent.active = 'step-' + this.step.temp;
        }
        await this.getPackageVersions();
        this.open();
    },
    unmounted() {
        this.$parent.active = null;
    },
    computed: {
        currentPackage() {
            if (!this.packages || !this.packages.length) {
                return null;
            }

            var package = this.packages.filter(x => x.version == this.stepVersion);
            if (!package.length) {
                return null;
            }

            return package[0];
        },
        stepId() {
            return !this.fromDiagram
                ? this.step.stepId
                : this.shape.arguments.StepId;
        },
        stepVersion() {
            return !this.fromDiagram
                ? this.step.version
                : this.shape.arguments.StepVersion;
        }
    },
    watch: {
        deep: true,
        'step.version': async function () {
            await this.open();
        },
        'step.method': function () {
            this.$containers[0].active.$forceUpdate();
        }
    },
    methods: {
        async getPackageVersions() {
            this.packages = (await Pomelo.CQ.Get(`/api/gallery/${this.stepId}`)).data;
        },
        async open() {
            if (this.fromDiagram) {
                this.step = {
                    stepId: this.shape.arguments.StepId || this.stepId,
                    version: this.shape.arguments.StepVersion || this.stepVersion,
                    method: this.shape.arguments.method || this.currentPackage.methods.split(',')[0],
                    name: this.shape.name || this.currentPackage.name,
                    arguments: this.shape.arguments.arguments || {},
                    timeout: this.shape.arguments.timeout || -1,
                    retry: this.shape.arguments.retry || 0,
                    errorHandlingMode: this.shape.arguments.errorHandlingMode || 'Normal'
                };
                console.log(this.shape);
                console.log(this.step);
            }
            await this.$containers[0].open(`/api/gallery/${this.stepId}/version/${this.stepVersion}/index`, {
                step: this.step,
                stepId: this.step.stepId,
                stepVersion: this.step.stepVersion
            });
        },
        async toGeneralSettings() {
            if (this.active != 'general') {
                await this.$containers[0].open('/pages/gallery/step-in-main/general', {
                    step: this.step,
                    stepId: this.step.stepId,
                    stepVersion: this.step.stepVersion,
                    fromDiagram: this.fromDiagram
                });
            } else {
                await this.open();
            }
        },
        close() { // Only for diagram mode
            this.$parent.$containers[0].close();
        },
        save() { // Only for diagram mode
            this.shape.name = this.step.name;
            this.shape.StepVersion = this.step.version;
            this.shape.arguments.arguments = this.step.arguments;
            this.shape.arguments.timeout = this.step.timeout;
            this.shape.arguments.retry = this.step.retry;
            this.shape.arguments.method = this.step.method;
            this.shape.arguments.errorHandlingMode = this.step.errorHandlingMode;
            this.close();
        }
    }
});