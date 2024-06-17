namespace PVZDotNetResGen.Utils.XnbContent;

public class XnbContent(object primaryResource, int sharedResourceCount)
{
    public readonly object PrimaryResource = primaryResource;
    public readonly object?[] SharedResources = new object?[sharedResourceCount];
}