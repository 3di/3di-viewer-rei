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
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace OpenViewer
{
    
    class SculptMeshLOD : IDisposable
    {
        Bitmap bLOD = null;
        Bitmap bBitmap = null;
        private int lod = 32;

        public int Scale
        {
            get
            {
                return lod;
            }
        }

        public Bitmap ResultBitmap
        {
            get { return bLOD; }
        }

        public int LOD
        {
            get
            {
                return (int)Math.Log(Scale, 2);
            }
            set
            {
                int power = value;
                if (power == 0)
                    power = 6;
                if (power < 2)
                    power = 2;
                if (power > 9)
                    power = 9;
                int t = (int)Math.Pow(2, power);
                if (t != Scale)
                {
                    lod = t;
                }
            }
        }

        public SculptMeshLOD(Bitmap oBitmap, float lod)
        {
            if (lod == 2f || lod == 4f || lod == 8f || lod == 16f || lod == 32f || lod == 64f)
                this.lod = (int)lod;

            bBitmap = new Bitmap(oBitmap);
            DoLOD();
        }
        private void DoLOD()
        {
            int x_max = Math.Min(Scale, bBitmap.Width);
            int y_max = Math.Min(Scale, bBitmap.Height);
            if (bBitmap.Width == x_max && bBitmap.Height == y_max)
                bLOD = bBitmap;

            else if (bLOD == null || x_max != bLOD.Width || y_max != bLOD.Height)//don't resize if you don't need to.
            {
                System.Drawing.Bitmap tile = new System.Drawing.Bitmap(bBitmap.Width * 2, bBitmap.Height, PixelFormat.Format24bppRgb);
                System.Drawing.Bitmap tile_LOD = new System.Drawing.Bitmap(x_max * 2, y_max, PixelFormat.Format24bppRgb);

                bLOD = new System.Drawing.Bitmap(x_max, y_max, PixelFormat.Format24bppRgb);
                bLOD.SetResolution(bBitmap.HorizontalResolution, bBitmap.VerticalResolution);

                System.Drawing.Graphics grPhoto = System.Drawing.Graphics.FromImage(tile);
                grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                grPhoto.DrawImage(bBitmap,
                    new System.Drawing.Rectangle(0, 0, bBitmap.Width / 2, bBitmap.Height),
                    new System.Drawing.Rectangle(bBitmap.Width / 2, 0, bBitmap.Width / 2, bBitmap.Height),
                    System.Drawing.GraphicsUnit.Pixel);

                grPhoto.DrawImage(bBitmap,
                    new System.Drawing.Rectangle((3 * bBitmap.Width) / 2, 0, bBitmap.Width / 2, bBitmap.Height),
                    new System.Drawing.Rectangle(0, 0, bBitmap.Width / 2, bBitmap.Height),
                    System.Drawing.GraphicsUnit.Pixel);

                grPhoto.DrawImage(bBitmap,
                    new System.Drawing.Rectangle(bBitmap.Width / 2, 0, bBitmap.Width, bBitmap.Height),
                    new System.Drawing.Rectangle(0, 0, bBitmap.Width, bBitmap.Height),
                    System.Drawing.GraphicsUnit.Pixel);

                grPhoto = System.Drawing.Graphics.FromImage(tile_LOD);
                grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;

                grPhoto.DrawImage(tile,
                    new System.Drawing.Rectangle(0, 0, tile_LOD.Width, tile_LOD.Height),
                    new System.Drawing.Rectangle(0, 0, tile.Width, tile.Height),
                    System.Drawing.GraphicsUnit.Pixel);

                grPhoto = System.Drawing.Graphics.FromImage(bLOD);
                grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                grPhoto.DrawImage(tile_LOD,
                    new System.Drawing.Rectangle(0, 0, bLOD.Width, bLOD.Height),
                    new System.Drawing.Rectangle(tile_LOD.Width / 4, 0, tile_LOD.Width / 2, tile_LOD.Height),
                    System.Drawing.GraphicsUnit.Pixel);

                grPhoto.Dispose();
                tile_LOD.Dispose();
                tile.Dispose();
            }

        }
        
    
#region IDisposable Members

        public void  Dispose()
        {
            bBitmap.Dispose();
            bLOD.Dispose();
        }

#endregion
    }
}
