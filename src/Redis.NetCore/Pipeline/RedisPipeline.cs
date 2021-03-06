﻿// <copyright file="RedisPipeline.cs" company="PayScale">
// Copyright (c) PayScale. All rights reserved.
// Licensed under the APACHE 2.0 license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Redis.NetCore.Constants;
using Redis.NetCore.Sockets;

namespace Redis.NetCore.Pipeline
{
    public class RedisPipeline : IRedisPipeline
    {
        private readonly int _pipelineId;
        private readonly IRedisReader _redisReader;
        private readonly IRedisWriter _redisWriter;
        private readonly ConcurrentQueue<RedisPipelineItem> _responseQueue = new ConcurrentQueue<RedisPipelineItem>();
        private readonly IAsyncSocket _socket;
        private readonly Stream _stream;
        private Exception _pipelineException;
        private int _receiveIsRunning;
        private Task _receiveTask;
        private int _sendIsRunning;
        private Task _sendTask;

        public RedisPipeline(int pipelineId, IAsyncSocket socket, Stream stream, IRedisWriter redisWriter, IRedisReader redisReader)
        {
            _pipelineId = pipelineId;
            _socket = socket;
            _stream = stream;
            _redisWriter = redisWriter;
            _redisReader = redisReader;
        }

        public bool IsErrorState => _pipelineException != null;

        public ConcurrentQueue<RedisPipelineItem> RequestQueue { get; } = new ConcurrentQueue<RedisPipelineItem>();

        public Task AuthenticateAsync(string password)
        {
            return SendCommandAsync(RedisCommands.Authenticate, password.ToBytes());
        }

        public Task<byte[]> SendCommandAsync(params byte[][] requestData)
        {
            var taskCompletion = new TaskCompletionSource<byte[]>();
            var request = new RedisPipelineItem(
                                                requestData,
                                                taskCompletion.SetException,
                                                result => taskCompletion.SetResult(result.CollapseArray()));

            RequestQueue.Enqueue(request);

            return EnsureSendCommandAsync(taskCompletion);
        }

        public Task<byte[][]> SendMultipleCommandAsync(params byte[][] requestData)
        {
            var taskCompletion = new TaskCompletionSource<byte[][]>();
            var request = new RedisPipelineItem(
                                                requestData,
                                                taskCompletion.SetException,
                                                result => taskCompletion.SetResult(result));
            RequestQueue.Enqueue(request);

            return EnsureSendCommandAsync(taskCompletion);
        }

        public void SaveQueue(IRedisPipeline redisPipeline)
        {
            while (RequestQueue.TryDequeue(out RedisPipelineItem currentItem))
            {
                redisPipeline.RequestQueue.Enqueue(currentItem);
            }
        }

        public void ThrowErrorForRemainingResponseQueueItems()
        {
            while (_responseQueue.TryDequeue(out RedisPipelineItem currentItem))
            {
                currentItem.OnError(_pipelineException);
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _socket?.Dispose();
        }

        private static void FireAndForget(Task task)
        {
        }

        private Task<T> EnsureSendCommandAsync<T>(TaskCompletionSource<T> taskCompletion)
        {
            StartSendIfNotRunning();

            return taskCompletion.Task;
        }

        private Task StartSendIfNotRunning()
        {
            if (_pipelineException != null)
            {
                return Task.CompletedTask;
            }

            if (Interlocked.CompareExchange(ref _sendIsRunning, 1, 0) == 0)
            {
                _sendTask = Task.Run(SendAndMarkDone);
                return _sendTask;
            }

            return Task.CompletedTask;
        }

        private async Task SendAndMarkDone()
        {
            while (true)
            {
                await SendAsync().ConfigureAwait(false);
                Interlocked.Exchange(ref _sendIsRunning, 0);
                if (RequestQueue.Count == 0 || Interlocked.CompareExchange(ref _sendIsRunning, 1, 0) != 0)
                {
                    return;
                }
            }
        }

        private async Task SendAsync()
        {
            try
            {
                await _redisWriter.CreateNewBufferAsync().ConfigureAwait(false);

                while (RequestQueue.TryDequeue(out RedisPipelineItem currentItem))
                {
                    await _redisWriter.WriteRedisRequestAsync(currentItem.Data).ConfigureAwait(false);
                    _responseQueue.Enqueue(currentItem);
                    FireAndForget(StartReceiveIfNotRunning());
                    if (_redisWriter.BufferCount <= 1)
                    {
                        continue;
                    }

                    await _redisWriter.FlushWriteBufferAsync().ConfigureAwait(false);
                    break;
                }

                await _redisWriter.FlushWriteBufferAsync().ConfigureAwait(false);
            }
            catch (Exception error)
            {
                _pipelineException = error;
                ThrowErrorForRemainingResponseQueueItems();
            }
            finally
            {
                _redisWriter.CheckInBuffers();
            }
        }

        private Task StartReceiveIfNotRunning()
        {
            if (_pipelineException != null)
            {
                return Task.CompletedTask;
            }

            if (Interlocked.CompareExchange(ref _receiveIsRunning, 1, 0) == 0)
            {
                _receiveTask = Task.Run(ReceiveAndMarkDone);
                return _receiveTask;
            }

            return Task.CompletedTask;
        }

        private async Task ReceiveAndMarkDone()
        {
            while (true)
            {
                await ReceiveAsync().ConfigureAwait(false);
                Interlocked.Exchange(ref _receiveIsRunning, 0);
                if (_responseQueue.Count == 0 || Interlocked.CompareExchange(ref _receiveIsRunning, 1, 0) != 0)
                {
                    return;
                }
            }
        }

        private async Task ReceiveAsync()
        {
            if (_pipelineException != null)
            {
                ThrowErrorForRemainingResponseQueueItems();
                return;
            }

            try
            {
                while (_responseQueue.TryDequeue(out RedisPipelineItem currentItem))
                {
                    await _redisReader.ReadAsync(currentItem).ConfigureAwait(false);
                }
            }
            catch (Exception error)
            {
                _pipelineException = error;
                ThrowErrorForRemainingResponseQueueItems();
            }
            finally
            {
                _redisReader.CheckInBuffers();
            }
        }
    }
}