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

using OpenMetaverse;

namespace Lib3Di
{
    public class IrrRootAsset : Asset
    {

        public override AssetType AssetType { get { return AssetType.Unknown; } }

        public IrrRootAsset() { }

        public IrrRootAsset(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
        }

        public override void Encode() { }
        public override bool Decode() { return true; }

    }

    public class IrrAnimationAsset : Asset
    {

        public override AssetType AssetType { get { return AssetType.Unknown; } }

        public IrrAnimationAsset() { }

        public IrrAnimationAsset(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
        }

        public override void Encode() { }
        public override bool Decode() { return true; }

    }

    public class IrrMeshAsset : Asset
    {

        public override AssetType AssetType { get { return AssetType.Unknown; } }

        public IrrMeshAsset() { }

        public IrrMeshAsset(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
        }

        public override void Encode() { }
        public override bool Decode() { return true; }

    }

    public class AssetFactory
    {
        public static Asset CreateAsset(AssetDownload download, int asset_type)
        {
            Asset asset;
            switch (asset_type)
            {
                case 70:
                    asset = new IrrRootAsset(download.AssetID, download.AssetData);
                    break;
                case 71:
                    asset = new IrrAnimationAsset(download.AssetID, download.AssetData);
                    break;
                case 72:
                    asset = new IrrMeshAsset(download.AssetID, download.AssetData);
                    break;
                default:
                    return null;
            }
            return asset;
        }
    }

}
