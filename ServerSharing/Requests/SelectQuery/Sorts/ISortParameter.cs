namespace ServerSharing
{
    internal interface ISortParameter
    {
        string View();
        string OrderBy();
        string[] Where();
    }
}