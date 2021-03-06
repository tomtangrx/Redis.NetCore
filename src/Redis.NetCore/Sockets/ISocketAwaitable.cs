﻿// <copyright file="ISocketAwaitable.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Runtime.CompilerServices;

namespace Redis.NetCore.Sockets
{
    public interface ISocketAwaitable<out T> : INotifyCompletion
    {
        bool IsCompleted { get; set; }

        T GetResult();

        ISocketAwaitable<T> GetAwaiter();
    }
}