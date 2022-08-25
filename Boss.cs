using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossManager
{
	public class Boss
	{
		public string[] BossNameList ={
				"史莱姆王",
				"克苏鲁之眼",
				"克苏鲁之脑",
				"世界吞噬者",
				"骷髅王",
				"鹿角怪",
				"蜂后",
				"肉山",
				"史莱姆女王",
				"双子魔眼",
				"机械骷髅王",
				"机械虫子",
				"猪鲨",
				"光之女皇",
				"世纪之花",
				"石巨人",
				"邪恶教徒",
				"星尘柱",
				"日曜柱",
				"星云柱",
				"旋涡柱",
				"月球领主",
			};
		public Dictionary<string, int> BossNameToIDMap = new Dictionary<string, int>();
		public Boss()
		{
			BossNameToIDMap["史莱姆王"] = 50;
			BossNameToIDMap["克苏鲁之眼"] = 4;
			BossNameToIDMap["克苏鲁之脑"] = 266;
			BossNameToIDMap["世界吞噬者"] = 13;
			BossNameToIDMap["骷髅王"] = 35;
			BossNameToIDMap["鹿角怪"] = 668;
			BossNameToIDMap["蜂后"] = 222;
			BossNameToIDMap["肉山"] = 113;
			BossNameToIDMap["史莱姆女王"] = 657;
			BossNameToIDMap["双子魔眼"] = 126;
			BossNameToIDMap["机械骷髅王"] = 127;
			BossNameToIDMap["机械虫子"] = 134;
			BossNameToIDMap["猪鲨"] = 370;
			BossNameToIDMap["光之女皇"] = 636;
			BossNameToIDMap["世纪之花"] = 262;
			BossNameToIDMap["石巨人"] = 245;
			BossNameToIDMap["邪恶教徒"] = 440;
			BossNameToIDMap["星尘柱"] = 493;
			BossNameToIDMap["日曜柱"] = 517;
			BossNameToIDMap["星云柱"] = 507;
			BossNameToIDMap["旋涡柱"] = 422;
			BossNameToIDMap["月球领主"] = 398;
		}
	}
}
