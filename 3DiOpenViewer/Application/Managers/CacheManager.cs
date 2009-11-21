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

using System.Collections.Generic;
using System.IO;

namespace OpenViewer.Managers
{
    public class CacheManager : BaseManager
    {
        public const long CACHE_MAX_MAX = 999 * 1000 * 1000; // 999GB
        public const long CACHE_MAX_MIN = 1 * 1000 * 1000;   // 001MB

        private const int DEFAULT_CACHE_SIZE = 300; // MB
        private const bool DEFAULT_CACHE_ALL_DELETE = false;

        public long CacheSize { get { return dc.TotalSize; } }
        public long CacheMaxSize { get; set; }
        public string CachePath { get { return cachePath; } set { cachePath = value; dc.Move(value); } }
        public bool CacheAllDelete { get; set; }

        private DirectoryController dc = new DirectoryController("");
        private string cachePath;

        public CacheManager(Viewer _viewer, int _id)
            : base(_viewer, _id)
        {
            CacheMaxSize = DEFAULT_CACHE_SIZE;
            CachePath = OpenViewer.Util.AssetFolder;
            CacheAllDelete = DEFAULT_CACHE_ALL_DELETE;
        }

        public void Clean()
        {
            long tmp = CacheMaxSize;

            if (CacheAllDelete)
                CacheMaxSize = 0;

            if (CacheMaxSize < dc.TotalSize)
                DeleteCache();

            CacheMaxSize = tmp;
        }

        #region Private function.
        private void DeleteCache()
        {
            Reference.Log.Debug("CACHE start to delete cache." + " " + this.ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);

            Queue<string> request = new Queue<string>();

            int index = 0;
            long requestTotalSize = 0;
            while (index < dc.CacheList.Count)
            {
                request.Enqueue(dc.CacheList[index].Path);
                requestTotalSize += dc.CacheList[index].Size;
                if (dc.TotalSize - requestTotalSize <= CacheMaxSize)
                    break;

                index++;
            }

            while (request.Count > 0)
            {
                string path = request.Dequeue();
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Reference.Log.Debug("CACHE delete cache: " + path + " " + this.ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
                }
            }

            dc.Reset();

            Reference.Log.Debug("CACHE completed to delete cache. cache folder size: " + dc.TotalSize.ToString() + " " + this.ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
        #endregion
    }
}
