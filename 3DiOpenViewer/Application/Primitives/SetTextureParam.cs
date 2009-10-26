using System;

namespace OpenViewer.Primitives
{
    public enum EventQueueType
    {
        None,
        TextureDownloaded,
    }

    public struct EventQueueParam
    {
        public EventQueueType Type;
        public object Option;

        public EventQueueParam(EventQueueType _type, object _option)
        {
            Type = _type;
            Option = _option;
        }

        public static EventQueueParam Empty = new EventQueueParam(EventQueueType.None, null);
    }

    public struct SetTextureParam
    {
        public string PrimID;
        public int MaterialIndex;
        public string TextureUUID;
        public string TextureExtension;

        public SetTextureParam(string _primID, int _materialIndex, string _textureUUID, string _textureExtension)
        {
            PrimID = _primID;
            MaterialIndex = _materialIndex;
            TextureUUID = _textureUUID;
            TextureExtension = _textureExtension;
        }
    }
}