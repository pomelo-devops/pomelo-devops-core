// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    style: true,
    components: [
        '/components/date-picker/index'
    ],
    data() {
        return {
            pats: [],
            form: {
                name: null,
                expireAt: null
            },
            result: null
        };
    },
    created() {
        this.getPats();
    },
    mounted() {
        this.$root.ui.active = 'system-pat';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getPats() {
            this.pats = (await Pomelo.CQ.Get(`/api/user/${window.localStorage.getItem('user')}/pat`)).data;
        },
        async deletePat(pat) {
            var notify = notification.push(pat.name, `Revoking PAT...`);
            await Pomelo.CQ.Delete(`/api/user/${window.localStorage.getItem('user')}/pat/${pat.id}`);
            notify.message = `${pat.name} has been revoked`;
            notify.time = 10;
            notify.type = 'success';

            await this.getPats();
        },
        async generateToken() {
            if (!this.form.name) {
                notification.push('Error', 'Please input token name', 'error', 10);
                return;
            }

            var notify = notification.push(this.form.name, 'Generating token');
            this.result = (await Pomelo.CQ.Post(`/api/user/${window.localStorage.getItem('user')}/pat`, this.form)).data.token;
            notify.type = 'success';
            notify.time = 10;
            notify.message = 'Token generated';

            this.form = {
                name: null,
                expireAt: null
            };

            await this.getPats();
        }
    }
});