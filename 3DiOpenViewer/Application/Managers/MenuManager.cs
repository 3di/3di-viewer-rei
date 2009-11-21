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

namespace OpenViewer.Managers
{
    public class MenuIcon
    {
        public string Image { get; set; }
        public string ImageActive { get; set; }
        public delegate void Activate();
        public event Activate OnActivate;
        public GUIImage guiImage;

        public Texture inactiveTexture = null;
        public Texture activeTexture = null;

        public MenuIcon(string image, Activate activate)
        {
            Image = image;
            OnActivate = activate;
        }

        public MenuIcon(string image, string imageActive, Activate activate)
        {
            Image = image;
            ImageActive = imageActive;
            OnActivate = activate;
        }
        
        public void Click()
        {
            if (OnActivate != null)
            {
                OnActivate();
            }
        }

        public void MouseOver(bool over)
        {
            if (over)
            {
                if (activeTexture != null)
                    guiImage.Image = activeTexture;

            }
            else
            {
                guiImage.Image = inactiveTexture;
            }
        }
    }

    public class MenuManager : BaseComponent
    {
        private const int MENU_R_WIDTH = 208;
        private const int MENU_L_WIDTH = 224;
        private const int MENU_HEIGHT = 32;
        private const int MENU_OFFSET_X = 12;
        private const int MENU_OFFSET_Y = 4;
        private const int MENU_ICONWIDTH = 32;
        private const int MENU_ICONOFFSET_X = 16;
        private const int MENU_ICONOFFSET_Y = 4;
        private const int MENU_SPACING = 16;

        private GUIImage m_menuBackground = null;
        private GUIImage m_textBackground = null;
        private GUIStaticText m_menuSIMName = null;
        private GUIStaticText m_menuLocation = null;
        private List<MenuIcon> m_menuIcons = new List<MenuIcon>();
        private bool m_menuVisible = false;
        private bool m_textVisible = false;
        private bool m_textEnabled = true;
        public bool Visible { get { return (m_menuVisible); } set { this.m_menuVisible = value; } }
        public bool TextVisible { get { return (m_textVisible); } set { this.m_textVisible = value; } }
        public string LocationText { get { return (m_textEnabled ? m_menuLocation.TextW : ""); } set { if (m_textEnabled) this.m_menuLocation.TextW = value; } }
        public string SIMName { get { return (m_textEnabled ? m_menuSIMName.TextW : ""); } set { if (m_textEnabled) this.m_menuSIMName.TextW = value; } }
        
        private int mouseOver = -1;

        public MenuManager(Viewer viewer)
            : base(viewer, -1)
        {

        }

        public override void Initialize()
        {
            m_menuBackground = Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/gui/menu/menu_background_r.tga"), new Position2D(Reference.Viewer.Width - MENU_R_WIDTH - MENU_OFFSET_X, 0), true, Reference.GUIEnvironment.RootElement, (int)GUIElementIDS.MENU_BACKGROUND, "background");
            if (Reference.Viewer.Width > 2*MENU_R_WIDTH+2*MENU_OFFSET_X)
            {
                m_textBackground = Reference.GUIEnvironment.AddImage(Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/gui/menu/menu_background_l.tga"), new Position2D(0, 0), true, Reference.GUIEnvironment.RootElement, (int)GUIElementIDS.MENU_BACKGROUND, "background");
                m_menuSIMName = Reference.GUIEnvironment.AddStaticTextW("", new Rect(5, 5, 295, 23), false, false, m_textBackground, (int)GUIElementIDS.MENU_SIMNAME, false);
                m_menuLocation = Reference.GUIEnvironment.AddStaticTextW("", new Rect(5, 25, 295, 43), false, false, m_textBackground, (int)GUIElementIDS.MENU_LOCATIONTEXT, false);
                m_textBackground.Visible = false;
            }
            else
            {
                m_textEnabled = false;
            }
            m_menuBackground.Visible = false;
        }

        public override void Cleanup()
        {
            lock (m_menuIcons)
            {
                m_menuIcons.Clear();
            }
            if (m_menuBackground != null)
            {
                m_menuBackground.Remove();
                m_menuBackground = null;
            }
            if (m_textBackground != null)
            {
                m_textBackground.Remove();
                m_textBackground = null;
            }
        }

        public void AddIcon(MenuIcon icon)
        {
            if (icon.Image != string.Empty)
            {
                icon.inactiveTexture = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/gui/menu/" + icon.Image);
                if(icon.ImageActive != string.Empty)
                {
                    icon.activeTexture = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/gui/menu/" + icon.ImageActive);
                }
                icon.guiImage = Reference.GUIEnvironment.AddImage(icon.inactiveTexture, new Position2D(MENU_R_WIDTH - (m_menuIcons.Count + 1) * (MENU_ICONWIDTH + MENU_SPACING) - 5, MENU_ICONOFFSET_Y), true, m_menuBackground, (int)GUIElementIDS.MENU_ITEMBASE + m_menuIcons.Count, "background");
                this.m_menuIcons.Add(icon);
            }
        }

        public bool CheckVisible(Position2D cursor)
        {
            if (m_menuBackground == null)
                return false;

            if (Reference.Viewer.StateManager.State != State.CONNECTED)
                return false;

            if (Reference.Viewer.IsDrawMenu == false)
            {
                m_menuVisible = false;
                m_textVisible = false;
                return false;
            }

            if ((Reference.Viewer.Width - MENU_R_WIDTH - MENU_OFFSET_X) < cursor.X && cursor.X < Reference.Viewer.Width
                && - 20 < cursor.Y && cursor.Y < (MENU_HEIGHT - MENU_OFFSET_Y))
            {
                CheckIconMouseOver(cursor);
                m_menuVisible = Reference.Device.WindowActive;
            }
            else
            {
                m_menuVisible = false;
            }
            if (m_textEnabled)
            {
                if (0 < cursor.X && cursor.X < MENU_L_WIDTH
                    && -20 < cursor.Y && cursor.Y < MENU_HEIGHT)
                {
                    m_textVisible = Reference.Device.WindowActive;
                }
                else
                {
                    m_textVisible = false;
                }
                m_textBackground.Visible = m_textVisible;
            }
            m_menuBackground.Visible = m_menuVisible;
            return m_textVisible;
        }

        private void CheckIconMouseOver(Position2D cursor)
        {
            if (cursor.Y <= MENU_HEIGHT && cursor.X > Reference.Viewer.Width - m_menuIcons.Count * (MENU_ICONWIDTH + MENU_SPACING) - MENU_ICONOFFSET_X && cursor.X <= Reference.Viewer.Width - MENU_ICONOFFSET_X)
            {
                int hit = (int)Math.Floor((double)(Reference.Viewer.Width - cursor.X - MENU_ICONOFFSET_X) / (double)(MENU_ICONWIDTH + MENU_SPACING));
                if (mouseOver != -1 && mouseOver != hit)
                {
                    m_menuIcons[mouseOver].MouseOver(false);
                    mouseOver = -1;
                }
                mouseOver = hit;
                m_menuIcons[hit].MouseOver(true);
            }
            else
            {
                if (mouseOver != -1 && m_menuIcons.Count > 0)
                {
                    m_menuIcons[mouseOver].MouseOver(false);
                    mouseOver = -1;
                }
            }
        }

        public bool Click(Position2D cursor)
        {
            // Icons are right-aligned 5px from the edge, all being a MENU_ICONWIDTHxMENU_ICONWIDTH image
            // {----------| 7 | 6 | 4 | 3 | 2 | 1 |}
            //                 ... -85 -65 -45 -25
            // icon range starts at WindowWidth - iconCnt*MENU_ICONWIDTH+5
            if (cursor.Y <= MENU_HEIGHT && cursor.X > Reference.Viewer.Width - m_menuIcons.Count * (MENU_ICONWIDTH + MENU_SPACING) - MENU_ICONOFFSET_X && cursor.X <= Reference.Viewer.Width - MENU_ICONOFFSET_X)
            {
                int hit = (int)Math.Floor((double)(Reference.Viewer.Width - cursor.X - MENU_ICONOFFSET_X) / (double)(MENU_ICONWIDTH + MENU_SPACING));
                m_menuIcons[hit].Click();
            }
            return (false);
        }
    }
}
