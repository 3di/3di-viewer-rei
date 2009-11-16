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
using System.Text;
using IrrlichtNETCP;
using System.Drawing;

namespace OpenViewer.Managers
{
    public class TerrainManager : BaseManager
    {
        private List<ulong> terrainReady = new List<ulong>();
        private Dictionary<ulong, float[,]> landmaps = new Dictionary<ulong, float[,]>();
        private Dictionary<ulong, Bitmap> terrainBitmaps = new Dictionary<ulong, Bitmap>();
        private Dictionary<ulong, TerrainSceneNode> terrains = new Dictionary<ulong, TerrainSceneNode>();

        public TerrainManager(Viewer viewer)
            : base(viewer, -1)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Cleanup()
        {
            lock (landmaps)
            {
                landmaps.Clear();
            }
            lock (terrainReady)
            {
                terrainReady.Clear();
            }
            lock (terrainBitmaps)
            {
                terrainBitmaps.Clear();
            }
            lock (terrains)
            {
                foreach (TerrainSceneNode tsn in terrains.Values)
                {
                    // TerrainSceneNode will automatically free up its TrianglePickers when dropped.
                    // This will happen on the native side, we have to remove the TPs manually.
                    if (tsn.TriangleSelector != null)
                    {
                        lock (NativeElement.Elements) { if (NativeElement.Elements.ContainsKey(tsn.TriangleSelector.Raw)) { NativeElement.Elements.Remove(tsn.TriangleSelector.Raw); } }
                    }
                    Reference.SceneManager.AddToDeletionQueue(tsn);
                }
                terrains.Clear();
            }

            base.Cleanup();
        }

        public override void Update(uint frame)
        {
            if (Reference.Viewer.IsDrawTerrain)
            {
                UpdateTerrain();
            }
            else
            {
                UpdateDummyTerrain();
            }

            base.Update(frame);
        }

        private void UpdateTerrain()
        {
            // Terrain updates
            lock (terrainReady)
            {
                if (terrainReady.Count > 0)
                {
                    foreach (ulong sim in terrainReady)
                    {
                        lock (terrains)
                        {
                            if (terrains.ContainsKey(sim))
                            {
                                TerrainSceneNode t = terrains[sim];
                                if (t.TriangleSelector != null)
                                {
                                    lock (NativeElement.Elements) { if (NativeElement.Elements.ContainsKey(t.TriangleSelector.Raw)) { NativeElement.Elements.Remove(t.TriangleSelector.Raw); } }
                                }
                                Reference.SceneManager.AddToDeletionQueue(t);
                                terrains.Remove(sim);
                            }
                        }

                        float[,] h = ResizeTerrain512Interpolation(landmaps[sim]);

                        TerrainSceneNode tsn = Reference.SceneManager.AddTerrainSceneNodeFromRawData(h,
                                                                                          514,
                                                                                          ParentNode,
                                                                                          -1,
                                                                                          new Vector3D(-1f, 0, -1f),
                                                                                          new Vector3D(),
                                                                                          new Vector3D(258f/513f, 1.0f, 258f/513f),
                                                                                          IrrlichtNETCP.Color.TransparentWhite,
                                                                                          4,
                                                                                          TerrainPatchSize.TPS9,
                                                                                          4);

                        if (tsn != null)
                        {
                            Texture tx = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/textures/sand01.jpg");
                            if (tx != null)
                                tsn.SetMaterialTexture(0, tx);
                            tsn.SetMaterialType(MaterialType.DetailMap);
                            tsn.SetMaterialFlag(MaterialFlag.NormalizeNormals, true);
                            tsn.ScaleTexture(16, 16);

                            tsn.TriangleSelector = Reference.SceneManager.CreateTerrainTriangleSelector(tsn, 1);
                            tsn.TriangleSelector.Drop();
                            for (int i = 0; i < tsn.MaterialCount; i++)
                            {
                                tsn.GetMaterial(i).DiffuseColor = IrrlichtNETCP.Color.White;
                                tsn.GetMaterial(i).AmbientColor = IrrlichtNETCP.Color.White;
                                tsn.GetMaterial(i).SpecularColor = IrrlichtNETCP.Color.Black;
                                tsn.GetMaterial(i).EmissiveColor = IrrlichtNETCP.Color.Black;
                                tsn.GetMaterial(i).Shininess = 0;
                            }

#if PREVENT_CAMERA_GOING_THROUGH_TERRAIN
                            CameraSceneNode camera = Reference.Viewer.Camera.SNCamera;
                            Animator anim = Reference.SceneManager.CreateCollisionResponseAnimator(
                                tsn.TriangleSelector,
                                camera,
                                new Vector3D(0.5f, 1.0f, 0.5f),
                                new Vector3D(0, 0, 0),
                                new Vector3D(0, 1.0f, 0), 0.01f);
                            camera.AddAnimator(anim);
#endif

                            terrains.Add(sim, tsn);
                        }
                    }
                    terrainReady.Clear();
                }
            }
        }

        private void UpdateDummyTerrain()
        {
        }

        public void SetPatch(ulong sim, int x, int y, float value)
        {
            lock (landmaps)
            {
                if (!landmaps.ContainsKey(sim))
                {
                    landmaps.Add(sim, new float[256, 256]);
                }
                landmaps[sim][x, y] = value;
            }
        }

        public void GenerateTerrain(ulong sim)
        {
            /*
            float[,] currentTerrain;

            lock (landmaps)
            {
                if (!landmaps.ContainsKey(sim))
                {
                    // Can't generate a non-existent terrain
                    return;
                }
                currentTerrain = landmaps[sim];
            }
            lock (terrainBitmaps)
            {
                if (terrainBitmaps.ContainsKey(sim))
                {
                    terrainBitmaps.Remove(sim);
                }

                float[,] h = ResizeTerrain512Interpolation(currentTerrain);
                Bitmap bm = new Bitmap(514, 514, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                for (int x = 0; x < 514; x++)
                {
                    for (int y = 0; y < 514; y++)
                    {
                        setPixcel(bm, x, y, h[Clip(y - 1, 0, 511), Clip(x - 1, 0, 511)]);
                    }
                }
                terrainBitmaps.Add(sim, bm);

                bm.Save(Util.TerrainFolder + sim + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            */
            lock (terrainReady) 
            { 
                if (!terrainReady.Contains(sim))
                    terrainReady.Add(sim); 
            }
        }

        public Dictionary<ulong, TerrainSceneNode> Terrains
        {
            get { return terrains; }
        }

        private void setPixcel(Bitmap bitmap, int x, int y, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            System.Drawing.Color c = System.Drawing.Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
            bitmap.SetPixel((bitmap.Width - 1) - x, y, c);
        }

        private static int Clip(int x, int min, int max)
        {
            return Math.Min(Math.Max(x, min), max);
        }

        private const uint m_regionWidth = 256;
        private const uint m_regionHeight = 256;

        private float[,] ResizeTerrain512Interpolation(float[,] heightMap)
        {
            float[,] resultarr = new float[m_regionWidth, m_regionHeight];

            // Filling out the array into it's multi-dimentional components
            for (int y = 0; y < m_regionHeight; y++)
            {
                for (int x = 0; x < m_regionWidth; x++)
                {
                    resultarr[y, x] = heightMap[x, y];
                }
            }

            // Resize using interpolation

            // This particular way is quick but it only works on a multiple of the original

            // The idea behind this method can be described with the following diagrams
            // second pass and third pass happen in the same loop really..  just separated
            // them to show what this does.

            // First Pass
            // ResultArr:
            // 1,1,1,1,1,1
            // 1,1,1,1,1,1
            // 1,1,1,1,1,1
            // 1,1,1,1,1,1
            // 1,1,1,1,1,1
            // 1,1,1,1,1,1

            // Second Pass
            // ResultArr2:
            // 1,,1,,1,,1,,1,,1,
            // ,,,,,,,,,,
            // 1,,1,,1,,1,,1,,1,
            // ,,,,,,,,,,
            // 1,,1,,1,,1,,1,,1,
            // ,,,,,,,,,,
            // 1,,1,,1,,1,,1,,1,
            // ,,,,,,,,,,
            // 1,,1,,1,,1,,1,,1,
            // ,,,,,,,,,,
            // 1,,1,,1,,1,,1,,1,

            // Third pass fills in the blanks
            // ResultArr2:
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1
            // 1,1,1,1,1,1,1,1,1,1,1,1

            // X,Y = .
            // X+1,y = ^
            // X,Y+1 = *
            // X+1,Y+1 = #

            // Filling in like this;
            // .*
            // ^#
            // 1st .
            // 2nd *
            // 3rd ^
            // 4th #
            // on single loop.

            float[,] resultarr2 = new float[512, 512];
            for (int y = 0; y < m_regionHeight; y++)
            {
                for (int x = 0; x < m_regionWidth; x++)
                {
                    resultarr2[y * 2, x * 2] = resultarr[y, x];

                    if (y < m_regionHeight)
                    {
                        if (y + 1 < m_regionHeight)
                        {
                            if (x + 1 < m_regionWidth)
                            {
                                resultarr2[(y * 2) + 1, x * 2] = ((resultarr[y, x] + resultarr[y + 1, x] +
                                                               resultarr[y, x + 1] + resultarr[y + 1, x + 1]) / 4);
                            }
                            else
                            {
                                resultarr2[(y * 2) + 1, x * 2] = ((resultarr[y, x] + resultarr[y + 1, x]) / 2);
                            }
                        }
                        else
                        {
                            resultarr2[(y * 2) + 1, x * 2] = resultarr[y, x];
                        }
                    }
                    if (x < m_regionWidth)
                    {
                        if (x + 1 < m_regionWidth)
                        {
                            if (y + 1 < m_regionHeight)
                            {
                                resultarr2[y * 2, (x * 2) + 1] = ((resultarr[y, x] + resultarr[y + 1, x] +
                                                               resultarr[y, x + 1] + resultarr[y + 1, x + 1]) / 4);
                            }
                            else
                            {
                                resultarr2[y * 2, (x * 2) + 1] = ((resultarr[y, x] + resultarr[y, x + 1]) / 2);
                            }
                        }
                        else
                        {
                            resultarr2[y * 2, (x * 2) + 1] = resultarr[y, x];
                        }
                    }
                    if (x < m_regionWidth && y < m_regionHeight)
                    {
                        if ((x + 1 < m_regionWidth) && (y + 1 < m_regionHeight))
                        {
                            resultarr2[(y * 2) + 1, (x * 2) + 1] = ((resultarr[y, x] + resultarr[y + 1, x] +
                                                                 resultarr[y, x + 1] + resultarr[y + 1, x + 1]) / 4);
                        }
                        else
                        {
                            resultarr2[(y * 2) + 1, (x * 2) + 1] = resultarr[y, x];
                        }
                    }
                }
            }

            return resultarr2;
        }
    }
}
