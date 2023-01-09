// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var loginChecker = require('/shared/login-checker');

Page({
    created() {
        loginChecker.signOut();
    }
});