namespace ServerSharing
{
    internal class RatingAverageSort : ISortParameter
    {
        private readonly float _ratingAverage;
        private readonly uint _ratingCount;
        private readonly DateTime _date;
        private readonly string _id;

        public RatingAverageSort(float ratingAverage, uint ratingCount, DateTime date, string id = null)
        {
            _ratingAverage = ratingAverage;
            _ratingCount = ratingCount;
            _date = date;
            _id = id;
        }

        public string View() => "idx_rating_avg_rating_count_date_id";

        public string OrderBy() => "ORDER BY rating_avg desc, rating_count desc, date desc, id desc";

        public string[] Where()
        {
            var where = new List<string>();

            if (_id != null)
                where.Add($"WHERE rating_avg = {_ratingAverage} and rating_count = {_ratingCount} and date = Datetime(\"{_date:s}Z\") and id < \"{_id}\"");

            where.Add($"WHERE rating_avg = {_ratingAverage} and rating_count = {_ratingCount} and date < Datetime(\"{_date:s}Z\")");
            where.Add($"WHERE rating_avg = {_ratingAverage} and rating_count < {_ratingCount}");
            where.Add($"WHERE rating_avg < {_ratingAverage}");

            return where.ToArray();
        }
    }
}