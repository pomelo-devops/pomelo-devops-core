// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

exports.calc = function (start, end) {
    start = start ? new Date(start) : new Date();
    end = end ? new Date(end) : new Date();
    if (start > end) {
        start = end;
    }
    var span = end - start;
    if (span < 1000) {
        return span + 'ms';
    }
    span /= 1000.0;
    if (span < 60) {
        return span.toFixed(1) + 's';
    }

    span /= 60.0;
    if (span < 60) {
        return span.toFixed(1) + 'm';
    }

    span /= 60.0;
    if (span < 24) {
        return span.toFixed(1) + 'h';
    }

    span /= 24.0;
    if (span < 30) {
        return span.toFixed(1) + 'd';
    }

    span /= 30.0;
    if (span < 12) {
        return span.toFixed(1) + 'mo';
    }

    span /= 12.0;
    return span.toFixed(1) + 'y';
};