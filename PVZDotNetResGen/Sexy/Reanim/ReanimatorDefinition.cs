using System.Collections.Generic;

namespace PVZDotNetResGen.Sexy.Reanim
{
    public class ReanimatorDefinition
    {
        public ReanimScaleType mDoScale = ReanimScaleType.ScaleFromPC;
        public List<ReanimatorTrack> mTracks = [];
        public float mFPS = 12.0f;
    }
}
