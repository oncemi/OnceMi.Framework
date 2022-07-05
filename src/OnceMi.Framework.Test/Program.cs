using System;

namespace OnceMi.Framework.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //#region AES

            //string beforeEncrypt = "abc";
            //string key = Guid.NewGuid().ToString("N");
            //string iv = Guid.NewGuid().ToString("N");

            //string encryptStr= Encrypt.AESEncrypt(beforeEncrypt, key, iv);

            //Console.WriteLine(encryptStr);

            //Console.WriteLine(Encrypt.AESDecrypt(encryptStr, key, iv));
            //Console.WriteLine(Encrypt.Base64Encode(beforeEncrypt));

            //#endregion

            //#region Json

            //string json = JsonUtil.SerializeToString(new TestModel());
            //Console.WriteLine(json);
            //string formatJson = JsonUtil.SerializeToFormatString(new TestModel());
            //Console.WriteLine(formatJson);

            //#endregion

            //#region 汉字测试

            //string testStr1 = "abcdefg";
            //string testStr2 = "我是大傻逼";
            //string testStr3 = "我是大傻逼,abc";
            //string testStr4 = "我是大傻逼，哈哈";
            //string testStr5 = "我是大傻逼，哈哈，abc";
            //string testStr6 = "，";
            //string testStr7 = " ";

            //char c = (char)189;

            //Console.WriteLine(TextUtil.IsEnglishString(testStr1));

            //#endregion

            //var redis  = new RedisClient("127.0.0.1:6379,password=,ConnectTimeout=3000,defaultdatabase=0");
            //var key = "test";

            //var cache = MemoryCache.Default; ;

            //List<Menus> menus = GetFromJson();

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            //for(int i = 0; i < 1000; i++)
            //{
            //    //byte[] value = JsonUtil.SerializeToByte(menus);
            //    //redis.Set($"{key}-{i + 1}", value);

            //    //byte[] data = redis.Get<byte[]>($"{key}-{i + 1}");
            //    //var obj = JsonUtil.DeserializeByteToObject<List<Menus>>(data);

            //    //string value = JsonUtil.SerializeToString(menus);
            //    //redis.Set($"{key}-{i + 1}", value);

            //    //string data = redis.Get($"{key}-{i + 1}");
            //    //var obj = JsonUtil.DeserializeStringToList<Menus>(data);

            //    byte[] value = JsonUtil.SerializeToByte(menus);
            //    cache.Set($"{key}-{i + 1}", value, null, null);

            //    byte[] data = redis.Get<byte[]>($"{key}-{i + 1}");
            //    var obj = cache.Get($"{key}-{i + 1}") as List<Menus>;
            //}

            //stopwatch.Stop();
            //Console.WriteLine($"耗时：{stopwatch.ElapsedMilliseconds}ms");

            //string a = "a:b:*";
            //string b = "a:*:c";
            //string c = "*:b:c";
            //string d = "a:b:c";

            //string va = "a:b:1";
            //string vb = "a:1:c";
            //string vc = "1:b:c";
            //string vd = "a:b:c";

            //string key = d.Replace("*", ".");

            //if (Regex.IsMatch(va, key))
            //{
            //    Console.WriteLine($"Key:{key}, Value:{va}");
            //}
            //if (Regex.IsMatch(vb, key))
            //{
            //    Console.WriteLine($"Key:{key}, Value:{vb}");
            //}
            //if (Regex.IsMatch(vc, key))
            //{
            //    Console.WriteLine($"Key:{key}, Value:{vc}");
            //}
            //if (Regex.IsMatch(vd, key))
            //{
            //    Console.WriteLine($"Key:{key}, Value:{vd}");
            //}


            Console.ReadKey(false);
        }
    }
}
