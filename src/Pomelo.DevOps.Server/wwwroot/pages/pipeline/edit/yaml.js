// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    style: true,
    components: [
        '/components/codemirror/index'
    ],
    data() {
        return {
            pipeline: null,
            yaml: null
        };
    },
    async created() {
        this.yaml = await this.convertObjectToYaml(this.pipeline);
    },
    mounted() {
        this.$root.ui.active = 'pipeline-yaml';
    },
    unmounted() {
        this.$root.ui.active = null;
    },
    methods: {
        async convertObjectToYaml(obj) {
            return await Pomelo.CQ.Post('/api/misc/convert-pipeline-to-yaml', obj, 'text');
        },
        async convertYamlToObject(yaml) {
            return await Pomelo.CQ.Post('/api/misc/convert-pipeline-yaml-to-json', { yaml: yaml }, );
        },
        async onSave() {
            var obj = await this.convertYamlToObject(this.yaml);
            this.pipeline.timeout = obj.timeout;
            this.pipeline.stages = obj.stages;
            this.pipeline.triggers = obj.triggers;
            this.pipeline.arguments = obj.arguments;
            this.pipeline.hashmap = obj.hashmap;
        }
    }
});