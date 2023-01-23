Page({
    layout: '/shared/devops',
    style: true,
    data() {
        return {
            pipeline: null,
        };
    },
    created() {
        console.log(this.pipeline);
    },
    mounted() {
        this.$root.ui.active = 'pipeline-diagram';
    },
    unmounted() {
        this.$root.ui.active = null;
    }
});