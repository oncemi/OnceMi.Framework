namespace OnceMi.Framework.Model.Dto
{
    public class DeleteCachesResponse : IResponse
    {
        public DeleteCachesResponse(long count)
        {
            this.Count = count;
        }

        public long Count { get; set; }
    }
}
