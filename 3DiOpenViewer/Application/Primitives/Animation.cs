
namespace OpenViewer.Primitives
{
    public struct Animation
    {
        public string Key;
        public bool Loop;

        public Animation(string _key, bool _loop)
        {
            Key = _key;
            Loop = _loop;
        }

        public static readonly Animation Empty = new Animation(string.Empty, true);
    }
}
