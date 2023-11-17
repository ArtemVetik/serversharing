namespace ServerSharing
{
    internal class LikeCountSort : ISortParameter
    {
        private readonly uint _likeCount;
        private readonly DateTime _date;
        private readonly string _id;

        public LikeCountSort(uint likeCount, DateTime date, string id = null)
        {
            _likeCount = likeCount;
            _date = date;
            _id = id;
        }

        public string View() => "idx_like_count_date_id";

        public string OrderBy() => "ORDER BY like_count desc, date desc, id desc";

        public string[] Where()
        {
            var where = new List<string>();

            if (_id != null)
                where.Add($"WHERE like_count = {_likeCount} and date = Datetime(\"{_date:s}Z\") and id < \"{_id}\"");

            where.Add($"WHERE like_count = {_likeCount} and date < Datetime(\"{_date:s}Z\")");
            where.Add($"WHERE like_count < {_likeCount}");

            return where.ToArray();
        }
    }
}