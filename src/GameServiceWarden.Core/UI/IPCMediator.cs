using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameServiceWarden.API.Communicable;
using GameServiceWarden.API.Communicable.Requests;
using GameServiceWarden.API.Communicable.Responses;
using GameServiceWarden.Core.Logging;

namespace GameServiceWarden.Core.UI
{
    public class IPCMediator
    {
        private const int CONNECT_TIMEOUT = 1000;
        private readonly string name;
        private readonly ConcurrentDictionary<string, (NamedPipeServerStream, Task)> pipes;
        public BlockingCollection<(string, CommunicableType, byte[])> RequestQueue { get; private set; }
        private volatile bool active;
        public bool IsRunning { get => active; }
        private Task acceptTask;
        private CancellationTokenSource stopAcceptingToken;
        public IPCMediator(string name)
        {
            this.name = name;
            pipes = new ConcurrentDictionary<string, (NamedPipeServerStream, Task)>();
            RequestQueue = new BlockingCollection<(string, CommunicableType, byte[])>(new ConcurrentQueue<(string, CommunicableType, byte[])>());
        }

        public void Open()
        {
            if (active) throw new InvalidOperationException("IPC already opened.");
            active = true;
            stopAcceptingToken = new CancellationTokenSource();
            acceptTask = AcceptConnections();
        }

        public void Close()
        {
            if (!active) throw new InvalidOperationException("IPC not open.");
            Logger.Log("Closing IPC mediator.");
            active = false;
            stopAcceptingToken.Cancel();
            try
            {
                acceptTask.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle((exception) => exception is TaskCanceledException);
            }
            InitiateDisconnectAll("Closing IPC system.").Wait();
            RequestQueue.CompleteAdding();
            stopAcceptingToken.Dispose();
        }

        public async Task ReplyAll(CommunicableType type, byte[] data)
        {
            IEnumerable<string> identifiers = pipes.Keys;
            Stack<Task> replyTasks = new Stack<Task>();
            foreach (string identifier in identifiers)
            {
                replyTasks.Push(Reply(identifier, type, data));
            }
            Task replyTask;
            while (replyTasks.TryPop(out replyTask))
            {
                await replyTask;
            }
        }

        public async Task Reply(string identifier, CommunicableType type, byte[] data)
        {
            CancellationTokenSource cancel = new CancellationTokenSource(1000);
            byte[] header = ResponseHeader.Encode(type, (uint)data.Length);
            await pipes[identifier].Item1.WriteAsync(header, 0, header.Length, cancel.Token);
            await pipes[identifier].Item1.WriteAsync(data, cancel.Token);
            cancel.Dispose();
        }

        public async Task InitiateDisconnect(string identifier, string reason)
        {
            Logger.Log($"Disconnecting \"{identifier}\". Reason: \"{reason}\"");
            DisconnectResponse response;
            response.reason = reason;
            await Reply(identifier, CommunicableType.Disconnect, JsonSerializer.SerializeToUtf8Bytes(response));
            (NamedPipeServerStream, Task) pipeAndTask = pipes[identifier];
            pipeAndTask.Item1.Close();
            await pipeAndTask.Item2;
            Logger.Log($"Successfully disconnected \"{identifier}\".");
        }

        public async Task InitiateDisconnectAll(string reason) {
            Logger.Log($"Disconnecting all of {pipes.Count} interfaces.");
            foreach (string id in pipes.Keys)
            {
                await InitiateDisconnect(id, reason);
            }
        }

        private async Task AcceptConnections()
        {
            List<Task> connectionTasks = new List<Task>();
            Logger.Log("Accepting pipe connections.");
            while (active)
            {
                NamedPipeServerStream pipe = new NamedPipeServerStream(name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await pipe.WaitForConnectionAsync(stopAcceptingToken.Token);
                connectionTasks.Add(OnConnection(pipe));
                for (int i = 0; i < connectionTasks.Count; i++)
                {
                    if (connectionTasks[i].IsCompleted) {
                        Task connectionTask = connectionTasks[i];
                        connectionTasks.RemoveAt(i);
                        connectionTask.Wait();
                    }
                }
            }
            foreach (Task task in connectionTasks)
            {
                task.Wait();
            }
            Logger.Log("Stopped accepting pipe connections.");
        }

        private async Task OnConnection(NamedPipeServerStream pipe)
        {
            Logger.Log("Interface attempting to connect.", LogLevel.DEBUG);
            byte[] headerBuffer = new byte[sizeof(uint) * 2];
            int headerFill = 0;
            CancellationTokenSource headerCancel = new CancellationTokenSource(CONNECT_TIMEOUT);
            try
            {
                int readLength;
                do
                {
                    readLength = await pipe.ReadAsync(headerBuffer, headerFill, headerBuffer.Length - headerFill, headerCancel.Token);
                    headerFill += readLength;
                } while (readLength != 0 && headerFill != headerBuffer.Length);
            }
            catch (AggregateException e)
            {
                e.Handle((exception) => exception is TaskCanceledException);
                Logger.Log($"Interface did not send header data within {CONNECT_TIMEOUT}ms.", LogLevel.DEBUG);
            } finally {
                await pipe.DisposeAsync();
                headerCancel.Dispose();
            }
            if (headerFill != headerBuffer.Length) {
                Logger.Log($"Interface failed to send header data.", LogLevel.DEBUG);
                return;
            }

            CommunicableType comType;
            uint bodyLength;
            RequestHeader.Decode(headerBuffer, out comType, out bodyLength); //TODO do exception check.

            byte[] bodyBuffer = new byte[bodyLength];
            int bodyFill = 0;
            CancellationTokenSource bodyCancel = new CancellationTokenSource(CONNECT_TIMEOUT);
            try
            {
                int readLength = 0;
                do
                {
                    readLength = await pipe.ReadAsync(bodyBuffer, bodyFill, bodyBuffer.Length - bodyFill, bodyCancel.Token);
                    bodyFill += readLength;
                } while (readLength != 0 && bodyFill != headerBuffer.Length);
            }
            catch (AggregateException e)
            {
                e.Handle((exception) => exception is TaskCanceledException);
                Logger.Log($"Interface failed to send body data within {CONNECT_TIMEOUT}.", LogLevel.DEBUG);
            } finally {
                await pipe.DisposeAsync();
                bodyCancel.Dispose();
            }
            if (bodyFill != bodyBuffer.Length) {
                Logger.Log($"Interface failed to send body data.", LogLevel.DEBUG);
                return;
            }

            ConnectRequest request = JsonSerializer.Deserialize<ConnectRequest>(bodyBuffer);
            ConnectResponse response = new ConnectResponse();
            bool requestAccepted = false;

            if (string.IsNullOrWhiteSpace(request.requestedIdentifier)) {
                response.invalidName = true;
                response.errorMsg = $"The requested identifier \"{request.requestedIdentifier}\" is null or whitespace.";
                Logger.Log(response.errorMsg, LogLevel.DEBUG);
            } else if (pipes.ContainsKey(request.requestedIdentifier)) {
                response.invalidName = true;
                response.nameTaken = true;
                response.errorMsg = $"Interface requested identifier \"{request.requestedIdentifier}\" is taken.";
                Logger.Log(response.errorMsg, LogLevel.DEBUG);
            } else {
                requestAccepted = true;
                response.identifier = request.requestedIdentifier;
            }
            CancellationTokenSource cancelResponse = new CancellationTokenSource(CONNECT_TIMEOUT);
            try
            {
                await pipe.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(response), cancelResponse.Token);                    
            }
            catch (AggregateException e)
            {
                e.Handle((exception) => exception is TaskCanceledException);
                Logger.Log($"Interface did not receive response within {CONNECT_TIMEOUT}ms.", LogLevel.DEBUG);
            }
            if (!requestAccepted) {
                cancelResponse.Dispose();
                await pipe.DisposeAsync();
                Logger.Log($"Interface failed to connect.");
                return;
            }

            Logger.Log($"Interface \"{response.identifier}\" connected.");
            pipes[request.requestedIdentifier] = (pipe, Listen(response.identifier, pipe));
        }

        private async Task Listen(string identifier, NamedPipeServerStream pipe)
        {
            Logger.Log($"Started listening to interface \"{identifier}\".", LogLevel.DEBUG);
            byte[] buffer = new byte[1024];
            byte[] headerBuffer = new byte[sizeof(uint) * 2];
            byte[] bodyBuffer = null;

            int bodyFill = 0;
            int headerFill = 0;

            int readLength = 0;

            CommunicableType? comType = null;
            while ((readLength = await pipe.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < readLength; i++)
                {
                    if (bodyBuffer == null)
                    {
                        headerBuffer[headerFill] = buffer[i];
                        headerFill++;
                        if (headerFill == headerBuffer.Length)
                        {
                            uint length;
                            CommunicableType type;
                            RequestHeader.Decode(headerBuffer, out type, out length);
                            bodyBuffer = new byte[length];
                            headerFill = 0;
                            comType = type;
                        }
                    }
                    else
                    {
                        bodyBuffer[bodyFill] = buffer[i];
                        bodyFill++;
                        if (bodyFill == bodyBuffer.Length)
                        {
                            RequestQueue.Add((identifier, comType.Value, bodyBuffer));
                            bodyFill = 0;
                            bodyBuffer = null;
                        }
                    }
                }
            }
            Logger.Log($"Pipe for interface \"{identifier}\" has closed.", LogLevel.DEBUG);
            (NamedPipeServerStream, Task) removedPipe;
            pipes.TryRemove(identifier, out removedPipe);
            await removedPipe.Item1.DisposeAsync();
            Logger.Log($"Stopped listening to interface \"{identifier}\".", LogLevel.DEBUG);
        }
    }
}