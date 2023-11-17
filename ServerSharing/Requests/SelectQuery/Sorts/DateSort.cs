namespace ServerSharing
{
    internal class DateSort : ISortParameter
    {
        private readonly DateTime _date;
        private readonly string _id;

        public DateSort(DateTime date, string id = null)
        {
            _date = date;
            _id = id;
        }

        public string View() => "idx_date_id";

        public string OrderBy() => "ORDER BY date desc, id desc";

        public string[] Where()
        {
            var where = new List<string>();

            if (_id != null)
                where.Add($"WHERE date = Datetime(\"{_date:s}Z\") and id < \"{_id}\"");

            where.Add($"WHERE date < Datetime(\"{_date:s}Z\")");

            return where.ToArray();
        }
    }
}