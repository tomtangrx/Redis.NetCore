// <copyright file="IRedisReader.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Threading.Tasks;

namespace Redis.NetCore.Pipeline
{
    public interface IRedisReader
    {
        Task ReadAsync(RedisPipelineItem redisItem);

        void CheckInBuffers();
    }
}