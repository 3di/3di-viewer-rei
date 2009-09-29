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

namespace OpenViewer
{
    /// <summary>
    /// IManagerPlugin is the basis of plugins that can be loaded into the Viewer
    /// </summary>
    public interface IManagerPlugin : IPlugin
    {
        /// <summary>
        /// Initializes the plugin. Reference must be initialized inside this method
        /// //Reference = viewer.Reference;
        /// </summary>
        /// <param name="viewer">The Viewer instance into which this plugin is loaded</param>
        void Initialise(Viewer viewer);

        /// <summary>
        /// Initializer to run on every (re)start of the Manager
        /// </summary>
        void Initialize();

        /// <summary>
        /// Update is called right before rendering.
        /// You can change items in the Scene ONLY within this function
        /// </summary>
        /// <param name="frame">Framecount, use it for frame skipping (not an accurate timing source)</param>
        void Update(uint frame);

        /// <summary>
        /// Called within the RenderLoop, after the default elements have been rendered
        /// </summary>
        void Draw();

        /// <summary>
        /// Called when the managers unload (exiting, logging out, teleporting)
        /// Needs to release all transient resources
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Reference is the entry to the core objects
        /// </summary>
        RefController Reference { get; set; }
    }

    public class ManagerPluginInitialiser : PluginInitialiserBase
    {
        private Viewer viewer;
        public ManagerPluginInitialiser (Viewer v) { viewer = v; }
        public override void Initialise (IPlugin plugin)
        {
            IManagerPlugin p = plugin as IManagerPlugin;
            p.Initialise (viewer);
            p.Reference = viewer.Reference;
        }
    }
}