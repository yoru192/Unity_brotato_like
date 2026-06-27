namespace BerserkPixel.Tilemap_Generator.OcclussionCulling
{
    public interface IOcclusionCulling
    {
        bool OnShown();

        bool OnDisappear();
    }
}