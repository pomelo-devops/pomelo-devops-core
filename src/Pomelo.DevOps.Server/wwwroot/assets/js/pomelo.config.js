var loginChecker = require('/shared/login-checker.js');

var PomeloVueOptions = {
    mobile() {
        return false;
    },
    removeStyleWhenUnmount: true
};

if (!window.expireTimer) {
    window.expireTimer = setInterval(function () {
        if (!window.localStorage.getItem('token')) {
            return;
        }
        var timestamp = new Date().getTime();
        var expire = parseInt(window.localStorage.getItem('expire'));
        if (timestamp >= expire) {
            console.log('Token expired, signing out...');
            loginChecker.signOut();
        }
    }, 10000);
}

var CQOptions = {
    beforeSend: function(xhr) {
        var token = loginChecker.getToken();
        xhr.setRequestHeader('Authorization', 'Session ' + token); 
    },
    onError(err) {
        if (window.localStorage.getItem('token') && err && err.code && err.code > 400 && err.code < 500 && err.code != 404) {
            loginChecker.signOut();
        }

        return Promise.resolve(err);
    },
    onSucceeded(ret) {
        if (window.localStorage.getItem('token')) {
            window.localStorage.setItem('expire', new Date().getTime() + 1000 * 60 * 20);
        }

        return Promise.resolve(ret);
    }
};