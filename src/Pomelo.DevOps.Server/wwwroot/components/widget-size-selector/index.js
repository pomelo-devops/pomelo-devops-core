// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('widget-size-selector', {
    props: ['width', 'height', 'sizes'],
    data() {
        selected: null
    },
    created() {
        var size = this.sizes.filter(x => x.width == this.width && x.height == this.height)[0];
        if (!size) {
            return;
        }

        this.selected = this.sizes.indexOf(size);
    },
    methods: {
        onSelected() {
            if (this.selected == null
                || this.selected == undefined) {
                return;
            }
            var size = this.sizes[this.selected];
            console.log(size);
            this.$emit('update:width', size.width);
            this.$emit('update:height', size.height);
        }
    }
});