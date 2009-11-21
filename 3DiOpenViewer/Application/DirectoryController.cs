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
using System.IO;

namespace OpenViewer
{
    public class DirectoryController
    {
        public struct CacheInfo
        {
            public string Path;
            public long Size;
            public DateTime Date;

            public CacheInfo(string _path, long _size, DateTime _date)
            {
                Path = _path;
                Size = _size;
                Date = _date;
            }
        }

        public enum SortType
        {
            /// <summary>
            /// Sort from old date (by the time stamp).
            /// </summary>
            Date,

            /// <summary>
            /// Sort from small file size.

            /// </summary>
            Size,
        }

        public List<CacheInfo> CacheList = new List<CacheInfo>();
        public SortType SortMode { get; set; }
        public long TotalSize { get { return totalSize; } }

        private string path = string.Empty;
        private long totalSize = 0;

        public DirectoryController(string _path)
        {
            SortMode = SortType.Date;

            Move(_path);
        }

        public void Move(string _path)
        {
            GetDirectoryInfoRoot(_path);
        }

        public void Reset()
        {
            GetDirectoryInfoRoot(path);
        }

        private int DateSort(CacheInfo _a, CacheInfo _b)
        {
            return _a.Date.CompareTo(_b.Date);
        }

        private int SizeSort(CacheInfo _a, CacheInfo _b)
        {
            return (_a.Size < _b.Size) ? -1 : 1;
        }

        private void GetDirectoryInfoRoot(string _path)
        {
            totalSize = 0;

            path = _path;
            CacheList.Clear();

            if (Directory.Exists(_path))
            {
                DirectoryInfo di = new DirectoryInfo(_path);
                GetDirectoryInfo(di);
            }

            switch (SortMode)
            {
                case SortType.Date:
                    CacheList.Sort(DateSort);
                    break;
                case SortType.Size:
                    CacheList.Sort(SizeSort);
                    break;
            }
        }

        private void GetDirectoryInfo(DirectoryInfo _directoryInfo)
        {
            foreach (FileInfo fi in _directoryInfo.GetFiles())
            {
                totalSize += fi.Length;
                CacheList.Add(new CacheInfo(fi.FullName, fi.Length, fi.LastAccessTime));
            }

            foreach (DirectoryInfo di in _directoryInfo.GetDirectories())
                GetDirectoryInfo(di);
        }
    }
}
