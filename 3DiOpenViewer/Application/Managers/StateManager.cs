/*
 * Copyright (c) 2008-2009, 3Di, Inc. (http://3di.jp/) and contributors.
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of 3Di, Inc., nor the name of the 3Di Viewer
 *       "Rei" project, nor the names of its contributors may be used to
 *       endorse or promote products derived from this software without
 *       specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY 3Di, Inc. AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 3Di, Inc. OR THE
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using IrrlichtNETCP;
using OpenMetaverse;

namespace OpenViewer.Managers
{
    /// <summary>
    /// States:
    /// _ENTRY: The default state before the initialization starts
    /// INITIALIZING: The application is starting, but the device and its surrounding infrastructure is not created ->AUTOMATIC
    /// INITIALIZED: The application is in a state capable of loggin in ->AUTOMATIC
    /// LOGIN: Login has started ->PRESS LOGIN
    /// CONNECTED: Login has ended and the application is connected to a simulator ->AUTOMATIC
    /// TELEPORTING: A teleport was requested and is processed ->PRESS TELEPORT
    /// CLOSING: Logout was requested ->PRESS LOGOUT
    /// EXITING: The application was requested to terminate ->CLOSE/MOVE PAGE
    /// </summary>
    public enum State { _ENTRY, INITIALIZING, INITIALIZED, LOGIN, DOWNLOADING, CONNECTED, TELEPORT_REQUESTED, TELEPORTING, CLOSING, ERROR, EXITING };

    public class StateManager : BaseComponent
    {
        private State memState = State._ENTRY;

        #region Events
        public delegate void ChangedListener(State _state);
        public event ChangedListener OnChanged;
        #endregion

        #region Properties
        private State state = State._ENTRY;
        private Object sync_state = new Object();
        public State State { get { return (state); } set { StateExit(); state = value; StateEntry(); } }
        public Object SyncStat { get { return sync_state; } }
        #endregion

        public StateManager(Viewer viewer)
            : base(viewer, -1)
        {
        }

        public override void Update(uint frame)
        {
            lock (SyncStat)
            {
                if (memState != state)
                {
                    memState = state;

                    if (OnChanged != null)
                    {
                        OnChanged(memState);

                        if (Reference.Viewer.Adapter != null)
                        {
                            // When this function called, browser was already dead.
                            if (memState != State.EXITING)
                            {
                                Reference.Viewer.Adapter.CallStateChanged((int)memState);
                            }
                        }
                    }
                }
            }
            base.Update(frame);
        }

        public override void Initialize()
        {
            state = State.INITIALIZING;
            base.Initialize();
        }

        #region Public Interface
        #endregion

        #region Internals
        // Currently state transition events are not fired
        private void StateEntry()
        {
        }

        private void StateExit()
        {
        }
        #endregion
    }
}
