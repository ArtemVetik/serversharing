namespace ServerSharing
{
    internal class DownloadCountSort : ISortParameter
    {
        private readonly uint _downloadCount;
        private readonly DateTime _date;
        private readonly string _id;

        public DownloadCountSort(uint downloadCount, DateTime date, string id = null)
        {
            _downloadCount = downloadCount;
            _date = date;
            _id = id;
        }

        public string View() => "idx_download_count_date_id";

        public string OrderBy() => "ORDER BY download_count desc, date desc, id desc";

        public string[] Where()
        {
            var where = new List<string>();

            if (_id != null)
                where.Add($"WHERE download_count = {_downloadCount} and date = Datetime(\"{_date:s}Z\") and id < \"{_id}\"");

            where.Add($"WHERE download_count = {_downloadCount} and date < Datetime(\"{_date:s}Z\")");
            where.Add($"WHERE download_count < {_downloadCount}");

            return where.ToArray();
        }
    }
}