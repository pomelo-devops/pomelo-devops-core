// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('toggle-button', {
    style: true,
    props: ['modelValue'],
    created() {
        this.$emit('update:modelValue', !!this.modelValue);
    },
    methods: {
        toggle() {
            this.$emit('update:modelValue', !!!this.modelValue);
        }
    }
});