// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var loginChecker = require('login-checker');
var moment = require('/assets/js/moment/moment.js');
var signalR = require('/assets/js/signalr/dist/browser/signalr');

Layout({
    components: [
        '/components/notification/index'
    ],
    data() {
        return {
            projectId: null,
            data: {
                projects: [],
                defaultProject: null,
                currentPipeline: null,
                currentJob: null
            },
            ui: {
                projects: false,
                active: null
            },
            views: {
                projects: null
            },
            members: [],
            pipelineAccess: null,
            policies: [],
            jobExtensions: [],
            localization: null
        }
    },
    style: true,
    async created() {
        if (!await loginChecker.check()) {
            Pomelo.Redirect('/login');
            return;
        }

        this.localization = require('/shared/localization').create()
        this.localization.addLocale('/shared/localization/en-US', ['en', 'en-US', 'en-GB', 'en-AU', 'en-CA'], true);
        this.localization.addLocale('/shared/localization/zh-CN', ['zh', 'zh-CN']);
        this.localization.setLocale();

        this.getPolicies();
        this.getMembership();
        this.getPipelineAccess();
        this.fetchProjects();
        this.getJobExtensions();
        this.ui.currentProject = this.projectId || window.localStorage.getItem('defaultProjectId');
        if (this.ui.currentProject) {
            if (!this.projectId) {
                this.projectId = this.ui.currentProject;
            }
            await this.getDefaultProject();
        }
    },
    unmounted() {
        if (this.signalr) {
            this.stopSignalR();
        }
    },
    watch: {
        deep: true,
        'data.currentPipeline': async function() {
            this.getPipelineAccess();
        }
    },
    methods: {
        async startSignalR() {
            this.signalr = new signalR.HubConnectionBuilder()
                .withUrl('/api/pipelinehub')
                .build();
            await this.signalr.start();
            return this.signalr;
        },
        async stopSignalR() {
            await this.signalr.stop();
            this.signalr = null;
        },
        fetchProjects() {
            this.views.projects = Pomelo.CQ.CreateView('/api/project', {});
            var self = this;
            this.views.projects.fetch(result => {
                self.data.projects = result.data;
            });
        },
        async getDefaultProject() {
            if (this.projectId) {
                this.data.defaultProject = (await Pomelo.CQ.Get('/api/project/' + this.projectId)).data;
            }
        },
        setDefaultProject(id) {
            window.localStorage.setItem('defaultProjectId', id);
            this.ui.projects = false;
            window.location = `/project/${id}`;
        },
        moment(str) {
            return moment(str);
        },
        async getPolicies() {
            this.policies = (await Pomelo.CQ.Get('/api/policy')).data;
            document.querySelector('title').innerHTML = this.getPolicyValue('DEVOPS_NAME');
        },
        getPolicyValue(key) {
            var policy = this.policies.filter(x => x.key == key)[0];
            if (!policy) {
                return null;
            } else {
                return policy.value;
            }
        },
        getCurrentRole() {
            return window.localStorage.getItem('role');
        },
        async getMembership() {
            if (!window.localStorage.getItem('user')) {
                return;
            }
            this.members = (await Pomelo.CQ.Get(`/api/user/${window.localStorage.getItem('loginProvider')}/${window.localStorage.getItem('username') }/project-access`)).data;
        },
        hasPermissionToProject(projectId, requiredAdmin) {
            // Only for UI

            if (window.localStorage.getItem('role') == 'SystemAdmin') {
                return true;
            }

            var membership = this.members.filter(x => x.projectId == projectId)[0];
            if (!membership) {
                return false;
            }

            if (requiredAdmin && membership.role != 'Admin') {
                return false;
            }

            return true;
        },
        async getPipelineAccess() {
            if (this.pipelineId) {
                try {
                    this.pipelineAccess = (await Pomelo.CQ.Get(`/api/project/${this.projectId}/pipeline/${this.pipelineId}/access/my`)).data;
                } catch { }
            }
        },
        async getJobExtensions() {
            this.jobExtensions = (await Pomelo.CQ.Get('/api/jobextension')).data;
        },
        hasPermissionToPipeline(projectId, pipelineId, type = 0) {
            if (this.hasPermissionToProject(projectId, true)) {
                return true;
            }

            if (!this.pipelineAccess) {
                return false;
            }

            return type == 0 && (this.pipelineAccess || this.hasPermissionToProject(projectId, false))
                || type == 1 && (this.pipelineAccess.accessType == 'Collaborator' || this.pipelineAccess.accessType == 'Master')
                || type == 2 && this.pipelineAccess.accessType == 'Master';
        },
        getToken() {
            return window.localStorage.token;
        }
    }
});