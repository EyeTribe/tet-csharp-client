/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EyeTribe.ClientSdk.Data;
using EyeTribe.ClientSdk.Response;

namespace EyeTribe.ClientSdk
{
    /// <summary>
    /// This singleton is the main entry point of the EyeTribe C# SDK. It exposes all routines associated to gaze control.
    /// <para>
    /// Using this class a developer can 'calibrate' an eye tracking setup and attach listeners to receive live data streams
    /// of <see cref="EyeTribe.ClientSdk.Data.GazeData"/> updates.
    /// <para>
    /// Note that this is a thin wrapper class. Core SDK implementation can be found in <see cref="EyeTribe.ClientSdk.GazeManagerCore"/>.
    /// </summary>
    public class GazeManager : GazeManagerCore
    {
        #region Constructor

        private GazeManager() : base()
        {
        }

        #endregion

        #region Get/Set

        public static GazeManager Instance
        {
            get { return Holder.INSTANCE; }
        }

        private class Holder
        {
            static Holder() { }
            //thread-safe initialization on demand
            internal static readonly GazeManager INSTANCE = new GazeManager();
        }

        #endregion

        #region Methods

        internal override GazeApiManager CreateApiManager(IGazeApiReponseListener responseListener, IGazeApiConnectionListener connectionListener)
        {
            return new GazeApiManager(responseListener, connectionListener);
        }

        #endregion
    }
}