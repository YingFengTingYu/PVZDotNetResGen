namespace PVZDotNetResGen.Sexy.Reanim
{
    public class ReanimatorDefinition
    {
        public ReanimatorDefinition()
        {
            mFPS = 12f;
            mTrackCount = 0;
            mTracks = null;
        }

        public ReanimScaleType mDoScale;
        public ReanimatorTrack[]? mTracks;
        public float mFPS;
    }
}
