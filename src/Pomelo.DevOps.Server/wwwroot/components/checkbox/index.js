// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('checkbox', {
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
            if (!this.modelValue) {
                return false;
            }

            return this.modelValue.split(',').map(x => x.trim()).some(x => x == this.value);
        }
    },
    methods: {
        click() {
            var items = this.modelValue.split(',').map(x => x.trim());
            if (items.some(x => x == this.value)) {
                items = items.filter(x => x != this.value);
            } else {
                items.push(this.value);
            }
            this.$emit('update:modelValue', items.filter(x => x).join());
        }
    }
});