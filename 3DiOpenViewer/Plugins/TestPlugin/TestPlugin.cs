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

// This is a sample plugin that keeps track of a user's health, displays and updates the health meter.
// This plugin communicates with the javascript interface through the following methods:
// damage(hitpoints:float) : void Decreases the user's health by hitpoints point
// heal(hitpoints:float) : void Increases the user's health by hitpoints point

using IrrlichtNETCP;
using System.Collections.Generic;

namespace OpenViewer.Plugins
{
    public class TestPlugin : IManagerPlugin 
    {
        private SceneNode parent = null;                    // The SceneNode under which all graphical elements are added
        private BillboardSceneNode healthBar = null;        // A simple healthbar to display above the avatar's head
        private TextSceneNode healthText = null;            // A text displaying when the avatar is dead
        private Texture healthBarG = null;                  // A green health bar to display when health > 50
        private Texture healthBarR = null;                  // A red health bar to display when health < 50
        private float health = 100f;                        // The health of the user

        // The following are the damage and heal hitpoint texts that travel up from the avatar on damage/heal
        private Dictionary<string, TextSceneNode> damages = new Dictionary<string, TextSceneNode>();
        private Dictionary<string, TextSceneNode> heals = new Dictionary<string, TextSceneNode>();

        public void Initialise(Viewer viewer)
        {
            // Mandatory initialization of Reference
            Reference = viewer.Reference;

            // Registering Javascript callbacks
            Reference.Viewer.Adapter.RegisterCallback("damage", new Callback(on_damage));
            Reference.Viewer.Adapter.RegisterCallback("heal", new Callback(on_heal));

            // Initializing persistent resources. These will not be dropped on Cleanup()
            healthBarG = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\healthbar_g.png");
            healthBarR = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"\media\healthbar_r.png");
        }

        /// <summary>
        /// Javascript callback for Damage()
        /// </summary>
        /// <param name="message">A float value that represents the hitpoints to substract</param>
        /// <returns>empty string</returns>
        public string on_damage(string message)
        {
            if (health > 0.1f)
            {
                // Create the hitpoint text billboard
                TextSceneNode tsn =
                    Reference.SceneManager.AddBillboardTextSceneNodeW(Reference.GUIEnvironment.BuiltInFont, message,
                                                                      Reference.Viewer.AvatarManager.UserObject.Node,
                                                                      new Dimension2Df(0.5f, 0.5f),
                                                                      new Vector3D(), -1, Color.White, Color.Red);
                float damage = 0f;
                float.TryParse(message, out damage);
                health -= damage;
                damages.Add(System.DateTime.Now.Ticks.ToString(), tsn);
                tsn.Position = new Vector3D(0, 2, 0);
            }

            if (health < 0.1f)
            {
                // Send a message to other plugins listening to events called 'testplugin_dead'
                Reference.Viewer.Adapter.SendMessage("testplugin_dead", null);
            }

            return (string.Empty);
        }

        /// <summary>
        /// Javascript callback for Heal()
        /// </summary>
        /// <param name="message">A float value that represents the hitpoints to substract</param>
        /// <returns>empty string</returns>
        public string on_heal(string message)
        {
            if (health < 100.0f)
            {
                // Create the hitpoint text billboard
                TextSceneNode tsn =
                    Reference.SceneManager.AddBillboardTextSceneNodeW(Reference.GUIEnvironment.BuiltInFont, message,
                                                                      Reference.Viewer.AvatarManager.UserObject.Node,
                                                                      new Dimension2Df(0.5f, 0.5f),
                                                                      new Vector3D(), -1, Color.White, Color.Green);
                float heal = 0f;
                float.TryParse(message, out heal);
                health += heal;
                heals.Add(System.DateTime.Now.Ticks.ToString(), tsn);
                tsn.Position = new Vector3D(0, 2, 0);
            }
            return (string.Empty);
        }

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///  This is the Initializer to run on every (re)start of the manager
        /// </summary>
        public void Initialize()
        {
            // Create the SceneNode under which we will add our Scene elements
            parent = Reference.SceneManager.AddEmptySceneNode(Reference.SceneManager.RootSceneNode, -1);

            // Add elements under parent
            healthBar = Reference.SceneManager.AddBillboardSceneNode(parent, new Dimension2Df(0.5f, 0.1f), -1);
            healthBar.SetMaterialTexture(0, healthBarG);
            healthBar.SetMaterialType(MaterialType.TransparentAlphaChannel);
            healthBar.SetMaterialFlag(MaterialFlag.Lighting, false);

            healthText = Reference.SceneManager.AddBillboardTextSceneNodeW(Reference.GUIEnvironment.BuiltInFont, "(x_x)",
                                                                           parent, new Dimension2Df(0.7f, 0.2f),
                                                                           new Vector3D(), -1, Color.White,
                                                                           Color.White);
            // We will only display this text when the avatar is dead
            healthText.Visible = false;
        }

        /// <summary>
        /// Called every frame before rendering
        /// </summary>
        /// <param name="frame">Framecount</param>
        public void Update(uint frame)
        {
            // Only render while we are logged in
            if (Reference.Viewer.StateManager.State == OpenViewer.Managers.State.CONNECTED)
            {
                // Check if we are dead, you can optimize this checking
                if (health < 0.1f)
                {
                    healthBar.Visible = false;
                    healthText.Visible = true;
                    healthText.Position = Reference.Viewer.AvatarManager.UserObject.Node.AbsolutePosition + new Vector3D(0, 2.3f, 0);
                }
                else
                {
                    // Not dead, update the healthbar
                    healthBar.Visible = true;
                    healthText.TextW = "";
                    healthText.Visible = false;
                    healthBar.Position = Reference.Viewer.AvatarManager.UserObject.Node.AbsolutePosition + new Vector3D(0, 2.3f, 0);
                    healthBar.Size = new Dimension2Df(0.5f*health/100.0f, 0.1f);
                    healthBar.SetMaterialTexture(0, health > 50.0f ? healthBarG : healthBarR);
                }

                // Move damage texts upward
                if (damages.Count > 0)
                {
                    List<string> remove = new List<string>();
                    foreach (KeyValuePair<string, TextSceneNode> kvp in damages)
                    {
                        kvp.Value.Position += new Vector3D(0.0f, 0.1f, 0.0f);
                        // Remove the text after it traveled 5m
                        if (kvp.Value.Position.Y > 5.0f)
                            remove.Add(kvp.Key);
                    }
                    foreach (string s in remove)
                    {
                        TextSceneNode tsn = damages[s];
                        Reference.SceneManager.AddToDeletionQueue(tsn);
                        damages.Remove(s);
                    }
                }

                // Move healing texts upward
                if (heals.Count > 0)
                {
                    List<string> remove = new List<string>();
                    foreach (KeyValuePair<string, TextSceneNode> kvp in heals)
                    {
                        kvp.Value.Position += new Vector3D(0.0f, 0.1f, 0.0f);
                        // Remove the text after it traveled 5m
                        if (kvp.Value.Position.Y > 5.0f)
                            remove.Add(kvp.Key);
                    }
                    foreach (string s in remove)
                    {
                        TextSceneNode tsn = heals[s];
                        Reference.SceneManager.AddToDeletionQueue(tsn);
                        heals.Remove(s);
                    }
                }
            }
        }

        /// <summary>
        /// Custom scene drawing can happen here
        /// </summary>
        public void Draw()
        {
        }

        /// <summary>
        /// Clean up transient resources
        /// </summary>
        public void Cleanup()
        {
            // Delete everything below the parent SceneNode
            if (parent != null)
                Reference.SceneManager.AddToDeletionQueue(parent);

            // Delete status text queues and reset health
            heals.Clear();
            damages.Clear();
            health = 100f;
        }
        
        public RefController Reference { get; set; }

        /// <summary>
        /// Returns the plugin version
        /// </summary>
        /// <returns>Plugin version in MAJOR.MINOR.REVISION.BUILD format</returns>
        public string Version { get { return("0.0.1"); } }

        /// <summary>
        /// Returns the plugin name
        /// </summary>
        /// <returns>Plugin name, eg MySQL User Provider</returns>
        public string Name { get { return ("TestPlugin");} }

        /// <summary>
        /// Default-initialises the plugin
        /// </summary>
        public void Initialise()
        {
        }
    }
}