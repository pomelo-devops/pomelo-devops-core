// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

exports.check = function(step) {
    if (step.method == 'Execute Powershell File' && !step.arguments.POWERSHELL_SCRIPT_PATH) {
        return 'POWERSHELL_SCRIPT_PATH cannot be empty';
    }

    return null;
};