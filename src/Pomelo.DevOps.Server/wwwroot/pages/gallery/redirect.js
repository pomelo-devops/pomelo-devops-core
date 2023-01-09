// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/gallery',
    data() {
        return {
            packageId: null
        };
    },
    async created() {
        var result = await Pomelo.CQ.Get(`/api/gallery/${this.packageId}`);
        Pomelo.Redirect(`/gallery/package/${this.packageId}/version/${result.data[0].version}`);
    }
});