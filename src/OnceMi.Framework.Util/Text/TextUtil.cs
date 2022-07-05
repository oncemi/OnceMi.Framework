using Chinese;
using System;
using System.Text;

namespace OnceMi.Framework.Util.Text
{
    public static class TextUtil
    {
        /// <summary>
        /// 数字转中文
        /// </summary>
        /// <param name="number">要转换的数字</param>
        /// <returns>返回数字对应的中文。</returns>
        public static string NumberToCNString(int number)
        {
            int absNumber = Math.Abs(number);

            string result = string.Empty;

            string cnString = "零一二三四五六七八九";
            string unitString = " 十百千";
            string bigUnitString = " 万亿兆吉";
            string absNumberSrting = absNumber.ToString();
            // 分割数量
            int partitionCount = 4;
            // 开始分割索引
            int sIndex = 0;
            // 总分割次数
            int count = Convert.ToInt32(Math.Ceiling(absNumberSrting.Length * 1.0d / partitionCount));
            for (int i = 0; i < count; i++)
            {
                // 获取要转换的短数字字符串
                int tIndex = (count - i) * partitionCount;
                int subLength = partitionCount;
                if (tIndex > absNumberSrting.Length && i == 0)// 第一次进入，查询最后一页的数据条数
                {
                    subLength = partitionCount - (tIndex - absNumberSrting.Length);
                }
                string tempNumberString = Convert.ToInt32(absNumberSrting.Substring(sIndex, subLength)).ToString();
                sIndex += subLength;

                for (int j = 0; j < tempNumberString.Length; j++)
                {
                    if (i > 0 && i < count - 1)// 不是最后一页
                    {
                        if (j == 0 && tempNumberString.Length < partitionCount)
                        {
                            result += cnString[0];
                        }
                    }
                    else if (i == count - 1 && count > 1)
                    {
                        if (j == 0 && tempNumberString.Length < partitionCount)
                        {
                            if (result.Length > 0 && result[result.Length - 1].Equals(cnString[0]) == false)
                                result += cnString[0];
                        }
                    }
                    if (Convert.ToInt32(tempNumberString) == 0 && (result.Length <= 0 || result[result.Length - 1].Equals(cnString[0]) == false))
                    {
                        result += cnString[0];
                        break;
                    }
                    int unitIdx = -1;
                    // 不到最后一位数，计算小单位索引
                    if (tempNumberString.Length - (j + 1) != 0)// 小单位
                    {
                        int tempIdx = j;
                        if (tempNumberString.Length != partitionCount)
                        {
                            tempIdx += partitionCount - tempNumberString.Length;
                        }
                        unitIdx = (unitString.Length - 1) - tempIdx;
                    }
                    // 数字转中文
                    int cnIdx = Convert.ToInt32(tempNumberString[j].ToString());
                    result += $"{cnString[cnIdx]}{(unitIdx != -1 ? unitString[unitIdx].ToString() : string.Empty)}";
                    // 看看后面的字符串是否全是0，全是0这一页加个单位转换就结束了，不是的话，就加个零，然后把剩余的数字拿来重新骚整一下。
                    string tempString = tempNumberString.Substring(j + 1);
                    if (string.IsNullOrEmpty(tempString) == false)
                    {
                        int temp = Convert.ToInt32(tempString);
                        if (temp > 0)
                        {
                            if (tempString.Equals(temp.ToString()) == false)
                            {
                                result += cnString[0];
                                tempNumberString = temp.ToString();
                                j = -1;
                            }
                        }
                        else
                        {
                            // 加个单位退出循环
                            break;
                        }
                    }
                }
                if (i != count - 1)// 大单位
                {
                    result += bigUnitString[count - (i + 1)];
                }
                // 如果后面全是0，整个转换就完成了
                string surplusString = absNumberSrting.Substring(subLength);
                if (string.IsNullOrEmpty(surplusString) == false)
                {
                    if (surplusString.Length > 7 && Convert.ToInt64(surplusString) == 0)
                    {
                        break;
                    }
                    else if (surplusString.Length <= 7 && Convert.ToInt32(surplusString) == 0)
                    {
                        break;
                    }
                }
            }
            if (number < 0)
            {
                result = $"负{result}";
            }
            return result;
        }

        /// <summary>
        /// 是否为纯英文字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEnglishString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] > 127)
                {
                    return false;
                }
            }
            return true;
        }

        #region 获取汉语拼音首字母

        public static string GetPinyin(string text)
        {
            return Pinyin.GetString(text, PinyinFormat.WithoutTone);
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetPinyinSpellCode(char text)
        {
            return GetPinyinSpellCode(text.ToString());
        }

        /// <summary>
        /// 获取拼音首字母
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetPinyinSpellCode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            string py = GetPinyin(text);
            if (string.IsNullOrEmpty(py))
                return string.Empty;

            char[] pyArg = py.ToCharArray();
            for (int i = 0; i < pyArg.Length; i++)
            {
                if ((pyArg[i] >= 65 && pyArg[i] <= 90)
                    || (pyArg[i] >= 97 && pyArg[i] <= 122)
                    || (pyArg[i] >= 48 && pyArg[i] <= 57)
                    || pyArg[i] == 32)
                {
                    continue;
                }
                pyArg[i] = (char)32;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pyArg.Length; i++)
            {

                if (i == 0 || (pyArg[i] == (char)32 && i < pyArg.Length - 1))
                {
                    sb.Append(i == 0 ? pyArg[i] : pyArg[i + 1]);
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}
