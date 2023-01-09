// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('input-number', {
    style: true,
    props: ['modelValue', 'max', 'min', 'step'],
    created() {
        if (!this.modelValue) {
            this.$emit('update:modelValue', 0);
        }
    },
    methods: {
        defend(e) {
            var val = e.target.value;

            if (this.max && parseFloat(val) > parseFloat(this.max)) {
                this.$emit('update:modelValue', parseFloat(this.max));
                return;
            }

            if (parseFloat(val) < parseFloat(this.min || 0)) {
                this.$emit('update:modelValue', parseFloat(this.min || 0));
                return;
            }

            this.$emit('update:modelValue', parseFloat(val));
        },
        up() {
            var step = parseFloat(this.step || 1);
            if (this.max) {
                if (this.modelValue + step > parseFloat(this.max)) {
                    return;
                }
            }
            this.$emit('update:modelValue', parseFloat(this.modelValue) + step);
        },
        down() {
            var step = parseFloat(this.step || 1);
            if (parseFloat(this.modelValue) - step < parseFloat(this.min || 0)) {
                return;
            }
            this.$emit('update:modelValue', parseFloat(this.modelValue) - step);
        }
    }
});