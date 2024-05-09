using PVZDotNetResGen.Utils.JsonHelper;
using System.Collections.Generic;

namespace PVZDotNetResGen.Sexy.Atlas
{
    public class SpriteItem
    {
        public required string mId;
        public int mX;
        public int mY;
        public int mWidth;
        public int mHeight;
        public int mRows;
        public int mCols;
        public AnimType mAnim;
        public int mFrameDelay;
        public int mBeginDelay;
        public int mEndDelay;
    }

    public class AtlasInfo : IJsonVersionCheckable
    {
        public static uint JsonVersion => 0;

        public required List<SpriteItem> mSubImages;
    }
}
