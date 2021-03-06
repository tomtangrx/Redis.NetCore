﻿// <copyright file="EndpointResolution.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Redis.NetCore
{
    public static class EndpointResolution
    {
        public static async Task<Tuple<string, EndPoint>> GetEndpointAsync(string endpointConfiguration)
        {
            var parts = endpointConfiguration.Split(':');
            if (parts.Length == 0)
            {
                throw new ArgumentException($"Could not parse value [{endpointConfiguration}]", nameof(endpointConfiguration));
            }

            var host = parts[0];
            var address = await GetIpAddressAsync(host).ConfigureAwait(false);

            var port = GetPort(endpointConfiguration, parts);

            var endpoint = new IPEndPoint(address, port);
            return Tuple.Create(host, (EndPoint)endpoint);
        }

        private static int GetPort(string endpointConfiguration, string[] parts)
        {
            var port = 6379;
            if (parts.Length != 2)
            {
                return port;
            }

            var portConfiguration = parts[1];
            if (int.TryParse(portConfiguration, out port) == false)
            {
                throw new ArgumentException(
                                            $"Could not parse port [{portConfiguration}] from [{endpointConfiguration}]",
                                            nameof(endpointConfiguration));
            }

            return port;
        }

        private static async Task<IPAddress> GetIpAddressAsync(string host)
        {
            IPAddress address;
            if (IsIpAddress(host))
            {
                address = IPAddress.Parse(host);
            }

            var resolvedAddresses = await Dns.GetHostEntryAsync(host).ConfigureAwait(false);
            address = resolvedAddresses.AddressList.First(resolvedAddress => resolvedAddress.AddressFamily == AddressFamily.InterNetwork);
            return address;
        }

        private static bool IsIpAddress(string host)
        {
            return Regex.IsMatch(host, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        }
    }
}
