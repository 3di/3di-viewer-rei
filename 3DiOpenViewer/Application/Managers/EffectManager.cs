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

//#define BILLBOARD_NAME
using System;
using System.Collections.Generic;
using IrrlichtNETCP;

namespace OpenViewer.Managers
{
    public class EffectManager : BaseComponent
    {
        public enum AvatarNameType
        {
            FirstAndLast,
            First,
            Last,
            None,
        }

        private Dictionary<IntPtr, ParticleEmitter> emitterCleanup = new Dictionary<IntPtr, ParticleEmitter>();
        private Dictionary<IntPtr, ParticleAffector> affectorCleanup = new Dictionary<IntPtr, ParticleAffector>();
        private AlphaController fadeAlpha = new AlphaController(0, 255, 8);
        private EventHandler fadeEvent;

        private Texture[] voiceEffectTexture = new Texture[10];

        public EffectManager(Viewer _viewer)
            : base(_viewer, -1)
        {
            fadeAlpha.OnEnd += AlphaEventHandler;

            for (int i = 0; i < voiceEffectTexture.Length; i++)
                voiceEffectTexture[i] = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\textures\voice_level_" + i.ToString() + ".png");
        }

        public void FadeIn(EventHandler _eventHandler)
        {
            fadeEvent = _eventHandler;
            fadeAlpha.Set(255, 0, 8);
        }

        public void FadeOut(EventHandler _eventHandler)
        {
            fadeEvent = _eventHandler;
            fadeAlpha.Set(0, 255, 8);
        }

        private void AlphaEventHandler(object _sender, EventArgs _arg)
        {
            if (fadeEvent != null)
                fadeEvent(_sender, _arg);
        }

        public override void Update(uint frame)
        {
            if (fadeAlpha != null)
                fadeAlpha.Update();

            base.Update(frame);
        }

        public override void Draw()
        {
            if (Reference.Viewer.StateManager.State == State.CONNECTED)
            {
                Texture tex = Reference.Viewer.GuiManager.TeleportWindowBackground;
                if (tex != null && fadeAlpha.IsEnd == false)
                {
                    Position2D pos = new Position2D(
                        Reference.Viewer.Width / 2 - tex.OriginalSize.Width / 2,
                        Reference.Viewer.Height / 2 - tex.OriginalSize.Height / 2
                        );

                    Reference.VideoDriver.Draw2DImage(tex, pos, new Color((int)fadeAlpha.Value255, 255, 255, 255), true);
                }
            }

            base.Draw();
        }

        public SceneNode AddVoiceEffectSceneNode(SceneNode _parentNode)
        {
            BillboardSceneNode node = Reference.SceneManager.AddBillboardSceneNode(_parentNode, new Dimension2Df(0.5f, 0.25f), -1);
            node.SetMaterialTexture(0, voiceEffectTexture[0]);
            node.Position = new Vector3D(0, 2.2f, 0);
            node.SetMaterialType(MaterialType.TransparentAlphaChannel);
            node.SetMaterialFlag(MaterialFlag.Lighting, false);
            node.Visible = false;

            return node;
        }

        public void UpdateVoiceEffectSceneNode(SceneNode _parentNode, int _level)
        {
            _level = OpenMetaverse.Utils.Clamp(_level, 0, voiceEffectTexture.Length - 1);

            if (_parentNode is BillboardSceneNode)
                _parentNode.SetMaterialTexture(0, voiceEffectTexture[_level]);
        }


        /// <summary>
        /// This function add avatar name to target node.
        /// </summary>
        /// <param name="_parentNode">target node</param>
        /// <param name="_name"></param>
        /// <param name="_frame"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public SceneNode AddNameSceneNode(SceneNode _parentNode, string _first, string _last, bool _frame, AvatarNameType _type)
        {
            if (_type == AvatarNameType.None)
                return null;

            SceneNode node = Reference.SceneManager.AddEmptySceneNode(_parentNode, -1);
            node.Position = new Vector3D(0, 1.9f, 0);

            string name = _first + " " + _last;
            Dimension2Df size = new Dimension2Df(0.4f, 0.1f);
            switch (_type)
            {
                case AvatarNameType.FirstAndLast:
                    // default
                    break;
                case AvatarNameType.First:
                    name = _first;
                    size = new Dimension2Df(0.2f, 0.1f);
                    break;
                case AvatarNameType.Last:
                    name = _last;
                    size = new Dimension2Df(0.2f, 0.1f);
                    break;
            }

#if BILLBOARD_NAME
            TextSceneNode textNode = Reference.SceneManager.AddBillboardTextSceneNodeW(Reference.GUIEnvironment.BuiltInFont, name, node, size, new Vector3D(), -1, Color.White, Color.White);
#else
            TextSceneNode textNode = Reference.SceneManager.AddTextSceneNode(Reference.GUIEnvironment.Skin.Font, name, Color.White, node);
#endif

            // If _frame is true, set font shadow.
            if (_frame)
            {
#if BILLBOARD_NAME
#else
                textNode = Reference.SceneManager.AddTextSceneNode(Reference.GUIEnvironment.Skin.Font, name, Color.Black, node);
#endif
            }

            return node;
        }

        public SceneNode AddGhostSceneNode(SceneNode _parentNode)
        {
            ParticleSystemSceneNode node = Reference.SceneManager.AddParticleSystemSceneNode(false, _parentNode, -1);

            ParticleEmitter em = node.CreateBoxEmitter(
                new Box3D(0, 0, 0, 0.4f, 1.4f, 0.4f),
                new Vector3D(0.000f, 0.0004f, 0.000f),
                20,
                26,
                new Color(0, 255, 255, 191),
                new Color(0, 191, 255, 255),
                600,
                900,
                360
                );

            node.SetEmitter(em);
            em.Drop();
            lock (emitterCleanup) { if (!emitterCleanup.ContainsKey(_parentNode.Raw)) { emitterCleanup.Add(_parentNode.Raw, em); } }

            ParticleAffector paf = node.CreateFadeOutParticleAffector(new Color(0, 0, 0, 0), 150);
            node.AddAffector(paf);
            // paf.Dispose(); <- seems to cause AccessViolationExceptions in AvatarManager
            paf.Drop();
            lock (affectorCleanup) { if (!affectorCleanup.ContainsKey(_parentNode.Raw)) { affectorCleanup.Add(_parentNode.Raw, paf); } }

            node.Position = new Vector3D(0, 0, 0);
            node.ParticleSize = new Dimension2Df(0.6f, 0.6f);
            node.SetMaterialFlag(MaterialFlag.Lighting, false);
            node.SetMaterialType(MaterialType.TransparentVertexAlpha);

            Texture tex = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\textures\ghost.bmp");
            if (tex != null)
                node.SetMaterialTexture(0, tex);

            return node;
        }

        public void RemoveGhostNode(SceneNode _parentNode)
        {
            IntPtr key = _parentNode.Raw;
            if (_parentNode.Children.Length != 0)
            {
                Reference.SceneManager.AddToDeletionQueue(_parentNode.Children[0]);
            }
            lock (emitterCleanup)
            {
                if (emitterCleanup.ContainsKey(key))
                {
                    if (NativeElement.Elements.ContainsKey(emitterCleanup[key].Raw))
                        NativeElement.Elements.Remove(emitterCleanup[key].Raw); 
                    emitterCleanup.Remove(key);
                }
            }
            lock (affectorCleanup)
            {
                if (affectorCleanup.ContainsKey(key))
                {
                    if (NativeElement.Elements.ContainsKey(affectorCleanup[key].Raw))
                        NativeElement.Elements.Remove(affectorCleanup[key].Raw); 
                    affectorCleanup.Remove(key);
                }
            }
        }
    }
}
