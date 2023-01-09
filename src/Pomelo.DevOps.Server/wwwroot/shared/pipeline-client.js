// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

var randomString = require('/shared/random-string.js');

exports.getPipeline = async function (projectId, pipelineId) {
    try {
        var pipeline = (await Pomelo.CQ.Get(`/api/project/${projectId}/pipeline/${pipelineId}`)).data;
        for (var i = 0; i < pipeline.stages.length; ++i) {
            var stage = pipeline.stages[i];
            stage.temp = randomString.rand();
            for (var j = 0; j < stage.steps.length; ++j) {
                var step = stage.steps[j];
                step.temp = randomString.rand();
            }
        }
        return pipeline;
    } catch (ex) {
        console.warn(ex);
        return null;
    }
};