﻿using System;
using System.Collections.Generic;

namespace AgpWps.Model.Messages
{
    public class ExecutionFinishedMessage
    {
        public ExecutionFinishedMessage(string jobId, List<Tuple<string, string>> outputs, DateTime expiresOn)
        {
            JobId = jobId;
            Outputs = outputs;
            ExpiresOn = expiresOn;
        }

        public string JobId { get; }

        public DateTime ExpiresOn { get; }

        /// <summary>
        /// List of outputs containing their id and then their saving path, in this order.
        /// </summary>
        public List<Tuple<string, string>> Outputs { get; }

    }
}