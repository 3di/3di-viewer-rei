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
using System.IO.Compression;

namespace Lib3Di
{
    public class DecompressionRequest
    {
        public string sourceFilename;
        public string targetFilename;
        public DecompressionRequest(string sourceFilename, string targetFilename)
        {
            this.sourceFilename = sourceFilename;
            this.targetFilename = targetFilename;
        }
    }

    public static class Compress
    {
#if ORIGIN // by liu.
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                string zipName = args[0].Trim();
                string fileName = args[1].Trim();
                Decompress(zipName, fileName);
            }
            else
            {
                Console.WriteLine("usage: unzip <zipPath> <filePath>");
            }
        }
#endif
        private static Queue<DecompressionRequest> DecompressionRequests = new Queue<DecompressionRequest>();

        public static void AddDecompressionRequest(DecompressionRequest req)
        {
            lock(DecompressionRequests)
            {
                DecompressionRequests.Enqueue(req);
            }
        }

        public static void DecompressWaitingRequests()
        {
            lock(DecompressionRequests)
            {
                while (DecompressionRequests.Count > 0)
                {
                    DecompressionRequest req = DecompressionRequests.Dequeue();
                    Decompress(req);
                }
            }
        }

        private static int Decompress(DecompressionRequest req)
        {
            string zipName = req.sourceFilename;
            string fileName = req.targetFilename;
            string dstFile = "";
            FileStream fsIn = null;
            FileStream fsOut = null;
            GZipStream gzip = null;
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            int totalCount = 0;
            try
            {
                dstFile = fileName;
                fsIn = new FileStream(zipName, FileMode.Open, FileAccess.Read, FileShare.Read);
                fsOut = new FileStream(dstFile, FileMode.Create, FileAccess.Write, FileShare.None);
                gzip = new GZipStream(fsIn, CompressionMode.Decompress, true);
                while (true)
                {
                    count = gzip.Read(buffer, 0, bufferSize);
                    if (count != 0)
                    {
                        fsOut.Write(buffer, 0, count);
                        totalCount += count;
                    }
                    if (count != bufferSize)
                    {
                        // have reached the end
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // handle or display the error
                System.Diagnostics.Debug.Assert(false, ex.ToString());
            }

            finally
            {
                if (gzip != null)
                {
                    gzip.Close();
                    gzip = null;
                }
                if (fsOut != null)
                {
                    fsOut.Close();
                    fsOut = null;
                }
                if (fsIn != null)
                {
                    fsIn.Close();
                    fsIn = null;
                }
            }

            return totalCount;
        }
    }
}
