Component('stage', {
    style: true,
    props: ['shape', 'arguments'],
    components: [
        '/components/agent-pool-selector/index',
        '/components/radio-button/index',
        '/components/input-number/index'
    ],
    data() {
        return {
            stages: [],
            showAgentSelector: false,
            agentPool: null
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
        },
        projectId() {
            return Pomelo.root().projectId;
        }
    },
    watch: {
        deep: true,
        settingsActive() {
            if (!this.settingsActive) {
                this.$parent.$parent.active = null;
            }
        },
        'shape.arguments.AgentPoolId': async function () {
            await this.updateAgentPool();
        }
    },
    created() {
        window.test = this;
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

        if (!this.shape.arguments.StageWorkflowId) {
            this.shape.arguments.StageWorkflowId = null;
        }

        if (!this.shape.arguments.AgentPoolId) {
            this.shape.arguments.AgentPoolId = null;
        }

        if (!this.shape.arguments.AgentCount) {
            this.shape.arguments.AgentCount = 0;
        }

        if (!this.shape.arguments.IsolationLevel) {
            this.shape.arguments.IsolationLevel = 'Parallel';
        }

        this.updateAgentPool();

        Pomelo.CQ.CreateView(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/diagram-stage`, {}, 60000).fetch(function (result) {
            self.stages = result.data;
        });
    },
    methods: {
        localization() {
            return Pomelo.root().localization;
        },
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
        },
        async updateAgentPool() {
            if (this.shape && this.shape.arguments && this.shape.arguments.AgentPoolId) {
                var pool = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentPool/${this.shape.arguments.AgentPoolId}`)).data;
                pool.agents = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/agentPool/${this.shape.arguments.AgentPoolId}/agent`)).data;
                this.agentPool = pool;
            } else {
                this.agentPool = null;
            }
        },
    }
});