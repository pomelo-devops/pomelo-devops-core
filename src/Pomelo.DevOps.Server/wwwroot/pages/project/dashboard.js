// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var rand = require('/shared/random-string').rand;
var yield = require('/shared/sleep').yield;

Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            projectId: null,
            project: null,
            matrix: null,
            widgets: {},
            editing: false,
            loading: true,
            sidebar: false,
            addWidget: false,
            editContainer: null,
            editingWidget: null,
            widgetResults: [],
            search: null,
            selectedCell: null
        };
    },
    created() {
        window.localStorage.setItem('default-project', this.projectId);
        var matrix = [];
        for (var i = 0; i < 20; ++i) {
            var tmp = [];
            for (var j = 0; j < 30; ++j) {
                tmp.push({ y: i, x: j });
            }
            matrix.push(tmp);
        }
        this.matrix = matrix;
        this.getWidgets();
    },
    watch: {
        search() {
            this.getWidgets();
        }
    },
    async mounted() {
        this.$root.ui.active = 'project-dashboard';
        this.editContainer = this.$container('#widget-edit-container');
        await this.getProject();
        var widgetIds = this.project.dashboard.items.map(x => x.widgetId);
        if (widgetIds.length) {
            var widgets = (await Pomelo.CQ.Post(`/api/misc/batch-query-widgets`, { widgetIds: widgetIds })).data;
            for (var i = 0; i < widgets.length; ++i) {
                this.widgets[widgets[i].id] = widgets[i];
            }
        }

        for (var i = 0; i < this.project.dashboard.items.length; ++i) {
            var id = '#' + this.project.dashboard.items[i].id;
            while (!document.querySelector(id)) {
                await yield();
            }
            var container = this.$container(id);
            container.open(`/api/widget/${this.project.dashboard.items[i].widgetId}/resource/${this.widgets[this.project.dashboard.items[i].widgetId].widgetEntryView}`, this.project.dashboard.items[i].arguments);
        }
        this.loading = false;
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async getProject() {
            var project = (await Pomelo.CQ.Get(`/api/project/${this.projectId}`)).data;
            for (var i = 0; i < project.dashboard.items.length; ++i) {
                project.dashboard.items[i].id = 'widget-' + rand();
            }
            this.project = project;
        },
        deleteWidget(widget) {
            if (!confirm('Are you sure you want to remove this widget?')) {
                return;
            }

            var index = this.project.dashboard.items.indexOf(widget);
            if (index == -1) {
                return;
            }

            this.project.dashboard.items.splice(index, 1);
        },
        popEditDialog(widget) {
            this.editContainer.open(
                `/api/widget/${widget.widgetId}/resource/${this.widgets[widget.widgetId].configEntryView}`,
                {
                    widget: widget,
                    projectId: this.projectId
                });
            this.addWidget = false;
            this.sidebar = true;
            this.editingWidget = widget;
        },
        hideSidebar() {
            this.addWidget = false;
            this.sidebar = false;
            this.editContainer.close();
            this.editingWidget = null;
            this.selectedCell = null;
        },
        exitEditingMode() {
            this.hideSidebar();
            this.editing = false;
        },
        async saveDashboard() {
            var notify = notification.push('Saving Dashboard', `Saving ${this.project.name}...`);
            try {
                await Pomelo.CQ.Post(`/api/project/${this.projectId}`, this.project);
                notify.type = 'success';
                notify.message = `${this.project.name} has been saved.`;
                notify.time = 10;
                Pomelo.Redirect(`/project/${this.projectId}`);
            } catch (ex) {
                notify.type = 'error';
                notify.message = ex.message;
                notify.time = 10;
            }
        },
        popCreateWidgetSidebar(cell) {
            this.selectedCell = cell;
            this.selectedWidgetType = null;
            this.editingWidget = false;
            this.addWidget = true;
            this.search = null;
            this.sidebar = true;
        },
        async getWidgets() {
            this.widgetResults = (await Pomelo.CQ.Get(`/api/widget`, { name: this.search })).data;
        },
        async addWidgetIntoDashboard(widget) {
            this.widgets[widget.id] = widget;
            _widget = {
                arguments: {},
                height: 1,
                width: 1,
                x: this.selectedCell.x,
                y: this.selectedCell.y,
                widgetId: widget.id,
                id: 'widget-' + rand()
            };
            this.project.dashboard.items.push(_widget);
            this.hideSidebar();
            var id = '#' + _widget.id;
            while (!document.querySelector(id)) {
                await yield();
            }
            var container = this.$container(id);
            container.open(`/api/widget/${_widget.widgetId}/resource/${this.widgets[_widget.widgetId].widgetEntryView}`, _widget.arguments);

            this.popEditDialog(this.project.dashboard.items.filter(x => x.id == _widget.id)[0]);
        }
    }
});