// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

exports.check = function () {
    var user = this.getUser();
    var token = this.getToken();
    var loginProvider = this.getLoginProvider();
    var username = this.getUsername();
    if (!user || !token) {
        return Promise.resolve(false);
    }
    return Pomelo.CQ.Get('/api/user/' + loginProvider + '/' + username + '/session', { session: token })
        .then(() => {
            return Promise.resolve(true);
        })
        .catch(err => {
            window.localStorage.removeItem('token');
            window.localStorage.removeItem('user');
            window.localStorage.removeItem('role');
            window.localStorage.removeItem('username');
            window.localStorage.removeItem('loginProvider');
            window.localStorage.removeItem('userDisplayName');
            return Promise.resolve(false);
        });
};

exports.getToken = function () {
    return window.localStorage.getItem('token');
};

exports.getUser = function () {
    return window.localStorage.getItem('user');
};

exports.getUsername = function () {
    return window.localStorage.getItem('username');
};

exports.getLoginProvider = function () {
    return window.localStorage.getItem('loginProvider');
};

exports.signOut = function () {
    window.localStorage.removeItem('token');
    window.localStorage.removeItem('user');
    window.localStorage.removeItem('role');
    window.localStorage.removeItem('username');
    window.localStorage.removeItem('loginProvider');
    window.localStorage.removeItem('userDisplayName');
    window.location = '/login';
};

exports.storeSession = function (result) {
    window.localStorage.setItem('token', result.id);
    window.localStorage.setItem('user', result.userId);
    window.localStorage.setItem('role', result.user.role);
    window.localStorage.setItem('username', result.user.username);
    window.localStorage.setItem('loginProvider', result.user.loginProviderId);
    window.localStorage.setItem('userDisplayName', result.user.displayName);
};