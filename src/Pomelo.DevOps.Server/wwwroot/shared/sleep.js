// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

exports.sleep = function (ms) {
    return new Promise((res) => setTimeout(() => { res() }, ms));
};
exports.yield = function () {
    return new Promise((res) => setTimeout(() => { res() }, 0));
};