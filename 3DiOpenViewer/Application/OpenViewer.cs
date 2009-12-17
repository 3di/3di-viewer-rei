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
 * 
 * Additionally, portions of this file bear the following BSD-style license
 * from the IdealistViewer project (URL http://idealistviewer.org/):
 * 
 * Copyright (c) Contributors, http://idealistviewer.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenViewer Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using IrrlichtNETCP;
using IrrlichtNETCP.Extensions;
using OpenViewer.Managers;
using log4net;
using OpenMetaverse;

namespace OpenViewer
{
    public class Viewer
    {
        private const int DEFAULT_AVATAR_NAME_TYPE = 0; // 0:[first last] 1:[first] 2:[last] 3:none
        private const float DEFAULT_FIRST_DISTANCE = 5f;
        private const float DEFAULT_MIN_DISTANCE = 2f;
        private const float DEFAULT_MAX_DISTANCE = 32f;
        private const float DEFAULT_CAMRA_FOV = 0.7853985f; // deg45 (1.047198f; // deg60)
        private const float DEFAULT_CAMRA_OFFSET_Y = 1.2f;
        private const float DEFAULT_CAMERA_ROTATION_ANGLE_PHI = 1.57f;
        private const float DEFAULT_CAMERA_ROTATION_ANGLE_THETA = 3.14f;
        private const float DEFAULT_AVATAR_DISAPPEAR_DISTANCE = 2;
        private const int DEFAULT_CACHE_MB_SIZE = 300;

        public const int DEFAULT_DEBUG_VOICE_WAIT_TIME = 1; // sec

        #region Private members
        private RenderForm f = null;
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static IrrlichtNETCP.Quaternion Coordinate_XYZ_XZY =
            new IrrlichtNETCP.Quaternion(
                new IrrlichtNETCP.Matrix4( // row-major, with translations in 4th row
                    new float[]
                    { 
                        1,0,0,0, // x-axis
                        0,0,1,0, // our y axis is original z axis
                        0,1,0,0, // our z axis is original y axis
                        0,0,0,1, // no translation
                    }
                    ));
        private static bool LMheld = false;
        private static bool RMheld = false;
        private static bool MMheld = false;
        private int OldMouseX = 0;
        private int OldMouseY = 0;
        private System.Threading.Thread guithread;
        private System.Threading.Thread supportPage;
        private bool firstLogin = true;
        private EventHandler OnRequest;
        private bool debug_on = false;

        private List<IManagerPlugin> m_managers = new List<IManagerPlugin>();

        #endregion

        #region Properties
        private IntPtr renderTarget;
        public IntPtr RenderTarget
        {
            get { return (renderTarget); }
            set { renderTarget = value; }
        }

        private readonly OpenViewerConfigSource m_config = new OpenViewerConfigSource();
        public OpenViewerConfigSource Config { get { return m_config; } }

        private IrrlichtDevice device;
        public IrrlichtDevice Device { get { return (device); } }

        private RefController reference;
        public RefController Reference { get { return (reference); } }

        private int width = 800;
        public int Width { set { width = value; } get { return (width); } }

        private int height = 600;
        public int Height { set { height = value; } get { return (height); } }

        private Color clearColor = Color.White;
        public Color ClearColor { get { return clearColor; } }

        private Color progressBarColor = new Color(63, 0, 0, 0);
        public Color ProgressBarColor { get { return progressBarColor; } }

        private string baseFolder = Util.UserCacheDirectory;
        public string BaseFolder { get { return (baseFolder); } }

        private DateTime worldTime = DateTime.Parse("2009-01-01 00:00:00");
        public DateTime WorldTime { get { return (worldTime); } }

        private Camera camera = null;
        public Camera Camera { get { return (camera); } }

        private uint timeDilation = 0;
        public uint TimeDilation { get { return (timeDilation); } set { timeDilation = value; } }

        private static readonly ILog log = LogManager.GetLogger(typeof(Managers.DebugManager));
        public static ILog Log { get { return (log); } }

        private string log4netLevel = "DEBUG";
        public string Log4netLevel { get { return log4netLevel; } }

        private IAdapter adapter;
        public IAdapter Adapter { get { return adapter; } set { adapter = value; } }

        public Position2D CursolOffset { get; set; }
        public float FPSRate { get; set; }

        private Texture videoTexture;
        public Texture VideoTexture { get { return videoTexture; } }

        public enum ShaderLevelType
        {
            Low,
            Middle,
            High,
        }

        // DHTML API properties
        // General
        public string BackgroundColor { get; set; }
        public string ProgressColor { get; set; }

        // Init window
        public string InitBackgroundMusicURL { get; set; }
        public string InitBackgroundURL { get; set; }

        // Login window
        public string LoginBackgroundURL { get; set; }
        public string LoginLocation { get; set; }
        public string LoginMode { get; set; }
        public string ServerURI { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string DHTMLRelationEnable { get; set; }

        // Draw setting
        public string DrawTerrain { get; set; }
        public string DrawShadow { get; set; }
        public string DrawSea { get; set; }
        public string DrawSky { get; set; }
        public string DrawMenu { get; set; }
        public string TickOn { get; set; }
        public string WorldAmbientColor { get; set; }
        public int AvatarNameType { get; set; }
        public string FixDirectional { get; set; }
        public string FixDirectionalRotation { get; set; }
        public string FixDirectionalDiffuseColor { get; set; }
        public string FixDirectionalAmbientColor { get; set; }
        public bool IsDrawTerrain { get { return DrawTerrain != "false"; } }
        public bool IsDrawShadow { get { return DrawShadow != "false"; } }
        public bool IsDrawSea { get { return DrawSea != "false"; } }
        public bool IsDrawSky { get { return DrawSky != "false"; } }
        public bool IsDHTMLRelationEnable { get { return DHTMLRelationEnable != "false"; } }
        public bool IsDrawMenu { get { return DrawMenu != "false"; } }
        public bool IsTickOn { get { return TickOn != "false"; } }
        public bool IsFixDirectional { get { return FixDirectional == "true"; } }

        // Camera setting
        public float CameraDistance { get; set; }
        public float CameraKeyWalkingDistance { get; set; }
        public float CameraMinDistance { get; set; }
        public float CameraMaxDistance { get; set; }
        public float CameraFOV { get; set; }
        public float CameraOffsetY { get; set; }
        public float CameraMinAngleY { get; set; }
        public float CameraMaxAngleY { get; set; }
        public float CamRotationAnglePHI { get; set; }
        public float CamRotationAngleTHETA { get; set; }
        public float CamDefaultRotationAnglePHI { get; set; }
        public float CamDefaultRotationAngleTHETA { get; set; }
        public float AvatarDisappearDistance { get; set; }

        public bool IsStandUpIcon { get; set; }

        public Version Version { get; set; }
        public string HelpURL { get; set; }
        public string Locale { get; set; }
        public bool JapaneseEnabled { get; set; }
        public bool IsVisibleDebutTab { get; set; }
        public bool IsCameraPitchReverse { get; set; }
        public ShaderLevelType SeaQuality { get; set; }
        public ShaderLevelType SkyQuality { get; set; }
        public long TeleportTimeout { get; set; }
        public int VoiceWaitTime { get; set; }
        public Colorf AmbientLightColor = new Colorf();
        public Vector3D DirectionalRotation = new Vector3D();
        public Colorf DirectionalDiffuseColor = new Colorf();
        public Colorf DirectionalAmbientColor = new Colorf();
        public int DebugVoiceWaitTime { get; set; }

        #endregion

        #region Managers

        private Managers.StateManager stateManager = null;
        public Managers.StateManager StateManager { get { return (stateManager); } }

        private Managers.MenuManager menuManager = null;
        public Managers.MenuManager MenuManager { get { return (menuManager); } }

        private Managers.GuiManager guiManager = null;
        public Managers.GuiManager GuiManager { get { return (guiManager); } }

        private Managers.ShaderManager shaderManager = null;
        public Managers.ShaderManager ShaderManager { get { return (shaderManager); } }

        private Managers.ProtocolManager protocolManager = null;
        public Managers.ProtocolManager ProtocolManager { get { return (protocolManager); } }

        private Managers.EntityManager entityManager = null;
        public Managers.EntityManager EntityManager { get { return (entityManager); } }

        private Managers.TextureManager textureManager = null;
        public Managers.TextureManager TextureManager { get { return (textureManager); } }

        private Managers.IrrManager irrManager = null;
        public Managers.IrrManager IrrManager { get { return (irrManager); } }

        private Managers.TerrainManager terrainManager = null;
        public Managers.TerrainManager TerrainManager { get { return (terrainManager); } }

        private Managers.AvatarManager avatarManager = null;
        public Managers.AvatarManager AvatarManager { get { return (avatarManager); } }

        private Managers.EffectManager effectManager = null;
        public Managers.EffectManager EffectManager { get { return (effectManager); } }

        private Managers.ChatManager chatManager = null;
        public Managers.ChatManager ChatManager { get { return (chatManager); } }

        private Managers.SoundManager soundManager = null;
        public Managers.SoundManager SoundManager { get { return (soundManager); } }

        private Managers.DebugManager debugManager = null;
        public Managers.DebugManager DebugManager { get { return (debugManager); } }

        private Managers.CacheManager cacheManager = null;
        public Managers.CacheManager CacheManager { get { return (cacheManager); } }
        #endregion

#if _SECOND_SURFACE_ON
        public Texture pip = null;
        private GUIImage pipImage = null;
        private CameraSceneNode pipCamera = null;
        private IrrlichtNETCP.Material material = new IrrlichtNETCP.Material(true);
#endif

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

#if !LINUX
        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

#else
        public static int ShowCursor(bool bShow) {return(0);}
#endif
		
        public Viewer()
        {
            // Check and create folders if needed at the user's location.
            Util.InitializeFolderHierarchy();

            FPSRate = 1;

            BackgroundColor = "white";
            ProgressColor = "#3f000000";

            InitBackgroundMusicURL = string.Empty;
            InitBackgroundURL = string.Empty;

            LoginBackgroundURL = string.Empty;
            LoginLocation = "last";
            LoginMode = "hide";
            ServerURI = String.Empty;
            FirstName = String.Empty;
            LastName = String.Empty;
            Password = String.Empty;
            DHTMLRelationEnable = "true";
            DrawTerrain = "true";
            DrawShadow = "true";
            DrawSea = "true";
            DrawSky = "true";
            DrawMenu = "true";
            TickOn = "true";
            WorldAmbientColor = "0.5,0.5,0.5";
            AvatarNameType = DEFAULT_AVATAR_NAME_TYPE;
            FixDirectional = "false";
            FixDirectionalRotation = "1.75,0,0";
            FixDirectionalDiffuseColor = "1.0,0.4,0.4";
            FixDirectionalAmbientColor = "0.2,0.08,0.08";

            CameraDistance = DEFAULT_FIRST_DISTANCE;
            CameraKeyWalkingDistance = DEFAULT_FIRST_DISTANCE;
            CameraMinDistance = DEFAULT_MIN_DISTANCE;
            CameraMaxDistance = DEFAULT_MAX_DISTANCE;
            CameraFOV = DEFAULT_CAMRA_FOV;
            CameraOffsetY = DEFAULT_CAMRA_OFFSET_Y;

            // Only allow PHI angles in one hemisphere to prevent gimbal issues. 
            // we allow 0+epsilon to 180-epsilon
            float epsilon = 0.01f;
            CameraMinAngleY = (float)Math.PI * 0.0f + epsilon;
            CameraMaxAngleY = (float)Math.PI * 1.0f - epsilon;
            CamRotationAnglePHI = DEFAULT_CAMERA_ROTATION_ANGLE_PHI;
            CamRotationAngleTHETA = DEFAULT_CAMERA_ROTATION_ANGLE_THETA;
            CamDefaultRotationAnglePHI = DEFAULT_CAMERA_ROTATION_ANGLE_PHI;
            CamDefaultRotationAngleTHETA = DEFAULT_CAMERA_ROTATION_ANGLE_THETA;
            AvatarDisappearDistance = DEFAULT_AVATAR_DISAPPEAR_DISTANCE;

            IsStandUpIcon = false;

            HelpURL = "http://3di-opensim.com/openviewer/";
            Locale = "jp";
            JapaneseEnabled = true;
            IsVisibleDebutTab = false;
            IsCameraPitchReverse = false;
            SeaQuality = ShaderLevelType.Low;
            SkyQuality = ShaderLevelType.Low;

            TeleportTimeout = 20 * 10000000; // 20s by default
            VoiceWaitTime = DEFAULT_DEBUG_VOICE_WAIT_TIME;
        }

        public void Startup(IntPtr target)
        {
            guithread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(StartupGUI));
            guithread.SetApartmentState(System.Threading.ApartmentState.STA);
            guithread.Start((object)target);
        }

        private void StartupGUI(object target)
        {
            // Enforce the underlying Irrlicht engine to load a local D3DX9_40.dll
            // (Note: Environment.CurrentDirectory is visible to other plug-ins)
            Environment.CurrentDirectory = Util.ApplicationDataDirectory;

            SetupAddins();
            SetupLog();

            f = new RenderForm();
            f.Width = width;
            f.Height = height;
            f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            f.ImeMode = System.Windows.Forms.ImeMode.Inherit;
            try
            {
                SetParent(f.Handle, (IntPtr)target);
            }
            catch (Exception e)
            {
                m_log.Warn(@"[REI]: Exception occured while trying to set parent window - " + e.Message);
                m_log.Debug(@"[REI]: Exception occured while trying to set parent window - " + e.StackTrace);
            }
            f.Location = new System.Drawing.Point(0, 0);
            //f.FormClosed += new System.Windows.Forms.FormClosedEventHandler(f_FormClosed);
            f.Show();
            renderTarget = f.Handle;

            try
            {
                string iniconfig = System.IO.Path.Combine(Util.ConfigFolder, "OpenViewer.ini");
                m_config.Load(iniconfig);

                // Check and clean cache if request last time.
                string cacheFlag = m_config.Source.Configs["Startup"].Get("cache_delete", "false");
                if (cacheFlag == "true")
                {
                    CleanChache();
                }

            }
            catch (Exception e)
            {
                m_log.Fatal("can't load config file.", e);
            }

            try
            {
#if !LINUX
                device = new IrrlichtDevice(DriverType.Direct3D9, new Dimension2D(width, height), 32, false, true, false, false, renderTarget);
#else
                device = new IrrlichtDevice(DriverType.OpenGL, new Dimension2D(width, height), 24, false, true, false, false);
#endif
                if (device == null)
                {
                    m_log.Error("can't create irrlicht device.");
                    System.Windows.Forms.MessageBox.Show(DialogText.ErrorGraphicDriverMessage, DialogText.ErrorGraphicDriverCaption);
                }
            }
            catch (Exception e)
            {
                m_log.Error("can't create irrlicht device.", e);
                System.Windows.Forms.MessageBox.Show(DialogText.ErrorGraphicDriverMessage, DialogText.ErrorGraphicDriverCaption);

                device = null;
            }

            if (device == null)
            {
                // release all.

                // exit application.

                return;
            }
            else
            {
#if !LINUX
                m_log.InfoFormat("AdapterVendorID: 0x{0:x8}", device.VideoDriver.AdapterVendorId);
                m_log.InfoFormat("AdapterDeviceId: 0x{0:x8}", device.VideoDriver.AdapterDeviceId);
                m_log.InfoFormat("AdapterSubSysId: 0x{0:x8}", device.VideoDriver.AdapterSubSysId);
                m_log.InfoFormat("AdapterRevision: 0x{0:x8}", device.VideoDriver.AdapterRevision);
                m_log.InfoFormat("AdapterMaxTextureHeight: {0}", device.VideoDriver.AdapterMaxTextureHeight);
                m_log.InfoFormat("AdapterMaxTextureWidth: {0}", device.VideoDriver.AdapterMaxTextureWidth);
                m_log.InfoFormat("AdapterMaxActiveLights: {0}", device.VideoDriver.AdapterMaxActiveLights);
                m_log.InfoFormat("AdapterVertexShaderVersion: 0x{0:x8}", device.VideoDriver.AdapterVertexShaderVersion);
                m_log.InfoFormat("AdapterPixelShaderVersion: 0x{0:x8}", device.VideoDriver.AdapterPixelShaderVersion);

                // AdapterVendorId
                // 0x1002 : ATI Technologies Inc.
                // 0x10DE : NVIDIA Corporation
                // 0x102B : Matrox Electronic Systems Ltd.
                // 0x121A : 3dfx Interactive Inc
                // 0x5333 : S3 Graphics Co., Ltd.
                // 0x8086 : Intel Corporation
                //
                if (device.VideoDriver.AdapterVendorId == 0x1002)
                {
                    string warningMessage = DialogText.WarningGraphicCardMessage;
                    string warningCaption = DialogText.WarningGraphicCardCaption;

                    /*
                    System.Windows.Forms.DialogResult yesno = System.Windows.Forms.MessageBox.Show(warningMessage, warningCaption, System.Windows.Forms.MessageBoxButtons.YesNo);
                    if (System.Windows.Forms.DialogResult.Yes != yesno)
                    {
                        return;
                    }
                    */
                }
#endif
            }

            reference = new RefController(this);

            device.OnEvent += new OnEventDelegate(device_OnEvent);

            // If enabled is true, videotexture is not correctly render.
            device.VideoDriver.SetTextureFlag(TextureCreationFlag.CreateMipMaps, false);

#if DEBUG
            device.Logger.LogLevel = LogLevel.Information;
#endif
            // Create video Textrue
            videoTexture = device.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/textures/videoTexture.tga");

            IrrlichtNETCP.Extensions.TTFont font = new TTFont(Device.VideoDriver);
            IrrlichtNETCP.Extensions.TTFace face = new TTFace();
            bool font_loaded = false;
            uint fontsize = 16;
            string fontface = Config.Source.Configs["Startup"].GetString("guifont_face");

            if (!string.IsNullOrEmpty(Config.Source.Configs["Startup"].GetString("guifont_size")))
            {
                fontsize = (uint)Config.Source.Configs["Startup"].GetInt("guifont_size");
            }
#if !LINUX
            if (!string.IsNullOrEmpty(fontface))
            {
                if (System.IO.File.Exists(System.Environment.SystemDirectory + @"/../Fonts/" + fontface))
                {
                    face.Load(System.Environment.SystemDirectory + @"/../Fonts/" + fontface);
                    font.Attach(face, fontsize);
                    font.Antialias = true;
                    Reference.GUIEnvironment.Skin.Font = font;
                    font_loaded = true;
                }
                else
                {
                    m_log.Warn("[FONT]: The specified font (" + fontface + ") was not available on this system. Reverting to default.");
                }
            }

            if (!font_loaded)
            {
                if (System.IO.File.Exists(System.Environment.SystemDirectory + @"\..\Fonts\msgothic.ttc"))
                {
                    face.Load(System.Environment.SystemDirectory + @"\..\Fonts\msgothic.ttc");
                    font.Attach(face, fontsize);
                    font.Antialias = true;
                    Reference.GUIEnvironment.Skin.Font = font;
                }
                else
                {
                    JapaneseEnabled = false;
                    Locale = "en";
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                    if (System.IO.File.Exists(System.Environment.SystemDirectory + @"\..\Fonts\arial.ttf"))
                    {
                        face.Load(System.Environment.SystemDirectory + @"\..\Fonts\arial.ttf");
                        font.Attach(face, fontsize);
                        font.Antialias = true;
                        Reference.GUIEnvironment.Skin.Font = font;
                    }
                    else
                    {
                        // Use built in font--- this looks horrible and should be avoided if possible
                        Reference.GUIEnvironment.Skin.Font = Reference.GUIEnvironment.BuiltInFont;
                    }
                }
            }
#else
            if (!string.IsNullOrEmpty(fontface))
            {
                if (System.IO.File.Exists("/usr/share/fonts/" + fontface))
                {
                    face.Load("/usr/share/fonts/" + fontface);
                    font.Attach(face, fontsize);
                    font.Antialias = true;
                    Reference.GUIEnvironment.Skin.Font = font;
                    font_loaded = true;
                }
                else
                {
                    m_log.Warn("[FONT]: The specified font (" + fontface + ") was not available on this system. Reverting to default.");
                }
            }

            if (!font_loaded)
            {
                if (System.IO.File.Exists("/usr/share/fonts/truetype/kochi/kochi-gothic.ttf"))
                {
                    face.Load("/usr/share/fonts/truetype/kochi/kochi-gothic.ttf");
					m_log.Info("[FONT]: Loading font: /usr/share/fonts/truetype/kochi/kochi-gothic.ttf");
                    font.Attach(face, fontsize);
                    font.Antialias = true;
                    Reference.GUIEnvironment.Skin.Font = font;
                }
                else
                {
                    JapaneseEnabled = false;
                    Locale = "en";
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                    if (System.IO.File.Exists("/usr/share/fonts/truetype/ttf-bitstream-vera/Vera.ttf"))
                    {
                        face.Load("/usr/share/fonts/truetype/ttf-bitstream-vera/Vera.ttf");
						m_log.Info("[FONT]: Loading font: /usr/share/fonts/truetype/ttf-bitstream-vera");
                        font.Attach(face, fontsize);
                        font.Antialias = true;
                        Reference.GUIEnvironment.Skin.Font = font;
                    }
                    else
                    {
                        // Use built in font--- this looks horrible and should be avoided if possible
                        Reference.GUIEnvironment.Skin.Font = Reference.GUIEnvironment.BuiltInFont;
						m_log.Info("[FONT]: Using built-in font.");
                    }
                }
            }
#endif
            font.Drop();
            face.Drop();
            // if font and face are being used, at this point face and font should both have reference counts of 1.
            // if font and face are not being used (last else branch above, using built-in font), font and face should both be deleted at this point.

            // Zaki: Adding Japanese support end

            Reference.VideoDriver.SetFog(new Color(0, 255, 255, 255), FogType.Exponential, 9999, 9999, 0, false, false);

            //Reference.Device.FileSystem.WorkingDirectory = Util.UserCacheDirectory;
            Reference.Device.FileSystem.AddFolderFileArchive(Util.ApplicationDataDirectory);
            Reference.Device.FileSystem.AddFolderFileArchive(Util.ModelFolder);
            Reference.Device.FileSystem.AddFolderFileArchive(Util.TextureFolder);

            camera = new Camera(this);

            // Create managers.
            cacheManager = new CacheManager(this, -1);
            try
            {
                string iniconfig = System.IO.Path.Combine(Util.ConfigFolder, "OpenViewer.ini");
                m_config.Load(iniconfig);

                // Check and clean cache if request last time.
                string s_cacheMB = m_config.Source.Configs["Startup"].Get("cache_size", DEFAULT_CACHE_MB_SIZE.ToString());

                long cacheMB = DEFAULT_CACHE_MB_SIZE;
                if (long.TryParse(s_cacheMB, out cacheMB))
                {
                    cacheMB = cacheMB * 1000 * 1000;

                    cacheManager.CacheMaxSize = cacheMB;
                    cacheManager.Clean();
                }
            }
            catch (Exception e)
            {
                m_log.Fatal("can't load config file.", e);
            }

            // Locale can be used from this point on
            if (Locale == "en")
            {
                DialogText.Culture = new System.Globalization.CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            }
            if (Locale == "jp")
            {
                DialogText.Culture = new System.Globalization.CultureInfo("ja-JP");
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja-JP");
            }
            
            stateManager = new Managers.StateManager(this);
            protocolManager = new Managers.ProtocolManager(this);
            guiManager = new GuiManager(this);
            menuManager = new MenuManager(this);
            shaderManager = new ShaderManager(this);
            entityManager = new EntityManager(this);
            irrManager = new IrrManager(this, -1);
            textureManager = new TextureManager(this);
            textureManager.OnTextureLoaded += OnTextureComplete;
            terrainManager = new Managers.TerrainManager(this);
            avatarManager = new Managers.AvatarManager(this);
            effectManager = new Managers.EffectManager(this);
            chatManager = new Managers.ChatManager(this);
            try
            {
                soundManager = new Managers.SoundManager(this);
            }
            catch (BadImageFormatException e)
            {
                // Trying to load 32-bit assembly on 64-bit architecture
                m_log.Warn("[MANAGERS]: SoundManager is not available due to incompatible assembly.");
            }
            debugManager = new DebugManager(this, -1);

            // Experimental manager plugin loader
            LoadPlugins();

            // Initialize.
            Initialize();

            // Event
            stateManager.OnChanged += StateChanged;

            clearColor = Color.FromBCL(Util.FromString(BackgroundColor));
            progressBarColor = Color.FromBCL(Util.FromString(ProgressColor, System.Drawing.Color.FromArgb(63, 0, 0, 0)));


            guiManager.LoadBackgrounds();

            if (soundManager != null)
            {
                soundManager.InitWindowBackgroundMusicURL = InitBackgroundMusicURL;
                soundManager.LoadBackgroundMusic();
            }
            // After all managers all initialized
            stateManager.State = State.INITIALIZED;

            adapter.Initialize(this.reference);

            adapter.CallSetWorldAmbientColor(WorldAmbientColor);
            adapter.CallSetFixDirectional(FixDirectional);
            adapter.CallSetFixDirectionalRotation(FixDirectionalRotation);
            adapter.CallSetFixDirectionalDiffuseColor(FixDirectionalDiffuseColor);
            adapter.CallSetFixDirectionalAmbientColor(FixDirectionalAmbientColor);

            SetLogFilter();

            guiManager.ShowLoginWindow(new LoginInfo(ServerURI, FirstName, LastName, Password, LoginMode, true, true));

            // Hide mouse cursor.
            ShowCursor(false);

#if _SECOND_SURFACE_ON
            pip = Reference.VideoDriver.CreateRenderTargetTexture(new Dimension2D(256, 256));
            pipImage = Reference.GUIEnvironment.AddImage(pip, new Position2D(Width - pip.OriginalSize.Width - 10, 40), false, Reference.GUIEnvironment.RootElement, -1, "");

            CameraSceneNode oldCam = Reference.SceneManager.ActiveCamera;
            pipCamera = Reference.SceneManager.AddCameraSceneNode(Reference.SceneManager.RootSceneNode);
            material.Lighting = false;
            if (Reference.SceneManager.ActiveCamera.Raw != oldCam.Raw)
                Reference.SceneManager.ActiveCamera = oldCam;
#endif

            System.Windows.Forms.DialogResult dialogRes = System.Windows.Forms.DialogResult.Retry;
            int restarted = 0;
            while (dialogRes == System.Windows.Forms.DialogResult.Retry)
            {
                dialogRes = RenderLoop(restarted++);
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }

        private void LoadPlugins()
        {
            PluginLoader < IManagerPlugin > loader = new PluginLoader<IManagerPlugin>(new ManagerPluginInitialiser(this));
            loader.Load("/OpenViewer/Managers");
            m_managers = loader.Plugins;
        }

        private void CleanChache()
        {
            try
            {
                System.IO.Directory.Delete(Util.AssetFolder, true);

                Util.InitializeFolderHierarchy();

                m_config.Source.Configs["Startup"].Set("cache_delete", "false");
                m_config.Source.Save();
            }
            catch (Exception e)
            {
                m_log.Error("CleanChache", e);
            }
        }

        /// <summary>
        /// VUtil.LogConsole filter. (this filter check "_classInfo")
        /// </summary>
        private void SetLogFilter()
        {
#if DEBUG
            VUtil.LogFilterList.Add("IrrFileCreateCache");
            VUtil.LogFilterList.Add(avatarManager.ToString());
            if (soundManager != null)
                VUtil.LogFilterList.Add(soundManager.ToString());
            VUtil.LogFilterList.Add(irrManager.ToString());
            VUtil.LogFilterList.Add(protocolManager.AvatarConnection.ToString());
#endif
        }

        private void SetupLog()
        {
            string path = Util.LogFolder + "app.config";

            System.Xml.XmlDocument config = new System.Xml.XmlDocument();

            if (System.IO.File.Exists(path))
            {
                config.Load(path);
            }
            else
            {
                config.InnerXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>"
                                + "<log4net>"
                                + "    <appender name=\"FileAppender\" type=\"log4net.Appender.RollingFileAppender\" >"
                                + "         <file value=\"" + Util.LogFolder + "openviewer.log\" />"
                                + "         <maxSizeRollBackups value=\"10\" />"
                                + "         <rollingStyle value=\"Size\" />"
                                + "         <maximumFileSize value=\"1MB\" />"
                                + "         <layout type=\"log4net.Layout.PatternLayout\">"
                                + "              <conversionPattern value=\"%d %-5p %c - %m%n\" />"
                                + "         </layout>"
                                + "     </appender>"
                                + "     <root>"
                                + "         <level value=\"INFO\" />"
                                + "         <appender-ref ref=\"FileAppender\" />"
                                + "     </root>"
                                + "</log4net>";

                config.Save(path);
            }

            foreach (XmlNode item in config.ChildNodes)
            {
                if (item.Name != "log4net")
                    continue;

                foreach (XmlNode item2 in item.ChildNodes)
                {
                    if (item2.Name != "root")
                        continue;

                    foreach (XmlNode item3 in item2.ChildNodes)
                    {
                        if (item3.Name != "level")
                            continue;

                        XmlAttribute at = item3.Attributes["value"];

                        if (at != null)
                            log4netLevel = at.Value;
                    }
                }
            }

            log4net.Config.XmlConfigurator.Configure(config.DocumentElement);
        }

        private void SetupAddins()
        {
            string path = Util.UserCacheDirectory + @"/plugins/viewer.addins";

            System.Xml.XmlDocument config = new System.Xml.XmlDocument();

            if (System.IO.File.Exists(path))
            {
                return;
            }
            else
            {
                config.InnerXml = "<Addins>"
                                + "    <Directory>"+Util.ApplicationDataDirectory+"</Directory>"
                                + "</Addins>";

                config.Save(path);
            }
        }

        private void Initialize()
        {
            stateManager.Initialize();
            protocolManager.Initialize();
            guiManager.Initialize();
            menuManager.Initialize();
            // Add icons
            menuManager.AddIcon(new MenuIcon("menu_logout.png", LogoutRequest));
            menuManager.AddIcon(new MenuIcon("menu_settings.png", guiManager.ShowSettingWindow));
            menuManager.AddIcon(new MenuIcon("menu_chat.png", guiManager.ShowChatWindow));
            menuManager.AddIcon(new MenuIcon("menu_teleport.png", guiManager.ShowTeleportWindow));
            cacheManager.Initialize();
            shaderManager.Initialize();
            entityManager.Initialize();
            irrManager.Initialize();
            textureManager.Initialize();
            terrainManager.Initialize();
            avatarManager.Initialize();
            effectManager.Initialize();
            chatManager.Initialize();
            if (soundManager != null)
                soundManager.Initialize();
            debugManager.Initialize();

            foreach (IManagerPlugin plugin in m_managers)
            {
                plugin.Initialize();
            }
        }

        private System.Windows.Forms.DialogResult RenderLoop(int restarted)
        {
            uint frames = 1;
            long teleportTimeout = 0;
            long loginTimeout = 0;
            bool loginRerequest = false;

            long interval = 3;
            long frameTarget = 30;
            long frameBase = (1000 * interval) / frameTarget;
            long lastTime = 0;
            long errorTime = 0;
            long currentTime;
            long pastTime;

            System.Windows.Forms.DialogResult dialogRes = System.Windows.Forms.DialogResult.Cancel;

            try
            {
                try
                {
                    while (stateManager.State != State.EXITING)
                    {
                        currentTime = System.Environment.TickCount;
                        pastTime = (currentTime - lastTime) * interval + errorTime;
                        if (pastTime >= frameBase)
                        {
                            lastTime = currentTime;
                            errorTime = pastTime % frameBase;

                            FPSRate = Utils.Clamp(((float)device.VideoDriver.FPS / 30.0f), 0.00001f, 1);

                            Render();
                            Update(frames);

                            this.worldTime.AddMinutes(10.0);
                            frames++;
                            lock (Reference.Viewer.StateManager.SyncStat)
                            {
                                if (stateManager.State == State.TELEPORT_REQUESTED)
                                {
                                    protocolManager.fullCleanup = false;
                                    Cleanup();
                                    Initialize();
                                    guiManager.ShowLoginProgress();
                                    avatarManager.Start();
                                    if (firstLogin)
                                    {
                                        shaderManager.LoadAll();
                                        firstLogin = false;
                                    }
                                    stateManager.State = State.TELEPORTING;
                                    teleportTimeout = DateTime.Now.Ticks;
                                }
                                if (stateManager.State == State.TELEPORTING)
                                {
                                    if (DateTime.Now.Ticks - teleportTimeout > TeleportTimeout)
                                    {
                                        // Teleporting timeout, go back to login (we have probably disconnected from the original
                                        // region, so we can't just show the region we came from)
                                        stateManager.State = State.INITIALIZED;
                                        guiManager.LoginFailed = true;
                                        guiManager.LoginMessage = DialogText.ErrorTeleportMessage;

                                        m_log.Error("Teleporting timed out. Please log in again");

                                        // Maybe when failed message is closed, show login window.
                                        //guiManager.ShowLoginWindow(new LoginInfo(ServerURI, FirstName, LastName, Password, LoginMode, true, true));
                                    }
                                }
                                if (stateManager.State == State.INITIALIZING)
                                {
                                    Initialize();
                                    stateManager.State = State.INITIALIZED;
                                    guiManager.ShowLoginWindow(new LoginInfo(ServerURI, FirstName, LastName, Password, LoginMode, true, true));
                                }
                                if (stateManager.State == State.CLOSING)
                                {
                                    Cleanup();
                                    stateManager.State = State.INITIALIZING;
                                }
                                if (stateManager.State == State.LOGIN)
                                {
                                    if (loginTimeout == 0)
                                    {
                                        loginTimeout = DateTime.Now.Ticks;
                                        loginRerequest = false;
                                    }
                                    else if (!loginRerequest && DateTime.Now.Ticks - loginTimeout > 10 * 10000000)
                                    {
                                        protocolManager.RerequestTerrain();
                                        loginRerequest = true;
                                    }
                                    else if (DateTime.Now.Ticks - loginTimeout > 30 * 10000000)
                                    {
                                        // Login is not progressing... at all, just kill the current login
                                        protocolManager.fullCleanup = false;
                                        Cleanup();
                                        Initialize();
                                        stateManager.State = State.INITIALIZED;
                                        guiManager.LoginFailed = true;
                                        guiManager.LoginMessage = DialogText.ErrorLoginMessageme00;

                                        m_log.Error("Login timed out. Please log in again");
                                    }
                                    System.Threading.Thread.Sleep(100);
                                }
                                else
                                {
                                    loginTimeout = 0;
                                }
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(1);
                        }
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                }
                catch (Exception e)
                {
                    m_log.Fatal("RenderLoop", e);

                    if (restarted == 0)
                    {
                        ShowCursor(true);
                        System.Windows.Forms.MessageBox.Show(DialogText.ErrorFinalMessageme01, "Restart Plug-in", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        ShowCursor(false);
                        dialogRes = System.Windows.Forms.DialogResult.Retry;
                    }
                    else
                    {
                        ShowCursor(true);
                        dialogRes = System.Windows.Forms.MessageBox.Show(DialogText.ErrorFinalMessageme00, "Restart plug-in", System.Windows.Forms.MessageBoxButtons.RetryCancel, System.Windows.Forms.MessageBoxIcon.Information);
                        ShowCursor(false);
                    }
                }
            }
            finally
            {
                // Exiting, final cleanup
                protocolManager.fullCleanup = true;
                Cleanup();
                lock (stateManager.SyncStat)
                {
                    stateManager.State = State._ENTRY;

                    if (dialogRes == System.Windows.Forms.DialogResult.Retry)
                        stateManager.State = State.INITIALIZING;
                }
            }
            return dialogRes;
        }

        private void SetDebug(SceneNode node, DebugSceneType debug)
        {
#if DEBUG
            if (node != null)
            {
                node.DebugDataVisible = debug;

                foreach (SceneNode child in node.Children)
                {
                    SetDebug(child, debug);
                }
            }
#endif
        }


        /// <summary>
        /// The normal render loop. Run->BeginScene->EndScene
        /// NO UPDATES ARE ALLOWED WITHIN THE SCOPE OF THIS FUNCTION
        /// </summary>
        private void Render()
        {
            try
            {
                // If you close the gui window, device.Run returns false.
                lock (reference.SceneManager)
                {
                    bool running = Device.Run();
                    if (!running)
                    {
                        lock (stateManager.SyncStat)
                        {
                            stateManager.State = State.EXITING;
                        }
                        return;
                    }
                }
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "OpenViewer::Render");
            }

            reference.VideoDriver.BeginScene(true, true, guiManager.GetBackgroundColor(stateManager.State));

#if _SECOND_SURFACE_ON
            if (Reference.SceneManager.ActiveCamera.Raw != Camera.SNCamera.Raw)
                Reference.SceneManager.ActiveCamera = Camera.SNCamera;

            Reference.VideoDriver.SetRenderTarget(pip, true, true, Color.White);

            CameraSceneNode oldCam = Reference.SceneManager.ActiveCamera;
            Vector3D pos = oldCam.Position;
            Vector3D target = oldCam.Target;
            Vector3D line = target - pos;
            line.Normalize();
            pipCamera.Position = pos - line * 50f;
            pipCamera.Target = target;

            Vector3D min = new Vector3D(-1, -1, -1);
            Vector3D max = new Vector3D(1, 1, 1);

            ViewFrustum vf = oldCam.ViewFrustum;
            ViewFrustum vf2 = Camera.SNCamera.ViewFrustum;

            Reference.VideoDriver.SetMaterial(material);
            Reference.VideoDriver.SetTransform(TransformationState.World, IrrlichtNETCP.Matrix4.Identity);

            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarLeftDown), Color.Blue);
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarLeftUp), Color.Blue);
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarRightDown), Color.Blue);
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarRightUp), Color.Blue);

            Reference.VideoDriver.Draw3DLine(new Line3D(Camera.SNCamera.Position, vf2.FarLeftDown), Color.White);
            Reference.VideoDriver.Draw3DLine(new Line3D(Camera.SNCamera.Position, vf2.FarLeftUp), Color.White);
            Reference.VideoDriver.Draw3DLine(new Line3D(Camera.SNCamera.Position, vf2.FarRightDown), Color.White);
            Reference.VideoDriver.Draw3DLine(new Line3D(Camera.SNCamera.Position, vf2.FarRightUp), Color.White);

            Reference.SceneManager.ActiveCamera = pipCamera;

            //for (int i = 0; i < (int)ViewFrustumPlanes.Count; i++)
            //{
            //    Plane3Df plane = vf.GetPlane(i);
            //    plane.D = -plane.D;
            //    plane.Normal = -plane.Normal;
            //    Reference.VideoDriver.SetClipPlane(i, plane, true);
            //}

            Reference.SceneManager.DrawAll();

            Reference.VideoDriver.SetMaterial(material);
            Reference.VideoDriver.SetTransform(TransformationState.World, IrrlichtNETCP.Matrix4.Identity);

            // Camera box
            Reference.VideoDriver.Draw3DBox(new Box3D(min + pos, max + pos), Color.White);

            // Target box
            Reference.VideoDriver.Draw3DBox(new Box3D(min + target, max + target), Color.Green);

            // View frustum
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarLeftDown), Color.Red);
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarLeftUp), Color.Red);
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarRightDown), Color.Red);
            Reference.VideoDriver.Draw3DLine(new Line3D(pos, vf.FarRightUp), Color.Red);

            Reference.VideoDriver.Draw3DLine(new Line3D(vf.FarLeftDown, vf.FarLeftUp), Color.Red);
            Reference.VideoDriver.Draw3DLine(new Line3D(vf.FarLeftUp, vf.FarRightUp), Color.Red);
            Reference.VideoDriver.Draw3DLine(new Line3D(vf.FarRightUp, vf.FarRightDown), Color.Red);
            Reference.VideoDriver.Draw3DLine(new Line3D(vf.FarRightDown, vf.FarLeftDown), Color.Red);

            Reference.SceneManager.ActiveCamera = oldCam;
            Reference.VideoDriver.SetRenderTarget(null, true, true, guiManager.GetBackgroundColor(stateManager.State));

            //for (int i = 0; i < (int)ViewFrustumPlanes.Count; i++)
            //{
            //    Reference.VideoDriver.EnableClipPlane(i, false);
            //}
#endif

#if DEBUG
            if (debug_on)
            {
                SetDebug(entityManager.ParentNode, DebugSceneType.BoundingBox);
                SetDebug(avatarManager.ParentNode, DebugSceneType.BoundingBox);
            }
            else
            {
                SetDebug(entityManager.ParentNode, DebugSceneType.Off);
                SetDebug(avatarManager.ParentNode, DebugSceneType.Off);
            }
#endif


            // Reset transformation to be able to cull properly
            Reference.VideoDriver.SetTransform(TransformationState.World, IrrlichtNETCP.Matrix4.Identity);

            //if (Reference.Viewer.SkyQuality == ShaderLevelType.Low && Reference.Viewer.SeaQuality == ShaderLevelType.Low)
            //{
                // Culling by user clip planes temporarily disabled (due to visual tearing at the screen edges when rotating)
                // Simply increasing the FOV or moving the camera does not alleviate this problem, even when
                // recalculateViewArea is EXPORTed and used after changes to the cameras. However a call
                // to scenemanager->drawall() - as in the above second surface code - does seem to solve
                // the issue (at a performance cost, which is not appreciated).
                // .AutomaticCulling = CullingType.FrustumBox does not seem to work properly either, further investigation is needed

                //for (int i = 0; i < (int)ViewFrustumPlanes.Count; i++)
                //{
                //    // TODO: Fine tune for better visual quality
                //    Plane3Df plane = Reference.SceneManager.ActiveCamera.ViewFrustum.GetPlane(i);
                //    plane.D = -plane.D;
                //    plane.Normal = -plane.Normal;
                //    Reference.VideoDriver.SetClipPlane(i, plane, true);
                //}
            //}

            try
            {
                reference.SceneManager.DrawAll();
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "OpenViewer::Render::DrawAll");
            }

            if (Reference.Viewer.SkyQuality == ShaderLevelType.Low && Reference.Viewer.SeaQuality == ShaderLevelType.Low)
            {
                //for (int i = 0; i < (int)ViewFrustumPlanes.Count; i++)
                //{
                //    Reference.VideoDriver.EnableClipPlane(i, false);
                //}
            }

            try
            {
                reference.GUIEnvironment.DrawAll();
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "OpenViewer::Render::GUIDrawAll");
            }

            try
            {
                effectManager.Draw();
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "OpenViewer::Render::CustomRender");
            }

            guiManager.DrawCursor();

            // Check menu window
            if (menuManager != null)
            {
                if (menuManager.CheckVisible(Device.CursorControl.Position))
                {
                    if (protocolManager.AvatarConnection.m_user.Network.Connected
                        && protocolManager.AvatarConnection.m_user.Network.CurrentSim != null
                        && protocolManager.AvatarConnection.m_user.Network.CurrentSim.Connected
                        )
                    {
                        Vector3 position = new Vector3();

                        if (avatarManager.UserObject.Prim != null)
                        {
                            position = avatarManager.UserObject.Prim.Position + avatarManager.UserObject.ParentPosition;
                        }
                        // Magic number 16 is the number of characters that fit in the current situation (with ellipses when necessary)
                        menuManager.SIMName = DialogText.MenuRegionMessage + protocolManager.GetCurrentSimName();
                        menuManager.LocationText = DialogText.MenuPositionMessage + ((int)position.X).ToString() + " y:" + ((int)position.Y).ToString() + " z:" + ((int)position.Z).ToString();
                    }
                    else
                    {
                        string message = DialogText.MenuDisconnectMessage;
                        if (menuManager.LocationText != message)
                            menuManager.LocationText = message;
                    }
                }
            }

            if (protocolManager.AvatarConnection.m_user.Network.Connected == false ||
                (protocolManager.AvatarConnection.m_user.Network.CurrentSim != null
                 && protocolManager.AvatarConnection.m_user.Network.CurrentSim.Connected == false))
            {
                if (Reference.Viewer.StateManager.State == State.CONNECTED)
                {
                    Reference.Viewer.GuiManager.LoginFailed = true;
                    Reference.Viewer.GuiManager.LoginMessage = DialogText.ErrorConnectionMessage;
                    Reference.Log.Error("Network error!, you disconnected from server. please log out.");
                }
            }

            avatarManager.Draw();

            foreach (IManagerPlugin plugin in m_managers)
            {
                try
                {
                    plugin.Draw();
                }
                catch (Exception e)
                {
                    Log.Error("ERROR in ["+plugin.Name+"]: " + e.Message);
                }
            }

            reference.VideoDriver.EndScene();
        }

        /// <summary>
        /// Update the world. Update entities, locations, state, shaders, etc
        /// ALL UPDATES MUST HAPPEN WITHIN THE SCOPE OF THIS FUNCTION
        /// </summary>
        private void Update(uint frame)
        {
            if (device.WindowActive == false)
            {
                LMheld = false;
                RMheld = false;
                MMheld = false;
            }

            protocolManager.Update(frame);

            stateManager.Update(frame);
            guiManager.Update(frame);
            menuManager.Update(frame);
            shaderManager.Update(frame);
            entityManager.Update(frame);
            terrainManager.Update(frame);
            avatarManager.Update(frame);
            effectManager.Update(frame);
            chatManager.Update(frame);
            cacheManager.Update(frame);
            debugManager.Update(frame);

            foreach (IManagerPlugin plugin in m_managers)
            {
                plugin.Update(frame);
            }

            int avatarManagerRate = 1;
            if (frame % avatarManagerRate == 0)
            {
                camera.SetTarget(Reference.Viewer.avatarManager.UserObject);
            }
            camera.Update(frame);

            adapter.Update();

            if (OnRequest != null)
                OnRequest(this, EventArgs.Empty);

            if (IsTickOn)
                worldTime = worldTime.AddMilliseconds(7500);
        }

        private void Cleanup()
        {
            foreach (IManagerPlugin plugin in m_managers)
            {
                plugin.Cleanup();
            }
            
            adapter.Cleanup();
            
            protocolManager.Cleanup();
            guiManager.Cleanup();
            menuManager.Cleanup();
            shaderManager.Cleanup();
            entityManager.Cleanup();
            irrManager.Cleanup();
            stateManager.Cleanup();
            textureManager.Cleanup();
            terrainManager.Cleanup();
            avatarManager.Cleanup();
            effectManager.Cleanup();
            chatManager.Cleanup();
            if (soundManager != null)
                soundManager.Cleanup();
            cacheManager.Cleanup();
            debugManager.Cleanup();
        }

        /// <summary>
        /// Event handlers for the Irrlicht device
        /// DIRECT UPDATES MUST NOT HAPPEN TO THE SCENE WITHIN THE SCOPE OF THIS FUNCTION
        /// </summary>
        /// <param name="p_event"></param>
        /// <returns></returns>
        public bool device_OnEvent(Event p_event)
        {
            if (p_event.Type == EventType.GUIEvent)
            {
                // Dispatch to GUI Manager
                if (guiManager != null)
                {
                    bool flag = (guiManager.HandleEvent(p_event));

                    if (guiManager.Focused)
                    {
                        avatarManager.UserPushForward(false);
                        avatarManager.UserPushBackward(false);
                    }

                    return flag;
                }
            }

            if (p_event.Type == EventType.KeyInputEvent)
            {
                return KeyInputEventProcessor(p_event);
            }

            if (p_event.Type == EventType.MouseInputEvent)
            {
                return MouseEventProcessor(p_event);
            }
            return false;
        }

        public void OnTextureComplete(string tex, string extension, VObject vObj, UUID AssetID)
        {
            TextureCompleteNotification tx = new TextureCompleteNotification();
            tx.texture = tex;
            tx.extension = extension;
            tx.vObj = vObj;
            tx.textureID = AssetID;
            entityManager.AddTexture(tx);
        }

        public bool KeyInputEventProcessor(Event p_event)
        {
            if (guiManager.Focused)
                return false;

            if (p_event.KeyPressedDown)
            {
                bool isAvatarControl = false;

                switch (p_event.KeyCode)
                {
                    case KeyCode.Up:
                        avatarManager.UserPushForward(true);
                        isAvatarControl = true;
                        break;
                    case KeyCode.Down:
                        avatarManager.UserPushBackward(true);
                        isAvatarControl = true;
                        break;
                    case KeyCode.Left:
                        avatarManager.UserPushLeft();
                        isAvatarControl = true;
                        break;
                    case KeyCode.Right:
                        avatarManager.UserPushRight();
                        isAvatarControl = true;
                        break;
#if DEBUG
                    // camera up / dn
                    case KeyCode.Numpad_7:
                        camera.MoveDown();
                        break;
                    case KeyCode.Numpad_9:
                        camera.MoveUp();
                        break;

                    // camera f / b / l / r
                    case KeyCode.Numpad_8:
                        camera.MoveForward();
                        break;
                    case KeyCode.Numpad_2:
                        camera.MoveBackward();
                        break;
                    case KeyCode.Numpad_1:
                        camera.MoveLeft();
                        break;
                    case KeyCode.Numpad_3:
                        camera.MoveRight();
                        break;

                    // camera turn l / r
                    case KeyCode.Numpad_4:
                        camera.MoveTurnLeft();
                        break;
                    case KeyCode.Numpad_6:
                        camera.MoveTurnRight();
                        break;

                    // camera move speed.
                    case KeyCode.Numpad_5:
                        camera.MoveSwitchSpeed();
                        break;
#endif
                }

                if (isAvatarControl)
                {
                    camera.SwitchMode(ECameraMode.Third);
                }
            }
            else
            {
                switch (p_event.KeyCode)
                {
                    case KeyCode.Up:
                        avatarManager.UserPushForward(false);
                        break;
                    case KeyCode.Down:
                        avatarManager.UserPushBackward(false);
                        break;
                    case KeyCode.F11:
                        guiManager.ShowDebugWindow();
                        break;
#if DEBUG
                    case KeyCode.F1:
                        
                        break;
                    case KeyCode.F2:
                        break;
#if DEBUG
                    case KeyCode.F3:
                        debug_on = !debug_on;
                        break;
#endif
                    case KeyCode.F5:
                        break;
                    case KeyCode.F6:
                        break;
                    case KeyCode.F7:
                        //throw new Exception("Dead!!");
                        break;
                    case KeyCode.F10:
                        //Shaders.AdvancedSea sea = (Shaders.AdvancedSea)shaderManager.GetShaderObject(ShaderManager.ShaderType.AdvancedSea);
                        //sea.SaveMaps();
                        break;
                    case KeyCode.F12:
                        SetWorldTime("2009-02-20 10:00:00");
                        break;
#endif
                }
            }

            return false;
        }

        public bool MouseEventProcessor(Event p_event)
        {
            if (p_event.MouseInputEvent == MouseInputEvent.MouseWheel)
            {
                if (camera.CameraMode != ECameraMode.Build)
                {
                    camera.SwitchMode(ECameraMode.Build);
                }

                //KeyCode.RButton
                if (guiManager.Focused == false)
                    camera.MouseWheelAction(p_event.MouseWheelDelta);
            }

            if (p_event.MouseInputEvent == MouseInputEvent.LMouseLeftUp)
            {
                LMheld = false;
                // FIXME: Remove this code
                //if (menuManager == null && menuManager.Click(Device.CursorControl.Position))
                //{
                //    return (false);
                //}
                if (stateManager.State == State.INITIALIZED)
                {
                    if (LoginMode == "click")
                    {
                        LoginRequest();
                    }

                    return (false);
                }

                if (stateManager.State == State.CONNECTED)
                {
                    if (guiManager.Focused)
                        return (false);

                    if (menuManager != null && menuManager.Visible)
                    {
                        menuManager.Click(Device.CursorControl.Position);
                        return (false);
                    }

                    // start touch code
                    Cursors currentCursor = guiManager.GetCursor();
                    if (currentCursor == Cursors.HAND && entityManager.PrimitiveUnderMouse != null)
                    {
                        string uuid = entityManager.GetUUIDFromLocalID(entityManager.PrimitiveUnderMouse.LocalID);
                        if (uuid != string.Empty)
                            adapter.CallTouchTo(uuid);

                        // FIXME: this only handles "Touch" actions. Other possible ClickAction values include Buy, Pay, OpenTask, PlayMedia, OpenMedia                
                    }
                    else if (currentCursor == Cursors.CHAIR && entityManager.PrimitiveUnderMouse != null)
                    {
                        protocolManager.SitOn(entityManager.PrimitiveUnderMouse.ID.ToString());
                    }
                    // end touch code
                }

                avatarManager.UserPushForward(false);
                avatarManager.AvatarPickRequest();
            }
            if (p_event.MouseInputEvent == MouseInputEvent.LMousePressedDown)
            {
                if (stateManager.State == State.CONNECTED)
                {
                    if (menuManager.Visible || guiManager.Focused)
                    {
                        return (false);
                    }

                    // start touch code
                    Cursors currentCursor = guiManager.GetCursor();
                    if (currentCursor == Cursors.NORMAL)
                    {
                        if (camera.CameraMode != ECameraMode.Build)
                        {
                            camera.SwitchMode(ECameraMode.Build);
                        }

                        if (protocolManager.AvatarConnection.SittingOn() == 0)
                        {
                            avatarManager.UserUpdateMousePosition(p_event.MousePosition);
                        }
                        else
                        {
                            if (!IsStandUpIcon)
                                protocolManager.StandUp();
                        }
                    }
                }

                LMheld = true;
            }
            if (p_event.MouseInputEvent == MouseInputEvent.RMouseLeftUp)
            {
                RMheld = false;
            }
            if (p_event.MouseInputEvent == MouseInputEvent.RMousePressedDown)
            {
                RMheld = true;

                if (camera.CameraMode != ECameraMode.Build)
                {
                    camera.SwitchMode(ECameraMode.Build);
                }
            }
            if (p_event.MouseInputEvent == MouseInputEvent.MouseMoved)
            {
                Cursors currentCursor = guiManager.GetCursor();

                if (stateManager.State == State.CONNECTED)
                {
                    if (LMheld)
                    {
                        if (currentCursor == Cursors.NORMAL)
                        {
                            avatarManager.UserUpdateMousePosition(p_event.MousePosition);
                        }
                    }
                }

                if (RMheld)
                {
                    int deltaX = p_event.MousePosition.X - OldMouseX;
                    int deltaY = p_event.MousePosition.Y - OldMouseY;

                    camera.SetDeltaFromMouse(deltaX, deltaY);
                }
                else if (MMheld)
                {
                    float deltaY = (float)(p_event.MousePosition.Y - OldMouseY) * -0.1f; // negate and scale it

                    // TODO: copied from MouseEventProcessor, should refactor common code
                    if (camera.CameraMode != ECameraMode.Build)
                    {
                        camera.SwitchMode(ECameraMode.Build);
                    }
                    //KeyCode.RButton
                    camera.MouseWheelAction(deltaY);
                }

                OldMouseX = p_event.MousePosition.X;
                OldMouseY = p_event.MousePosition.Y;
            }
            return false;

        }

        private void StateChanged(Managers.State _state)
        {
            Reference.Viewer.Adapter.SendMessage("simconnected", false);

            if (_state == State.CONNECTED)
            {
                if (soundManager != null)
                    soundManager.Stop();

                Reference.Viewer.Adapter.SendMessage("simconnected", true);

                camera.SmoothingReset = true;
                camera.SwitchMode(ECameraMode.Build);

                effectManager.FadeIn(null);
            }

            if (_state == State.INITIALIZED || _state == State.CONNECTED || _state == State.ERROR)
            {
                guiManager.IsShowCursor = true;
            }
            else
            {
                guiManager.IsShowCursor = false;
            }
        }

        #region Login
        public void LoginRequest()
        {
            if (stateManager.State == State.INITIALIZED)
                OnRequest += Login;
        }

        private void Login(object sender, EventArgs args)
        {
            OnRequest -= Login;

            if (LoginValidationCheck() == false)
                return;

            string location = "last";
            if (!string.IsNullOrEmpty(LoginLocation))
                location = LoginLocation;

            lock (stateManager.SyncStat)
            {
                if (stateManager.State == State.INITIALIZED)
                {
                    guiManager.LoginRemoveWindow();
                    protocolManager.Login(ServerURI, FirstName, LastName.ToLower(), Password, location);
                    stateManager.State = State.LOGIN;
                    guiManager.ShowLoginProgress();
                    avatarManager.Start();
                    if (firstLogin)
                    {
                        shaderManager.LoadAll();
                        firstLogin = false;
                    }
                }
            }
        }

        private bool LoginValidationCheck()
        {
            string message = string.Empty;

            ServerURI = (ServerURI == null) ? string.Empty : ServerURI;
            FirstName = (FirstName == null) ? string.Empty : FirstName;
            LastName = (LastName == null) ? string.Empty : LastName;
            Password = (Password == null) ? string.Empty : Password;

            // Null or Empty.
            message += ValidationLengthCheck("ServerURI", ServerURI, 255);
            message += ValidationLengthCheck("FirstName", FirstName, 32);
            message += ValidationLengthCheck("LastName", LastName, 32);
            message += ValidationLengthCheck("Password", Password, 128);
            // chara check.
            message += ValidationCharaCheck("ServerURI", ServerURI);
            message += ValidationCharaCheck("FirstName", FirstName);
            message += ValidationCharaCheck("LastName", LastName);
            message += ValidationCharaCheck("Password", Password);

            if (message != string.Empty)
            {
                ShowMessageBox(message);
                return false;
            }


            // Port check.
            int index = ServerURI.LastIndexOf(":");
            if (index > 0)
            {
                bool isPortOK = true;
                string port = ServerURI.Substring(index + 1);
                int portNum;
                if (int.TryParse(port, out portNum))
                {
                    if (portNum < 0 || 65535 < portNum)
                        isPortOK = false;
                }
                else
                {
                    isPortOK = false;
                }

                if (isPortOK == false)
                {
                    ShowMessageBox(DialogText.PortError);
                    return false;
                }
            }

            return true;
        }

        private string ValidationLengthCheck(string _elementName, string _value, int _maxLength)
        {
            if (string.IsNullOrEmpty(_value))
            {
                m_log.Warn(_elementName + " empty.");

                return _elementName + "\n - " + DialogText.ValidationEmpty + "\n　\n";
            }

            if (_value.Length > _maxLength)
            {
                m_log.Warn(_elementName + " over max number.");

                return _elementName + "\n - " + DialogText.ValidationOver + "\n - (" + DialogText.ValidationMaxLength + ":" + _maxLength.ToString() + ")" + "\n　\n";
            }

            return string.Empty;
        }

        private string ValidationCharaCheck(string _elementName, string _value)
        {
            for (int i = 0; i < _value.Length; i++)
            {
                if (_value[i] < '!' || '~' < _value[i])
                {
                    m_log.Warn(_elementName + " input invalid chara.");

                    return _elementName + "\n - " + DialogText.ValidationInvalidChar + "\n　\n";
                }
            }

            return string.Empty;
        }

        public void ShowMessageBox(string _message)
        {
            guiManager.ShowMessageWindow(_message);
        }

        #endregion

        #region Logout
        public void LogoutRequest()
        {
            lock (stateManager.SyncStat)
            {
                if (stateManager.State != State.CLOSING)
                {
                    guiManager.LogoutRemoveWindow();
                    guiManager.ShowBackground();
                    stateManager.State = State.CLOSING;

                    OnRequest += Logout;
                }
            }
        }

        private void Logout(object sender, EventArgs args)
        {
            OnRequest -= Logout;

            protocolManager.AvatarConnection.Logout();
        }
        #endregion

        public void Shutdown()
        {
            if (stateManager != null)
            {
                lock (stateManager.SyncStat)
                {
                    stateManager.State = State.EXITING;
                }
            }
            if (guithread != null)
            {
                try
                {
                    if (!guithread.Join(30000))
                    {
                        m_log.Error("Abort GUI thread due to timeout");
                        guithread.Abort();
                    }
                }
                catch (Exception e)
                {
                    Reference.Log.Warn(@"[REI]: An exception occured in Shutdown - " + e.Message);
                    Reference.Log.Debug(@"[REI]: An exception occured in Shutdown - " + e.StackTrace);
                }
            }
        }

        public void OpenSupportPageRequest()
        {
            supportPage = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(OpenSupportPage));
            supportPage.Start();
        }

        private void OpenSupportPage(object _parent)
        {
            if (adapter != null)
                adapter.CallOpenWindow("_blank", HelpURL);

            supportPage = null;
        }

        private void NOP()
        {

        }

        /// <summary>
        /// Set shader level.
        /// </summary>
        /// <param name="_level">"low", "middle", "hight"</param>
        public void SetShaderQuality(ShaderManager.ShaderType _type, string _level)
        {
            ShaderLevelType type = ShaderLevelType.High;

            switch (_level)
            {
                case "high":
                    type = ShaderLevelType.High;
                    break;

                case "middle":
                    type = ShaderLevelType.Middle;
                    break;

                case "low":
                    type = ShaderLevelType.Low;
                    break;

            }

            switch (_type)
            {
#if !LINUX
                case ShaderManager.ShaderType.Sea:
                    SeaQuality = type;
                    break;

                case ShaderManager.ShaderType.AdvancedSea:
                    SeaQuality = type;
                    break;

                case ShaderManager.ShaderType.Shadow:
                    break;
#endif

                case ShaderManager.ShaderType.Sky:
                    SkyQuality = type;
                    break;
            }

            m_log.Debug("SetShaderQuality: Level:" + _level.ToString() + " Type:" + _type.ToString());
        }

        public static bool CreateDefaultConfig(string _path)
        {
            System.IO.StreamWriter sw = null;
            try
            {
                try
                {
                    sw = new System.IO.StreamWriter(_path, false);
                    sw.WriteLine("[Startup]");
                    sw.WriteLine("version_server_url = http://3di-opensim.com/openviewer/current-version.xml");
                    sw.WriteLine("help_url = http://3di-opensim.com/openviewer/");
                    sw.WriteLine("locale = jp");
                    sw.WriteLine("debug_tab = false");
                    sw.WriteLine("reverse_camera_pitch = false");
                    sw.WriteLine("teleport_timeout = 90");
                    sw.WriteLine("cache_delete = false");
                    sw.WriteLine("cache_size = " + DEFAULT_CACHE_MB_SIZE.ToString());
                    sw.WriteLine("guifont_face = msgothic.ttf");
                    sw.WriteLine("guifont_size = 15");
                    sw.WriteLine();
                    sw.WriteLine("[Shader]");
                    sw.WriteLine("sea_quality = low");
                    sw.WriteLine("sky_quality = low");
                    sw.WriteLine();
                    sw.WriteLine("[Debug]");
                    sw.WriteLine("voice_wait_time = 1");
                }
                catch (Exception e)
                {
                    m_log.Fatal("CreateDefaultConfig", e);
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return true;
        }

        public void SetWorldTime(string _dataTime)
        {
            DateTime res;
            if (DateTime.TryParse(_dataTime, out res))
            {
                worldTime = res;
            }
        }

        #region WorldAmbientLightColor
        public void RequestSetWorldAmbientLightColor()
        {
            OnRequest += SetWorldAmbientLightColor;
        }

        private void SetWorldAmbientLightColor(object sender, EventArgs args)
        {
            OnRequest -= SetWorldAmbientLightColor;

            Reference.SceneManager.SetAmbientLight(AmbientLightColor);
        }
        #endregion
    }
}
