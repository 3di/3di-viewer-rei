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
using System.Runtime.InteropServices;

namespace OpenViewer.Managers
{
    public enum Cursors {NORMAL, HAND, CHAIR};
    
    public enum LoginType {AUTO, CLICK, MANUAL, NONE};
    public struct LoginInfo
    {
        public string URI;
        public string FirstName;
        public string LastName;
        public string Password;
        public LoginType LoginMode;

        public bool ShowURI;
        public bool ShowAccount;

        public LoginInfo(string u, string f, string l, string p, string m, bool su, bool sa)
        {
            URI = u;
            FirstName = f;
            LastName = l;
            Password = p;
            LoginMode = LoginType.NONE;
            ShowURI = su;
            ShowAccount = sa;

            LoginMode = GetLoginType(m);
        }

        private LoginType GetLoginType(string _typeString)
        {
            LoginType type = LoginType.NONE;

            if (_typeString != string.Empty)
                _typeString = _typeString.ToLower();

            switch (_typeString)
            {
                case "auto":
                    type = LoginType.AUTO;
                    break;

                case "click":
                    type = LoginType.CLICK;
                    break;

                case "manual":
                    type = LoginType.MANUAL;
                    break;

                default :
                    type = LoginType.NONE;
                    break;
            }

            return type;
        }
    }

    public class GuiManager : BaseComponent
    {
        private const int BUTTON_DEFULT_HEIGHT = 32;

        #region Properties

        // ******************** INIT
        private Texture initWindowBackground = null;

        // ******************** LOGIN
        private Texture loginWindowBackground = null;

        // ******************** TELEPORT
        private Texture teleportWindowBackground = null;
        public Texture TeleportWindowBackground { get { return teleportWindowBackground; } }

        private List<int> removeGUI = new List<int>();
        private bool loginFailed = false;
        public bool LoginFailed { get { return (loginFailed); } set { loginFailed = value; } }
        private string loginMessage = "";
        public string LoginMessage { get { return (loginMessage); } set { loginMessage = value; } }

        private LoginInfo loginInfo;
        public LoginInfo LoginInfo { get { return loginInfo; } }

        private int loginProgress = 0;

        public bool IsActive { get { return false; } }

        private bool focused = false;
        public bool Focused { get { return(focused); } }

        public bool IsShowCursor { get; set; }

        #endregion

        #region Private elements
        // Login window
        private GUIEditBox loginServerURI = null;
        private GUIEditBox loginFirstName = null;
        private GUIEditBox loginLastName = null;
        private GUIEditBox loginPassword = null;

        // Chat window
        private GUIListBox chatBoxMessageList;
        private GUIEditBox chatBoxInput;

        //Teleport window
        private GUIEditBox teleportRegionName;
        private GUIEditBox teleportX;
        private GUIEditBox teleportY;
        private GUIEditBox teleportZ;

        // Setting window
        private GUITabControl debugTab;
        private GUIListBox settingTab1SkyQualityList;
        private GUIListBox settingTab1SeaQualityList;
        private GUIListBox settingTab1Locale;
        private GUIEditBox settingTab2CacheSize;
        private Dictionary<int, GUIListBox> debugListBoxList = null;

        // Cursors
        private Texture cursorImageNormal;
        private Texture cursorImageHand;
        private Texture cursorImageChair;
        public Texture currentCursorImage;

        // GUIElements
        private Texture closeButton;
        #endregion

        private GUIElement parentElement;
        private System.Timers.Timer timer = new System.Timers.Timer(1000 * 2);
        private System.Timers.Timer blinkTimer = new System.Timers.Timer(500);

        private bool isVisbleIcon = true;
        private bool isChacheDeleteMessage = false;
        private GUIImage imageChat;
        private GUIButton imageChair;
        private Position2D chatIconPosition;
        private int chatRange = 1;

        public GuiManager(Viewer viewer)
            : base(viewer, -1)
        {
            // Cursors stay during the whole lifecycle of this manager
            cursorImageNormal = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\textures\cursor-arrow.png");
            cursorImageHand = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\textures\cursor-hand.png");
            cursorImageChair = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\textures\cursor-chair.png");
            currentCursorImage = cursorImageNormal;
            Reference.Viewer.CursolOffset = new Position2D(-1, cursorImageNormal.OriginalSize.Height / 2 + 2);

            closeButton = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_close.png");

            //chatIconPosition = new Position2D(Reference.Viewer.Width - 48, Reference.Viewer.Height - 48);
#if YK_REMOVE_HELP
            chatIconPosition = new Position2D(Reference.Viewer.Width - 40 * 5, 4);
#else
            chatIconPosition = new Position2D(Reference.Viewer.Width - 40 * 4, 4);
#endif
            imageChat = Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\menu\menu_chat.png"), chatIconPosition, true, parentElement, -1, "");
            imageChat.Visible = false;

            Texture chairTexture = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\textures\stand_icon_master.png");
            imageChair = Reference.GUIEnvironment.AddButton(
                new Rect(new Position2D(32, Reference.Viewer.Height - chairTexture.OriginalSize.Height - 32), new Dimension2D(chairTexture.OriginalSize.Width, chairTexture.OriginalSize.Height)),
                parentElement,
                (int)GUIElementIDS.STANDUP_BUTTON, "");
            imageChair.SetImage(chairTexture);
            imageChair.UseAlphaChannel = true;
            imageChair.Visible = false;

#if MANAGED_D3D
            //GUIImage image = Reference.GUIEnvironment.AddImage(Reference.Viewer.VideoTexture, new Position2D(), true, parentElement, -1, "");
#endif

            // timer.
            timer.Elapsed += FocuseLostTimer;
            timer.Enabled = false;

            blinkTimer.Elapsed += BlinkEffectTimer;
            blinkTimer.Enabled = true;
        }

        public override void Initialize()
        {
            parentElement = Reference.GUIEnvironment.RootElement;

            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.ActiveBorder, new Color(0, 204, 204, 204));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.ActiveCaption, Color.Black);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.AppWorkspace, Color.Red);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.ButtonText, Color.Black);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.Count, Color.White);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.DarkShadow3D, new Color(128, 0, 0, 0));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.Face3D, new Color(128, 204, 204, 204));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.GrayText, Color.Gray);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.HighLight, new Color(191, 0, 0, 0));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.HighLight3D, new Color(32, 255, 255, 255));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.HighLightText, new Color(32, 255, 255, 255));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.InactiveBorder, Color.Red);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.InactiveCaption, Color.Red);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.Light3D, Color.Gray);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.Scrollbar, Color.White);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.Shadow3D, new Color(240, 204, 204, 204));
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.ToolTip, Color.Black);
            Reference.GUIEnvironment.Skin.SetColor(GuiDefaultColor.Window, new Color(191, 95, 95, 95));

            blinkTimer.Enabled = true;

            isChacheDeleteMessage = false;
        }

        public override void Cleanup()
        {
            if (imageChat != null)
                imageChat.Visible = false;

            if (imageChair != null)
                imageChair.Visible = false;

            // Background textures need not be disposed in cleanup
            if (parentElement != null)
            {
                try
                {
                    try
                    {
                        parentElement.Remove();
                    }
                    catch { }
                }
                finally
                {
                    parentElement = null;
                }
            }
            timer.Enabled = false;
            blinkTimer.Enabled = false;

        }

        public override void Update(uint frame)
        {
            if (loginFailed)
            {
                lock (removeGUI)
                {
                    removeGUI.Add((int)GUIElementIDS.LOGIN_BACKGROUND);
                    removeGUI.Add((int)GUIElementIDS.LOGIN_PROGRESS);
                }
            }

            lock (removeGUI)
            {
                if (loginFailed || (removeGUI.Count != 0 && Reference.Viewer.StateManager.State == State.CONNECTED))
                {
                    foreach (int i in removeGUI)
                    {
                        Remove(i);
                    }
                    removeGUI.Clear();
                }
            }
            if (loginFailed)
            {
                Texture img = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\background\error_background.png");
                Dimension2D imageSize = img.OriginalSize;

                // Show error dialog.
                GUIImage menu = Reference.GUIEnvironment.AddImage(img,
                                                                        new Position2D(Reference.Viewer.Width / 2 - imageSize.Width / 2,
                                                                                       Reference.Viewer.Height / 2 - imageSize.Height / 2),
                                                                        true, parentElement, (int)GUIElementIDS.ERROR_DIALOG_WINDOW, "");

                ShowMessageWindow(loginMessage);

                loginFailed = false;
                lock (Reference.Viewer.StateManager.SyncStat)
                {
                    Reference.Viewer.StateManager.State = State.ERROR;
                }
            }

            if (Reference.Viewer.StateManager.State == State.DOWNLOADING)
            {
                // Update login progress
                GUIElement e = parentElement.GetElementFromID((int)GUIElementIDS.LOGIN_PROGRESS, true);
                if (e != null) ((GUIProgressBar)e).Pos = loginProgress;
            }
            else if (Reference.Viewer.StateManager.State == State.CONNECTED)
            {
                long cacheMB = CacheManager.CACHE_MAX_MIN;

                if (settingTab2CacheSize != null)
                {
                    if (!string.IsNullOrEmpty(settingTab2CacheSize.Text))
                    {
                        if (long.TryParse(settingTab2CacheSize.Text, out cacheMB))
                        {
                            cacheMB = cacheMB * 1000 * 1000;
                            cacheMB = Util.Clamp<long>(cacheMB, CacheManager.CACHE_MAX_MIN, CacheManager.CACHE_MAX_MAX);

                            if (Reference.Viewer.CacheManager.CacheMaxSize != cacheMB)
                            {
                                Reference.Viewer.CacheManager.CacheMaxSize = cacheMB;
                                SaveChacheSize();
                            }
                        }
                    }
                }
            }

            base.Update(frame);
        }

        public void Remove(int id)
        {
            GUIElement root = parentElement;
            if (root == null)
                return;
            GUIElement e = root.GetElementFromID(id, true);
            if (e != null) e.Remove();
        }

        #region Public Interface

        #region GetCursor
        public Cursors GetCursor()
        {
            if (currentCursorImage == cursorImageNormal)
                return (Cursors.NORMAL);
            if (currentCursorImage == cursorImageHand)
                return (Cursors.HAND);
            if (currentCursorImage == cursorImageChair)
                return (Cursors.CHAIR);
            return (Cursors.NORMAL);
        }
        #endregion

        #region DrawCursor

        public void DrawCursor()
        {
            if (IsShowCursor == false)
                return;

            if (Reference.Viewer.EntityManager.PrimitiveUnderMouse == null)
            {
                currentCursorImage = cursorImageNormal;
            }
            else
            {
                if (Reference.Viewer.MenuManager.Visible || Reference.Viewer.GuiManager.Focused)
                {
                    currentCursorImage = cursorImageNormal;
                }
                else if ((Reference.Viewer.EntityManager.PrimitiveUnderMouse.ClickAction == ClickAction.Sit))
                {
                    currentCursorImage = cursorImageChair;
                }
                else if ((Reference.Viewer.EntityManager.PrimitiveUnderMouse.Flags & PrimFlags.Touch) != 0)
                {
                    currentCursorImage = cursorImageHand;
                }
                else
                {
                    currentCursorImage = cursorImageNormal;
                }
            }

            Position2D position;
            if (currentCursorImage == cursorImageNormal)
            {
                position = new Position2D(Reference.Device.CursorControl.Position.X, Reference.Device.CursorControl.Position.Y);
            }
            else
            {
                position = new Position2D(Reference.Device.CursorControl.Position.X - currentCursorImage.OriginalSize.Width / 2, Reference.Device.CursorControl.Position.Y - currentCursorImage.OriginalSize.Height / 2);
            }

            Reference.VideoDriver.Draw2DImage(currentCursorImage, position + Reference.Viewer.CursolOffset, true);
        }
        #endregion

        #region Progress
        public void ShowLoginProgress()
        {
            Remove((int)GUIElementIDS.INIT_BACKGROUND);
            Remove((int)GUIElementIDS.LOGIN_BACKGROUND);
            Remove((int)GUIElementIDS.LOGIN_PROGRESS);

            if (loginWindowBackground != null)
            {
                Dimension2D imageSize = loginWindowBackground.OriginalSize;
                GUIImage background = Reference.GUIEnvironment.AddImage(loginWindowBackground,
                                                                        new Position2D(Reference.Viewer.Width / 2 - imageSize.Width / 2,
                                                                                       Reference.Viewer.Height / 2 - imageSize.Height / 2),
                                                                        true, parentElement, (int)GUIElementIDS.LOGIN_BACKGROUND, "");
                removeGUI.Add((int)GUIElementIDS.LOGIN_BACKGROUND);
            }

//            GUIProgressBar gpb = Reference.GUIEnvironment.AddProgressBar(true, new Rect(5, Reference.Viewer.Height / 2 - 5, Reference.Viewer.Width - 5, Reference.Viewer.Height / 2 + 5),
//                                                                       parentElement, (int)GUIElementIDS.LOGIN_PROGRESS);
            GUIProgressBar gpb = Reference.GUIEnvironment.AddProgressBar(true, new Rect(Reference.Viewer.Width / 2 - Reference.Viewer.Width / 4, 
                                                                                        (int)(Reference.Viewer.Height * 0.55f) - 6,
                                                                                        Reference.Viewer.Width / 2 + Reference.Viewer.Width / 4,
                                                                                        (int)(Reference.Viewer.Height * 0.55f) + 6),
                                                                         parentElement, (int)GUIElementIDS.LOGIN_PROGRESS);

            removeGUI.Add((int)GUIElementIDS.LOGIN_PROGRESS);
            gpb.Pos = 0;
            loginProgress = 0;
            gpb.Background = Color.TransparentWhite;
            //gpb.Foreground = new Color(64, 0, 0, 0);
            gpb.Foreground = Reference.Viewer.ProgressBarColor;
        }

        public void ProgressLogin(int progress)
        {
            loginProgress = Math.Max(loginProgress, progress);
        }
        #endregion

        #region LoadBackgrounds
        public void LoadBackgrounds()
        {
            bool isLoad = true;
            if (!string.IsNullOrEmpty(Reference.Viewer.LoginBackgroundURL))
            {
                string local = Util.GuiFolder + UUID.Random().ToString();
                try
                {
                    Util.DownloadImage(Reference.Viewer.LoginBackgroundURL, local);
                    loginWindowBackground = Reference.VideoDriver.GetTexture(local + ".png");
                    if (loginWindowBackground == null)
                        isLoad = false;
                }
                catch (Exception e)
                {
                    Reference.Log.Error("not exsit:" + local, e);
                    isLoad = false;
                }
            }
            else
            {
                Reference.Log.Error("loginWindowBackground is null or not exsit");
                isLoad = false;
            }

            if (isLoad == false)
                loginWindowBackground = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\background\login_background.png");

            isLoad = true;
            if (!string.IsNullOrEmpty(Reference.Viewer.InitBackgroundURL))
            {
                string local = Util.GuiFolder + UUID.Random().ToString();
                try
                {
                    Util.DownloadImage(Reference.Viewer.InitBackgroundURL, local);
                    initWindowBackground = Reference.VideoDriver.GetTexture(local + ".png");
                    if (initWindowBackground == null)
                        isLoad = false;
                }
                catch (Exception e)
                {
                    Reference.Log.Error("not exsit:" + local, e);
                    isLoad = false;
                }
            }
            else
            {
                Reference.Log.Error("initWindowBackground is null or not exsit");
                isLoad = false;
            }

            if (isLoad == false)
                initWindowBackground = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\background\init_background.png");

            isLoad = true;
            if (!string.IsNullOrEmpty(Reference.Viewer.LoginBackgroundURL))
            {
                string local = Util.GuiFolder + UUID.Random().ToString();
                try
                {
                    Util.DownloadImage(Reference.Viewer.LoginBackgroundURL, local);
                    teleportWindowBackground = Reference.VideoDriver.GetTexture(local + ".png");
                    if (teleportWindowBackground == null)
                        isLoad = false;
                }
                catch (Exception e)
                {
                    Reference.Log.Error("not exsit:" + local, e);
                    isLoad = false;
                }
            }
            else
            {
                Reference.Log.Error("teleportWindowBackground is null or not exsit");
                isLoad = false;
            }

            if (isLoad == false)
                teleportWindowBackground = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\background\login_background.png");

        }
        #endregion

        #region ShowBackground
        public void ShowBackground()
        {
            Remove((int)GUIElementIDS.INIT_BACKGROUND);

            if (initWindowBackground != null)
            {
                Dimension2D imageSize = initWindowBackground.OriginalSize;
                GUIImage background = Reference.GUIEnvironment.AddImage(initWindowBackground,
                                                                        new Position2D(Reference.Viewer.Width / 2 - imageSize.Width / 2,
                                                                                       Reference.Viewer.Height / 2 - imageSize.Height / 2),
                                                                        true, parentElement, (int)GUIElementIDS.INIT_BACKGROUND, "");
            }
        }
        #endregion

        #region ShowLoginWindow
        public void ShowLoginWindow(LoginInfo p)
        {
            loginInfo = p;

            // remove tool box if already there
            Remove((int)GUIElementIDS.LOGIN_WINDOW);
            Remove((int)GUIElementIDS.ERROR_DIALOG_WINDOW);

            if (CheckParam() == false)
            {
                loginInfo.LoginMode = LoginType.MANUAL;
                p.LoginMode = loginInfo.LoginMode;
            }


            switch (p.LoginMode)
            {
                case LoginType.AUTO:
                    ShowBackground();
                    ShowLoginWindowAuto();
                    break;

                case LoginType.CLICK:
                    ShowBackground();
                    ShowLoginWindowClick();
                    break;

                case LoginType.MANUAL:
                    ShowBackground();
                    ShowLoginWindowManual();
                    break;

                case LoginType.NONE:
                    ShowBackground();
                    break;

                default:
                    Reference.Log.Debug("New type ?");
                    break;
            }
        }

        private bool CheckParam()
        {
            return true;
        }

        private void ShowLoginWindowAuto()
        {
            Reference.Viewer.LoginRequest();
        }

        private void ShowLoginWindowClick()
        {
        }

        private void ShowLoginWindowManual()
        {
            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(
                new Rect(new Position2D(Reference.Viewer.Width / 2 - 150, Reference.Viewer.Height / 2 - 88), new Dimension2D(300, 166)),
                false, "   " + DialogText.Login, parentElement, (int)GUIElementIDS.LOGIN_WINDOW);

            wnd.CloseButton.Visible = false;

            int x = 4;
            int y = 28;
            int w = 128;
            int h = 24;
            int oy = 24;
            //Reference.GUIEnvironment.AddStaticTextW(DialogText.ServerURI, new Rect(new Position2D(4, 8), new Position2D(200, 24)), false, false, wnd, (int)GUIElementIDS.LOGIN_SERVERURI_TEXT, false);
            Reference.GUIEnvironment.AddStaticTextW(DialogText.Firstname, new Rect(new Position2D(x, y * 1 + oy), new Dimension2D(w, h)), false, false, wnd, (int)GUIElementIDS.LOGIN_FIRSTNAME_TEXT, false);
            Reference.GUIEnvironment.AddStaticTextW(DialogText.Lastname, new Rect(new Position2D(x, y * 2 + oy), new Dimension2D(w, h)), false, false, wnd, (int)GUIElementIDS.LOGIN_LASTNAME_TEXT, false);
            Reference.GUIEnvironment.AddStaticTextW(DialogText.Password, new Rect(new Position2D(x, y * 3 + oy), new Dimension2D(w, h)), false, false, wnd, (int)GUIElementIDS.LOGIN_PASSWORD_TEXT, false);

            loginServerURI = Reference.GUIEnvironment.AddEditBox(DialogText.ServerURI, new Rect(new Position2D(x, y * 0 + oy), new Dimension2D(292, h)), true, wnd, (int)GUIElementIDS.LOGIN_SERVERURI);

            x = 136;
            w = 160;
            loginFirstName = Reference.GUIEnvironment.AddEditBox("", new Rect(new Position2D(x, y * 1 + oy), new Dimension2D(w, h)), true, wnd, (int)GUIElementIDS.LOGIN_FIRSTNAME);
            loginLastName = Reference.GUIEnvironment.AddEditBox("", new Rect(new Position2D(x, y * 2 + oy), new Dimension2D(w, h)), true, wnd, (int)GUIElementIDS.LOGIN_LASTNAME);
            loginPassword = Reference.GUIEnvironment.AddEditBox("", new Rect(new Position2D(x, y * 3 + oy), new Dimension2D(w, h)), true, wnd, (int)GUIElementIDS.LOGIN_PASSWORD);
            loginPassword.Password = true;

            Reference.GUIEnvironment.AddButtonW(new Rect(new Position2D(50, y * 4 + oy), new Dimension2D(200, h)), wnd, (int)GUIElementIDS.LOGIN_LOGINBUTTON, DialogText.Login);

            Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_login.png"), new Position2D(5, 2), true, wnd, -1, "");

            /* Initialize parameters */
            loginServerURI.Text = loginInfo.URI;
            loginFirstName.Text = loginInfo.FirstName;
            loginLastName.Text = loginInfo.LastName;
            loginPassword.Text = loginInfo.Password;

            // Set focus.
            Reference.GUIEnvironment.SetFocus(loginServerURI);
        }
        #endregion

        #region ShowChatWindow
        public void ShowChatWindow()
        {
            // remove tool box if already there
            Remove((int)GUIElementIDS.CHAT_WINDOW);

            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(
                new Rect(new Position2D(Reference.Viewer.Width / 2 - 150, Reference.Viewer.Height / 2 - 110), new Dimension2D(300, 220)),
                false, "   " + DialogText.Chat, parentElement, (int)GUIElementIDS.CHAT_WINDOW);

            wnd.CloseButton.UseAlphaChannel = true;
            wnd.CloseButton.SetImage(closeButton);
            wnd.ToolTipText = string.Empty;
            wnd.ToolTipTextW = string.Empty;

            GUIListBox chatBoxMessageList_private = Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(4, 24), new Dimension2D(290, 140)), wnd, (int)GUIElementIDS.CHAT_MSGLIST, true);

            Texture buttonTex = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\button_chatenter.png");
            chatBoxInput = Reference.GUIEnvironment.AddEditBoxW("", new Rect(new Position2D(4, 172), new Dimension2D(276, 44)), true, wnd, (int)GUIElementIDS.CHAT_ENTERMSG);
            GUIButton btn = Reference.GUIEnvironment.AddButtonW(new Rect(new Position2D(284, 172), new Dimension2D(buttonTex.OriginalSize.Width, buttonTex.OriginalSize.Height)), wnd, (int)GUIElementIDS.CHAT_SENDBUTTON, "");
            btn.SetImage(buttonTex);
            //Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\button_chatenter.png"), new Position2D(0, 0), true, btn, -1, "");

            Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_chat.png"), new Position2D(5, 2), true, wnd, -1, "");

            // Get all chat history.
            for (int i = 0; i < Reference.Viewer.ChatManager.Messages.Length; i++)
            {
                chatBoxMessageList_private.AddItemW(Reference.Viewer.ChatManager.Messages[i]);
            }

            // Set last message.
            chatBoxMessageList_private.Selected = chatBoxMessageList_private.ItemCount - 1;
            chatBoxMessageList = chatBoxMessageList_private;
            // Set focus.
            Reference.GUIEnvironment.SetFocus(chatBoxInput);

            imageChat.Visible = false;
        }
        #endregion

        #region ShowTeleportWindow
        public void ShowTeleportWindow()
        {
            // remove tool box if already there
            Remove((int)GUIElementIDS.TELEPORT_WINDOW);

            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(
                new Rect(new Position2D(Reference.Viewer.Width / 2 - 150, Reference.Viewer.Height / 2 - 64), new Dimension2D(300, 128)),
                false, "   " + DialogText.Teleport, parentElement, (int)GUIElementIDS.TELEPORT_WINDOW);

            wnd.CloseButton.UseAlphaChannel = true;
            wnd.CloseButton.SetImage(closeButton);

            // teleport region
            Reference.GUIEnvironment.AddStaticTextW(DialogText.RegionName, new Rect(new Position2D(4, 24), new Dimension2D(96, 24)), false, false, wnd, -1, false);
            teleportRegionName = Reference.GUIEnvironment.AddEditBoxW(Reference.Viewer.ProtocolManager.GetCurrentSimName(), new Rect(new Position2D(100, 24), new Dimension2D(196, 24)), true, wnd, (int)GUIElementIDS.TELEPORT_REGIONNAME);

            int ox = 4;
            // teleport position
            Reference.GUIEnvironment.AddStaticTextW("X", new Rect(new Position2D(ox, 52), new Dimension2D(12, 24)), false, false, wnd, -1, false);
            Reference.GUIEnvironment.AddStaticTextW("Y", new Rect(new Position2D(96 + ox, 52), new Dimension2D(12, 24)), false, false, wnd, -1, false);
            Reference.GUIEnvironment.AddStaticTextW("Z", new Rect(new Position2D(196 + ox, 52), new Dimension2D(12, 24)), false, false, wnd, -1, false);

            teleportX = Reference.GUIEnvironment.AddEditBoxW("128", new Rect(new Position2D(12 + ox, 52), new Dimension2D(80, 24)), true, wnd, (int)GUIElementIDS.TELEPORT_X);
            teleportY = Reference.GUIEnvironment.AddEditBoxW("128", new Rect(new Position2D(112 + ox, 52), new Dimension2D(80, 24)), true, wnd, (int)GUIElementIDS.TELEPORT_Y);
            teleportZ = Reference.GUIEnvironment.AddEditBoxW("128", new Rect(new Position2D(212 + ox, 52), new Dimension2D(80, 24)), true, wnd, (int)GUIElementIDS.TELEPORT_Z);

            // teleport button
            Reference.GUIEnvironment.AddButtonW(new Rect(new Position2D(4, 96), new Dimension2D(300 - 8, BUTTON_DEFULT_HEIGHT)), wnd, (int)GUIElementIDS.TELEPORT_TELEPORTBUTTON, DialogText.Teleport);

            Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_teleport.png"), new Position2D(5, 2), true, wnd, -1, "");

            // Set focus.
            Reference.GUIEnvironment.SetFocus(teleportRegionName);
        }

        public void ShowTeleportFailedWindow(string message)
        {
            IsShowCursor = true;

            // remove tool box if already there
            Remove((int)GUIElementIDS.TELEPORT_FAILED_WINDOW);

            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(
                new Rect(new Position2D(Reference.Viewer.Width / 2 - 150, Reference.Viewer.Height / 2 - 64), new Dimension2D(300, 128)),
                false, "   " + DialogText.Teleport, parentElement, (int)GUIElementIDS.TELEPORT_FAILED_WINDOW);

            GUIImage img = Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_teleport.png"), new Position2D(5, 2), true, wnd, -1, "");

            wnd.CloseButton.Visible = false;

            // teleport failed message
            Reference.GUIEnvironment.AddStaticTextW(message, new Rect(new Position2D(4, 24), new Dimension2D(wnd.RelativePosition.dotNETRect.Width - 4, wnd.RelativePosition.dotNETRect.Height - 24)), false, true, wnd, -1, false);

            // back button
            GUIButton backBtn = Reference.GUIEnvironment.AddButtonW(new Rect(new Position2D(4, 96), new Dimension2D(300 - 8, BUTTON_DEFULT_HEIGHT)), wnd, (int)GUIElementIDS.TELEPORT_BACKBUTTON, "Back");

            // Set focus.
            Reference.GUIEnvironment.SetFocus(backBtn);
        }

        #endregion

        #region ShowMessageWindow
        public void ShowMessageWindow(string _message)
        {
            ShowMessageWindow(_message, false);
        }

        public void ShowMessageWindow(string _message, bool _useCancelButton)
        {
            IsShowCursor = true;

            // remote message window.
            Remove((int)GUIElementIDS.GENERAL_MESSAGE_WINDOW);

            //-------------------------------------------------
            // window setting.
            //-------------------------------------------------
            Rect windowRect = new Rect(
                new Position2D(Reference.Viewer.Width / 2 - 150, Reference.Viewer.Height / 2 - 64),
                new Dimension2D(300, 128)
                );

            string caption = "   " + DialogText.MessageWindowCaption;
            int windowID = (int)GUIElementIDS.GENERAL_MESSAGE_WINDOW;

            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(windowRect, true, caption, parentElement, windowID);
            wnd.CloseButton.Visible = false;

            string[] messages = _message.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            //-------------------------------------------------
            // text area setting.
            //-------------------------------------------------
            Rect textRect = new Rect(
                new Position2D(4, 24),
                new Dimension2D(wnd.RelativePosition.dotNETRect.Width - 4, wnd.RelativePosition.dotNETRect.Height - BUTTON_DEFULT_HEIGHT - 24)
                );

            GUIListBox listBox = Reference.GUIEnvironment.AddListBox(textRect, wnd, -1, false);
            listBox.Enabled = false;
            for (int i = 0; i < messages.Length; i++)
                listBox.AddItemW(messages[i]);

            //-------------------------------------------------
            // button setting.
            //-------------------------------------------------
            Rect buttonRect = new Rect(
                new Position2D(4, wnd.RelativePosition.dotNETRect.Height - BUTTON_DEFULT_HEIGHT),
                new Dimension2D(300 - 8, BUTTON_DEFULT_HEIGHT)
                );
            int button00ID = (int)GUIElementIDS.GENERAL_MESSAGE_BUTTON00;
            int button01ID = (int)GUIElementIDS.GENERAL_MESSAGE_BUTTON01;
            string button00Text = "OK";
            string button01Text = "Cancel";

            GUIButton back00Btn;
            GUIButton back01Btn;
            if (_useCancelButton)
            {
                buttonRect.Width = buttonRect.Width / 2;
                back00Btn = Reference.GUIEnvironment.AddButtonW(buttonRect, wnd, button00ID, button00Text);
                buttonRect.X = buttonRect.Width;
                back01Btn = Reference.GUIEnvironment.AddButtonW(buttonRect, wnd, button01ID, button01Text);
            }
            else
            {
                back00Btn = Reference.GUIEnvironment.AddButtonW(buttonRect, wnd, button00ID, button00Text);
            }

            // Set focus.
            Reference.GUIEnvironment.SetFocus(back00Btn);
        }
        #endregion

        #region ShowSettingWindow
        public void ShowSettingWindow()
        {
            //----------------------------------------------------------------------
            // Remove window
            //----------------------------------------------------------------------
            Remove((int)GUIElementIDS.SETTING_WINDOW);

            //----------------------------------------------------------------------
            // Set window
            //----------------------------------------------------------------------
            Rect rect = new Rect(new Position2D(Reference.Viewer.Width / 2 - 150, Reference.Viewer.Height / 2 - 110), new Dimension2D(300, 220));
            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(rect, false, "   " + DialogText.Settings, parentElement, (int)GUIElementIDS.SETTING_WINDOW);

            Texture tex = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_settings.png");
            if (tex != null)
                Reference.GUIEnvironment.AddImage(tex, new Position2D(5, 2), true, wnd, -1, "");

            wnd.CloseButton.UseAlphaChannel = true;
            wnd.CloseButton.SetImage(closeButton);

            //----------------------------------------------------------------------
            // Set tab
            //----------------------------------------------------------------------
            const int width = 4;
            const int height = 32;
            rect.X = width / 2;
            rect.Width = rect.Width - width;
            rect.Y = height;
            rect.Height = (int)(rect.Height * 2f) - (rect.Y / 2);
            GUITabControl tab = Reference.GUIEnvironment.AddTabControl(rect, wnd, true, true, -1);

            //----------------------------------------------------------------------
            // Graphic tab
            //----------------------------------------------------------------------
            GUITab t1 = tab.AddTabW(DialogText.General, -1);
            Rect t1Rect = new Rect();

            t1Rect.X = 8;
            t1Rect.Y = 24;
            t1Rect.Width = 80;
            t1Rect.Height = 20 * 1;
            Reference.GUIEnvironment.AddStaticTextW(DialogText.SeaShader+":", t1Rect, false, false, t1, -1, false);
            t1Rect.X = t1Rect.X + t1Rect.Width;
            t1Rect.Width = 48;
            t1Rect.Height = 20 * 2;
            settingTab1SeaQualityList = Reference.GUIEnvironment.AddListBox(t1Rect, t1, -1, false);
            settingTab1SeaQualityList.AddItemW(DialogText.High);
            settingTab1SeaQualityList.AddItemW(DialogText.Low);
            settingTab1SeaQualityList.Selected = (Reference.Viewer.SeaQuality == Viewer.ShaderLevelType.High ? 0 : 1);

            t1Rect.X = 16 + t1Rect.X + t1Rect.Width;
            t1Rect.Width = 80;
            t1Rect.Height = 20 * 1;
            Reference.GUIEnvironment.AddStaticTextW(DialogText.SkyShader+":", t1Rect, false, false, t1, -1, false);
            t1Rect.X = t1Rect.X + t1Rect.Width;
            t1Rect.Width = 48;
            t1Rect.Height = 20 * 2;
            settingTab1SkyQualityList = Reference.GUIEnvironment.AddListBox(t1Rect, t1, -1, false);
            settingTab1SkyQualityList.AddItemW(DialogText.High);
            settingTab1SkyQualityList.AddItemW(DialogText.Low);
            settingTab1SkyQualityList.Selected = (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.High ? 0 : 1);

            t1Rect.X = 8;
            t1Rect.Y = t1Rect.Y + t1Rect.Height + 6;
            t1Rect.Width = 80;
            t1Rect.Height = 20 * 1;
            Reference.GUIEnvironment.AddStaticTextW(DialogText.Locale+":", t1Rect, false, false, t1, -1, false);
            t1Rect.X = t1Rect.X + t1Rect.Width;
            t1Rect.Width = 192;
            t1Rect.Height = 16 * 2;
            settingTab1Locale = Reference.GUIEnvironment.AddListBox(t1Rect, t1, -1, false);
#if YK_LANGUAGE
#endif
            settingTab1Locale.AddItemW(DialogText.EN);
            if (Reference.Viewer.JapaneseEnabled)
                settingTab1Locale.AddItemW(DialogText.JP);
            settingTab1Locale.Selected = (Reference.Viewer.Locale == "en" ? 0 : 1);

            t1Rect.X = 8;
            t1Rect.Y = t1Rect.Y + t1Rect.Height + 16;
            t1Rect.Width = 278;
            t1Rect.Height = BUTTON_DEFULT_HEIGHT;
            Reference.GUIEnvironment.AddButtonW(t1Rect, t1, (int)GUIElementIDS.SETTING_TAB1_BUTTON00, "OK");

            //----------------------------------------------------------------------
            // Cache tab
            //----------------------------------------------------------------------
            GUITab t2 = tab.AddTabW(DialogText.Cache, -1);

            Rect t2Rect;
            t2Rect = new Rect(new Position2D(8, 24), new Dimension2D(278, 64));
            Reference.GUIEnvironment.AddStaticTextW(DialogText.CacheInformationMessage, t2Rect, false, true, t2, -1, false);

            t2Rect = new Rect(new Position2D(112, 88), new Dimension2D(278, 32));
            Reference.GUIEnvironment.AddStaticTextW(DialogText.CacheUnitMessage, t2Rect, false, true, t2, -1, false);

            t2Rect = new Rect(new Position2D(224, 80), new Dimension2D(64, 32));
            long cacheMB = Reference.Viewer.CacheManager.CacheMaxSize / (1000 * 1000);
            settingTab2CacheSize = Reference.GUIEnvironment.AddEditBox(cacheMB.ToString(), t2Rect, true, t2, -1);

            t2Rect = new Rect(new Position2D(8, 118), new Dimension2D(278, BUTTON_DEFULT_HEIGHT));
            Reference.GUIEnvironment.AddButtonW(t2Rect, t2, (int)GUIElementIDS.SETTING_TAB2_BUTTON00, DialogText.Delete);

            //----------------------------------------------------------------------
            // Information tab
            //----------------------------------------------------------------------
            GUITab t3 = tab.AddTabW(DialogText.Information, -1);
            Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\logo.tga"), new Position2D(22, 24), false, t3, -1, string.Empty);
            Reference.GUIEnvironment.AddStaticTextW(DialogText.Version + ": " + Reference.Viewer.Version.ToString(), new Rect(new Position2D(22, 100), new Dimension2D(Reference.Viewer.Width - 8, 24)), false, true, t3, -1, false);
            Reference.GUIEnvironment.AddStaticTextW("Copyright (C) 2009 3Di, Inc.", new Rect(new Position2D(22, 120), new Dimension2D(Reference.Viewer.Width - 8, 24)), false, true, t3, -1, false);
        }
        #endregion

        #region ShowDebugWindow
        public bool IsVisibleDebugWindow()
        {
            bool flag = false;

            if (parentElement != null)
                flag = parentElement.GetElementFromID((int)GUIElementIDS.DEBUG_WINDOW, true) != null;

            return flag;
        }

        public void ShowDebugWindow()
        {
            if (Reference.Viewer.IsVisibleDebutTab == false)
                return;

            //----------------------------------------------------------------------
            // Remove window
            //----------------------------------------------------------------------
            Remove((int)GUIElementIDS.DEBUG_WINDOW);

            //----------------------------------------------------------------------
            // Set window
            //----------------------------------------------------------------------
            Rect rect = new Rect(0, 0, Reference.VideoDriver.ViewPort.Width / 2, Reference.VideoDriver.ViewPort.Height); 
            GUIWindow wnd = Reference.GUIEnvironment.AddWindowW(
                rect,
                false, "   " + DialogText.Settings, parentElement, (int)GUIElementIDS.DEBUG_WINDOW);

            Texture tex = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\gui\windows\window_settings.png");
            if (tex != null)
                Reference.GUIEnvironment.AddImage(tex, new Position2D(5, 2), true, wnd, -1, "");

            wnd.CloseButton.UseAlphaChannel = true;
            wnd.CloseButton.SetImage(closeButton);

            //----------------------------------------------------------------------
            // Debug tab
            //----------------------------------------------------------------------
            const int width = 16;
            const int height = 32;
            rect.X = width / 2;
            rect.Width = rect.Width - width;

            rect.Y = height;
            rect.Height = (int)(rect.Height * 2f) - (rect.Y / 2); 
            if (Reference.Viewer.IsVisibleDebutTab)
            {
                debugTab = Reference.GUIEnvironment.AddTabControl(rect, wnd, true, true, (int)GUIElementIDS.DEBUG_DTAB_CONTROLLER);
                GUITab dt0 = debugTab.AddTab("Sys", -1);
                GUITab dt1 = debugTab.AddTab("Node", -1);
                GUITab dt2 = debugTab.AddTab("User", -1);
                GUITab dt3 = debugTab.AddTab("View", -1);
                GUITab dt4 = debugTab.AddTab("P-N", -1);
                GUITab dt5 = debugTab.AddTab("P-C", -1);
                GUITab dt6 = debugTab.AddTab("LMNT", -1);
                GUITab dt7 = debugTab.AddTab("Voice", -1);

                debugListBoxList = new Dictionary<int, GUIListBox>(10);
                debugListBoxList.Add(0, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt0, -1, false));
                debugListBoxList.Add(1, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt1, -1, false));
                debugListBoxList.Add(2, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt2, -1, false));
                debugListBoxList.Add(3, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt3, -1, false));
                debugListBoxList.Add(4, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt4, -1, false));
                debugListBoxList.Add(5, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt5, -1, false));
                debugListBoxList.Add(6, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt6, -1, false));
                debugListBoxList.Add(7, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt7, -1, false));
#if DEBUG_QUEUE
                GUITab dt7 = debugTab.AddTab("QUEUE", -1);
                debugListBoxList.Add(7, Reference.GUIEnvironment.AddListBox(new Rect(new Position2D(0, 0), new Dimension2D(Reference.Viewer.Width / 2, Reference.Viewer.Height - 64)), dt7, -1, false));
#endif
            
            }
        }
        #endregion

        #region DrawChairButton
        public void DrawChairButton(bool _flag)
        {
            if (imageChair != null && Reference.Viewer.IsStandUpIcon)
                imageChair.Visible = _flag;
        }
        #endregion

        #region HandleEvent

        public delegate void ButtonClickCallback();
        private Dictionary<int, ButtonClickCallback> buttonClickCallbacks = new Dictionary<int, ButtonClickCallback>();
        public bool AddButtonClickCallback(int buttonID, ButtonClickCallback callback)
        {
            if (buttonClickCallbacks.ContainsKey(buttonID))
            {
                return (false);
            }
            else
            {
                buttonClickCallbacks.Add(buttonID, callback);
                return (true);
            }
        }

        public bool RemoveButtonClickCallback(int buttonID, ButtonClickCallback callback)
        {
            if (buttonClickCallbacks.ContainsKey(buttonID))
            {
                return (false);
            }
            else
            {
                buttonClickCallbacks.Remove(buttonID);
                return (true);
            }
        }

        public bool HandleEvent(Event evnt)
        {
            switch (evnt.GUIEvent)
            {
                case GUIEventType.ElementFocused:
                    if (evnt.Caller.Type != ElementType.Image)
                    {
                        focused = true;
                    }
                    break;
                
                case GUIEventType.ElementFocusLost:
                    focused = false;
                    break;
                
                case GUIEventType.EditBoxEnter:
                    // Login.
                    if (evnt.Caller.ID == (int)GUIElementIDS.LOGIN_SERVERURI
                        || evnt.Caller.ID == (int)GUIElementIDS.LOGIN_FIRSTNAME
                        || evnt.Caller.ID == (int)GUIElementIDS.LOGIN_LASTNAME
                        || evnt.Caller.ID == (int)GUIElementIDS.LOGIN_PASSWORD)
                    {
                        LoginRequest();
                    }

                    // Teleport.
                    if (evnt.Caller.ID == (int)GUIElementIDS.TELEPORT_REGIONNAME
                        || evnt.Caller.ID == (int)GUIElementIDS.TELEPORT_X
                        || evnt.Caller.ID == (int)GUIElementIDS.TELEPORT_Y
                        || evnt.Caller.ID == (int)GUIElementIDS.TELEPORT_Z)
                    {
                        TeleportRequest();
                    }

                    // Chat.
                    if (evnt.Caller.ID == (int)GUIElementIDS.CHAT_ENTERMSG)
                    {
                        RequestSendChatMessage();
                    }
                    break;

                case GUIEventType.ElementClosed:
                    focused = false;
                    break;

                case GUIEventType.TabChanged:
                    if (evnt.Caller.ID == (int)GUIElementIDS.DEBUG_DTAB_CONTROLLER)
                    {
                        Reference.Viewer.DebugManager.ChangeMode(debugTab.ActiveTab);
                    }
                    break;

                case GUIEventType.ButtonClicked:
                    if (evnt.Caller.ID == (int)GUIElementIDS.LOGIN_LOGINBUTTON)
                    {
                        LoginRequest();
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.GENERAL_MESSAGE_BUTTON00)
                    {
                        if (isChacheDeleteMessage)
                        {
                            SaveReserveChacheDelete();

                            ShowMessageWindow(DialogText.CacheDeleteMessage);
                        }
                        else
                        {
                            Remove((int)GUIElementIDS.GENERAL_MESSAGE_WINDOW);
                            Remove((int)GUIElementIDS.SETTING_WINDOW);

                            if (Reference.Viewer.StateManager.State == State.ERROR)
                            {
                                // Change login mode by now mode.
                                switch (loginInfo.LoginMode)
                                {
                                    case LoginType.AUTO:
                                        Reference.Viewer.LoginMode = "click";
                                        break;

                                    case LoginType.CLICK:
                                        Reference.Viewer.LoginMode = "click";
                                        break;

                                    case LoginType.MANUAL:
                                        Reference.Viewer.LoginMode = "manual";
                                        break;

                                    case LoginType.NONE:
                                        Reference.Viewer.LoginMode = "hide";
                                        break;
                                }

                                Reference.Viewer.LogoutRequest();
                            }
                        }

                        isChacheDeleteMessage = false;
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.GENERAL_MESSAGE_BUTTON01)
                    {
                        Remove((int)GUIElementIDS.GENERAL_MESSAGE_WINDOW);
                        Remove((int)GUIElementIDS.SETTING_WINDOW);

                        isChacheDeleteMessage = false;
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.TELEPORT_BACKBUTTON)
                    {
                        Remove((int)GUIElementIDS.TELEPORT_FAILED_WINDOW);
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.TELEPORT_TELEPORTBUTTON)
                    {
                        TeleportRequest();
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.CHAT_SENDBUTTON)
                    {
                        RequestSendChatMessage();
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.SETTING_TAB1_BUTTON00)
                    {
                        SaveIniFile();

                        ShowMessageWindow(DialogText.SettingMessage);
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.SETTING_TAB2_BUTTON00)
                    {
                        ShowMessageWindow(DialogText.CacheDeleteConfirm, true);
                        isChacheDeleteMessage = true;
                    }
                    else if (evnt.Caller.ID == (int)GUIElementIDS.STANDUP_BUTTON)
                    {
                        if (Reference.Viewer.IsStandUpIcon)
                        {
                            imageChair.Visible = false;
                            Reference.Viewer.Adapter.CallStandUp();
                        }
                    }
                    else
                    {
                        // Not default button click, see if we have a callback defined for it
                        if (buttonClickCallbacks.ContainsKey(evnt.Caller.ID))
                        {
                            buttonClickCallbacks[evnt.Caller.ID].Invoke();
                        }
                    }

                    timer.Enabled = true;

                    break;
            }
            return (false);
        }
        #endregion

        #region GetBackgroundColor
        public Color GetBackgroundColor(State state)
        {
            return Reference.Viewer.ClearColor;

#if YK_BACKGROUND_COLOR
            switch (state)
            {
                case State.INITIALIZING:
                case State.INITIALIZED:
                    return (initWindowBackgroundColor);
                case State.LOGIN:
                case State.DOWNLOADING:
                    return (loginWindowBackgroundColor);
                case State.TELEPORTING:
                    return (teleportWindowBackgroundColor);
                default:
                    return (Color.TransparentBlue);
            }
#endif
        }
        #endregion

        #region Login/Logout function
        public void LoginRemoveWindow()
        {
            Remove((int)GUIElementIDS.LOGIN_WINDOW);

            if (loginWindowBackground != null)
                Remove((int)GUIElementIDS.INIT_BACKGROUND);

            focused = false;
        }

        public void LogoutRemoveWindow()
        {
            Remove((int)GUIElementIDS.TELEPORT_WINDOW);
            Remove((int)GUIElementIDS.CHAT_WINDOW);
            Remove((int)GUIElementIDS.SETTING_WINDOW);
            Remove((int)GUIElementIDS.GENERAL_MESSAGE_WINDOW);

            focused = false;
        }
        #endregion

        #region Chat function
        public void ChatAddMessage(string _message)
        {
            try
            {
                if (parentElement.GetElementFromID((int)GUIElementIDS.CHAT_WINDOW, true) == null)
                {
                    if (Reference.Viewer.StateManager.State == State.CONNECTED
                        && Reference.Viewer.IsDrawMenu
                        )
                    {
                        imageChat.Visible = true;
                    }
                }

                if (chatBoxMessageList != null)
                {
                    lock (chatBoxMessageList)
                    {
                        chatBoxMessageList.AddItemW(_message);
                        chatBoxMessageList.Selected = chatBoxMessageList.ItemCount - 1;
                    }
                }
            }
            catch (Exception e)
            {
                Reference.Log.Debug("ChatAddMessage:" + e.Message);
            }
        }
        #endregion

        #region Debug function
        public void DebugAdd(int _key, string _message)
        {
            if (parentElement == null)
                return;

            if (parentElement.GetElementFromID((int)GUIElementIDS.DEBUG_WINDOW, true) == null)
            {
                if (debugListBoxList != null)
                {
                    debugListBoxList.Clear();
                    debugListBoxList = null;
                }
                return;
            }

            if (debugListBoxList != null)
            {
                if (debugListBoxList.ContainsKey(_key))
                    debugListBoxList[_key].AddItemW(_message);
            }
        }

        public void DebugClear()
        {
            if (parentElement == null)
            {
                return;
            }

            if (parentElement.GetElementFromID((int)GUIElementIDS.DEBUG_WINDOW, true) == null)
            {
                if (debugListBoxList != null)
                {
                    debugListBoxList.Clear();
                    debugListBoxList = null;
                }
                return;
            }

            if (debugListBoxList != null)
            {
                foreach (GUIListBox box in debugListBoxList.Values)
                {
                    box.Clear();
                }
            }
        }
        #endregion

        private void RequestSendChatMessage()
        {
            if (chatBoxInput.Text == string.Empty)
            {
                Reference.Log.Debug("RequestSendChatMessage: Empty");
            }
            else
            {
                Reference.Log.Debug("RequestSendChatMessage: Message:" + chatBoxInput.Text);

                try
                {
                    // second param is range. 0: wisper, 1: say, 2...
                    Reference.Viewer.Adapter.CallSendChat(chatBoxInput.Text, chatRange);
                }
                catch (Exception e)
                {
                    Reference.Log.Debug("RequestSendChatMessage: ERROR:" + e.Message);
                }

                chatBoxInput.Text = string.Empty;
            }
        }

        private void LoginRequest()
        {
            Reference.Viewer.ServerURI = loginServerURI.Text;
            Reference.Viewer.FirstName = loginFirstName.Text;
            Reference.Viewer.LastName = loginLastName.Text;
            Reference.Viewer.Password = loginPassword.Text;

            loginInfo.URI = loginServerURI.Text;
            loginInfo.FirstName = loginFirstName.Text;
            loginInfo.LastName = loginLastName.Text;
            loginInfo.Password = loginPassword.Text;

            // Start Login
            Reference.Viewer.LoginRequest();
        }

        private void TeleportRequest()
        {
            // Start Teleport
            string regionName = teleportRegionName.Text;
            float x, y, z;
            bool ok = true;
            ok &= (regionName.Length > 0);
            ok &= float.TryParse(teleportX.Text, out x);
            ok &= float.TryParse(teleportY.Text, out y);
            ok &= float.TryParse(teleportZ.Text, out z);

            if (!ok)
            {

            }
            else
            {
                Remove((int)GUIElementIDS.TELEPORT_WINDOW);
                Remove((int)GUIElementIDS.CHAT_WINDOW);
                Remove((int)GUIElementIDS.SETTING_WINDOW);
                Remove((int)GUIElementIDS.GENERAL_MESSAGE_WINDOW);
                Remove((int)GUIElementIDS.VOICEACCEPT_WINDOW);

                focused = false;
                Reference.Viewer.ProtocolManager.Teleport(regionName, (int)x, (int)y, (int)z);
            }
        }

        private void FocuseLostTimer(object obj, System.Timers.ElapsedEventArgs arg)
        {
            focused = false;
            timer.Enabled = false;
        }

        private void BlinkEffectTimer(object obj, System.Timers.ElapsedEventArgs arg)
        {
            isVisbleIcon = !isVisbleIcon;

            if (isVisbleIcon)
            {
                Position2D move = -(new Position2D(imageChat.AbsolutePosition.X, imageChat.AbsolutePosition.Y) - chatIconPosition);
                imageChat.Move(move);
            }
            else
            {
                Position2D move = (new Position2D(Reference.Viewer.Width, 0) - chatIconPosition);
                imageChat.Move(move);
            }
            imageChat.UpdateAbsolutePosition();
        }

        private void SaveIniFile()
        {
            Reference.Viewer.Config.Source.Configs["Startup"].Set("locale", (settingTab1Locale.Selected == 0 ? "en" : "jp"));
            Reference.Viewer.Config.Source.Configs["Startup"].Set("reverse_camera_pitch", "false");
            Reference.Viewer.Config.Source.Configs["Shader"].Set("sea_quality", (settingTab1SeaQualityList.Selected == 0 ? "high" : "low"));
            Reference.Viewer.Config.Source.Configs["Shader"].Set("sky_quality", (settingTab1SkyQualityList.Selected == 0 ? "high" : "low"));
            Reference.Viewer.Config.Source.Save();
        }

        private void SaveReserveChacheDelete()
        {
            Reference.Viewer.Config.Source.Configs["Startup"].Set("cache_delete", "true");
            Reference.Viewer.Config.Source.Save();
        }

        private void SaveChacheSize()
        {
            if (settingTab2CacheSize == null)
                return;

            long val = 1;
            if (long.TryParse(settingTab2CacheSize.Text, out val))
            {
                long cacheMB = Reference.Viewer.CacheManager.CacheMaxSize / (1000 * 1000);
                Reference.Viewer.Config.Source.Configs["Startup"].Set("cache_size", cacheMB.ToString());
                Reference.Viewer.Config.Source.Save();
            }
        }
        #endregion
    }
}