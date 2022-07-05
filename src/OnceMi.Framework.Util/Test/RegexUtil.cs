using System.Text.RegularExpressions;

namespace OnceMi.Framework.Util.Test
{
    public static class RegexUtil
    {
        public static bool IsMobilePhone(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            if (input.Length != 11)
            {
                return false;
            }
            //2019最新电话号码正则
            string ruleStr = @"^0{0,1}(13[4-9]|15[7-9]|15[0-2]|17[0-9]|18[7-8]|19[7-9])[0-9]{8}$";
            Regex regex = new Regex(ruleStr);

            if (regex.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
