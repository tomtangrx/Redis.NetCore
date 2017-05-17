﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redis.NetCore.Abstractions
{
    public interface IRedisSetStringClient
    {
        Task<int> SetAddStringAsync(string setKey, params string[] members);

        Task<bool> SetIsMemberStringAsync(string setKey, string member);
    }
}