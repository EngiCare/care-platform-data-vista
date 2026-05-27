// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using CarePlatform.Data.CPRS;


namespace CarePlatform.Data.VistA
{
    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 4096;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        public MemoryStream ms = new MemoryStream();

        public bool PostInitialRead { get; set; }
    }

    public class VistaRemoteLinkException : ApplicationException
    {
        public VistaRemoteLinkException(string message) : base(message)
        { }
    }

    public class VistaConnection
    {
        private readonly ILogger _logger = ECConfiguration.LoggerFactory?.CreateLogger<VistaConnection>();

        /// <summary>
        /// Shared cross-tier log category. Set
        /// <c>"Logging:LogLevel:VistaRpcTrace": "Trace"</c> in appsettings to
        /// capture every outbound RPC name + parameter list and the full raw
        /// VistA response — the data needed to feed real CPRS sessions back
        /// into the mock RPC server for test-data and parser fixture work.
        /// </summary>
        private static readonly ILogger? _rpcTrace =
            ECConfiguration.LoggerFactory?.CreateLogger("VistaRpcTrace");

        public Dictionary<string, bool> IssedBulletin = new Dictionary<string, bool>();

        const int CONNECTION_TIMEOUT = 30000;
        const int READ_TIMEOUT = 60000;
        const int DEFAULT_PORT = 9200;

        private string HostName;
        private int Port;

        private Socket Socket;

        public DateTime LastUsed { get; private set; }
        public string SiteId { get; private set; }
        public string Uid { get; set; }
        public VistaAccount Account { get; private set; }

               
        public bool IsConnected { get; private set; }
        private bool IsConnecting { get; set; }
        private int ConnectTimeout { get; set; }
        private  int ReadTimeout { get; set; }


        private readonly object socketLock = new object();
        private string QueryResponse { get; set; }
        public bool isErrorMsg { get; private set; }

        private AutoResetEvent connectDone = new AutoResetEvent(false);
        private AutoResetEvent sendDone = new AutoResetEvent(false);
        private AutoResetEvent receiveDone = new AutoResetEvent(false);
        private volatile Exception? _connectException;
        private volatile Exception? _socketException;


        public VistaConnection(string hostname, string port, string siteId)
        {
            Account = new VistaAccount(this);
            this.ConnectTimeout = CONNECTION_TIMEOUT;
            this.ReadTimeout = READ_TIMEOUT;

            this.HostName = hostname;
            this.Port = string.IsNullOrEmpty(port) ? DEFAULT_PORT : int.Parse(port);

            this.SiteId = siteId;

            connectDone = new AutoResetEvent(false);
            sendDone = new AutoResetEvent(false);
            receiveDone = new AutoResetEvent(false);
        }


        public async Task connect()
        {
            if (String.IsNullOrEmpty(this.HostName))
            {
                throw new ArgumentNullException("No provider (hostname)");
            }

            //Console.WriteLine("[Trace] " + String.Format("Connecting to a Vista system"));

            IsConnecting = true;
            this.IsConnected = false;

            //Console.WriteLine("[Trace] " + String.Format("Connecting to VistA System: {0}:{1} - {2}", dataSource.Provider, dataSource.Port, dataSource.Protocol));


            //Who am I?
            IPHostEntry hostEntry = Dns.GetHostEntry("localhost");
            IPAddress myIP = (IPAddress)Dns.GetHostEntry(hostEntry.HostName).AddressList[0];


            // Get the VistA IP and connect
            // See if the host is an IP adress or FQDN
            IPAddress vistaIP = null;
            if (!IPAddress.TryParse(this.HostName, out vistaIP))
            {
                try
                {
                    vistaIP = (IPAddress)Dns.GetHostEntry(this.HostName).AddressList[0];
                }
                catch (SocketException se)
                {
                    throw new ApplicationException("No route to host " + this.HostName, se);
                }
            }
            IPEndPoint vistaEndPoint = new IPEndPoint(vistaIP, this.Port);
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Console.WriteLine("[Trace] " + String.Format("Setting socket ReceiveTimeout to: {0}", cxn.ConnectTimeout));

            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, this.ConnectTimeout);

            // Connect to the remote endpoint.  
            //Console.WriteLine("[Trace] Attempting to connect to " + vistaEndPoint.Address);

            _connectException = null;
            this.Socket.BeginConnect(vistaEndPoint,
                new AsyncCallback(ConnectCallback), this.Socket);
            connectDone.WaitOne();

            // Check for connection errors captured in the callback
            if (_connectException != null)
                throw _connectException;

            if (!this.Socket.Connected)
                throw new ApplicationException("Unable to connect to " + this.HostName + ", port " + this.Port);
            

            //Build the request message
            int COUNT_WIDTH = 3;
            string request = "[XWB]10" + COUNT_WIDTH.ToString() + "04\nTCPConnect50" +
                                StringUtils.strPack(myIP.ToString(), COUNT_WIDTH) +
                                "f0" + StringUtils.strPack(Convert.ToString(0), COUNT_WIDTH) + "f0" +
                                StringUtils.strPack(hostEntry.HostName, COUNT_WIDTH) + "f\x0004";

            //Console.WriteLine("[Trace] " + "Preparing request message");


            string reply = "";
            try
            {
                //Console.WriteLine("[Trace] " + "Sending query... ");
                reply = await this.query(request);
                //Console.WriteLine("[Trace] " + "received reply" + reply);
            }
            catch (SocketException se)
            {
                throw new ApplicationException("No VistA listener at " + this.HostName + ", port " + this.Port, se);
            }
            if (reply != "accept")
            {
                this.Socket.Close();
                throw new Exception("Unaccepted by " + this.HostName);
            }

            //Console.WriteLine("[Trace] " + String.Format("Setting socket ReceiveTimeout to: {0}", cxn.ReadTimeout));
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, this.ReadTimeout);

            request = "[XWB]11302\x00010\rXUS INTRO MSG54f\x0004";
            //Console.WriteLine("[Trace] " + "Sending intro message request... ");
            reply = await this.query(request);
            //Console.WriteLine("[Trace] " + "Received intro message: \n" + reply);
            
            //Console.WriteLine("[Trace] " + String.Format("Connectd to VistA System: {0}", reply));


            this.IsConnected = true;

            //IsTestSource = isTestSystem();
            IsConnecting = false;
            this.LastUsed = DateTime.Now;
        }


        public async Task<User> authorizedConnect(VistaCredentials credentials, AbstractPermission permission, DataSource validationDataSource)
        {
            try
            {
                //Console.WriteLine("[Trace] " + "attempting authorization with AC\\VC... ");
                //Console.WriteLine("[Trace] " + "connecting... ");
                await connect();
                //Console.WriteLine("[Trace] " + "connected.");
                //Console.WriteLine("[Trace] " + "logging in... ");
                var user = await Account.authenticateAndAuthorize(credentials, permission, validationDataSource);
                //Console.WriteLine("[Trace] " + "logged in.");
                return user;
            }
            catch (Exception ex)
            {
                if (!(ex is UnauthorizedAccessException))
                    _logger?.LogError(ex, "Error during authorized connect");

                throw ex;
            }
        }

        public async Task<User> authorizedConnectSSO(VistaCredentials credentials, AbstractPermission permission, DataSource validationDataSource)
        {
            try
            {
                _logger?.LogTrace("Attempting authorization via SSO");
                await connect();
                return await Account.authenticateAndAuthorizeSSO(credentials, permission, validationDataSource);
            }
            catch (Exception ex)
            {
                if (!(ex is UnauthorizedAccessException))
                {
                    _logger?.LogError(ex, "Error during SSO authorized connect");
                }
                throw ex;
            }
        }


        public async Task<string> remoteQuery(string siteId, VistaQuery vq, AbstractPermission context = null, bool bUpdateDateTime = true)
        {
            var remoteKey = await ScheduleRemoteQuery(siteId, vq, context, bUpdateDateTime);

            for (int i = 0; i < 20; i++)
            {
                if (await RemoteQueryCompletionCheck(remoteKey, context, bUpdateDateTime))
                    break;

                Thread.Sleep(250);
            }

            return await RemoteQueryGetData(remoteKey, context, bUpdateDateTime);
        }

        public async Task<string> RemoteQueryGetData(string remoteKey, AbstractPermission context = null, bool bUpdateDateTime = true )
        {
            VistaQuery retrieveData = new VistaQuery("XWB REMOTE GETDATA");
            retrieveData.addParameter(VistaQuery.LITERAL, remoteKey);
            string request = retrieveData.buildMessage();
            _logger?.LogTrace("Request: {Request}", request);
            return await query(request, context, bUpdateDateTime);
        }

        public async Task<bool> RemoteQueryCompletionCheck(string remoteKey, AbstractPermission context = null, bool bUpdateDateTime = true)
        {
            VistaQuery queryStatus = new VistaQuery("XWB REMOTE STATUS CHECK");
            queryStatus.addParameter(VistaQuery.LITERAL, remoteKey);
            string request = queryStatus.buildMessage();
            _logger?.LogTrace("Request: {Request}", request);
            var statusResponse = await query(request, context, bUpdateDateTime);
            _logger?.LogTrace("Remote status response: {Response}", statusResponse);

            var returncode = statusResponse.Split('^');
            if (returncode[0] == "1")
                return true;
            else if (returncode[0] == "-1")
            {
                _logger?.LogTrace("Status response: '{Response}'", statusResponse);
                    throw new ApplicationException(string.Format("Remote response error: {0} - {1}.  Response: {2}", SiteId, remoteKey, statusResponse));
            } 

            return false;
        }


        public async Task<string> ScheduleRemoteQuery(string siteId, VistaQuery vq, AbstractPermission context = null, bool bUpdateDateTime = true)
        {
            string remoteKey = "";
            // The remote query returns a handle that you need to call back against to 
            // retrieve the final results.  
            VistaQuery remoteVq = new VistaQuery("XWB REMOTE RPC");
            remoteVq.addParameter(VistaQuery.LITERAL, siteId);
            remoteVq.addParameter(VistaQuery.LITERAL, vq.RpcName);
            foreach (var param in vq.Parameters)
            {
                remoteVq.Parameters.Add(param);
            }

            string request = remoteVq.buildMessage();
            _logger?.LogTrace("Request: {Request}", request);
            remoteKey = await query(request, context, bUpdateDateTime);
            remoteKey = remoteKey.Trim();

            // check to see if there's an error where the systems are not linked.
            if (remoteKey.Contains("^") && remoteKey.Split('^').Length > 1 && remoteKey.Split('^')[1].Trim() == "Link not setup")
                    throw new VistaRemoteLinkException(string.Format("Link not setup error: {0}.  Response: {1}", SiteId, remoteKey));

            _logger?.LogTrace("Remote key: {RemoteKey}", remoteKey);
            return remoteKey;
        }

        public async Task<string> query(VistaQuery vq, AbstractPermission context = null, bool bUpdateDateTime = true)
        {
            string request = vq.buildMessage();
            _logger?.LogTrace("Request: {Request}", request);

            // Cross-tier RPC trace: name + each parameter value, gated on Trace.
            // Pair this with the post-call response trace below so a complete
            // call/response record can be reconstructed from the log.
            if (_rpcTrace != null && _rpcTrace.IsEnabled(LogLevel.Trace))
            {
                _rpcTrace.LogTrace(
                    "[data] {SiteId} {RpcName}\n--params--\n{Params}\n--end-params--",
                    SiteId, vq.RpcName, FormatVistaQueryParams(vq));
            }

            var reply = await query(request, context, bUpdateDateTime);

            if (_rpcTrace != null && _rpcTrace.IsEnabled(LogLevel.Trace))
            {
                _rpcTrace.LogTrace(
                    "[data] {SiteId} {RpcName} -- response ({Length} bytes) --\n{Response}\n--end-response--",
                    SiteId, vq.RpcName, reply?.Length ?? 0, reply ?? "");
            }

            return reply;
        }

        private static string FormatVistaQueryParams(VistaQuery vq)
        {
            if (vq.Parameters == null || vq.Parameters.Count == 0) return "(none)";
            var sb = new StringBuilder();
            for (int i = 0; i < vq.Parameters.Count; i++)
            {
                var p = (VistaQuery.Parameter)vq.Parameters[i];
                sb.Append('[').Append(i).Append("] type=").Append(p.Type)
                  .Append(" value=").Append(p.Value ?? "");
                if (p.List != null && p.List.Count > 0)
                {
                    sb.Append(" list={ ");
                    for (int j = 0; j < p.List.Count; j++)
                    {
                        var entry = p.List[j];
                        sb.Append(entry.Key).Append('=').Append(entry.Value).Append("; ");
                    }
                    sb.Append('}');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private async Task<string> query(string request, AbstractPermission context = null, bool bUpdateDateTime = true)
        {
            if (!IsConnecting && !IsConnected)
            {
                throw new ApplicationException("Not connected");
            }
            AbstractPermission currentContext = null;
            if (context != null && context.Name != this.Account.PrimaryPermission.Name)
            {
                currentContext = this.Account.PrimaryPermission;
                await ((VistaAccount)this.Account).setContext(context);
            }

            try
            {

                string reply = "";
                bool wasError = false;

                //this lock prevents the keep alive thread from coming in here while we're on the socket
                _logger?.LogTrace("Locking... {Request}", request);
                lock (socketLock)
                {
                    try
                    {
                        sendDone.Reset();
                        receiveDone.Reset();
                        _socketException = null;
                        isErrorMsg = false;

                        // Send test data to the remote device.
                        _logger?.LogTrace("Sending query...");
                        Send(Socket, request);
                        if (!sendDone.WaitOne(ReadTimeout))
                        {
                            _logger?.LogError("Send timed out after {Timeout}ms", ReadTimeout);
                            throw new ApplicationException($"Send to VistA timed out after {ReadTimeout}ms");
                        }
                        if (_socketException != null)
                            throw new ApplicationException("Error sending to VistA", _socketException);
                        _logger?.LogTrace("Query sent.");

                        // Receive the response from the remote device.
                        _logger?.LogTrace("Receiving response...");
                        Receive(Socket);
                        _logger?.LogTrace("Waiting for response to complete...");
                        if (!receiveDone.WaitOne(ReadTimeout))
                        {
                            _logger?.LogError("Receive timed out after {Timeout}ms", ReadTimeout);
                            throw new ApplicationException($"Receive from VistA timed out after {ReadTimeout}ms");
                        }
                        if (_socketException != null)
                            throw new ApplicationException("Error receiving from VistA", _socketException);
                        _logger?.LogTrace("Response received.");

                        reply = QueryResponse;
                        wasError = isErrorMsg;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error during socket query, releasing lock");
                        throw;
                    }
                }  //done reading - let's unlock
                _logger?.LogTrace("Unlocked: {Reply}", reply);


                if (currentContext != null)
                {
                    await ((VistaAccount)this.Account).setContext(currentContext);
                }

                if (wasError || reply.Contains("M  ERROR"))
                {
                    throw new ApplicationException("M ERROR!! " + reply);
                }

                if (bUpdateDateTime)
                {
                    this.LastUsed = DateTime.Now;
                }

                return reply;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            _logger?.LogTrace("In Connect Callback...");

            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                _logger?.LogTrace("Socket connected to {Endpoint}", client.RemoteEndPoint);

                _logger?.LogTrace("Signalling that we're done.");
            }
            catch (SocketException se)
            {
                _connectException = new ApplicationException("No VistA listener at " + this.HostName + ", port " + this.Port, se);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error in connect callback");
                _connectException = e;
            }
            finally
            {
                // Always signal so the caller doesn't hang forever
                connectDone.Set();
            }
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error in send callback");
                _socketException = e;
                sendDone.Set();
            }
        }


        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;
                state.PostInitialRead = false;

                _logger?.LogTrace("Beginning receive...");

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error starting receive");
                _socketException = e;
                receiveDone.Set();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                _logger?.LogTrace("Receive callback...");

                // Retrieve the state object and the client socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                bool PostInitialRead = state.PostInitialRead;

                _logger?.LogTrace("Calling EndReceive...");

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                _logger?.LogTrace("EndReceive called.");

                var thisBatch = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                var endIdx = thisBatch.IndexOf('\x04');

                _logger?.LogTrace("thisBatch: {Batch}", thisBatch);

                _logger?.LogTrace("Processing bytes...");
                if (!PostInitialRead)
                {
                    if (bytesRead == 0)
                        throw new ApplicationException("Timeout waiting for response from VistA");

                    if (endIdx != -1)
                    {
                        thisBatch = thisBatch.Substring(0, endIdx);
                    }
                    if (state.buffer[0] != 0)
                    {
                        thisBatch = thisBatch.Substring(1, state.buffer[0]);
                        isErrorMsg = true;
                    }
                    else if (state.buffer[1] != 0)
                    {
                        thisBatch = thisBatch.Substring(2);
                        isErrorMsg = true;
                    }
                    else
                    {
                        thisBatch = thisBatch.Substring(2);
                    }
                    _logger?.LogTrace("Appending: {Batch}", thisBatch);
                    state.sb.Append(thisBatch);
                    state.PostInitialRead = true;
                }
                else
                {
                    _logger?.LogTrace("endIdx: {EndIdx}", endIdx);

                    state.ms.Write(state.buffer, 0, endIdx!=-1?endIdx:bytesRead);
                }

                if (endIdx != -1)
                {
                    _logger?.LogTrace("Got everything");
                    state.sb.Append(Encoding.ASCII.GetString(state.ms.ToArray()));
                    QueryResponse = state.sb.ToString();
                    _logger?.LogTrace("Query response: {Response}", QueryResponse);
                    receiveDone.Set();
                    _logger?.LogTrace("Returning.");
                    return;
                }
                else
                {
                    _logger?.LogTrace("More data to get");
                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }

                _logger?.LogTrace("Bytes processed.");

            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error in receive callback");
                _socketException = e;
                receiveDone.Set();
            }
        }




        public async Task disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                string msg = "[XWB]10304\x0005#BYE#\x0004";
                msg = await query(msg);
                Socket.Close();
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "Exception disconnecting");
            }
            finally
            {
                IsConnected = false;
            }
        }


        //public string getWelcomeMessage()
        //{

        //    VistaQuery request = buildGetWelcomeMessageRequest();

        //    //Console.WriteLine("[Trace] " + String.Format("Buit welcome message request: {0}", request.RpcName));
        //    string response = (string)query(request);
        //    //Console.WriteLine("[Trace] " + String.Format("Buit welcome message response: {0}", response));
        //    return response;
        //}

        //internal VistaQuery buildGetWelcomeMessageRequest()
        //{
        //    //Console.WriteLine("[Trace] " + String.Format("Building Welcome Message request"));
        //    return new VistaQuery("XUS INTRO MSG");
        //}

        //public bool hasPatch(string patchId)
        //{
        //    VistaQuery request = buildHasPatchRequest(patchId);
        //    string response = (string)query(request);
        //    return (response == "1");
        //}

        //internal VistaQuery buildHasPatchRequest(string patchId)
        //{
        //    VistaQuery vq = new VistaQuery("ORWU PATCH");
        //    vq.addParameter(VistaQuery.LITERAL, patchId);
        //    return vq;
        //}

        

        public async Task heartbeat(bool updateSessionLastUsedTime = false)
        {
            VistaQuery vq = new VistaQuery("XWB IM HERE");
            await this.query(vq, null, updateSessionLastUsedTime);
        }

    }
}

