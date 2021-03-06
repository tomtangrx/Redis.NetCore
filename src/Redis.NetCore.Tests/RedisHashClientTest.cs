﻿// <copyright file="RedisHashClientTest.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Redis.NetCore.Tests
{
    [Trait("Category", "Integration")]
    public class RedisHashClientTest
    {
        [Fact]
        public async Task HashMulitpleGetBytesAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HashMulitpleGetBytesAsync);
                const string hashKey = "Hash" + field;
                const string expected1 = "Foo!";
                const string expected2 = "Bar!";
                await client.HashSetFieldAsync(hashKey, field + "1", Encoding.UTF8.GetBytes(expected1));
                await client.HashSetFieldAsync(hashKey, field + "2", Encoding.UTF8.GetBytes(expected2));
                var bytes = await client.HashGetFieldsAsync(hashKey, field + "1", field + "2", "NoKey");
                Assert.Equal(expected1, Encoding.UTF8.GetString(bytes[0]));
                Assert.Equal(expected2, Encoding.UTF8.GetString(bytes[1]));
                Assert.Equal(null, bytes[2]);
            }
        }

        [Fact]
        public async Task HasheGetAllBytesAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HasheGetAllBytesAsync);
                const string hashKey = "Hash" + field;
                const string expected1 = "Foo!";
                const string expected2 = "Bar!";
                await client.HashSetFieldAsync(hashKey, field + "1", Encoding.UTF8.GetBytes(expected1));
                await client.HashSetFieldAsync(hashKey, field + "2", Encoding.UTF8.GetBytes(expected2));
                var results = await client.HashGetAllFieldsAsync(hashKey);
                Assert.Equal(expected1, Encoding.UTF8.GetString(results[field + "1"]));
                Assert.Equal(expected2, Encoding.UTF8.GetString(results[field + "2"]));
            }
        }

        [Fact]
        public async Task HasheGetValuesBytesAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HasheGetValuesBytesAsync);
                const string hashKey = "Hash" + field;
                const string expected1 = "Foo!";
                const string expected2 = "Bar!";
                await client.HashSetFieldAsync(hashKey, field + "1", Encoding.UTF8.GetBytes(expected1));
                await client.HashSetFieldAsync(hashKey, field + "2", Encoding.UTF8.GetBytes(expected2));
                var bytes = await client.HashGetValuesAsync(hashKey);
                Assert.Equal(expected1, Encoding.UTF8.GetString(bytes[0]));
                Assert.Equal(expected2, Encoding.UTF8.GetString(bytes[1]));
            }
        }

        [Fact]
        public async Task HasheGetLengthAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HasheGetLengthAsync);
                const string hashKey = "Hash" + field;
                await client.DeleteKeyAsync(hashKey);
                const string expected1 = "Foo!";
                const string expected2 = "Bar!";
                await client.HashSetFieldAsync(hashKey, field + "1", Encoding.UTF8.GetBytes(expected1));
                var length = await client.HashGetLengthAsync(hashKey);
                Assert.Equal(1, length);
                await client.HashSetFieldAsync(hashKey, field + "2", Encoding.UTF8.GetBytes(expected2));
                length = await client.HashGetLengthAsync(hashKey);
                Assert.Equal(2, length);
            }
        }

        [Fact]
        public async Task HashSetBytesAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string expected = "Foo!";
                const string field = nameof(HashSetBytesAsync);
                const string hashKey = "Hash" + field;
                await client.HashSetFieldAsync(hashKey, field, Encoding.UTF8.GetBytes(expected));
                var bytes = await client.HashGetFieldAsync(hashKey, field);
                Assert.Equal(expected, Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public async Task HashSetNotExistsBytesAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string expected = "Foo!";
                const string field = nameof(HashSetNotExistsBytesAsync);
                const string hashKey = "Hash" + field;
                await client.DeleteKeyAsync(hashKey);
                var set = await client.HashSetFieldNotExistsAsync(hashKey, field, Encoding.UTF8.GetBytes(expected));
                Assert.Equal(true, set);
                var bytes = await client.HashGetFieldAsync(hashKey, field);
                Assert.Equal(expected, Encoding.UTF8.GetString(bytes));

                set = await client.HashSetFieldNotExistsAsync(hashKey, field, Encoding.UTF8.GetBytes("Bar!"));
                Assert.Equal(false, set);
                bytes = await client.HashGetFieldAsync(hashKey, field);
                Assert.Equal(expected, Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public async Task HashMultipleSetBytesAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HashMultipleSetBytesAsync);
                const string hashKey = "Hash" + field;
                const string field1 = field + "1";
                const string field2 = field + "2";
                var data = new Dictionary<string, byte[]>
                           {
                               { field1, "Foo1".ToBytes() },
                               { field2, "Foo2".ToBytes() }
                           };
                await client.HashSetFieldsAsync(hashKey, data);
                var value1 = await client.HashGetFieldAsync(hashKey, field1);
                var value2 = await client.HashGetFieldAsync(hashKey, field2);
                Assert.Equal("Foo1", Encoding.UTF8.GetString(value1));
                Assert.Equal("Foo2", Encoding.UTF8.GetString(value2));
            }
        }

        [Fact]
        public async Task HashDeleteAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string expected = "Foo!";
                const string field = nameof(HashDeleteAsync);
                const string hashKey = "Hash" + field;
                await client.HashSetFieldAsync(hashKey, field, Encoding.UTF8.GetBytes(expected));
                var bytes = await client.HashGetFieldAsync(hashKey, field);
                Assert.Equal(expected, Encoding.UTF8.GetString(bytes));
                await client.HashDeleteFieldsAsync(hashKey, field);
                bytes = await client.HashGetFieldAsync(hashKey, field);
                Assert.Equal(null, bytes);
            }
        }

        [Fact]
        public async Task HashExistAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HashExistAsync);
                const string hashKey = "Hash" + field;
                await client.HashSetFieldAsync(hashKey, field, "FooExists!".ToBytes());
                var exists = await client.HashFieldExistsAsync(hashKey, field);
                Assert.Equal(true, exists);

                const string noExistsField = "NoExistsAsync";
                exists = await client.HashFieldExistsAsync(hashKey, noExistsField);
                Assert.Equal(false, exists);
            }
        }

        [Fact]
        public async Task HasheGetKeysAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HasheGetKeysAsync);
                const string hashKey = "Hash" + field;
                const string expected1 = "Foo!";
                const string expected2 = "Bar!";
                await client.HashSetFieldAsync(hashKey, field + "1", Encoding.UTF8.GetBytes(expected1));
                await client.HashSetFieldAsync(hashKey, field + "2", Encoding.UTF8.GetBytes(expected2));
                var keys = await client.HashGetKeysAsync(hashKey);
                Assert.Equal(field + "1", keys[0]);
                Assert.Equal(field + "2", keys[1]);
            }
        }

        [Fact]
        public async Task HashIncrementByAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HashIncrementByAsync);
                const string hashKey = "Hash" + field;
                const string expected = "10";
                await client.HashSetFieldAsync(hashKey, field, Encoding.UTF8.GetBytes(expected));
                var value = await client.HashIncrementAsync(hashKey, field, 5);
                Assert.Equal(15, value);
            }
        }

        [Fact]
        public async Task HashIncrementByFloatAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string field = nameof(HashIncrementByAsync);
                const string hashKey = "Hash" + field;
                const string expected = "10.5";
                await client.HashSetFieldAsync(hashKey, field, Encoding.UTF8.GetBytes(expected));
                var value = await client.HashIncrementAsync(hashKey, field, .75f);
                Assert.Equal(11.25, value);
            }
        }

        [Fact]
        public async Task HashScanAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string hashKey = nameof(HashScanAsync);
                var fields = TestClient.SetupTestHashFields();

                await client.HashSetFieldsStringAsync(hashKey, fields);
                var cursor = await client.HashScanAsync(hashKey);
                var keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);

                cursor = await client.HashScanAsync(hashKey, cursor);
                keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);
            }
        }

        [Fact]
        public async Task HashScanWithCountAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string hashKey = nameof(HashScanWithCountAsync);
                var fields = TestClient.SetupTestHashFields();

                await client.HashSetFieldsStringAsync(hashKey, fields);
                var cursor = await client.HashScanAsync(hashKey, 5);
                var keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);

                cursor = await client.HashScanAsync(hashKey, cursor, 5);
                keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);
            }
        }

        [Fact]
        public async Task HashScanWithMatchAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string hashKey = nameof(HashScanWithMatchAsync);
                var fields = TestClient.SetupTestHashFields();
                await client.HashSetFieldsStringAsync(hashKey, fields);
                fields = TestClient.SetupTestHashFields("matchField");
                await client.HashSetFieldsStringAsync(hashKey, fields);
                var cursor = await client.HashScanAsync(hashKey, "match*");
                var keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);

                cursor = await client.HashScanAsync(hashKey, cursor, "match*");
                keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);
            }
        }

        [Fact]
        public async Task HashScanWithMatchAndCountAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string hashKey = nameof(HashScanWithMatchAndCountAsync);
                var fields = TestClient.SetupTestHashFields();
                await client.HashSetFieldsStringAsync(hashKey, fields);
                fields = TestClient.SetupTestHashFields("matchField");
                await client.HashSetFieldsStringAsync(hashKey, fields);
                var cursor = await client.HashScanAsync(hashKey, "match*", 5);
                var keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);

                cursor = await client.HashScanAsync(hashKey, cursor, "match*", 5);
                keys = cursor.GetFields();
                Assert.NotEqual(0, keys.Count);
                CheckKeys(keys);
            }
        }

        private static void CheckKeys(IDictionary<string, byte[]> keys)
        {
            foreach (var pair in keys)
            {
                var keyLastChar = pair.Key[pair.Key.Length - 1];
                var valueLastChar = pair.Value[pair.Value.Length - 1];
                Assert.Equal(keyLastChar, (char)valueLastChar);
            }
        }
    }
}