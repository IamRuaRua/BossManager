using On.Terraria;
using System.Collections.Generic;
using System.Linq;

namespace BossManager
{
    public class Boss
    {
        //BossNameList， BossIDList，BossIDToNameMap 必须在id顺序和第一个名字之间保持一致，我懒得改了
        public string[] BossNameList = {
                "史莱姆王",
                "克苏鲁之眼",
                "克苏鲁之脑",
                "世界吞噬者",
                "骷髅王",
                "鹿角怪",
                "蜂王",
                "肉山",
                "史莱姆皇后",
                "双子魔眼",
                "机械骷髅王",
                "毁灭者",
                "猪龙鱼公爵",
                "光之女皇",
                "世纪之花",
                "石巨人",
                "拜月教教徒",
                "星尘柱",
                "日耀柱",
                "星云柱",
                "星旋柱",
                "月球领主"
            };
        public int[] BossIDList =
        {
            50 ,
            4  ,
            266,
            13 ,
            35 ,
            668,
            222,
            113,
            657,
            126,
            127,
            134,
            370,
            636,
            262,
            245,
            440,
            493,
            517,
            507,
            422,
            398
        };

        public Dictionary<int, List<string>> BossIDToNameMap = new Dictionary<int, List<string>>();

        public Boss()
        {
            BossIDToNameMap[50] = new List<string> { "史莱姆王", "史王", "slmw", "sw" };
            BossIDToNameMap[4] = new List<string> { "克苏鲁之眼", "kslzy", "ky" };
            BossIDToNameMap[266] = new List<string> { "克苏鲁之脑", "kslzn", "kn" };
            BossIDToNameMap[13] = new List<string> { "世界吞噬者", "世界吞噬怪", "黑长直", "sjtsz", "sjtsg", "rc", "hcz" };
            BossIDToNameMap[35] = new List<string> { "骷髅王", "吴克", "klw" };
            BossIDToNameMap[668] = new List<string> { "鹿角怪", "巨鹿", "ljg", "jl" };
            BossIDToNameMap[222] = new List<string> { "蜂王", "蜂后", "fh", "fw" };
            BossIDToNameMap[113] = new List<string> { "肉山", "血肉墙", "rs", "xrq" };
            BossIDToNameMap[657] = new List<string> { "史莱姆皇后", "史莱姆女皇", "史莱姆女王", "史后", "slmhh", "slmnh", "slmnw", "sh" };
            BossIDToNameMap[126] = new List<string> { "双子魔眼", "双胞胎", "szmy" };
            BossIDToNameMap[127] = new List<string> { "机械骷髅王", "铁吴克", "jxklw" };
            BossIDToNameMap[134] = new List<string> { "毁灭者", "机械虫子", "机械蠕虫", "hmz", "jxcz", "jxrc" };
            BossIDToNameMap[370] = new List<string> { "猪龙鱼公爵", "猪鲨", "zs", "zlygj" };
            BossIDToNameMap[636] = new List<string> { "光之女皇", "光女", "gznh", "gn" };
            BossIDToNameMap[262] = new List<string> { "世纪之花", "世花", "sjzh" };
            BossIDToNameMap[245] = new List<string> { "石巨人", "sjr" };
            BossIDToNameMap[440] = new List<string> { "拜月教教徒", "拜月教邪教徒", "邪教徒", "邪恶教徒", "byjxjt", "byjjt", "xjt", "xejt" };
            BossIDToNameMap[493] = new List<string> { "星尘柱", "xcz" };
            BossIDToNameMap[517] = new List<string> { "日耀柱", "ryz" };
            BossIDToNameMap[507] = new List<string> { "星云柱", "xyz" };
            BossIDToNameMap[422] = new List<string> { "星旋柱", "xxz" };
            BossIDToNameMap[398] = new List<string> { "月球领主", "月亮领主", "月总", "yqlz", "yllz", "yz" };
        }

        /// <summary>
        /// 返回 id 对应的boss名字，第一个名字 key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string getIDToNameFirst(int id,string npcName="")
        {
            if (BossIDToNameMap.ContainsKey(id))
            {
                return BossIDToNameMap[id].First();
            }
            else
            { 
                return npcName;
            }
                
        }

        /// <summary>
        /// 获取完整boss名称通过缩写
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public string getNameByAbbreviation(string a)
        {
            a = a.ToLower();
            foreach (int i in BossIDList)
            {
                if (BossIDToNameMap[i].Contains(a))
                {
                    return BossIDToNameMap[i].First();
                }
            }
            return "";
        }
    }
}
