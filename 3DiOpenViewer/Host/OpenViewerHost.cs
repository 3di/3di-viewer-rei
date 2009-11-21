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
using System.Drawing;
using System.Windows.Forms;
using OpenViewer;

namespace OpenViewerHost
{
    public partial class OpenViewerHost : Form
    {
        private OpenViewer.Viewer viewer = null;
        private readonly OpenViewerConfigSource m_config = new OpenViewerConfigSource();
        public OpenViewerHost()
        {
            InitializeComponent();

            viewer = new Viewer();
            viewer.Height = 600;
            viewer.Width = 800;

            viewer.LoginMode = "manual";
            viewer.ServerURI = "10.0.1.81:10001";
            viewer.FirstName = "test";
            viewer.LastName = "test";
            viewer.Password = "testtest";

            viewer.DrawSea = "true";
            viewer.DrawShadow = "false";
            viewer.DrawSky = "true";
            viewer.DrawTerrain = "true";

            viewer.IsStandUpIcon = true;

            viewer.CameraMinDistance = 0.1f;

            SetupConfig();

            viewer.Adapter = new Adapter();
            viewer.Adapter.OnOpenWindow += OnOpenWindow;
            viewer.Version = new Version(1, 0, 0, 0);

            string helpURL = m_config.Source.Configs["Startup"].Get("help_url", "http://3di-opensim.com/");
            string locale = m_config.Source.Configs["Startup"].Get("locale", "jp");
            string debugTab = m_config.Source.Configs["Startup"].Get("debug_tab", "false");
            string cameraReverse = m_config.Source.Configs["Startup"].Get("reverse_camera_pitch", "false");
            string seaQuality = m_config.Source.Configs["Shader"].Get("sea_quality", "low");
            string skyQuality = m_config.Source.Configs["Shader"].Get("sky_quality", "low");
            long teleportTimeout = m_config.Source.Configs["Startup"].GetLong("teleport_timeout", 20);

            // Read Debug config.
            Nini.Config.IConfig debug = m_config.Source.Configs["Debug"];
            if (debug != null)
            {
                viewer.VoiceWaitTime = debug.GetInt("voice_wait_time", Viewer.DEFAULT_DEBUG_VOICE_WAIT_TIME);
            }

            viewer.HelpURL = helpURL;
            viewer.Locale = locale;
            viewer.IsVisibleDebutTab = (debugTab == "true" ? true : false);
            viewer.IsCameraPitchReverse = (cameraReverse == "true" ? true : false);
#if !LINUX			
            viewer.SetShaderQuality(OpenViewer.Managers.ShaderManager.ShaderType.Sea, seaQuality);
            viewer.SetShaderQuality(OpenViewer.Managers.ShaderManager.ShaderType.AdvancedSea, seaQuality);
#endif
            viewer.SetShaderQuality(OpenViewer.Managers.ShaderManager.ShaderType.Sky, skyQuality);
            viewer.TeleportTimeout = teleportTimeout * 10000000;

            viewer.Startup(this.Handle);
        }

        void OnOpenWindow(string _target, string _uri)
        {
            WebBrowser wb = new WebBrowser();
            wb.Location = new Point(10, 10);
            wb.Size = new Size(300, 300);
            wb.Url = new Uri(_uri);
            wb.Show();
        }

        private void SetupConfig()
        {
            try
            {
                bool readConfig = false;
                string iniconfig = System.IO.Path.Combine(Util.ConfigFolder, "OpenViewer.ini");
                if (System.IO.File.Exists(iniconfig))
                {
                    readConfig = true;
                }
                else
                {
                    readConfig = OpenViewer.Viewer.CreateDefaultConfig(iniconfig);
                }

                if (readConfig)
                {
                    m_config.Load(iniconfig);
                }
                else
                {
                    VUtil.LogConsole(this.ToString(), "SetupConfig: ERROR: can't load config file.");
                }
            }
            catch (Exception e)
            {
                VUtil.LogConsole(this.ToString(), "SetupConfig: FETAL:" + e.Message);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 2)
            {
                viewer.Shutdown();
            }
            base.WndProc(ref m);
        }
    }
}
