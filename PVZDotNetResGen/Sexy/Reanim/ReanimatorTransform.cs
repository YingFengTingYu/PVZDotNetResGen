namespace PVZDotNetResGen.Sexy.Reanim
{
    public class ReanimatorTransform
    {
        public float mTransX;
        public float mTransY;
        public float mScaleX;
        public float mScaleY;
        public float mSkewX;
        public float mSkewY;
        public float mFrame;
        public float mAlpha;
        public string? mImage;
        public string? mFont;
        public string? mText;

        public static readonly ReanimatorTransform Default = new ReanimatorTransform
        {
            mTransX = 0,
        };
    }
}
