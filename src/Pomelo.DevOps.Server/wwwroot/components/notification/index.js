// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var randomString = require('/shared/random-string.js');

Component('notification', {
    style: true,
    props: [ 'top' ],
    data() {
        return {
            timer: null,
            notifications: []
        }
    },
    mounted() {
        var self = this;
        window.notification = this;
        this.timer = setInterval(() => {
            for (let i = 0; i < self.notifications.length; ++i) {
                let notficication = self.notifications[i];
                if (notficication.time != -1) {
                    --notficication.time;
                }
            }
            self.notifications = self.notifications.filter(x => x.time == -1 || x.time > 0);
        }, 1000);
    },
    unmounted() {
        window.notification = null;
        if (this.timer) {
            clearInterval(this.timer);
        }
    },
    methods: {
        push(title, message, type, time) {
            var notification = {
                time: time || 10,
                title: title,
                type: type || 'info',
                message: message,
                id: randomString.rand(16)
            };

            this.notifications = [notification].concat(this.notifications);
            return this.notifications.filter(x => x.id == notification.id)[0];
        }
    }
});