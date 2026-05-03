namespace TopSpeed.Protocol
{
    public enum TrackPackageUploadStatus : byte
    {
        None = 0,
        Accepted = 1,
        Reused = 2,
        Rejected = 3
    }
}
