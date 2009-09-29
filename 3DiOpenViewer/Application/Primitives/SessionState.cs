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
using System.Text;

namespace OpenViewer.Primitives
{
    public enum StreamState
    {
        streamStateUnknown = 0,
        streamStateIdle = 1,
        streamStateConnected = 2,
        streamStateRinging = 3,
    };

	public class SessionState
	{
		public SessionState()
        {
            mErrorStatusCode = 0;
            mMediaStreamState = StreamState.streamStateUnknown;
            mTextStreamState = StreamState.streamStateUnknown;
            mCreateInProgress = false;
            mMediaConnectInProgress = false;
            mVoiceInvitePending = false;
            mTextInvitePending = false;
            mSynthesizedCallerID = false;
            mIsChannel = false;
            mIsSpatial = false;
            mIsP2P = false;
            mIncoming = false;
            mVoiceEnabled = false;
            mReconnect = false;
            mVolumeDirty = false;
            mParticipantsChanged = false;
        }

		public string mHandle;
		public string mGroupHandle;
		public string mSIPURI;
		public string mAlias;
		public string mName;
		public string mAlternateSIPURI;
		public string mHash;
		public string mErrorStatusString;
		public Queue<string> mTextMsgQueue;

        public string mIMSessionID; // uuid
        public string mCallerID; // uuid
        public int mErrorStatusCode;
        public StreamState mMediaStreamState;
        public StreamState mTextStreamState;
        public bool mCreateInProgress;
        public bool mMediaConnectInProgress;
        public bool mVoiceInvitePending;
        public bool mTextInvitePending;
        public bool mSynthesizedCallerID;
        public bool mIsChannel;
        public bool mIsSpatial;
        public bool mIsP2P;
        public bool mIncoming;
        public bool mVoiceEnabled;
        public bool mReconnect;
        public bool mVolumeDirty;
        public bool mParticipantsChanged;
        //participantMap mParticipantsByURI;
        //participantUUIDMap mParticipantsByUUID;
	};
}
