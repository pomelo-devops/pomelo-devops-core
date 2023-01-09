// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Component('user-selector', {
    style: true,
    props: ['title', 'modelValue', 'visible', 'projectId', 'outUser'],
    data() {
        return {
            selected: null,
            search: null,
            users: []
        };
    },
    created() {
        this.getUsers();
    },
    watch: {
        search() {
            this.getUsers();
        }
    },
    methods: {
        async getUsers() {
            this.users = (await Pomelo.CQ.Get(`/api/user`, { name: this.search })).data;
        },
        select(user) {
            this.selected = user;
        },
        close() {
            this.$emit('update:visible', false);
            this.visible = false;
        },
        callback() {
            console.log(this.selected);
            this.$emit('update:modelValue', this.selected.id);
            this.$emit('update:outUser', this.selected);
            this.$emit('update:visible', false);
        }
    }
});