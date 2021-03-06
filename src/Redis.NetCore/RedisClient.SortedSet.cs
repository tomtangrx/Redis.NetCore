﻿// <copyright file="RedisClient.SortedSet.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redis.NetCore.Abstractions;
using Redis.NetCore.Constants;

namespace Redis.NetCore
{
    [SuppressMessage("StyleCop", "SA1008", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    [SuppressMessage("StyleCop", "SA1009", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    public partial class RedisClient : IRedisSortedSetClient
    {
        public async Task<int> SortedSetAddMembersAsync(string setKey, params(byte[] member, double score)[] items)
        {
            CheckSetKey(setKey);

            var byteArray = ConvertTupleItemsToByteArray(items);

            var request = ComposeRequest(RedisCommands.SortedSetAdd, setKey.ToBytes(), byteArray);
            var bytes = await SendMultipleCommandAsync(request).ConfigureAwait(false);
            return bytes[0].ConvertBytesToInteger();
        }

        public async Task<int> SortedSetAddOnlyMembersAsync(string setKey, params (byte[] member, double score)[] items)
        {
            CheckSetKey(setKey);

            var byteArray = ConvertTupleItemsToByteArray(items);

            var request = ComposeRequest(RedisCommands.SortedSetAdd, setKey.ToBytes(), RedisCommands.SetNotExists, byteArray);
            var bytes = await SendMultipleCommandAsync(request).ConfigureAwait(false);
            return bytes[0].ConvertBytesToInteger();
        }

        public async Task<int> SortedSetUpsertMembersAsync(string setKey, params (byte[] member, double score)[] items)
        {
            CheckSetKey(setKey);

            var byteArray = ConvertTupleItemsToByteArray(items);

            var request = ComposeRequest(RedisCommands.SortedSetAdd, setKey.ToBytes(), RedisCommands.SetChanged, byteArray);
            var bytes = await SendMultipleCommandAsync(request).ConfigureAwait(false);
            return bytes[0].ConvertBytesToInteger();
        }

        public async Task<int> SortedSetUpdateMembersAsync(string setKey, params (byte[] member, double score)[] items)
        {
            CheckSetKey(setKey);

            var byteArray = ConvertTupleItemsToByteArray(items);

            var request = ComposeRequest(RedisCommands.SortedSetAdd, setKey.ToBytes(), RedisCommands.SetExists, byteArray);
            var bytes = await SendMultipleCommandAsync(request).ConfigureAwait(false);
            return bytes[0].ConvertBytesToInteger();
        }

        public async Task<int?> SortedSetGetScoreAsync(string setKey, byte[] member)
        {
            CheckSetKey(setKey);

            var bytes = await SendCommandAsync(RedisCommands.SortedSetScore, setKey.ToBytes(), member).ConfigureAwait(false);
            return bytes?.ConvertBytesToInteger();
        }

        public async Task<int?> SortedSetGetRankAsync(string setKey, byte[] member)
        {
            CheckSetKey(setKey);

            var bytes = await SendCommandAsync(RedisCommands.SortedSetRank, setKey.ToBytes(), member).ConfigureAwait(false);
            return bytes?.ConvertBytesToInteger();
        }

        public async Task<int?> SortedSetGetReverseRankAsync(string setKey, byte[] member)
        {
            CheckSetKey(setKey);

            var bytes = await SendCommandAsync(RedisCommands.SortedSetReverseRank, setKey.ToBytes(), member).ConfigureAwait(false);
            return bytes?.ConvertBytesToInteger();
        }

        public async Task<int> SortedSetCardinalityAsync(string setKey)
        {
            CheckSetKey(setKey);

            var bytes = await SendCommandAsync(RedisCommands.SortedSetCardinality, setKey.ToBytes()).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> SortedSetGetCountAsync(string setKey, string min, string max)
        {
            CheckSetKey(setKey);

            var bytes = await SendCommandAsync(RedisCommands.SortedSetCount, setKey.ToBytes(), min.ToBytes(), max.ToBytes()).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> SortedSetIncrementByAsync(string setKey, byte[] member, int increment)
        {
            CheckSetKey(setKey);

            var incrementBytes = increment.ToBytes();
            var bytes = await SendCommandAsync(RedisCommands.SortedSetIncrementBy, setKey.ToBytes(), incrementBytes, member).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public Task<int> SortedSetStoreIntersectionMembersAsync(string storeKey, string[] sets, RedisAggregate aggregate = RedisAggregate.Sum)
        {
            return SortedSetStoreCommandAsync(storeKey, sets, aggregate, RedisCommands.SortedSetIntersectionStore);
        }

        public Task<int> SortedSetStoreIntersectionMembersAsync(string storeKey, (string set, double weight)[] sets, RedisAggregate aggregate = RedisAggregate.Sum)
        {
            return SortedSetStoreCommandAsync(storeKey, sets, aggregate, RedisCommands.SortedSetIntersectionStore);
        }

        public Task<int> SortedSetStoreUnionMembersAsync(string storeKey, string[] sets, RedisAggregate aggregate = RedisAggregate.Sum)
        {
            return SortedSetStoreCommandAsync(storeKey, sets, aggregate, RedisCommands.SortedSetUnionStore);
        }

        public Task<int> SortedSetStoreUnionMembersAsync(string storeKey, (string set, double weight)[] sets, RedisAggregate aggregate = RedisAggregate.Sum)
        {
            return SortedSetStoreCommandAsync(storeKey, sets, aggregate, RedisCommands.SortedSetUnionStore);
        }

        public Task<byte[][]> SortedSetGetRangeAsync(string setKey, int start, int end)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRange, setKey.ToBytes(), start.ToBytes(), end.ToBytes());
            return SendMultipleCommandAsync(request);
        }

        public async Task<(byte[] member, double weight)[]> SortedSetGetRangeWithScoresAsync(string setKey, int start, int end)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRange, setKey.ToBytes(), start.ToBytes(), end.ToBytes(), RedisCommands.WithScores);
            var bytes = await SendMultipleCommandAsync(request);
            return ConvertToSortedSetTuple(bytes);
        }

        public Task<byte[][]> SortedSetGetReverseRangeAsync(string setKey, int start, int end)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetReverseRange, setKey.ToBytes(), start.ToBytes(), end.ToBytes());
            return SendMultipleCommandAsync(request);
        }

        public async Task<(byte[] member, double weight)[]> SortedSetGetReverseRangeWithScoresAsync(string setKey, int start, int end)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetReverseRange, setKey.ToBytes(), start.ToBytes(), end.ToBytes(), RedisCommands.WithScores);
            var bytes = await SendMultipleCommandAsync(request);
            return ConvertToSortedSetTuple(bytes);
        }

        public Task<byte[][]> SortedSetGetRangeByScoreAsync(string setKey, string min, string max)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes());
            return SendMultipleCommandAsync(request);
        }

        public Task<byte[][]> SortedSetGetRangeByScoreAsync(string setKey, string min, string max, int offset, int count)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes(), RedisCommands.Limit, offset.ToBytes(), count.ToBytes());
            return SendMultipleCommandAsync(request);
        }

        public async Task<(byte[] member, double weight)[]> SortedSetGetRangeByScoreWithScoresAsync(string setKey, string min, string max)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes(), RedisCommands.WithScores);
            var bytes = await SendMultipleCommandAsync(request);
            return ConvertToSortedSetTuple(bytes);
        }

        public async Task<(byte[] member, double weight)[]> SortedSetGetRangeByScoreWithScoresAsync(string setKey, string min, string max, int offset, int count)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes(), RedisCommands.WithScores, RedisCommands.Limit, offset.ToBytes(), count.ToBytes());
            var bytes = await SendMultipleCommandAsync(request);
            return ConvertToSortedSetTuple(bytes);
        }

        public Task<byte[][]> SortedSetGetReverseRangeByScoreAsync(string setKey, string min, string max)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetReverseRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes());
            return SendMultipleCommandAsync(request);
        }

        public Task<byte[][]> SortedSetGetReverseRangeByScoreAsync(string setKey, string min, string max, int offset, int count)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetReverseRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes(), RedisCommands.Limit, offset.ToBytes(), count.ToBytes());
            return SendMultipleCommandAsync(request);
        }

        public async Task<(byte[] member, double weight)[]> SortedSetGetReverseRangeByScoreWithScoresAsync(string setKey, string min, string max)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetReverseRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes(), RedisCommands.WithScores);
            var bytes = await SendMultipleCommandAsync(request);
            return ConvertToSortedSetTuple(bytes);
        }

        public async Task<(byte[] member, double weight)[]> SortedSetGetReverseRangeByScoreWithScoresAsync(string setKey, string min, string max, int offset, int count)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetReverseRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes(), RedisCommands.WithScores, RedisCommands.Limit, offset.ToBytes(), count.ToBytes());
            var bytes = await SendMultipleCommandAsync(request);
            return ConvertToSortedSetTuple(bytes);
        }

        public async Task<int> SortedSetRemoveMembersAsync(string setKey, params byte[][] members)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRemove, setKey.ToBytes(), members);
            var bytes = await SendCommandAsync(request).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> SortedSetRemoveRangeByRankAsync(string setKey, int start, int end)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRemoveRangeByRank, setKey.ToBytes(), start.ToBytes(), end.ToBytes());
            var bytes = await SendCommandAsync(request).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        public async Task<int> SortedSetRemoveRangeByScoreAsync(string setKey, string min, string max)
        {
            CheckSetKey(setKey);

            var request = ComposeRequest(RedisCommands.SortedSetRemoveRangeByScore, setKey.ToBytes(), min.ToBytes(), max.ToBytes());
            var bytes = await SendCommandAsync(request).ConfigureAwait(false);
            return bytes.ConvertBytesToInteger();
        }

        private static (byte[] member, double weight)[] ConvertToSortedSetTuple(IReadOnlyList<byte[]> bytes)
        {
            var results = new List<(byte[] member, double weight)>();
            for (var i = 0; i < bytes.Count; i += 2)
            {
                var member = bytes[i];
                var weight = bytes[i + 1].ConvertBytesToDouble();
                results.Add((member, weight));
            }

            return results.ToArray();
        }

        private static byte[][] ConvertTupleItemsToByteArray(IReadOnlyList<(byte[] member, double weight)> items)
        {
            var byteArray = new byte[items.Count * 2][];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var position = i * 2;
                byteArray[position] = item.weight.ToBytes();
                byteArray[position + 1] = item.member;
            }

            return byteArray;
        }

        private static void AddAggregateTypeToRequest(RedisAggregate aggregate, IList<byte[]> request, int index)
        {
            request[index] = RedisCommands.Aggregate;
            switch (aggregate)
            {
                case RedisAggregate.Sum:
                    request[index + 1] = RedisCommands.AggregateSum;
                    break;
                case RedisAggregate.Min:
                    request[index + 1] = RedisCommands.AggregateMin;
                    break;
                case RedisAggregate.Max:
                    request[index + 1] = RedisCommands.AggregateMax;
                    break;
            }
        }

        private async Task<int> SortedSetStoreCommandAsync(string storeKey, IReadOnlyCollection<string> sets, RedisAggregate aggregate, byte[] command)
        {
            CheckSetKey(storeKey);

            var countBytes = sets.Count.ToBytes();
            var request = new byte[sets.Count + 5][];
            request[0] = command;
            request[1] = storeKey.ToBytes();
            request[2] = countBytes;
            var index = CopyBytesToRequest(sets.ToBytes(), request, 3);
            AddAggregateTypeToRequest(aggregate, request, index);

            var bytes = await SendMultipleCommandAsync(request).ConfigureAwait(false);
            return bytes[0].ConvertBytesToInteger();
        }

        private async Task<int> SortedSetStoreCommandAsync(string storeKey, ValueTuple<string, double>[] sets, RedisAggregate aggregate, byte[] command)
        {
            CheckSetKey(storeKey);

            var countBytes = sets.Length.ToBytes();
            var request = new byte[(2 * sets.Length) + 6][];
            request[0] = command;
            request[1] = storeKey.ToBytes();
            request[2] = countBytes;
            var index = CopyBytesToRequest(sets.Select<(string set, double weight), string>(item => item.set).ToBytes(), request, 3);
            request[index] = RedisCommands.Weight;
            index = CopyBytesToRequest(sets.Select<(string set, double weight), byte[]>(item => item.weight.ToBytes()).ToArray(), request, index + 1);
            AddAggregateTypeToRequest(aggregate, request, index);

            var bytes = await SendMultipleCommandAsync(request).ConfigureAwait(false);
            return bytes[0].ConvertBytesToInteger();
        }
    }
}