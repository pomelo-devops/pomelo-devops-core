// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var mc = require('/assets/js/mc-datepicker/dist/mc-calendar.min');
var moment = require('/assets/js/moment/moment');
var randomString = require('/shared/random-string');

Component('date-picker', {
    style: [
        '/assets/js/mc-datepicker/dist/mc-calendar.min.css',
        '@(css)'
    ],
    props: ['modelValue', 'placeholder'],
    data() {
        return {
            id: 'mc-' + randomString.rand(),
            picker: null
        };
    },
    watch: {
        modelValue(newVal, oldVal) {
            if (newVal == oldVal) {
                return;
            }

            if (this.picker) {
                this.picker.setFullDate(new Date(this.modelValue));
            }
        }
    },
    mounted() {
        var picker = mc.create();
        var self = this;
        picker.onClear(function () {
            self.$emit('update:modelValue', null);
        });
        picker.onSelect(function (date) {
            self.$emit('update:modelValue', moment(date).format('YYYY-MM-DD'));
        });
        this.picker = picker;
    },
    unmounted() {
        if (this.picker) {
            this.picker.destroy();
        }
    },
    methods: {
        onClick() {
            this.picker.open();
        },
        onChange(e) {
            var date = new Date(e.target.value);
            if (date == 'Invalid Date') {
                return;
            }
            this.$emit('update:modelValue', moment(date).format('YYYY-MM-DD'));
        }
    }
});