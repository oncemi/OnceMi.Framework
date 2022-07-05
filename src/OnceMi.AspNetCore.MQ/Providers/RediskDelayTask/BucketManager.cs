using FreeRedis;
using OnceMi.AspNetCore.MQ.Models;
using System;

namespace OnceMi.AspNetCore.MQ.Providers.RediskDelayTask
{
    class BucketManager
    {
        private const string REDIS_QUEUE_KEY_PREFIX = "ONCEMI_REDIS_DELAY_QUEUE";
        private const string REDIS_SLIST_KEY = REDIS_QUEUE_KEY_PREFIX + ":MAIN";
        private const long TICK_START = 621355968000000000;
        private readonly RedisClient _client;

        public BucketManager(RedisClient client)
        {
            _client = client;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public JobMember Add(string channel, string data, TimeSpan delay)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            JobMember result = new JobMember()
            {
                Id = Guid.NewGuid().ToString("N"),
                Source = GetSource(delay),
                Data = data,
                Channel = channel,
            };

            using (var tran = _client.Multi())
            {
                tran.ZAdd(REDIS_SLIST_KEY, result.Source, result.Id);
                tran.Set($"{REDIS_QUEUE_KEY_PREFIX}:{result.Id}", JsonUtil.SerializeToByte(result));
                object[] ret = tran.Exec();
            }
            return result;
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="id"></param>
        public void Delete(string id)
        {
            using (var tran = _client.Multi())
            {
                _client.ZRem(REDIS_SLIST_KEY, id);
                _client.Del($"{REDIS_QUEUE_KEY_PREFIX}:{id}");
                object[] ret = tran.Exec();
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JobMember Get(string id)
        {
            decimal? val = _client.ZScore(REDIS_SLIST_KEY, id);
            if (val == null)
            {
                return null;
            }
            string dataStr = _client.Get($"{REDIS_QUEUE_KEY_PREFIX}:{id}");
            if (string.IsNullOrEmpty(dataStr))
            {
                return null;
            }
            return new JobMember()
            {
                Source = (long)val,
                Data = dataStr
            };
        }

        /// <summary>
        /// 获取最小的那个
        /// </summary>
        /// <returns></returns>
        public JobMember Min()
        {
            ZMember[] members = _client.ZRangeByScoreWithScores(REDIS_SLIST_KEY, 0, MaxSource(), 0, 1);
            if (members == null || members.Length == 0)
            {
                return null;
            }
            string id = members[0].member;
            string dataStr = _client.Get($"{REDIS_QUEUE_KEY_PREFIX}:{id}");
            if (string.IsNullOrEmpty(dataStr))
            {
                return null;
            }
            return JsonUtil.DeserializeStringToObject<JobMember>(dataStr);
        }

        /// <summary>
        /// 获取某个时间范围内的值
        /// </summary>
        /// <param name="span">时间</param>
        /// <param name="count">限制获取多少个</param>
        /// <returns></returns>
        public JobMember[] Range(DateTime time, uint? count = null)
        {
            if (time <= DateTime.Now.AddSeconds(-3))
            {
                throw new ArgumentException("time can not less than now time");
            }
            if (count == null)
            {
                count = 0;
            }
            long maxTime = time.ToUniversalTime().Ticks - TICK_START;
            if (maxTime > MaxSource())
            {
                throw new ArgumentException("time range too large");
            }
            ZMember[] members = _client.ZRangeByScoreWithScores(REDIS_SLIST_KEY, 0, maxTime, 0, (int)count.Value);
            if (members == null || members.Length == 0)
            {
                return new JobMember[0];
            }
            JobMember[] jobs = new JobMember[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                jobs[i] = new JobMember()
                {
                    Source = (long)members[i].score,
                    Data = _client.Get(members[i].member)
                };
            }
            return jobs;
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return _client.ZCount(REDIS_SLIST_KEY, 0, MaxSource());
        }

        #region private

        private long GetSource(TimeSpan delay)
        {
            return DateTime.Now.AddMilliseconds(delay.TotalMilliseconds).ToUniversalTime().Ticks - TICK_START;
        }

        private long MaxSource()
        {
            return DateTime.MaxValue.ToUniversalTime().Ticks - TICK_START;
        }

        #endregion

    }
}
