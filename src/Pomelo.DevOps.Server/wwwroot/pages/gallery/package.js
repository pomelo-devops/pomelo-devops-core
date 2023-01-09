// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var marked = require('/assets/js/marked/marked.min');
var jsYaml = require('/assets/js/js-yaml/dist/js-yaml');

Page({
    layout: '/shared/gallery',
    style: true,
    data() {
        return {
            packageId: null,
            version: null,
            versions: [],
            package: null,
            iconUrl: null,
            active: 'readme',
            readme: 'This package does not have a README. Add a `readme.md` to your package so that users know how to get started.',
            yaml: null
        };
    },
    created() {
        this.iconUrl = `/api/gallery/${this.packageId}/version/${this.version}/package.png`;
        this.getPackage();
        this.getReadme();
        this.getVersions();
    },
    computed: {
        readmeHtml() {
            return marked.parse(this.readme);
        }
    },
    methods: {
        async getPackage() {
            var package = (await Pomelo.CQ.Get(`/api/gallery/${this.packageId}/version/${this.version}`)).data;
            package.platforms = package.platform.split(',').map(x => x.trim());
            this.package = package;
            var yaml = await Pomelo.CQ.Get(`/api/gallery/${this.packageId}/version/${this.version}/step.yml`, null, 'text');
            this.yaml = jsYaml.load(yaml);
        },
        async getVersions() {
            this.versions = (await Pomelo.CQ.Get(`/api/gallery/${this.packageId}`)).data;
        },
        async getReadme() {
            try {
                this.readme = await Pomelo.CQ.Get(`/api/gallery/${this.packageId}/version/${this.version}/readme.md`, null, 'text');
            } catch (ex) { }
        }
    }
});