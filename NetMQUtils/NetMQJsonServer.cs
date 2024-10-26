/*
 * Copyright 2024 - 2024 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Monitoring;
using NetMQ.Sockets;
using QuickJSON;

namespace NetMQUtils
{
    // JSON Server (Dealer) communicating in JSON to and from the clients

    public class NetMQJsonServer
    {
        public Action<List<JToken>> Received { get; set; } = null;      // callback, in poller thread, list of JSON messages received
        public Action Accepted { get; set; } = null;     // callback, in poller thread, when a client accepts
        public Action Disconnected { get; set; } = null;     // callback, in poller thread, when a client disconnects
        public bool Running => server != null;

        private DealerSocket server;
        private NetMQPoller poller;
        private NetMQQueue<JToken> queue;
        private NetMQMonitor monitor;

        public bool Init(string host, int socketnumber, string threadname)
        {
            try
            {
                string bindstring = host + ":" + socketnumber.ToStringInvariant();
                server = new DealerSocket();
                server.Bind(bindstring);

                poller = new NetMQPoller(); // this is a thread, which picks up events and send them thru callers

                queue = new NetMQQueue<JToken>();
                queue.ReceiveReady += Queue_ReceiveReady;

                poller.Add(server);
                poller.Add(queue);

                monitor = new NetMQMonitor(server, $"inproc://addr:" + socketnumber.ToStringInvariant(), SocketEvents.All);
                monitor.AttachToPoller(poller);

                server.ReceiveReady += Server_ReceiveReady;
                monitor.Accepted += (s, e) => { Accepted?.Invoke(); };
                monitor.Disconnected += (s, e) => { Disconnected?.Invoke(); };

                poller.RunAsync(threadname);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"NetMQJsonServer exception {ex}");
                server = null;
                return false;
            }
        }

        public void Close()
        {
            if (server != null)
            {
                poller.Remove(queue);
                monitor.DetachFromPoller();         // temperamental.
                monitor.Stop();
                poller.RemoveAndDispose(server);
                poller.Stop();

                //while ( poller.IsRunning) // debug when testing with stopasync()
                //{
                //    System.Diagnostics.Debug.WriteLine("Poller running");
                //    System.Threading.Thread.Sleep(100);
                //}

                server = null;
            }
        }

        // called by poller when messages are available. empty all messages, JSON decode, add to queue and send
        // run in poller thread.
        private void Server_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine($"Server received in {System.Threading.Thread.CurrentThread.Name}");

            List<JToken> messages = new List<JToken>();
            while (server.TryReceiveFrameString(out string clientsend))
            {
                JObject json = JObject.Parse(clientsend);
                if (json != null)
                {
                    messages.Add(json);
                }
                else
                    System.Diagnostics.Trace.WriteLine($"NetMQ Server received but not JSON {clientsend} ");
            }

            if (messages.Count > 0)
                Received?.Invoke(messages);
        }

        // called by poller when something gets into the queue. empty the queue and send the messages
        // run in poller thread.
        private void Queue_ReceiveReady(object sender, NetMQQueueEventArgs<JToken> e)
        {
            while (queue.TryDequeue(out JToken json, new TimeSpan(0, 0, 0, 0, 10)))
            {
                string msg = json.ToString();
                //System.Diagnostics.Debug.WriteLine($"NetMQUtils queued send data in thread {System.Threading.Thread.CurrentThread.Name}: {msg}");
                if (!server.TrySendFrame(msg))      // may not be a friend to get the message, so don't block but just try
                    System.Diagnostics.Debug.WriteLine($"NetMQ Failed to send frame {msg}");
            }
        }

        // thread safe, can be called from any thread.
        public void Send(JToken s)
        {
            queue.Enqueue(s);
        }

    }
}
