// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('radio-button', {
    style: true,
    props: ['modelValue', 'value', 'title', 'hint'],
    data() {
        return {
        };
    },
    created() {
    },
    computed: {
        active() {
            return this.modelValue == this.value;
        }
    },
    methods: {
        click() {
            this.$emit('update:modelValue', this.value);
        }
    }
});