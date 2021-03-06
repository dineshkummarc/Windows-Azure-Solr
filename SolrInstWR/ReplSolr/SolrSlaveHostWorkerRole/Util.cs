﻿#region Copyright Notice
/*
Copyright © Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SolrSlaveHostWorkerRole
{
	public class Util
	{
        public static string GetMasterUrl()
        {
            HelperLib.RoleInfoDataSource rids = new HelperLib.RoleInfoDataSource();
            int numTries = 100;

            while (--numTries > 0) // try multiple times since master may be initializing
            {
                try
                {
                    IPEndPoint endpoint = rids.GetMasterEndpoint().IPEndpoint;
                    
                    if (!CheckEndpoint(endpoint))
                        throw new ApplicationException("Master not ready");

                    string masterUrl = "http://" + endpoint.ToString() + "/solr/";
                    return masterUrl;
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            }

            throw new ApplicationException("Master not ready");
        }

        private static bool CheckEndpoint(IPEndPoint solrEndpoint)
        {
            var valid = false;
            using (var s = new Socket(solrEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    s.Connect(solrEndpoint);
                    if (s.Connected)
                    {
                        valid = true;
                        s.Disconnect(true);
                    }
                    else
                    {
                        valid = false;
                    }
                }
                catch
                {
                    valid = false;
                }
            }

            return valid;
        }
    }
}