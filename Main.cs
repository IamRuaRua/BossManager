using System;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Rests;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace BossManager
{
	[ApiVersion(2, 1)]
	public class ProgressManager : TerrariaPlugin
	{
		List<String> KilledBoss = new List<String>();
		List<String> NoKilledBoss = new List<String>();
        readonly Boss boss = new Boss();
		String KeyBoss;
		public Config PMConfig;
		public override string Author => "Rua";
		public override string Description => "Boss控制插件";
		public override string Name => "BossManager";
		public override Version Version => new Version(1, 0, 0, 1);
		public ProgressManager(Main game) : base(game) { }
		
		public override void Initialize()
		{ 
			Commands.ChatCommands.Add(new Command(permissions: "BossManager", cmd: this.BossManagerCmd, "BM","bm"));
			TShock.RestApi.Register(new SecureRestCommand("/bossstatus", CheckBoss, "rest.boss"));
			ServerApi.Hooks.NpcSpawn.Register(this, Onspawnboss, 10);//召唤BOSS 
			Config.EnsureFile();
			PMConfig = Config.ReadConfig();
		}
		void BossManagerCmd(CommandArgs args)
		{
			var cmdArgs = args.Parameters;
			var sendPlayer = args.Player;
			try
			{
				switch (cmdArgs[0])
				{
					case "help":
						sendPlayer.SendMessage("/BM lock bossName          -- 锁定某个boss"
											+ "\n/BM unlock bossName                 -- 解锁某个boss"
											+"\n/BM status                --获取boss 锁定状态"
											+ "\n/BM listboss             -- 列出所有可锁定boss名称", new Microsoft.Xna.Framework.Color(255, 255, 0));
						return;
					case "status":
						string LockedBoss = "";
						string UnLockedBoss = "";
						string BossName;
						for (int i = 0; i < boss.BossNameList.Length; i++)
						{
							 BossName = boss.BossNameList[i];
							if (CheckABossIsLock(BossName))
							{
								LockedBoss += BossName + ",";

							}
							else
							{
								UnLockedBoss += BossName + ",";
							}
						}
						sendPlayer.SendMessage("已锁定Boss:\n" + LockedBoss.TrimEnd(',') + "\n未锁定Boss:\n" + UnLockedBoss.TrimEnd(','), new Microsoft.Xna.Framework.Color(255, 255, 0));
						return;
					case "lock":
                        if (SetBossLock(cmdArgs[1], true))
                        {
							sendPlayer.SendMessage("Boss锁定成功!", new Microsoft.Xna.Framework.Color(0, 255, 0));
                        }
                        else
                        {
							sendPlayer.SendMessage("Boss锁定失败!Boss不存在或名称有误,发送/BM listBoss 显示所有可锁定的boss", new Microsoft.Xna.Framework.Color(255, 0, 0));
						}
						break;
					case "unlock":
					case "unLock":
						if (SetBossLock(cmdArgs[1], false))
						{
							sendPlayer.SendMessage("Boss解锁成功!", new Microsoft.Xna.Framework.Color(0, 255, 0));
						}
						else
						{
							sendPlayer.SendMessage("Boss解锁失败!Boss不存在或名称有误,发送/BM listBoss 显示所有可锁定的boss", new Microsoft.Xna.Framework.Color(255, 0, 0));
						}
						break;
					case "listBoss":
					case "listboss":
						string message = "";
                        for (int i = 0; i < boss.BossNameList.Length; i++)
                        {
                            BossName = boss.BossNameList[i];
                            if (i % 2 == 0)
                            {
								message += BossName+"    ";

                            }
                            else
                            {
								message += BossName + "\n";
							}
                        }
						sendPlayer.SendMessage(message, new Microsoft.Xna.Framework.Color(255, 255, 0));
						break;
					default:
						sendPlayer.SendErrorMessage("语法错误,使用/BM help获取帮助");
						return;
				}

			}
			catch (Exception)
			{
				sendPlayer.SendErrorMessage("语法错误,使用/BM help获取帮助");
				return;
			}

		}
		public bool SetBossLock(string BossName,bool status)
        {
            if (PMConfig.Boss锁定状态.ContainsKey(BossName))
            { 
				PMConfig.Boss锁定状态[BossName] = status;
				Config.WriteConfig(PMConfig);
				return true;
            }
            else
            {
				return false;
            } 
        }
		public bool IsLock(int BossID)
        {
			PMConfig = Config.ReadConfig();
			switch (BossID)
			{
				case 4:
					return PMConfig.Boss锁定状态["克苏鲁之眼"];
				case 50:
					return PMConfig.Boss锁定状态["史莱姆王"];
				case 668:
					return PMConfig.Boss锁定状态["鹿角怪"];
				case 13:
					return PMConfig.Boss锁定状态["世界吞噬者"];
				case 266:
					return PMConfig.Boss锁定状态["克苏鲁之脑"];
				case 35:
					return PMConfig.Boss锁定状态["骷髅王"];
				case 134:
					return PMConfig.Boss锁定状态["机械虫子"];
				case 657:
					return PMConfig.Boss锁定状态["史莱姆女王"];
				case 398:
					return PMConfig.Boss锁定状态["月球领主"];
				case 262:
					return PMConfig.Boss锁定状态["世纪之花"];
				case 222:
					return PMConfig.Boss锁定状态["蜂后"];
				case 127:
					return PMConfig.Boss锁定状态["机械骷髅王"];
				case 125:
				case 126:
					return PMConfig.Boss锁定状态["双子魔眼"];
				case 370:
					return PMConfig.Boss锁定状态["猪鲨"];
				case 636:
					return PMConfig.Boss锁定状态["光之女皇"];
				case 245:
					return PMConfig.Boss锁定状态["石巨人"];
				case 507:
					return PMConfig.Boss锁定状态["星云柱"];
				case 113:
					return PMConfig.Boss锁定状态["肉山"];
				case 517:
					return PMConfig.Boss锁定状态["日曜柱"];
				case 493:
					return PMConfig.Boss锁定状态["星尘柱"];
				case 422:
					return PMConfig.Boss锁定状态["旋涡柱"];
				case 440:
					return PMConfig.Boss锁定状态["邪恶教徒"];
				default:
					return false;
			}
		}

		private void Onspawnboss(NpcSpawnEventArgs args)//控制进度
		{
			var npc = Main.npc[args.NpcId];
			if (npc == null)
				return;
			if (npc.active && IsLock(npc.netID))
				{
					Console.WriteLine(npc.FullName+"已锁定,无法召唤");
					npc.netID = 0;
					npc.active = false;
					TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", args.NpcId);
					TSPlayer.All.SendInfoMessage("该BOSS已被锁定，请联系服主解锁");
				}
			
		} 
		public void CheckABoss(bool Killed,string BossName,bool IsKeyBoss=false)
        {
            if (Killed)
            {
				KilledBoss.Add(BossName);
				
            }else {
			    NoKilledBoss.Add(BossName);
				if(IsKeyBoss&&KeyBoss==null)
					KeyBoss = BossName;
			}
        }
		public bool CheckABossIsLock(string BossName)
		{
			PMConfig = Config.ReadConfig();
			if (PMConfig.Boss锁定状态[BossName])
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private object CheckBoss(RestRequestArgs args)//获取进度
		{
			KilledBoss = new List<String>();
			NoKilledBoss = new List<String>();
			CheckABoss(NPC.downedSlimeKing, "史莱姆王", true);
			CheckABoss(NPC.downedBoss1, "克苏鲁之眼", true);
			string Boss2Name = (Main.drunkWorld)? "虫子或脑子":(WorldGen.crimson) ? "邪神大脑" : "世界吞噬者";
			CheckABoss(NPC.downedBoss2,Boss2Name,true);
			CheckABoss(NPC.downedBoss3, "骷髅王");
			CheckABoss(NPC.downedDeerclops, "鹿角兽");
			CheckABoss(NPC.downedQueenBee, "蜂后");
			CheckABoss(Main.hardMode, "肉山", true);
			CheckABoss(NPC.downedPirates, "海盗船");
			CheckABoss(NPC.downedQueenSlime, "史莱姆皇后");
			CheckABoss(NPC.downedFishron, "猪鲨");
			CheckABoss(NPC.downedMartians, "太空飞碟");
			CheckABoss(NPC.downedMechBoss1, "毁灭者");
			CheckABoss(NPC.downedMechBoss2, "双子魔眼");
			CheckABoss(NPC.downedMechBoss3, "机械骷髅王");
			CheckABoss(NPC.downedPlantBoss, "世纪之花", true);
			CheckABoss(NPC.downedGolemBoss, "石巨人", true);
			CheckABoss(NPC.downedEmpressOfLight, "光之女皇");
			CheckABoss(NPC.downedAncientCultist, "邪恶教徒", true);
			CheckABoss(NPC.downedTowerNebula, "星云柱");
			CheckABoss(NPC.downedTowerSolar, "日曜柱");
			CheckABoss(NPC.downedTowerStardust, "星尘柱");
			CheckABoss(NPC.downedTowerVortex, "旋涡柱");
			CheckABoss(NPC.downedMoonlord, "月总", true);
			CheckABoss(NPC.downedChristmasIceQueen, "冰霜女王");
			CheckABoss(NPC.downedChristmasSantank, "圣诞坦克");
			CheckABoss(NPC.downedChristmasTree, "圣诞树");
			CheckABoss(NPC.downedFrost, "霜月");
			CheckABoss(NPC.downedHalloweenKing, "南瓜王");
			CheckABoss(NPC.downedHalloweenTree, "阴森树");
			return new RestObject()
				{
					{
						"KilledBoss",
						KilledBoss
				},
				{
						"NoKilledBoss",
						NoKilledBoss
				},
				{
				"KeyBoss",
					KeyBoss+"前"
				}
			};
		}
		
		public static JObject GetHttp(string uri)
		{
			using (var client = new HttpClient())
			{
				client.Timeout = new TimeSpan(0, 0, 0, 3);
				string res = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
				//Console.WriteLine(res);
				return JObject.Parse(res);
			}
		}
		 
	}
}
