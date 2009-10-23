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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using OpenMetaverse;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenViewer
{
    public class Util
    {
        private static readonly log4net.ILog m_log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint SHGetKnownFolderPath(ref Guid rfid, uint dwFlags, IntPtr hToken, out StringBuilder path);

        #region Const
        /// <summary>
        /// Asset cache directory for Avatar's data.
        /// </summary>
        public const string AssetDirectory = "assets";

        /// <summary>
        /// Sound cache directory.
        /// </summary>
        public const string SoundDirectory = "sounds";
        public const string AvatarAnimationExtension = ".xml";
        public const string SoundExtension = ".ogg";
        #endregion

        public static string ApplicationDataDirectory
        {
            get
            {
                return (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            }
        }

        public static string UserCacheDirectory
        {
            get
            {
                string UserCacheLocation = @"\3Di\OpenViewer";

                OperatingSystem osInfo = Environment.OSVersion;

                if (osInfo.Version.Major >= 6)
                {
                    Guid FOLDERID_LocalAppDataLow = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
                    StringBuilder path = new StringBuilder(260);
                    uint retval = SHGetKnownFolderPath(ref FOLDERID_LocalAppDataLow, 0, IntPtr.Zero, out path);
                    if (retval == 0)
                        return path.ToString() + UserCacheLocation;
                }

                return (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + UserCacheLocation);
            }
        }

        public static string ConfigFolder = Path.Combine(Util.UserCacheDirectory, @"configs\");
        public static string LogFolder = Path.Combine(Util.UserCacheDirectory, @"logs\");
        public static string GuiFolder = Path.Combine(Util.UserCacheDirectory, @"gui\");
        public static string AssetFolder = Path.Combine(Util.UserCacheDirectory, @"assets\");
        public static string TextureFolder = Path.Combine(Util.UserCacheDirectory, @"assets\textures\");
        public static string TerrainFolder = Path.Combine(Util.UserCacheDirectory, @"assets\terrains\");
        public static string ModelFolder = Path.Combine(Util.UserCacheDirectory, @"assets\models\");
        public static string SoundFolder = Path.Combine(Util.UserCacheDirectory, @"assets\sounds\");

        public static void InitializeFolderHierarchy()
        {
            // Current hierarchy
            // -configs/
            // -logs/
            // -gui/
            // -assets/
            //     - terrains/
            //     - textures/
            //     - models/
            //     - sounds/
            
            string folderBase = Util.UserCacheDirectory;
            string[] folders = {
                                   Path.Combine(folderBase, @"configs\"),
                                   Path.Combine(folderBase, @"logs\"),
                                   Path.Combine(folderBase, @"gui\"), 
                                   Path.Combine(folderBase, @"assets\"), 
                                   Path.Combine(folderBase, @"assets\terrains\"),
                                   Path.Combine(folderBase, @"assets\textures\"),
                                   Path.Combine(folderBase, @"assets\models\"),
                                   Path.Combine(folderBase, @"assets\sounds\"),
                                   Path.Combine(folderBase, @"plugins\"),
                               };

            foreach (string s in folders)
            {
                if (!Directory.Exists(s))
                    Directory.CreateDirectory(s);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(Util.ApplicationDataDirectory, @"media\avatar\")))
            {
                if (!File.Exists(Path.Combine(Util.UserCacheDirectory, @"assets\models\" + Path.GetFileName(file))))
                    File.Copy(Path.Combine(Util.ApplicationDataDirectory, @"media\avatar\" + Path.GetFileName(file)), 
                              Path.Combine(Util.UserCacheDirectory, @"assets\models\" + Path.GetFileName(file)), 
                              true);
            }
        }

        private static UUID keepaliveUUID = UUID.Zero;
        public static UUID KeepaliveUUID { get { return (keepaliveUUID); } set { keepaliveUUID = value; } }

        /// <summary>
        /// This function checks the keepalive file to see if it is OK to start a viewer instance
        /// ie, another viewer is not running already
        /// </summary>
        /// <returns></returns>
        public static bool IsInitSafe()
        {
            const string keepaliveFile = "keepalive";
            bool isSafe = false;
            if (File.Exists(Util.UserCacheDirectory + @"\" + keepaliveFile))
            {
                // Check for expiry
                StreamReader sr = new StreamReader(Util.UserCacheDirectory + @"\" + keepaliveFile);
                string status = sr.ReadToEnd();
                sr.Close();

                if (string.IsNullOrEmpty(status))
                {
                    isSafe = true;
                }
                else
                {
                    string[] expiry = status.Split(':');
                    long currentTime = DateTime.Now.Ticks;
                    if (expiry.Length < 2)
                    {
                        isSafe = true;
                    }
                    else
                    {
                        //int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                        //int expPID = 0;

                        // Same process... this should not happen in the initial version,
                        // but this might be supported later
                        if (UUID.Parse(expiry[0]) == Util.keepaliveUUID)
                        {
                            isSafe = true;
                        }

                        long expDate = 0;
                        if (long.TryParse(expiry[1], out expDate))
                        {
                            if (expDate < currentTime - 10 * 10000000)
                                isSafe = true;
                        }
                    }
                }
            }
            else
            {
                isSafe = true;
            }
            return (isSafe);
        }

        /// <summary>
        /// This function updates a timestamp in a file to make sure that only one instance of the viewer can run
        /// at any time.
        /// </summary>
        public static void KeepAlive(object param)
        {
            //int PID = System.Diagnostics.Process.GetCurrentProcess().Id;
            long ticks = DateTime.Now.Ticks;

            // Format: PID:ticks
            const string keepaliveFile = "keepalive";
            System.IO.StreamWriter sw = new StreamWriter(Util.UserCacheDirectory + @"\" + keepaliveFile, false);
            sw.WriteLine(keepaliveUUID.ToString() + ":" + ticks.ToString());
            sw.Close();
        }

        /// <summary>
        /// Removes the keepalive file to enable other instances to run
        /// </summary>
        public static void CloseKeepAlive()
        {
            const string keepaliveFile = "keepalive";
            if (File.Exists(Util.UserCacheDirectory + @"\" + keepaliveFile))
            {
                File.Delete(Util.UserCacheDirectory + @"\" + keepaliveFile);
            }
        }

        public static T Clamp<T>(T x, T min, T max)
            where T : System.IComparable<T>
        {
            return x.CompareTo(max) > 0 ? max :
                x.CompareTo(min) < 0 ? min :
                x;
        }

        public static ImageCodecInfo GetImageEncoder(string imageType)
        {
            imageType = imageType.ToUpperInvariant();

            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatDescription == imageType)
                {
                    return info;
                }
            }
            return null;
        }

        /// <summary>
        /// Saves bmp for the terrain
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="filename"></param>
        public static void SaveBitmapToFile(Bitmap bitmap, string filename)
        {
            ImageCodecInfo bmpEncoder = GetImageEncoder("BMP");

            Bitmap resize = new Bitmap(bitmap);
            resize.Save( filename, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        /// <summary>
        /// Splits the Firstname and last name via space.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        public static void separateUsername(string username, out string firstname, out string lastname)
        {
            int index = username.IndexOf(" ");

            if (index == -1)
            {
                firstname = username.Trim();
                lastname = String.Empty;
            }
            else
            {
                firstname = username.Substring(0, index).Trim();
                lastname = username.Substring(index + 1).Trim();
            }
        }

        /// <summary>
        /// Fix invalid loginURIs
        /// </summary>
        /// <param name="loginURI"></param>
        /// <returns></returns>
        public static string getSaneLoginURI(string loginURI)
        {
            // libSL requires the login URI to begin with "http://" or "https://"

            Regex re = new Regex("://");
            string[] parts = re.Split(loginURI.Trim());

            if (parts.Length > 1)
            {
                if (parts[0].ToLower() == "http" || parts[0].ToLower() == "https")
                    return loginURI;
                else
                    return "http://" + parts[1];
            }
            else
                return "http://" + loginURI;
        }

        public static void DownloadImage(string uri, string local)
        {
            try
            {
                Uri imageURI = new Uri(uri);

                System.Net.WebClient wc = new System.Net.WebClient();
                try
                {
                    wc.DownloadFile(uri, local);
                    if (File.Exists(local))
                    {
                        Image im = System.Drawing.Image.FromFile(local);
                        im.Save(local + ".png", ImageFormat.Png);
                        im.Dispose();
                        File.Delete(local);
                    }
                }
                catch (Exception e)
                {
                    m_log.Warn("[IMAGE DL]: " + uri, e);
                }
            }
            catch (Exception e)
            {
                m_log.Error("DownloadImage:" + uri, e);
            }
        }

        public static void DownloadSound(string _uri, string _filename)
        {
            try
            {
                Uri imageURI = new Uri(_uri);
                System.Net.WebClient wc = new System.Net.WebClient();
                try
                {
                    wc.DownloadFile(_uri, _filename);
                }
                catch (Exception e)
                {
                    m_log.Warn("[SOUND DL]: " + e.Message);
                    m_log.Debug("[SOUND DL]:" + e.StackTrace);
                }
            }
            catch (Exception e)
            {
                m_log.Error("DownloadSound:" + _uri, e);
            }
        }

        /// <summary>
        /// Offsets a position by the Global position determined by the regionhandle
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3 OffsetGobal(ulong regionHandle, Vector3 pos)
        {
            
            uint locationx = 0;
            uint locationy = 0;
            Utils.LongToUInts(regionHandle, out locationx, out locationy);
            pos.X = (int)locationx + pos.X;
            pos.Y = (int)locationy + pos.Y;

            return pos;
        }

        /// <summary>
        /// Fix to the fact that Microsoft only provides int RGB
        /// :D
        /// </summary>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Color FromArgbf(float a, float r, float g, float b)
        {
            return Color.FromArgb((byte)(255 * a), (byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
        }

        /// <summary>
        /// Converts byte values into the more complicated 32 bit constructor
        /// Eevil :D
        /// </summary>
        /// <param name="a"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return Color.FromArgb((int)(a << 24 | r << 16 | g << 8 | b));
        }


        public static Color FromString(string s)
        {
            return FromString(s, Color.White);
        }

        public static Color FromString(string s, Color _color)
        {
            if (string.IsNullOrEmpty(s.Trim()))
            {
                m_log.Warn("[COLOR] Replaced the empty color with white.  " + s);
                return (_color);
            }

            try
            {
                Color c = System.Drawing.ColorTranslator.FromHtml(s);
                return (c);
            }
            catch (Exception)
            {
                m_log.Warn("[COLOR] An error occured while parsing " + s);
                return (_color);
            }
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static IrrlichtNETCP.Vector3D Lerp(IrrlichtNETCP.Vector3D value1, IrrlichtNETCP.Vector3D value2, float amount)
        {
            return new IrrlichtNETCP.Vector3D(
                Util.Lerp(value1.X, value2.X, amount),
                Util.Lerp(value1.Y, value2.Y, amount),
                Util.Lerp(value1.Z, value2.Z, amount));
        }

        public static IrrlichtNETCP.Vector3D Vector3DFromStringXYZ(string _vectors)
        {
            IrrlichtNETCP.Vector3D vector = new IrrlichtNETCP.Vector3D();

            if (string.IsNullOrEmpty(_vectors))
                return vector;

            string[] colors = _vectors.Split(new char[] { ',' });

            if (colors.Length < 3)
                return vector;

            float x, y, z;

            if (float.TryParse(colors[0], out x) == false)
                return vector;

            if (float.TryParse(colors[1], out y) == false)
                return vector;

            if (float.TryParse(colors[2], out z) == false)
                return vector;

            vector = new IrrlichtNETCP.Vector3D(x, y, z);

            return vector;
        }

        public static IrrlichtNETCP.Colorf ColorfFromStringRGB(string _colors)
        {
            IrrlichtNETCP.Colorf color = IrrlichtNETCP.Colorf.White;

            if (string.IsNullOrEmpty(_colors))
                return color;

            string[] colors = _colors.Split(new char[] { ',' });

            if (colors.Length < 3)
                return color;

            float r, g, b;

            if (float.TryParse(colors[0], out r) == false)
                return color;

            if (float.TryParse(colors[1], out g) == false)
                return color;

            if (float.TryParse(colors[2], out b) == false)
                return color;

            r = Util.Clamp<float>(r, 0, 1);
            g = Util.Clamp<float>(g, 0, 1);
            b = Util.Clamp<float>(b, 0, 1);

            color = new IrrlichtNETCP.Colorf(1, r, g, b);

            return color;
        }
    }
}
