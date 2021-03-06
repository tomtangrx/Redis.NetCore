﻿// <copyright file="RedisSetClientTest.cs" company="PayScale">
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
    public class RedisSetClientTest
    {
        [Fact]
        public async Task SetAddAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetAddAsync);
                await client.DeleteKeyAsync(setKey);

                var count = await client.SetAddMemberAsync(setKey, "Foo".ToBytes(), "Bar".ToBytes());
                Assert.Equal(2, count);

                var isMember = await client.SetIsMemberAsync(setKey, "FooBar".ToBytes());
                Assert.False(isMember);

                isMember = await client.SetIsMemberAsync(setKey, "Bar".ToBytes());
                Assert.True(isMember);
            }
        }

        [Fact]
        public async Task SetCardinalityAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetCardinalityAsync);
                await client.DeleteKeyAsync(setKey);

                var count = await client.SetAddMemberAsync(setKey, "Foo".ToBytes(), "Bar".ToBytes());
                Assert.Equal(2, count);

                count = await client.SetAddMemberAsync(setKey, "Bar".ToBytes(), "FooBar".ToBytes());
                Assert.Equal(1, count);

                count = await client.SetCardinalityAsync(setKey);
                Assert.Equal(3, count);
            }
        }

        [Fact]
        public async Task SetDifferenceAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetDifferenceAsync);
                const string setKey1 = setKey + "1";
                const string setKey2 = setKey + "2";
                await client.DeleteKeyAsync(setKey1);
                await client.DeleteKeyAsync(setKey2);

                await client.SetAddMemberAsync(setKey1, "Foo".ToBytes(), "Bar".ToBytes(), "FooBar".ToBytes());
                await client.SetAddMemberAsync(setKey2, "Bar".ToBytes());

                var bytes = await client.SetGetDifferenceMembersAsync(setKey1, setKey2);
                Assert.Equal(2, bytes.Length);
                var values = bytes.Select(item => Encoding.UTF8.GetString(item)).ToArray();
                Array.Sort(values);
                Assert.Equal("Foo", values[0]);
                Assert.Equal("FooBar", values[1]);
            }
        }

        [Fact]
        public async Task SetIntersectionAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetIntersectionAsync);
                const string setKey1 = setKey + "1";
                const string setKey2 = setKey + "2";
                await client.DeleteKeyAsync(setKey1);
                await client.DeleteKeyAsync(setKey2);

                await client.SetAddMemberAsync(setKey1, "Foo".ToBytes(), "Bar".ToBytes());
                await client.SetAddMemberAsync(setKey2, "Bar".ToBytes(), "2".ToBytes());

                var bytes = await client.SetGetIntersectionMembersAsync(setKey1, setKey2);
                Assert.Equal(1, bytes.Length);
                Assert.Equal("Bar".ToBytes(), bytes[0]);
            }
        }

        [Fact]
        public async Task SetUnionAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetUnionAsync);
                const string setKey1 = setKey + "1";
                const string setKey2 = setKey + "2";
                await client.DeleteKeyAsync(setKey1);
                await client.DeleteKeyAsync(setKey2);

                await client.SetAddMemberAsync(setKey1, "Foo".ToBytes(), "Bar".ToBytes());
                await client.SetAddMemberAsync(setKey2, "Bar".ToBytes(), "FooBar".ToBytes());

                var bytes = await client.SetGetUnionMembersAsync(setKey1, setKey2);
                var sortedValues = bytes.OrderBy(item => Encoding.UTF8.GetString(item)).ToArray();
                Assert.Equal(3, bytes.Length);
                Assert.Equal("Bar".ToBytes(), sortedValues[0]);
                Assert.Equal("Foo".ToBytes(), sortedValues[1]);
                Assert.Equal("FooBar".ToBytes(), sortedValues[2]);
            }
        }

        [Fact]
        public async Task SetDifferenceAndStoreAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetDifferenceAndStoreAsync);
                const string setKey1 = setKey + "1";
                const string setKey2 = setKey + "2";
                const string storeKey = setKey + "Store";
                await client.DeleteKeyAsync(setKey1);
                await client.DeleteKeyAsync(setKey2);
                await client.DeleteKeyAsync(storeKey);

                await client.SetAddMemberAsync(setKey1, "Foo".ToBytes(), "Bar".ToBytes(), "FooBar".ToBytes());
                await client.SetAddMemberAsync(setKey2, "Bar".ToBytes());

                var count = await client.SetStoreDifferenceMembersAsync(storeKey, setKey1, setKey2);
                Assert.Equal(2, count);

                var bytes = await client.SetGetMembersAsync(storeKey);
                var values = bytes.Select(item => Encoding.UTF8.GetString(item)).ToArray();
                Array.Sort(values);
                Assert.Equal("Foo", values[0]);
                Assert.Equal("FooBar", values[1]);
            }
        }

        [Fact]
        public async Task SetIntersectionAndStoreAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetIntersectionAndStoreAsync);
                const string setKey1 = setKey + "1";
                const string setKey2 = setKey + "2";
                const string storeKey = setKey + "Store";
                await client.DeleteKeyAsync(setKey1);
                await client.DeleteKeyAsync(setKey2);
                await client.DeleteKeyAsync(storeKey);

                await client.SetAddMemberAsync(setKey1, "Foo".ToBytes(), "Bar".ToBytes());
                await client.SetAddMemberAsync(setKey2, "Bar".ToBytes(), "2".ToBytes());

                var count = await client.SetStoreIntersectionMembersAsync(storeKey, setKey1, setKey2);
                Assert.Equal(1, count);

                var bytes = await client.SetGetMembersAsync(storeKey);
                Assert.Equal(1, bytes.Length);
                Assert.Equal("Bar".ToBytes(), bytes[0]);
            }
        }

        [Fact]
        public async Task SetUnionAndStoreAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetUnionAndStoreAsync);
                const string setKey1 = setKey + "1";
                const string setKey2 = setKey + "2";
                const string storeKey = setKey + "Store";
                await client.DeleteKeyAsync(setKey1);
                await client.DeleteKeyAsync(setKey2);
                await client.DeleteKeyAsync(storeKey);

                await client.SetAddMemberAsync(setKey1, "Foo".ToBytes(), "Bar".ToBytes());
                await client.SetAddMemberAsync(setKey2, "Bar".ToBytes(), "FooBar".ToBytes());

                var count = await client.SetStoreUnionMembersAsync(storeKey, setKey1, setKey2);
                Assert.Equal(3, count);

                var bytes = await client.SetGetMembersAsync(storeKey);
                var sortedValues = bytes.OrderBy(item => Encoding.UTF8.GetString(item)).ToArray();
                Assert.Equal(3, bytes.Length);
                Assert.Equal("Bar".ToBytes(), sortedValues[0]);
                Assert.Equal("Foo".ToBytes(), sortedValues[1]);
                Assert.Equal("FooBar".ToBytes(), sortedValues[2]);
            }
        }

        [Fact]
        public async Task SetMoveAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetMoveAsync);
                const string sourceKey = setKey + "Source";
                const string destKey = setKey + "Destination";
                await client.DeleteKeyAsync(sourceKey);
                await client.DeleteKeyAsync(destKey);

                await client.SetAddMemberAsync(sourceKey, "Foo".ToBytes(), "Bar".ToBytes());

                var moved = await client.SetMoveMemberAsync(sourceKey, destKey, "Foo".ToBytes());
                Assert.True(moved);

                var isMember = await client.SetIsMemberAsync(destKey, "Foo".ToBytes());
                Assert.True(isMember);

                isMember = await client.SetIsMemberAsync(sourceKey, "Foo".ToBytes());
                Assert.False(isMember);
            }
        }

        [Fact]
        public async Task SetPopAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetPopAsync);
                await client.DeleteKeyAsync(setKey);

                await client.SetAddMemberAsync(setKey, "Foo".ToBytes(), "Bar".ToBytes());

                var bytes = await client.SetPopMemberAsync(setKey);

                var isMember = await client.SetIsMemberAsync(setKey, bytes);
                Assert.False(isMember);
            }
        }

        [Fact]
        public async Task SetGetRandomMemberAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetGetRandomMemberAsync);
                await client.DeleteKeyAsync(setKey);

                await client.SetAddMemberAsync(setKey, "Foo".ToBytes(), "Bar".ToBytes());

                var bytes = await client.SetGetRandomMemberAsync(setKey, 2);

                var isMember = await client.SetIsMemberAsync(setKey, bytes[0]);
                Assert.True(isMember);
            }
        }

        [Fact]
        public async Task SetRemoveMembersAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetRemoveMembersAsync);
                await client.DeleteKeyAsync(setKey);

                await client.SetAddMemberAsync(setKey, "Foo".ToBytes(), "Bar".ToBytes(), "FooBar".ToBytes());

                var count = await client.SetRemoveMembersAsync(setKey, "Bar".ToBytes(), "Foo".ToBytes(), "Test".ToBytes());
                Assert.Equal(2, count);

                var members = await client.SetGetMembersAsync(setKey);
                Assert.Equal("FooBar".ToBytes(), members[0]);
            }
        }

        [Fact]
        public async Task SetScanAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetScanAsync);
                await client.DeleteKeyAsync(setKey);
                var members = TestClient.SetupTestSetMembers();

                await client.SetAddMemberStringAsync(setKey, members);
                var cursor = await client.SetScanAsync(setKey);
                var values = cursor.GetStringValues();
                Assert.NotEqual(0, values.Count());

                cursor = await client.SetScanAsync(setKey, cursor);
                values = cursor.GetStringValues();
                Assert.NotEqual(0, values.Count());
            }
        }

        [Fact]
        public async Task SetScanWithCountAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetScanWithCountAsync);
                await client.DeleteKeyAsync(setKey);
                var members = TestClient.SetupTestSetMembers();

                await client.SetAddMemberStringAsync(setKey, members);
                var cursor = await client.SetScanAsync(setKey, 5);
                var values = cursor.GetStringValues();
                Assert.NotEqual(0, values.Count());

                cursor = await client.SetScanAsync(setKey, cursor, 5);
                values = cursor.GetStringValues();
                Assert.NotEqual(0, values.Count());
            }
        }

        [Fact]
        public async Task SetScanWithMatchAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetScanWithMatchAsync);
                await client.DeleteKeyAsync(setKey);
                var members = TestClient.SetupTestSetMembers();
                await client.SetAddMemberStringAsync(setKey, members);
                members = TestClient.SetupTestSetMembers("match");
                await client.SetAddMemberStringAsync(setKey, members);

                var cursor = await client.SetScanAsync(setKey, "match*");
                var values = cursor.GetStringValues().ToArray();
                Assert.NotEqual(0, values.Length);
                CheckValues(values, "match");

                cursor = await client.SetScanAsync(setKey, cursor, "match*");
                values = cursor.GetStringValues().ToArray();
                Assert.NotEqual(0, values.Length);
                CheckValues(values, "match");
            }
        }

        [Fact]
        public async Task SetScanWithMatchAndCountAsync()
        {
            using (var client = TestClient.CreateClient())
            {
                const string setKey = nameof(SetScanWithMatchAndCountAsync);
                await client.DeleteKeyAsync(setKey);
                var members = TestClient.SetupTestSetMembers();
                await client.SetAddMemberStringAsync(setKey, members);
                members = TestClient.SetupTestSetMembers("match");
                await client.SetAddMemberStringAsync(setKey, members);

                var cursor = await client.SetScanAsync(setKey, "match*", 5);
                var values = cursor.GetStringValues().ToArray();
                Assert.NotEqual(0, values.Length);
                CheckValues(values, "match");

                cursor = await client.SetScanAsync(setKey, cursor, "match*", 5);
                values = cursor.GetStringValues().ToArray();
                Assert.NotEqual(0, values.Length);
                CheckValues(values, "match");
            }
        }

        private static void CheckValues(IEnumerable<string> values, string prefix = "")
        {
            foreach (var value in values)
            {
                Assert.Contains(prefix, value);
            }
        }
    }
}
