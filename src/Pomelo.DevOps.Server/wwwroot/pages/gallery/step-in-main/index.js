// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    data() {
        return {
            step: null,
            packages: null,
            active: null
        };
    },
    async created() {
        await this.getPackageVersions();
    },
    mounted() {
        console.log('mount');
        this.$container('#step-main');
        this.$parent.active = 'step-' + this.step.temp;
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

            var package = this.packages.filter(x => x.version == this.step.version);
            if (!package.length) {
                return null;
            }

            return package[0];
        }
    },
    watch: {
        deep: true,
        'step.version': async function () {
            await this.open();
        }
    },
    methods: {
        async getPackageVersions() {
            this.packages = (await Pomelo.CQ.Get(`/api/gallery/${this.step.stepId}`)).data;
        },
        async open() {
            await this.$containers[0].open(`/api/gallery/${this.step.stepId}/version/${this.step.version}/index`, {
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
                    stepVersion: this.step.stepVersion
                });
            } else {
                await this.open();
            }
        }
    }
});