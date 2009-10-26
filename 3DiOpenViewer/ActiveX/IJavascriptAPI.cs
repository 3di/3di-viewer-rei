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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;

using OpenViewer;
using System.Threading;

namespace OpenViewerAX
{
    /* Javascript API COM interfaces */
    // ISystemStart is the COM visible interface that the ActiveX control must implement
    // This interface must contain the signatures of all functions and properties
    // that are accessible FROM Javascript
    [ComVisible(true)]
    [Guid("21C194C7-4BF1-42C6-9BB2-4BB7ADFFCD3E")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IJavaScriptApi
    {
        int QueryVersion();
    }

    // ISystemStartEvent is the COM visible interface that describes the events that can be
    // dispatched TO javascript.
    // Normally these are implemented in javascript in the following manner:
    // <script language="javascript" for="[activexelementid]" event="[functionname(parameterlist)]" type="text/jscript">
    [ComVisible(true)]
    [Guid("BF4C96FA-074C-477f-91E7-206C5A7889F4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IJavaScriptApiEvent
    {
        [DispId(0)]
        void OnDebugMessage(string _message);

        [DispId(20)]
        void OnTouched(string _uuid);

        [DispId(40)]
        void OnReceivedMessage(string _uuid, string _avatarName, string _message);

        [DispId(41)]
        void OnReceivedInstantMessage(string _uuid, string _avatarName, string _message);

        [DispId(50)]
        void OnTeleport(string _regionName, int _x, int _y, int _z);

        [DispId(51)]
        void OnTeleported(string _uuid, string _avatar, int _x, int _y, int _z);

        [DispId(60)]
        void OnOpenWindow(string _target, string _uri);

        [DispId(70)]
        void OnAvatarPicked(string _uuid);

        [DispId(71)]
        void OnAnimationEnd(string _animationName);

        [DispId(80)]
        void OnStateChanged(int _state);

        [DispId(81)]
        void OnImageLoaded(string _assetUUID);

        [DispId(122)]
        void OnDispatch(string _action, string _message);
    }
}