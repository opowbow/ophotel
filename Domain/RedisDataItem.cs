using System;

namespace hotels.Domain
{
    public class RedisDataItem
    {
        public int Database { get; set; }
        public string Key { get; set; }
        public string Value {get; set; }

    }
}
