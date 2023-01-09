// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

exports.check = function(step) {
    if (step.method == 'Execute Cmd File' && !step.arguments.CMD_SCRIPT_PATH) {
        return 'CMD_SCRIPT_PATH cannot be empty';
    }

    return null;
};