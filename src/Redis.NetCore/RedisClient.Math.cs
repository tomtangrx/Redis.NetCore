﻿// <copyright file="RedisClient.Math.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Redis.NetCore.Abstractions;
using Redis.NetCore.Constants;

namespace Redis.NetCore
{
    public partial class RedisClient : IRedisMathClient
    {
        public async Task<int> IncrementAsync(string key)
        {
            CheckKey(key);

            var bytes = await SendCommandAsync(RedisCommands.Increment, key.ToBytes()).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> IncrementAsync(string key, int amount)
        {
            CheckKey(key);

            var amountBytes = amount.ToBytes();
            var bytes = await SendCommandAsync(RedisCommands.IncrementBy, key.ToBytes(), amountBytes).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> DecrementAsync(string key)
        {
            CheckKey(key);

            var bytes = await SendCommandAsync(RedisCommands.Decrement, key.ToBytes()).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> DecrementAsync(string key, int amount)
        {
            CheckKey(key);

            var amountBytes = amount.ToBytes();
            var bytes = await SendCommandAsync(RedisCommands.DecrementBy, key.ToBytes(), amountBytes).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<float> IncrementAsync(string key, float amount)
        {
            CheckKey(key);

            var amountBytes = amount.ToBytes();
            var bytes = await SendCommandAsync(RedisCommands.IncrementByFloat, key.ToBytes(), amountBytes).ConfigureAwait(false);
            var stringValue = Encoding.UTF8.GetString(bytes);
            if (float.TryParse(stringValue, out float value))
            {
                return value;
            }

            throw new RedisException($"Could not parse [{stringValue}] for key [{key}]");
        }
    }
}