Component('step', {
    style: [
        'index.css',
        '/pages/gallery/step-in-main/list.css'
    ],
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

        var self = this;
        if (!this.shape.arguments) {
            this.shape.arguments = {};
        }
    },
    methods: {
        onClicked(event) {
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
                if (this.shape.getOutgoingConnectedPolylines().length >= 2) {
                    notification.push('Pipeline Diagram', 'You cannot add more connections to other nodes.', 'error');
                    this.$parent.cancelOperations();
                    return;
                }

                var polylines = this.shape.getOutgoingConnectedPolylines();

                if (!polylines.some(x => x.getType() == 'default')) {
                    this.$parent.link(anchor, '#1ca725', 'default');
                } else {
                    this.$parent.link(anchor, '#d81e06', 'failed');
                }

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