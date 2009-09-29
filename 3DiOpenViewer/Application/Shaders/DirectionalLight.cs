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
using IrrlichtNETCP;

namespace OpenViewer.Shaders
{
    public class DirectionalLight: BaseComponent
    {
        #region private element.
        private SceneNode parentNode;
        private LightSceneNode light;
        private Vector3D rotation;
        private string name;
        #endregion

        #region property element.
        /// <summary>
        /// color value 0 - 1.
        /// </summary>
        public Colorf Diffuse = Colorf.White;
        public Colorf Specular = Colorf.White;
        public Colorf Ambient = Colorf.Gray;
        #endregion

        public DirectionalLight(Viewer viewer, SceneNode _parentNode, string _name)
            : base(viewer, -1)
        {
            parentNode = _parentNode;
            rotation = new Vector3D();
            name = _name;
        }

        public LightSceneNode Load(Colorf _diffuse, Colorf _specular, Colorf _ambient)
        {
            Diffuse = _diffuse;
            Specular = _specular;
            Ambient = _ambient;

            return Load();
        }

        /// <summary>
        /// Use this method only once, and after having set all parameters.
        /// </summary>
        public LightSceneNode Load()
        {
            light = Reference.SceneManager.AddLightSceneNode(parentNode, new Vector3D(), Colorf.Red, 0, -1);

            return light;
        }
        
        /// <summary>
        /// Use this method whenever the light parameters are changed.
        /// </summary>
        public void Update()
        {
            light.Rotation = rotation;

            Light data = new Light();
            data.Type = LightType.Directional;
            data.DiffuseColor = Diffuse;
            data.SpecularColor = Specular;
            data.AmbientColor = Ambient;

            light.LightData = data;
        }

        private const float sunriseAngle = 2.1f;
        private const float sunsetAngle = 4.2f;
        private float angle = 0;
        private Quaternion qx = new Quaternion();
        private Quaternion qy = new Quaternion();
        public void UpdateDirection()
        {
            DateTime now = Reference.Viewer.WorldTime;
            TimeSpan span = new DateTime(2009, 1, 1, now.Hour, now.Minute, now.Second) - DateTime.Parse("2009-01-01 07:00:00");
            int sec = span.Hours * 3600 + span.Minutes * 60 + span.Seconds;
            int end = 10 * 3600 + 0 * 60 + 0;
            float amount = (float)sec / end;
            amount = Util.Clamp<float>(amount, 0, 1);
            angle = OpenViewer.Util.Lerp(sunriseAngle, sunsetAngle, amount);

            qx.fromAngleAxis((float)(Math.PI / 1.25f), new Vector3D(1, 0, 0));
            qy.fromAngleAxis(angle, new Vector3D(0, 1, 0));
            qy = qx * qy;
            Vector3D rot;
            qy.toEuler(out rot);
            rotation = new Vector3D(rot.X, rot.Y, rot.Z) * OpenMetaverse.Utils.RAD_TO_DEG;
        }

        public Vector3D Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
    }
}
