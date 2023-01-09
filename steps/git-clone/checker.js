// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

exports.check = function(step) {
    if (!step.arguments.GIT_CLONE_REPO) {
        return 'GIT_CLONE_REPO cannot be empty';
    }

    if (!step.arguments.GIT_CLONE_PATH) {
        return 'GIT_CLONE_PATH cannot be empty';
    }

    return null;
};