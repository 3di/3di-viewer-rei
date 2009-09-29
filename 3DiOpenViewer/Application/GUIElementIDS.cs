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

namespace OpenViewer
{
public enum GUIElementIDS
{
    INIT_BACKGROUND = 900,

    // Login window
    LOGIN_WINDOW = 1000,
    LOGIN_BACKGROUND,
    LOGIN_PROGRESS,

    LOGIN_SERVERURI_TEXT,
    LOGIN_FIRSTNAME_TEXT,
    LOGIN_LASTNAME_TEXT,
    LOGIN_PASSWORD_TEXT,

    LOGIN_SERVERURI,
    LOGIN_FIRSTNAME,
    LOGIN_LASTNAME,
    LOGIN_PASSWORD,
    LOGIN_LOGINBUTTON,

    // Menu
    MENU_BACKGROUND = 2000,
    MENU_LOCATIONTEXT,
    MENU_SIMNAME,
    MENU_ITEMBASE = 2100,

    // Chat window
    CHAT_WINDOW = 3000,
    CHAT_MSGLIST,
    CHAT_ENTERMSG,
    CHAT_SENDBUTTON,

    // Teleport window
    TELEPORT_WINDOW = 4000,
    TELEPORT_REGIONNAME,
    TELEPORT_X,
    TELEPORT_Y,
    TELEPORT_Z,
    TELEPORT_TELEPORTBUTTON,

    // Teleport Failed window
    TELEPORT_FAILED_WINDOW = 4100,
    TELEPORT_BACKBUTTON,

    // Setting window
    SETTING_WINDOW = 5000,
    SETTING_TAB1_BUTTON00,
    SETTING_TAB2_BUTTON00,

    // Help window
    HELP_WINDOW = 6000,

    // Debug window
    DEBUG_WINDOW = 7000,
    DEBUG_DTAB_CONTROLLER = 5100,

    // Message window
    GENERAL_MESSAGE_WINDOW = 8000,
    GENERAL_MESSAGE_BUTTON00,
    GENERAL_MESSAGE_BUTTON01,

    ERROR_DIALOG_WINDOW = 9900,
    ERROR_DIALOG_MESSAGE = 9990,

    // Voice accept window
    VOICEACCEPT_WINDOW = 12000,
    VOICEACCEPT_OKBUTTON,
    VOICEACCEPT_CANCELBUTTON,

    STANDUP_BUTTON = 13000,
}
}
