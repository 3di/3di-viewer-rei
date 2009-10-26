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
using System.Xml;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;

using OpenViewer;
using OpenViewer.bootstrap;
using System.Threading;
using System.Security.Permissions;

[assembly: CLSCompliant(true)]
namespace OpenViewerAX
{
    [ComImport]
    [Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IObjectSafety
    {
        [PreserveSig]
        int GetInterfaceSafetyOptions(ref Guid riid, out int pdwSupportedOptions, out int pdwEnabledOptions);

        [PreserveSig]
        int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions);
    }

    [ComSourceInterfaces(typeof(IJavaScriptApiEvent))]
    [ComVisible(true)]
    [ProgId("OpenViewerAX.ActiveControl")]
    [Guid("AB809708-8AA8-4AA8-9E31-7A16213F46CD")]
    public partial class OpenViewerCtl : UserControl, IObjectSafety, IJavaScriptApi, IDisposable
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        private static extern bool InvalidateRect(IntPtr hwnd, IntPtr lpRect, bool bErase);

        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hwnd, Int32 Msg, IntPtr wParam, IntPtr lParam);


        // Constants for implementation of the IObjectSafety interface.
        private const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        private const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;
        private const int S_OK = 0;
        private const int WINDOW_WIDTH_MAX = 1024;
        private const int WINDOW_WIDTH_MIN = 0;
        private const int WINDOW_WIDTH_DEF = 800;
        private const int WINDOW_HEIGHT_MAX = 768;
        private const int WINDOW_HEIGHT_MIN = 0;
        private const int WINDOW_HEIGHT_DEF = 600;

        // Constants for DISPIDs of the Value and Enabled properties.
        internal const int DISPID_VALUE = 0;
        internal const int DISPID_ENABLED = 1;

        public int QueryVersion() { return(1); }

        protected override bool CanEnableIme
        {
            get
            {
                return (true);
            }
        }

        private OpenViewer.Viewer OV;

        #region DHTML API Properties
        // ******************* DHTML API *******************
        // Properties to support direct login parameters from within the DHTML page that displays the ActiveX control
        // Supported parameters are:
        // -------------------------------------------------
        // ServerURI : plain text uri
        // FirstName : plain text first name | Name is formed as FirstName.ToLower() + " " + LastName.ToLower()
        // LastName  : plain text last name  |
        // Password  : plain text password
        //
        // ******************* DHTML API *******************
        private string backGroundColor;
        [ComVisible(true)]
        public string BackgroundColor { get { return (this.backGroundColor); } set { this.backGroundColor = value; OV.BackgroundColor = value; } }

        private string progressColor;
        [ComVisible(true)]
        public string ProgressColor { get { return (this.progressColor); } set { this.progressColor = value; OV.ProgressColor = value; } }

        #region Init window settings
        private string initBackgroundMusicURL;
        [ComVisible(true)]
        public string InitBackgroundMusicURL { get { return (this.initBackgroundMusicURL); } set { this.initBackgroundMusicURL = value; OV.InitBackgroundMusicURL = value; } }

        private string initBackgroundURL;
        [ComVisible(true)]
        public string InitBackgroundURL { get { return (this.initBackgroundURL); } set { this.initBackgroundURL = value; OV.InitBackgroundURL = value; } }
        #endregion

        #region Login settings
        private string loginLoginMode;
        [ComVisible(true)]
        public string LoginMode { get { return (this.loginLoginMode); } set { this.loginLoginMode = value; OV.LoginMode = value; } }

        private string loginServerURI;
        [ComVisible(true)]
        public string ServerURI { get { return (this.loginServerURI); } set { this.loginServerURI = value; OV.ServerURI = value; } }

        private string loginFirstName;
        [ComVisible(true)]
        public string FirstName { get { return (this.loginFirstName); } set { this.loginFirstName = value; OV.FirstName = value; } }

        private string loginLastName;
        [ComVisible(true)]
        public string LastName { get { return (this.loginLastName); } set { this.loginLastName = value; OV.LastName = value; } }

        private string loginPassword;
        [ComVisible(true)]
        public string Password { get { return (this.loginPassword); } set { this.loginPassword = value; OV.Password = value; } }

        private string loginBackgroundURL;
        [ComVisible(true)]
        public string LoginBackgroundURL { get { return (this.loginBackgroundURL); } set { this.loginBackgroundURL = value; OV.LoginBackgroundURL = value; } }

        private string loginLocation;
        [ComVisible(true)]
        public string LoginLocation { get { return (this.loginLocation); } set { this.loginLocation = value; OV.LoginLocation = value; } }

        private string dHTMLRelationEnable;
        [ComVisible(true)]
        public string DHTMLRelationEnable { get { return (this.dHTMLRelationEnable); } set { this.dHTMLRelationEnable = value; OV.DHTMLRelationEnable = value; } }

        private string worldTime;
        [ComVisible(true)]
        public string WorldTime { get { return (this.worldTime); } set { this.worldTime = value; OV.SetWorldTime(worldTime); } }
        #endregion

        #region Draw Setting
        private string windowWidth;
        private int wwidth = 800;
        [ComVisible(true)]
        public string WindowWidth
        {
            get
            {
                return (this.windowWidth);
            }
            
            set
            {
                this.windowWidth = value;

                wwidth = int.Parse(value);
#if !DEBUG
                if (wwidth > WINDOW_WIDTH_MAX)
                {
                    wwidth = WINDOW_WIDTH_DEF;
                }
                else if (wwidth < WINDOW_WIDTH_MIN)
                {
                    wwidth = WINDOW_WIDTH_DEF;
                }
#endif
                this.Size = new Size(wwidth, wheight);
                OV.Width = wwidth;
            }
        }

        private string windowHeight;
        private int wheight = 600;
        [ComVisible(true)]
        public string WindowHeight
        {
            get
            {
                return (this.windowHeight);
            }

            set
            {
                this.windowHeight = value;

                wheight = int.Parse(value);
#if !DEBUG
                if (wheight > WINDOW_HEIGHT_MAX)
                {
                    wheight = WINDOW_HEIGHT_DEF;
                }
                else if (wheight < WINDOW_HEIGHT_MIN)
                {
                    wheight = WINDOW_HEIGHT_DEF;
                }
#endif

                this.Size = new Size(wwidth, wheight);
                OV.Height = wheight;
            }
        }

        private string drawTerrain;
        [ComVisible(true)]
        public string DrawTerrain { get { return (this.drawTerrain); } set { this.drawTerrain = value; OV.DrawTerrain = value; } }

        private string drawShadow;
        [ComVisible(true)]
        public string DrawShadow { get { return (this.drawShadow); } set { this.drawShadow = value; OV.DrawShadow = value; } }

        private string drawSea;
        [ComVisible(true)]
        public string DrawSea { get { return (this.drawSea); } set { this.drawSea = value; OV.DrawSea = value; } }

        private string drawSky;
        [ComVisible(true)]
        public string DrawSky { get { return (this.drawSky); } set { this.drawSky = value; OV.DrawSky = value; } }

        private string drawMenu;
        [ComVisible(true)]
        public string DrawMenu { get { return (this.drawMenu); } set { this.drawMenu = value; OV.DrawMenu = value; } }

        private string tickOn;
        [ComVisible(true)]
        public string TickOn { get { return (this.tickOn); } set { this.tickOn = value; OV.TickOn = value; } }

        private string worldAmbientColor;
        [ComVisible(true)]
        public string WorldAmbientColor { get { return (this.worldAmbientColor); } set { this.worldAmbientColor = value; OV.WorldAmbientColor = value; } }

        #region 11. Fix directional
        private string isFixDirectional;
        [ComVisible(true)]
        public string IsFixDirectional { get { return (this.isFixDirectional); } set { this.isFixDirectional = value; OV.FixDirectional = value; } }

        private string fixDirectionalDirection;
        [ComVisible(true)]
        public string FixDirectionalDirection { get { return (this.fixDirectionalDirection); }
            set
            {
                string s_value = "1.75,0,0";
                string text = value;
                bool success = false;
                if (!string.IsNullOrEmpty(text))
                {
                    string[] elem = text.Split(',');
                    if (elem.Length == 3)
                    {
                        float f = 0;
                        if (float.TryParse(elem[0], out f) && float.TryParse(elem[1], out f) && float.TryParse(elem[2], out f))
                        {
                            success = true;
                        }
                    }
                }

                if (success)
                    s_value = text;

                this.fixDirectionalDirection = s_value;
                OV.FixDirectionalRotation = s_value;
            }
        }

        private string fixDirectionalDiffuseColor;
        [ComVisible(true)]
        public string FixDirectionalDiffuseColor { get { return (this.fixDirectionalDiffuseColor); } set { this.fixDirectionalDiffuseColor = value; OV.FixDirectionalDiffuseColor = value; } }

        private string fixDirectionalAmbientColor;
        [ComVisible(true)]
        public string FixDirectionalAmbientColor { get { return (this.fixDirectionalAmbientColor); } set { this.fixDirectionalAmbientColor = value; OV.FixDirectionalAmbientColor = value; } }
        #endregion

        #endregion

        #region Camera Setting
        [ComVisible(true)]
        public string CameraStartDistance
        {
            get
            {
                return OV.CameraDistance.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    v = v < 0.1f ? 0.1f : v;
                    OV.CameraDistance = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraKeyWalkingDistance
        {
            get
            {
                return OV.CameraKeyWalkingDistance.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    v = v < 0.1f ? 0.1f : v;
                    OV.CameraKeyWalkingDistance = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraMinDistance
        {
            get
            {
                return OV.CameraMinDistance.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    v = v < 0.1f ? 0.1f : v;
                    OV.CameraMinDistance = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraMaxDistance
        {
            get
            {
                return OV.CameraMaxDistance.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    v = v < 0.2f ? 0.2f : v;
                    OV.CameraMaxDistance = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraFOV
        {
            get
            {
                return OV.CameraFOV.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    float maxDegreeValue = 180;

                    v = Util.Clamp<float>(v, 0.00001f, (maxDegreeValue / 180.0f * (float)Math.PI));
                    OV.CameraFOV = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraOffsetY
        {
            get
            {
                return OV.CameraOffsetY.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    OV.CameraOffsetY = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraMinAngleY
        {
            get
            {
                return OV.CameraMinAngleY.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    float epsilon = 0.01f;
                    v = Util.Clamp<float>(v, epsilon, (float)Math.PI - epsilon);

                    OV.CameraMinAngleY = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraMaxAngleY
        {
            get
            {
                return OV.CameraMaxAngleY.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    float epsilon = 0.01f;
                    v = Util.Clamp<float>(v, epsilon, (float)Math.PI - epsilon);

                    OV.CameraMaxAngleY = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraDefaultAngleY
        {
            get
            {
                return OV.CamRotationAnglePHI.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    OV.CamRotationAnglePHI = v;
                    OV.CamDefaultRotationAnglePHI = v;
                }
            }
        }

        [ComVisible(true)]
        public string CameraDefaultAngleX
        {
            get
            {
                return OV.CamRotationAngleTHETA.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    OV.CamRotationAngleTHETA = v;
                    OV.CamDefaultRotationAngleTHETA = v;
                }
            }
        }
        [ComVisible(true)]
        public string AvatarDisappearDistance
        {
            get
            {
                return OV.AvatarDisappearDistance.ToString();
            }

            set
            {
                float v = 0;
                if (float.TryParse(value, out v))
                {
                    OV.AvatarDisappearDistance = v;
                }
            }
        }
        #endregion

        #region Move Setting
        public string SetStandUpIcon
        {
            get { return OV.IsStandUpIcon.ToString().ToLower(); }
            set
            {
                bool flag = false;
                if (bool.TryParse(value, out flag))
                {
                    OV.IsStandUpIcon = flag;
                }
            }
        }
        #endregion

        #region Version Setting
        private string requireVersion;
        [ComVisible(true)]
        public string RequireVersion { get { return (this.requireVersion); } set { this.requireVersion = value; } }
        #endregion

        #endregion

        private System.Threading.Timer keepAliveTimer;
        
        #region Version Checker
        private readonly OpenViewerConfigSource m_config = new OpenViewerConfigSource();
        private Version version = OpenViewer.bootstrap.VersionInfo.version;
        private Version installedVersion;

        private bool InitializeVersion()
        {
            foreach (object o in typeof(OpenViewerCtl).GetCustomAttributes(false))
            {
                if (o is GuidAttribute)
                {
                    GuidAttribute guidattr = (GuidAttribute)o;
                    string key = (String)Registry.GetValue("HKEY_CLASSES_ROOT\\CLSID\\{" + guidattr.Value + "}\\InstalledVersion", "", "0,0,0,0");
                    installedVersion = new Version(key.Replace(",", "."));
                    return true;
                }
            }
            return false;
        }

        private void VersionCheckError(string mesg, string title)
        {
            System.Windows.Forms.MessageBox.Show(mesg, title, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        private bool IsVersionRequirementSatisfied(string requirement)
        {
            try
            {
                return new Version(requirement.Replace(",",".")) <= version;
            }
            catch
            {
                return true;
            }
        }

        private bool CheckCurrentVersion(string xml)
        {
            string description = null;
            string title = null;
            Version currentReleaseVersion = null;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(xml);

                foreach (XmlNode toplevel in xmlDoc.ChildNodes)
                {
                    if (toplevel.Name == "OpenViewer")
                    {
                        foreach (XmlNode secondary in toplevel.ChildNodes)
                        {
                            if (secondary.Name == "LatestRelease")
                            {
                                foreach (XmlNode leaf1 in secondary.ChildNodes)
                                {
                                    if (leaf1.Name == "Type" && leaf1.InnerText.Equals("stable", StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (XmlNode leaf2 in secondary.ChildNodes)
                                        {
                                            if (leaf2.Name == "Version")
                                            {
                                                currentReleaseVersion = new Version(leaf2.InnerText.Replace(",","."));
                                            }
                                            if (leaf2.Name == "Description")
                                            {
                                                description = leaf2.InnerText;
                                            }
                                            if (leaf2.Name == "Title")
                                            {
                                                title = leaf2.InnerText;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (currentReleaseVersion != null && version < currentReleaseVersion)
                {
                    if (!String.IsNullOrEmpty(description))
                    {
                        if (title == null)
                            title = "3Di OpenViewer Update";
                        System.Windows.Forms.MessageBox.Show(description, title, System.Windows.Forms.MessageBoxButtons.OK);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                VersionCheckError(e.Message, "バージョンを確認中にエラーが発生しました");
                return false;
            }
            return true;
        }

        private void VersionCheckCallback(Object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                CheckCurrentVersion(e.Result);
            }
            else
            {
            }
        }

        private bool ShowVersionCheckWindow(string uriString)
        {
            Uri uri = new Uri(uriString);
            WebClient client = new WebClient();
            try
            {
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(VersionCheckCallback);
                client.DownloadStringAsync(uri);
            }
            catch (Exception e)
            {
                VersionCheckError(e.Message, "最新バージョンを問い合わせ中にエラーが発生しました");
                return false;
            }
            return true;
        }

        private bool CheckDriveSize()
        {
            bool flag = true;

            try
            {
                string driveName = Path.GetPathRoot(Application.ExecutablePath);
                DriveInfo currentDrive = new DriveInfo(driveName);

                // to MB.
                long size = currentDrive.TotalFreeSpace / (1024 * 1024);

                DialogResult res = DialogResult.OK;
                if (size < 500) // 500MB
                {
                    res = System.Windows.Forms.MessageBox.Show("「HDDの空き容量が足りないために正常に起動できませんでした。", "起動エラー");
                    res = DialogResult.Cancel;
                }
                else if (size < 3 * 1024) // 3GB
                {
                    res = System.Windows.Forms.MessageBox.Show("正常に動作するために必要なHDD容量（3GB）が足りていません\nこのまま起動しますか？", "起動確認", MessageBoxButtons.OKCancel);
                }

                if (res != DialogResult.OK)
                    flag = false;
            }
            catch (Exception e)
            {
                VUtil.LogConsole(this.ToString(), "CheckDriveSize:" + e.Message);
            }

            return flag;
        }

        private void SetupConfig()
        {
            try
            {
                bool readConfig = false;
                string iniconfig = Path.Combine(Util.ConfigFolder, "OpenViewer.ini");
                if (File.Exists(iniconfig))
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
        #endregion
        
        public OpenViewerCtl()
        {
            InitializeComponent();
            InitializeVersion();
            OV = new OpenViewer.Viewer();
            OV.Adapter = new Adapter();
        }

        private void OpenViewerCtl_Load(object sender, EventArgs e)
        {
            if (CheckDriveSize() == false)
                return;

            this.Show();
            
            // Initial checking
            // Check if another instance is running
            Util.KeepaliveUUID = OpenMetaverse.UUID.Random();
            if (Util.IsInitSafe())
            {
                SetupConfig();

                if (!String.IsNullOrEmpty(RequireVersion) && !IsVersionRequirementSatisfied(RequireVersion))
                {
                    Label warning = new Label();
                    warning.Text = "本サービスをご使用いただくには、3Di OpenViewerのアップデートが必要です。";
                    warning.Location = new Point(10, Height / 2 - 40 + 30);
                    warning.Size = new Size(Width - 20, 40);
                    warning.ForeColor = Color.Red;
                    warning.BackColor = Color.White;
                    warning.TextAlign = ContentAlignment.MiddleCenter;
                    this.Controls.Add(warning);
                    LinkLabel link = new LinkLabel();
                    link.Text = "最新の情報はこちらでご覧いただけます。";
                    link.Links.Add(6, 3);
                    link.Location = new Point(10, Height / 2 + 30);
                    link.Size = new Size(Width - 20, 40);
                    link.ForeColor = Color.Red;
                    link.BackColor = Color.White;
                    link.TextAlign = ContentAlignment.MiddleCenter;
                    link.Click += new EventHandler(link_Click);
                    this.Controls.Add(link);
                    PictureBox bg = new PictureBox();
                    Bitmap bitmap = new Bitmap(Util.ApplicationDataDirectory + @"\media\gui\background\login_background.png");
                    bg.ClientSize = new Size(bitmap.Width, bitmap.Height);
                    bg.Location = new Point((Width - bitmap.Width) / 2,(Height - bitmap.Height) / 2);
                    bg.Image = bitmap; 
                    this.Controls.Add(bg);
                    return;
                }
                string versionServer = m_config.Source.Configs["Startup"].Get("version_server_url", null);
                if (versionServer != null)
                {
                    ShowVersionCheckWindow(versionServer);
                }
                keepAliveTimer = new System.Threading.Timer(new TimerCallback(Util.KeepAlive), null, 0, 5000);

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
                    OV.VoiceWaitTime = debug.GetInt("voice_wait_time", Viewer.DEFAULT_DEBUG_VOICE_WAIT_TIME);
                }

                // activex --> javascript.
                if (OV.IsDHTMLRelationEnable)
                {
                    OV.Adapter.OnDebugMessage += DebugMessageEventHandler;
                    OV.Adapter.OnTouched += TouchedEventHandler;
                    OV.Adapter.OnReceiveMessage += ReceiveMessageEventHandler;
                    OV.Adapter.OnTeleport += TeleportEventHandler;
                    OV.Adapter.OnTeleported += TeleportedEventHandler;
                    OV.Adapter.OnOpenWindow += OpenWindowEventHandler;
                    OV.Adapter.OnAvatarPicked += AvatarPickEventHandler;
                    OV.Adapter.OnStateChanged += StateChangedEventHandler;
                    OV.Adapter.OnDispatch += DispatchHandler;
                }

                OV.Version = version;
                OV.HelpURL = helpURL;
                OV.Locale = locale;
                OV.IsVisibleDebutTab = (debugTab == "true" ? true : false);
                OV.IsCameraPitchReverse = (cameraReverse == "true" ? true : false);
                OV.SetShaderQuality(OpenViewer.Managers.ShaderManager.ShaderType.Sea, seaQuality);
                OV.SetShaderQuality(OpenViewer.Managers.ShaderManager.ShaderType.AdvancedSea, seaQuality);
                OV.SetShaderQuality(OpenViewer.Managers.ShaderManager.ShaderType.Sky, skyQuality);

                OV.TeleportTimeout = teleportTimeout * 10000000;

                OV.Startup(this.Handle);
            }
            else
            {
                Label warning = new Label();
                warning.Text = "複数の3Di OpenViewerを同時に使用することはできません\r\nこちらのページをご覧になる前に、ご使用中の3Di OpenViewerをすべて閉じてください。";
                warning.Location = new Point(10, Height / 2 - 20 + 40);
                warning.Size = new Size(Width - 20, 40);
                warning.ForeColor = Color.Red;
                warning.BackColor = Color.White;
                warning.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(warning);
                PictureBox bg = new PictureBox();
                Bitmap bitmap = new Bitmap(Util.ApplicationDataDirectory + @"\media\gui\background\login_background.png");
                bg.ClientSize = new Size(bitmap.Width, bitmap.Height);
                bg.Location = new Point((Width - bitmap.Width) / 2, (Height - bitmap.Height) / 2);
                bg.Image = bitmap;
                this.Controls.Add(bg);
            }
        }

        void link_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(m_config.Source.Configs["Startup"].Get("help_url", "http://3di-opensim.com/openviewer/"));
        }

        protected override void Dispose(bool disposing)
        {
            if (m_config != null)
                m_config.Dispose();

            //if (OV != null)
            //    OV = null;

            if (keepAliveTimer != null)
            {
                keepAliveTimer.Dispose();
                Util.CloseKeepAlive();
            }

            System.GC.Collect();

            base.Dispose(disposing);
        }

        ~OpenViewerCtl()
        {
            if (keepAliveTimer != null)
            {
                keepAliveTimer.Dispose();
                Util.CloseKeepAlive();
            }
        }
        
        int IObjectSafety.GetInterfaceSafetyOptions(ref Guid riid, out int pdwSupportedOptions, out int pdwEnabledOptions)
        {
            pdwSupportedOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
            pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
            return S_OK;   // return S_OK
        }

        int IObjectSafety.SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            return S_OK;   // return S_OK
        }

        #region ComInteropのためのコード
        [ComRegisterFunction()]
        private static void RegisterClass(string key)
        {
            // Strip off HKEY_CLASSES_ROOT\ from the passed key as I don't need it
            StringBuilder sb = new StringBuilder(key);
            System.Console.WriteLine(key);
            sb.Replace(@"HKEY_CLASSES_ROOT\", "");

            // Open the CLSID\{guid} key for write access
            RegistryKey k = Registry.ClassesRoot.OpenSubKey(sb.ToString(), true);

            // And create the 'Control' key - this allows it to show up in 
            // the ActiveX control container 
            RegistryKey ctrl = k.CreateSubKey("Control");
            ctrl.Close();

            // Next create the CodeBase entry - needed if not string named and GACced.

            RegistryKey inprocServer32 = k.OpenSubKey("InprocServer32", true);
            inprocServer32.SetValue("CodeBase", Assembly.GetExecutingAssembly().CodeBase);
            inprocServer32.Close();

            k.CreateSubKey("Implemented Categories");
            RegistryKey implementedCategories = k.OpenSubKey("Implemented Categories", true);
            implementedCategories.CreateSubKey("{7DD95801-9882-11CF-9FA9-00AA006C42C4}");
            implementedCategories.CreateSubKey("{7DD95802-9882-11CF-9FA9-00AA006C42C4}");
            implementedCategories.Close();

            k.CreateSubKey("InstalledVersion");
            // Finally close the main key
            k.Close();
        }
        [ComUnregisterFunction()]
        private static void UnregisterClass(string key)
        {
            StringBuilder sb = new StringBuilder(key);
            sb.Replace(@"HKEY_CLASSES_ROOT\", "");

            // Open HKCR\CLSID\{guid} for write access
            RegistryKey k = Registry.ClassesRoot.OpenSubKey(sb.ToString(), true);

            // Delete the 'Control' key, but don't throw an exception if it does not exist
            k.DeleteSubKey("Control", false);

            // Next open up InprocServer32
            k.OpenSubKey("InprocServer32", true);
            // And delete the CodeBase key, again not throwing if missing 
            k.DeleteSubKey("CodeBase", false);

            k.DeleteSubKey("InstalledVersion", false);

            k.DeleteSubKeyTree("Implemented Categories");
            // Finally close the main key 
            k.Close();
        }
        #endregion

        /* Javascript exposed API */
        #region 0. Debug
        // To javascript.

        public event OpenViewer.DebugMessageListener OnDebugMessage;

        private void DebugMessageEventHandler(string _message)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            if (OnDebugMessage != null)
                OnDebugMessage(_message);
        }

        public string GetPluginInfo()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetPluginInfo");

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Version\":" + "\"" + version.ToString() + "\"");
            sb.Append(",");
            sb.Append("\"Type\":" + "\"ActiveX\"");
            sb.Append("}");

            return sb.ToString();
        }
        #endregion

        #region 1. Login / Logput

        // From javascript.
        public void Login(string _firstName, string _lastName, string _password, string _serverURL, string _loginLocation)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: Login");

            OV.Adapter.CallLogin(_firstName, _lastName, _password, _serverURL, _loginLocation);
        }

        // From javascript.
        public void Logout()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: Logout");

            OV.Adapter.CallLogout();
        }
        #endregion

        #region 2. Touch
        public event OpenViewer.TouchToListener OnTouched;

        // From javascript.
        public void TouchTo(string _uuid)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: TouchTo");

            OV.Adapter.CallTouchTo(_uuid);
        }

        // To javascript.
        private void TouchedEventHandler(string _uuid)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: TouchEventHandler");

            if (OnTouched != null)
                OnTouched(_uuid);
        }
        #endregion

        #region 3. Sit / Stand

        public void SitOn(string _uuid)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SitOn");

            OV.Adapter.CallSitOn(_uuid);
        }

        public void StandUp()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: StandUp");

            OV.Adapter.CallStandUp();
        }

        #endregion

        #region 4. Text Chat
        public event OpenViewer.OnReceiveMessageListener OnReceivedMessage;

        // From javascript.
        public void SendIM(string _target_uuid, string _message)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SendIM UUID:" + _target_uuid + " MSG: " + _message);

            OV.Adapter.CallSendIM(_target_uuid, _message);
        }

        public void SendChat(string _message, int _range)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SendChat Channel:" + " Range:" + _range.ToString());

            OV.Adapter.CallSendChat(_message, _range);
        }

        // To javascript.
        private void ReceiveMessageEventHandler(string _uuid, string _avatarName, string _message)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: ReceiveMessageEventHandler");

            if (OnReceivedMessage != null)
                OnReceivedMessage(_uuid, _avatarName, _message);
        }

        public string GetMessageFromHistory(int _index)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetMessageFromHistory");

            return OV.Adapter.CallGetMessageFromHistory(_index);
        }

        public int GetMessageHistoryLength()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return 0;

            DebugMessageEventHandler("OUT: GetMessageHistoryLength");

            return OV.Adapter.CallGetMessageHistoryLength();
        }
        #endregion

        #region 5. Teleport
        public event OpenViewer.TeleportToListener OnTeleport;
        public event OpenViewer.TeleportListener OnTeleported;

        // From javascript.
        public void TeleportTo(string _regionName, int _x, int _y, int _z)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: TeleportTo");

            OV.Adapter.CallTeleportTo(_regionName, _x, _y, _z);
        }

        // To javascript.
        private void TeleportEventHandler(string _regionName, int _x, int _y, int _z)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: TeleportEventHandler");

            if (OnTeleport != null)
                OnTeleport(_regionName, _x, _y, _z);
        }

        // To javascript.
        private void TeleportedEventHandler(string _uuid, string _avatar, int _x, int _y, int _z)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: TeleportedEventHandler");

            if (OnTeleported != null)
                OnTeleported(_uuid, _avatar, _x, _y, _z);
        }
        #endregion

        #region 6. LSL triggered html related manupuration
        public event OpenViewer.OpenWindowListener OnOpenWindow;

        // To javascript.
        private void OpenWindowEventHandler(string _target, string _uri)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: OpenWindowEventHandler");

            if (OnOpenWindow != null)
                OnOpenWindow(_target, _uri);
        }
        #endregion

        #region 7. User avatar
        public event OpenViewer.AvatarPickListener OnAvatarPicked;

        // To javascript.
        private void AvatarPickEventHandler(string _userInformation)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: AvatarPickEventHandler");

            if (OnAvatarPicked != null)
                OnAvatarPicked(_userInformation);
        }

        public string GetLoggedinAvatarUUIDList()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetLoggedInAvatarUUIDList");

            return OV.Adapter.CallGetLoggedinAvatarUUIDList();
        }

        public void StartCustomAnimation(int _index)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: StartCustomAnimation index:" + _index.ToString());

            OV.Adapter.CallAvatarCustomizeAnimation(_index);
        }

        public string GetUserUUID()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetUserUUID");

            return OV.Adapter.CallGetUserUUID();
        }

        public string GetUserAvatarPosition()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetUserAvatarPosition");

            return OV.Adapter.CallGetUserAvatarPosition();
        }

        public string GetUserAvatarUUID()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetUserAvatarUUID");

            return OV.Adapter.CallGetUserAvatarUUID();
        }

        public string GetUserAvatarName()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetUserAvatarName");

            return OV.Adapter.CallGetUserAvatarName();
        }

        public void UserAvatarUp()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: UserAvatarUp");

            OV.Adapter.CallUserAvatarUp(true);
            OV.Adapter.CallUserAvatarDown(false);
        }

        public void UserAvatarDown()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: UserAvatarDown");

            OV.Adapter.CallUserAvatarUp(false);
            OV.Adapter.CallUserAvatarDown(true);
        }

        public void UserAvatarLeft()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: UserAvatarLeft");

            OV.Adapter.CallUserAvatarLeft();
        }

        public void UserAvatarRight()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: UserAvatarRight");

            OV.Adapter.CallUserAvatarRight();
        }
        #endregion

        #region 8. Common
        public event OpenViewer.StateChangedListener OnStateChanged;

        // To javascript.
        private void StateChangedEventHandler(int _state)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("EVE: StateChangedEventHandler");

            if (OnStateChanged != null)
                OnStateChanged(_state);
        }

        public string GetFPS()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetFPS");

            return OV.Adapter.CallGetFPS().ToString();
        }

        public string GetPrimitiveCount()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetPrimitiveCount");

            return OV.Adapter.CallGetPrimitiveCount().ToString();
        }

        public string GetTextureCount()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetTextureCount");

            return OV.Adapter.CallGetTextureCount().ToString();
        }
        #endregion

        #region 9. Camera

        // From javascript.
        public void CameraLookAt(float _px, float _py, float _pz, float _tx, float _ty, float _tz)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: CameraLookAt");

            OV.Adapter.CallCameraLookAt(_px, _py, _pz, _tx, _ty, _tz);
        }

        // From javascript.
        public void SetCameraDistance(float _distance)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetCameraDistance");

            OV.Adapter.CallSetCameraDistance(_distance);
        }

        // To javascript.
        public string GetCameraDistance()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: SetCameraDistance");

            return OV.Adapter.CallGetCameraDistance();
        }

        // To javasctipt.
        public string GetCameraPosition()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetCameraPosition");

            return OV.Adapter.CallGetCameraPosition();
        }

        // To javasctipt.
        public string GetCameraTarget()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetCameraTarget");

            return OV.Adapter.CallGetCameraTarget();
        }

        public string GetCameraFov()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetCameraFov");

            return OV.Adapter.CallGetCameraFOV();
        }

        public void SetCameraFov(float _fov)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetCameraFov");

            OV.Adapter.CallSetCameraFOV(_fov);
        }

        public void SetCameraFovDegree(float _fov)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetCameraFovDegree");

            OV.Adapter.CallSetCameraFOVDegree(_fov);
        }

        public string GetCameraOffsetY()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetCameraOffsetY");

            return OV.Adapter.CallGetCameraOffsetY();
        }

        public void SetCameraOffsetY(float _offsetY)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetCameraOffsetY");

            OV.Adapter.CallSetCameraOffsetY(_offsetY);
        }

        public string GetCameraAngleY()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetCameraAngleY");

            return OV.Adapter.CallGetCameraAngleY();
        }

        public void SetCameraAngleY(float _min, float _max)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetCameraAngleY");

            float epsilon = 0.01f;
            _min = Util.Clamp<float>(_min, epsilon, (float)Math.PI - epsilon);
            _max = Util.Clamp<float>(_max, epsilon, (float)Math.PI - epsilon);

            OV.Adapter.CallSetCameraAngleY(_min, _max);
        }

        public void SetAvatarDisappearDistance(float _value)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetAvatarDisappearDistance distance:" + _value.ToString());

            float value = (_value >= 0 ? _value : 0);

            OV.AvatarDisappearDistance = value;
        }
        #endregion

        #region 10. World
        public string GetAvatarCount()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetAvatarCount");

            return OV.Adapter.CallGetAvatarCount();
        }

        public string GetObjectCount()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetObjectCount");

            return OV.Adapter.CallGetObjectCount();
        }

        public string GetRegionName()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetRegionName");

            return OV.Adapter.CallGetRegionName();
        }

        public string GetWorldTime()
        {
            if (OV.IsDHTMLRelationEnable == false)
                return string.Empty;

            DebugMessageEventHandler("OUT: GetWorldTime");

            return OV.Adapter.CallGetWorldTime();
        }

        public void SetWorldTime(string _dataTime)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetWorldTime");

            OV.Adapter.CallSetWorldTime(_dataTime);
        }

        public void SetTickOn(string _flag)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetTickOn");

            OV.Adapter.CallSetTickOn(_flag);
        }

        public void SetWorldAmbientColor(string _colors)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetWorldAmbientColor");

            OV.Adapter.CallSetWorldAmbientColor(_colors);
        }
        #endregion

        #region 11. Fix directional
        public void SetFixDirectional(string _flag)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetFixDirectional");

            OV.Adapter.CallSetFixDirectional(_flag);
        }

        public void SetFixDirectionalRotation(string _radRotation)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetFixDirectionalRotation");

            OV.Adapter.CallSetFixDirectionalRotation(_radRotation);
        }

        public void SetFixDirectionalDiffuseColor(string _colors)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetFixDirectionalDiffuseColor");

            OV.Adapter.CallSetFixDirectionalDiffuseColor(_colors);
        }

        public void SetFixDirectionalAmbientColor(string _colors)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;

            DebugMessageEventHandler("IN: SetFixDirectionalAmbientColor");

            OV.Adapter.CallSetFixDirectionalAmbientColor(_colors);
        }
        #endregion

        #region 13. Callback and Dispatch
        // To javascript
        public event OpenViewer.DispatchListener OnDispatch;

        private void DispatchHandler(string action, string message)
        {
            if (OV.IsDHTMLRelationEnable == false)
                return;
 
            DebugMessageEventHandler("EVE: Dispatch: " + action + "(" + message + ")");
 
            if (OnDispatch != null)
                OnDispatch(action, message);
        }

        // From javascript.
        public string Callback(string action, string message)
         {
            //System.Windows.Forms.MessageBox.Show("Meh");
             if (OV.IsDHTMLRelationEnable == false)
                return "";
 
            DebugMessageEventHandler("IN: Callback(" + action + ", " + message + ")");
 
            return (OV.Adapter.RunCallback(action, message));
        }
        #endregion

        /* Initialize function */
        #region Init
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // OpenViewerCtl
            // 
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.Name = "OpenViewerCtl";
            this.Size = new System.Drawing.Size(wwidth, wheight);
            this.Load += new System.EventHandler(this.OpenViewerCtl_Load);
            this.ResumeLayout(false);

        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0xf;
            const int WM_NCCREATE = 0x81;
            
            if (m.Msg == WM_PAINT)
            {
                Keys keyCode = (Keys)m.WParam & Keys.KeyCode;
                IntPtr p = GetAncestor(Handle, 1);
                InvalidateRect(p, IntPtr.Zero, true);
            }

            if (m.Msg == WM_NCCREATE)
            {
                Size = new Size(wwidth, wheight);
            }

            base.WndProc(ref m);
            if (m.Msg == 2)
            {
                //System.Windows.Forms.MessageBox.Show("WM_DESTROY");
                if (OV != null)
                {
                    //System.Windows.Forms.MessageBox.Show("OV not null");
                    OV.Shutdown();
                    if (keepAliveTimer != null)
                    {
                        keepAliveTimer.Dispose();
                        Util.CloseKeepAlive();
                    }
                }
                return;
            }
        }
        #endregion
    }
}
