Component('finish', {
    style: true,
    props: ['shape', 'arguments'],
    data() {
        return {
        };
    },
    computed: {
        active() {
            return this.$parent.active == this.shape;
        },
        settingsActive() {
            return this.active
                && !this.$parent.dragStart
                && this.$parent.$parent.active == this.shape;
        }
    },
    watch: {
        settingsActive() {
            if (!this.settingsActive) {
                this.$parent.$parent.active = null;
            }
        }
    },
    created() {
        if (this.shape.anchors.length == 0) {
            this.shape.createAnchor(.5, 0);
            this.shape.createAnchor(1, .5);
            this.shape.createAnchor(.5, 1);
            this.shape.createAnchor(0, .5);
        }
    },
    methods: {
        onClicked() {
            if (!this.$parent.edit) {
                return;
            }

            var target = event.target;
            while (target != null) {
                for (var i = 0; i < target.classList.length; ++i) {
                    if (target.classList[0].indexOf('settings') >= 0) {
                        return;
                    }
                }

                target = target.parentElement;
            }

            this.$parent.active = this.shape;
        },
        link(anchor) {
            if (this.$parent.isDeparture()) {
                notification.push('Pipeline Diagram', 'This node cannot connects to any other nodes', 'error');
                this.$parent.cancelOperations();
            } else {
                if (this.shape.getIncomingConnectedPolylines().length) {
                    notification.push('Pipeline Diagram', 'This node cannot be connected by any other nodes', 'error');
                    this.$parent.cancelOperations();
                    return;
                }
                this.$parent.link(anchor);
            }
        },
        blur() {
            this.$parent.$parent.active = null;
        }
    }
});