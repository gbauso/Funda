namespace Funda.Application
{
    public record FetchResponse
    {
        public int FetchingProgress { get; }

        public FetchStatus Status { get; }

        public FetchResponse(int progress, FetchStatus status) => (FetchingProgress, Status) = (progress, status);
    }

    public enum FetchStatus
    {
        APIRetrieve,
        Skipped,
        HitCache,
    }
}
