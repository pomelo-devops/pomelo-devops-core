// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

Page({
    layout: '/shared/devops',
    created() {
        var defaultProject = window.localStorage.getItem('default-project');
        if (defaultProject) {
            Pomelo.Redirect(`/project/${defaultProject}`);
        }
    }
});