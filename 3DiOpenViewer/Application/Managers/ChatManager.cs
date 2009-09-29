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
using OpenMetaverse;

namespace OpenViewer.Managers
{
    public class ChatManager: BaseComponent
    {
        private const int maxMessageLength = 300;

        private List<string> messageHistory = new List<string>();

        public ChatManager(Viewer _viewer)
            : base(_viewer, -1)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            lock (messageHistory)
            {
                messageHistory.Clear();
            }
        }

        public override void Cleanup()
        {
            lock (messageHistory)
            {
                messageHistory.Clear();
            }
            base.Cleanup();
        }

        public void Add(string _message, ChatAudibleLevel _audible, ChatType _type, ChatSourceType _sourcetype, string _fromName, UUID _id, UUID _ownerid, Vector3 _position)
        {
            Reference.Log.Debug(_message + " ChatAudibleLevel:" + _audible.ToString() + " ChatType:" + _type.ToString() + " ChatSourceType:" + _sourcetype.ToString() + " FromName:" + _fromName);

            // name.
            string fromName = _fromName + ":";
            lock (messageHistory)
            {
                messageHistory.Add(fromName);
                Reference.Viewer.GuiManager.ChatAddMessage(fromName);
            }

            // message.
            string msg = _message;
            if (_message.Length > maxMessageLength)
            {
                string tail = "・・・";

                msg = _message.Substring(0, maxMessageLength - tail.Length);
                msg += tail;
            }

            // wide-char space -> 2 char space.
            msg = msg.Replace("　", "  ");

            // trim space.
            msg = msg.Trim();

            List<int> colLen = new List<int>();
            int colMaxLen = 16 * 2;
            int lenCounter = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                char c = msg[i];
                if (c < '!' || '~' < c)
                {
                    lenCounter += 2;
                }
                else
                {
                    lenCounter += 1;
                }

                if ((lenCounter >= colMaxLen) || ((i + 1) == msg.Length))
                {
                    colLen.Add(i + 1);
                    lenCounter = 0;
                }
            }

            List<string> msgList = new List<string>();
            for (int i = 0; i < colLen.Count; i++)
            {
                int start = (i > 0 ? colLen[i - 1] : 0);
                int length = (i > 0 ? colLen[i] - colLen[i - 1] : colLen[i]);

                string text = msg.Substring(start, length);
                msgList.Add(text);
            }

            for (int i = 0; i < msgList.Count; i++)
            {
                // message.
                string addMessage = "  " + msgList[i];
                lock (messageHistory)
                {
                    messageHistory.Add(addMessage);
                    Reference.Viewer.GuiManager.ChatAddMessage(addMessage);
                }
            }

            Reference.Viewer.Adapter.CallReceiveMessaged(_id.ToString(), _fromName, _message);
        }

        public void RemoveAll()
        {
            lock (messageHistory)
            {
                messageHistory.Clear();
            }
        }

        public string[] Messages
        {
            get {
                lock (messageHistory)
                {
                    return messageHistory.ToArray();
                }
            }
        }
    }
}
