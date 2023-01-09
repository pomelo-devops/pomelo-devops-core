// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;

namespace Pomelo.DevOps.Agent
{
    public class ConnectorException : Exception
    {
        public int HttpStatus { get; private set; }

        public string ErrorMessage { get; private set; }

        public ConnectorException(int status, string error) : base($"An error occurred when invoking HTTP API. " + error)
        {
            HttpStatus = status;
            ErrorMessage = error;
        }
    }
}
