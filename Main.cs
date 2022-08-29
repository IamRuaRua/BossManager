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
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using System.Activities;
using OTAPI;
using System.Threading;
using System.IO;

namespace BossManager
{
    [ApiVersion(2, 1)]
    public class ProgressManager : TerrariaPlugin
    {
        List<String> KilledBoss = new List<String>();
        List<String> NoKilledBoss = new List<String>();
        readonly Boss boss = new Boss();
        public Config PMConfig;
        public override string Author => "Rua";
        public override string Description => "Boss和NPC控制插件";
        public override string Name => "BossManager";
        public override Version Version => new Version(1, 0, 0, 2);
        public ProgressManager(Main game) : base(game) { }

        public override void Initialize()
        {
            LanguageManager.Instance.SetLanguage(7);//设置Tshock为中文
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            Commands.ChatCommands.Add(new Command(permissions: "Rua.BossManager", cmd: this.BossManagerCmd, "BM", "bm"));//注册bm命令
            Commands.ChatCommands.Add(new Command(permissions: "Rua.NPCManager", cmd: this.NPCManagerCmd, "NM", "nm"));//注册nm命令
            TShock.RestApi.Register(new SecureRestCommand("/bossstatus", CheckBoss, "rest.boss"));
            ServerApi.Hooks.NpcSpawn.Register(this, OnSpawnNPC, 10);//召唤BOSS 
            ServerApi.Hooks.NetGetData.Register(this, (GetDataEventArgs e) =>
            {
                if (e.MsgID == PacketTypes.SpawnBossorInvasion)
                {
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length - 1))
                    {
                        BinaryReader reader = new BinaryReader(data);
                        int id = reader.ReadUInt16();
                        int type = reader.ReadUInt16();
                        TSPlayer player = TShock.Players[id];
                        if (player != null)
                        {
                            if (type > 0)
                            {
                                TSPlayer.All.SendMessage($"[{player.Name}]召唤了{Lang.GetNPCName(type)}", Color.Yellow);
                            }
                            else
                            {
                                TSPlayer.All.SendMessage($"[{player.Name}]召唤错误", Color.White);
                            }
                        }
                    }
                }
            });

            Config.EnsureFile();
            PMConfig = Config.ReadConfig();
        }
        private void OnPostInitialize(EventArgs e)
        { 
           new Thread(() =>
            {
                while (true)
                {
                    ClearNPC();
                    Thread.Sleep(1000);
                }
            }).Start();
        }
        public void ClearNPC()//清除NPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active &&IsLock(Main.npc[i].netID))
                {
                    Main.npc[i].active = false;
                    Main.npc[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", i);
                }
            }
        }
         

        public void BossManagerCmd(CommandArgs args)//bm命令
          {
              var cmdArgs = args.Parameters;
              var sendPlayer = args.Player;
              try
              {
                  switch (cmdArgs[0].ToLower())
                  {
                      case "help":
                          sendPlayer.SendMessage("/bm lock bossName          -- 锁定某个boss"
                                              + "\n/bm unlock bossName                 -- 解锁某个boss"
                                              + "\n/bm status                --获取boss 锁定状态"
                                              + "\n/bm listboss             -- 列出所有可锁定boss名称", new Color(255, 255, 0));
                          return;
                      case "status":
                          string LockedBoss = "";
                          string UnLockedBoss = "";
                          string BossName;
                          for (int i = 0; i < boss.BossNameList.Length; i++)
                          {
                              BossName = boss.BossNameList[i];
                              if (CheckANPCIsLock(BossName))
                              {
                                  LockedBoss += BossName + ",";

                              }
                              else
                              {
                                  UnLockedBoss += BossName + ",";
                              }
                          }
                          sendPlayer.SendMessage("已锁定Boss:\n" + LockedBoss.TrimEnd(',') + "\n未锁定Boss:\n" + UnLockedBoss.TrimEnd(','), new  Color(255, 255, 0));
                          return;
                      case "lock":
                          if (cmdArgs[1].ToLower() == "all")
                          {
                              SetAllBossLock(true);
                              sendPlayer.SendMessage("锁定所有Boss成功!", new Color(0, 255, 0));
                              break;
                          }
                          if (SetBossLock(cmdArgs[1], true))
                          {
                              sendPlayer.SendMessage($"Boss {boss.getNameByAbbreviation(cmdArgs[1])} 锁定成功!", new  Color(0, 255, 0));
                          }
                          else
                          {
                              sendPlayer.SendMessage($"Boss {boss.getNameByAbbreviation(cmdArgs[1])} 锁定失败!Boss不存在或名称有误,发送/bm listBoss 显示所有可锁定的boss", new  Color(255, 0, 0));
                          }
                          break; 
                      case "unlock":
                          if (cmdArgs[1].ToLower() == "all")
                          {
                              SetAllBossLock(false);
                              sendPlayer.SendMessage("解锁所有Boss成功!", new  Color(0, 255, 0));
                              break;
                          }
                          if (SetBossLock(cmdArgs[1], false))
                          {
                              sendPlayer.SendMessage($"Boss {boss.getNameByAbbreviation(cmdArgs[1])} 解锁成功!", new  Color(0, 255, 0));
                          }
                          else
                          {
                              sendPlayer.SendMessage($"Boss {boss.getNameByAbbreviation(cmdArgs[1])} 解锁失败!Boss不存在或名称有误,发送/bm listBoss 显示所有可锁定的boss", new  Color(255, 0, 0));
                          }
                          break;

                      case "listboss":
                          string message = "";
                          for (int i = 0; i < boss.BossNameList.Length; i++)
                          {
                              BossName = boss.BossNameList[i];
                              if (i % 2 == 0)
                              {
                                  message += BossName + "    ";

                              }
                              else
                              {
                                  message += BossName + "\n";
                              }
                          }
                          sendPlayer.SendMessage(message, new Microsoft.Xna.Framework.Color(255, 255, 0));
                          break;
                      default:
                          sendPlayer.SendErrorMessage("语法错误,使用/bm help获取帮助");
                          return;
                  }

              }
              catch (Exception)
              {
                  sendPlayer.SendErrorMessage("语法错误,使用/bm help获取帮助");
                  return;
              }

          }
        public  void NPCManagerCmd(CommandArgs args)//nm命令
          {
              var cmdArgs = args.Parameters;
              var sendPlayer = args.Player;
              try
              {
                  switch (cmdArgs[0].ToLower())
                  {
                      case "help":
                          sendPlayer.SendMessage("/nm locknpc NPCNameOrID       -- 锁定某个NPC"
                                              + "\n/nm unlocknpc NPCNameOrID    -- 解锁某个NPC"
                                              + "\n/nm status                --获取NPC锁定状态"
                                              + "\n/nm search              --搜索npc", new Color(255, 255, 0));
                          return;
                      case "status": 
                          sendPlayer.SendMessage("已锁定NPC:\n" + GetLockedNPC(), new Color(255, 255, 0));
                          return;

                      case "lock"://锁定NPC
                          List<NPC> list = FindNPCByStringOrId(cmdArgs[1]);
                          if (list == null || list.Count == 0)
                          {
                              sendPlayer.SendMessage($"NPC 解锁失败!未找到此NPC", new Color(255, 0, 0));
                              return;
                          }
                          if (list.Count == 1)
                          {
                              SetNPCLock(list.First(), true);
                              sendPlayer.SendMessage($"NPC {list.First().FullName+"("+list.First().netID.ToString()}) 锁定成功!", new Color(0, 255, 0));
                              return;
                          }
                        string FindNPC = "";
                        int i = 0;
                        foreach (NPC npc in list)
                        {
                            if (cmdArgs[1] == npc.FullName)
                            {
                                SetNPCLock(npc, true);
                                sendPlayer.SendMessage($"NPC {npc.FullName + "(" + npc.netID.ToString()}) 锁定成功!", new Color(0, 255, 0));
                                return;
                            }
                            i++;
                            if(i%4==0)
                                FindNPC += npc.FullName + "(" + npc.netID.ToString() + ")\n";
                            else
                                FindNPC += npc.FullName + "(" + npc.netID.ToString() + "),";
                        }
                        sendPlayer.SendMessage($"发现多个NPC:\n{FindNPC.TrimEnd(',')}", new Color(255, 255, 0));
                        break;

                    case "unlock"://解锁NPC
                        list = FindNPCByStringOrId(cmdArgs[1]);
                        if (list == null || list.Count == 0)
                        {
                            sendPlayer.SendMessage($"NPC 解锁失败!未找到此NPC", new Color(255, 0, 0));
                            return;
                        }
                        if (list.Count == 1)
                        {
                            SetNPCLock(list.First(), false);
                            sendPlayer.SendMessage($"NPC {list.First().FullName + "(" + list.First().netID.ToString()}) 解锁成功!", new Color(0, 255, 0));
                            return;
                        }
                         FindNPC = "";
                        i = 0;
                        foreach (NPC npc in list)
                        {
                            if (cmdArgs[1] == npc.FullName)
                            {
                                SetNPCLock(npc, false);
                                sendPlayer.SendMessage($"NPC {npc.FullName + "(" + npc.netID.ToString()}) 解锁成功!", new Color(0, 255, 0));
                                return;
                            }
                            i++;
                            if (i % 4 == 0)
                                FindNPC += npc.FullName + "(" + npc.netID.ToString() + ")\n";
                            else
                                FindNPC += npc.FullName + "(" + npc.netID.ToString() + "),";
                        }
                        sendPlayer.SendMessage($"发现多个NPC:\n{FindNPC.TrimEnd(',')}", new Color(255, 255, 0));
                        break;
                    case "s":
                    case "search"://搜索NPC
                        list = FindNPCByStringOrId(cmdArgs[1]);
                        if (list == null || list.Count == 0)
                        {
                            sendPlayer.SendMessage($"未找到此NPC", new Color(255, 0, 0));
                            return;
                        }
                        if (list.Count == 1)
                        { 
                            sendPlayer.SendMessage($"寻找到一个NPC {list.First().FullName + "(" + list.First().netID.ToString()})", new Color(0, 255, 0));
                            return;
                        }
                        FindNPC = "";
                        i = 0;
                        foreach (NPC npc in list)
                        {
                            if (cmdArgs[1] == npc.FullName)
                            {
                                sendPlayer.SendMessage($"寻找到一个NPC {npc.FullName + "(" + npc.netID.ToString()})", new Color(0, 255, 0));
                                return;
                            }
                            i++;
                            if (i % 4 == 0)
                                FindNPC += npc.FullName + "(" + npc.netID.ToString() + ")\n";
                            else
                                FindNPC += npc.FullName + "(" + npc.netID.ToString() + "),";
                        }
                        sendPlayer.SendMessage($"寻找到多个NPC:\n{FindNPC.TrimEnd(',')}", new Color(255, 255, 0));
                        break;
                    default:
                        sendPlayer.SendErrorMessage("语法错误,使用/nm help获取帮助");
                        return;
                }

            }
            catch (Exception)
            {
                sendPlayer.SendErrorMessage("语法错误,使用/nm help获取帮助");
                return;
            }

        }
        public List<NPC> FindNPCByStringOrId(string value)
        {
            List<NPC> list = new List<NPC>(); 
            try
            {
                NPC npc = new NPC();
                npc.SetDefaults(int.Parse(value));
                return new List<NPC> { npc };
            }
            catch (Exception)
            {
                for (int i = -65; i < NPCID.Count; i++)
                {
                    NPC npc = new NPC();
                    npc.SetDefaults(i); 
                    if (npc.FullName.Contains(value))
                    {
                        list.Add(npc);
                    }
                }
                return list;
            } 
        }

        public string GetLockedNPC()//获取已锁定的NPC
        {
            string LockedNPC = "";
            int j = 0;
            for (int i = -65; i < NPCID.Count; i++)
            {
                NPC npc = new NPC();
                npc.SetDefaults(i);
                if (IsLock(npc.netID))
                {
                    j++;
                    if (j % 4 == 0) 
                        LockedNPC += npc.FullName + "(" + npc.netID.ToString() + ")\n";
                    else 
                        LockedNPC += npc.FullName + "(" + npc.netID.ToString() + "),";
                }
            }
            return LockedNPC.TrimEnd(','); 
        }
        public void SetNPCLock(NPC npc, bool status)//修改NPC锁定状态
        { 
             PMConfig.NPC锁定状态[npc.FullName] = status;
             Config.WriteConfig(PMConfig); 
        }
        public bool SetBossLock(string BossName, bool status)//修改Boss锁定状态
        {
            foreach (int id in boss.BossIDList)
            {
                if (boss.BossIDToNameMap[id].Contains(BossName))
                {
                    PMConfig.NPC锁定状态[boss.getNameByAbbreviation(BossName)] = status;
                    Config.WriteConfig(PMConfig);
                    return true;
                }
            }
            return false;
        }
        public void SetAllBossLock(bool status)//设置所有Boss锁定状态
        {
            foreach (int id in boss.BossIDList)
            { 
                PMConfig.NPC锁定状态[boss.BossIDToNameMap[id].First()] = status; 
            } 
            Config.WriteConfig(PMConfig); 
        }
        public bool IsLock(int NPCID)//判断某个NPC是否锁定
        {
            PMConfig = Config.ReadConfig();
            String npcName = boss.getIDToNameFirst(NPCID, NPC.GetFullnameByID(NPCID));
            if (PMConfig.NPC锁定状态.ContainsKey(npcName))
            {
                return PMConfig.NPC锁定状态[npcName];
            }
            else
                return false; 
        }

        public void OnSpawnNPC(NpcSpawnEventArgs args)//控制NPC生成
        {
            args.Handled = true;
            var npc = Main.npc[args.NpcId];
            if (npc == null)
                return;
            if (npc.active && (npc.netID == 125 || npc.netID == 126) && IsLock(126))
            {
                npc.netID = 0;
                npc.active = false;
                TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npc.netID);
                if (boss.BossIDList.Contains(npc.netID))
                    TSPlayer.All.SendInfoMessage(npc.FullName + "已被锁定，请联系管理员解锁");
                return;
             }
            if (npc.active && IsLock(npc.netID))
            { 
                npc.netID = 0;
                npc.active = false;
                TSPlayer.All.SendData(PacketTypes.NpcUpdate, "", npc.netID); 
                  if(boss.BossIDList.Contains(npc.netID)){ 
                      TSPlayer.All.SendInfoMessage(npc.FullName+"已被锁定，请联系管理员解锁"); 
               }
                
            }

        }
       
        public bool CheckANPCIsLock(string BossName)//获取Boss是否生成
        {
            PMConfig = Config.ReadConfig();
            if (PMConfig.NPC锁定状态[BossName])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CheckANPC(bool Killed, string NPCName, bool IsKeyBoss = false)//将锁定和未锁定的NPC分开保存
        {
            if (Killed)
            {
                KilledBoss.Add(NPCName);
            }
            else
            {
                NoKilledBoss.Add(NPCName); 
            }
        }
        public string getKeyBoss()//获取关键boss进度
        {
            string Boss2Name = (Main.drunkWorld) ? "虫子或脑子" : (WorldGen.crimson) ? "邪神大脑" : "世界吞噬者";
            if (!NPC.downedBoss2) return Boss2Name + "前";
            if (!Main.hardMode) return "肉山前";
            if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3) return "新三王时期";
            if (!NPC.downedPlantBoss) return "世纪之花前";
            if(!NPC.downedGolemBoss)return  "石巨人前";
            if(!NPC.downedAncientCultist)return "邪恶教徒前";
            if (!NPC.downedTowerNebula || !NPC.downedTowerSolar || !NPC.downedTowerStardust || !NPC.downedTowerVortex) return "四柱时期";
            if(!NPC.downedMoonlord)return "月总前";
            return "毕业";
        }

        private object CheckBoss(RestRequestArgs args)//获取各boss击败状态
        {
            NPCKillsTracker NKT = Main.BestiaryTracker.Kills;
            KilledBoss = new List<String>();
            NoKilledBoss = new List<String>();
            //进度boss
            CheckANPC(NPC.downedMoonlord, "月总");
            CheckANPC(NPC.downedAncientCultist, "邪恶教徒");
            CheckANPC(NPC.downedGolemBoss, "石巨人");
            CheckANPC(NPC.downedPlantBoss, "世纪之花");
            CheckANPC(Main.hardMode, "肉山");
            string Boss2Name = (Main.drunkWorld) ? "虫子或脑子" : (WorldGen.crimson) ? "邪神大脑" : "世界吞噬者";
            CheckANPC(NPC.downedBoss2, Boss2Name);

            CheckANPC(NPC.downedSlimeKing, "史莱姆王");
            CheckANPC(NPC.downedBoss1, "克苏鲁之眼");
            CheckANPC(NPC.downedBoss3, "骷髅王");
            CheckANPC(NPC.downedDeerclops, "鹿角兽");
            CheckANPC(NPC.downedQueenBee, "蜂后");
            CheckANPC(NPC.downedPirates, "海盗船");
            CheckANPC(NPC.downedQueenSlime, "史莱姆皇后");
            CheckANPC(NPC.downedFishron, "猪鲨");
            CheckANPC(NPC.downedMartians, "太空飞碟");

            string name1 = "";
            if (!NPC.downedMechBoss1)
            {
                name1 += "毁灭者 ";
            }
            if (!NPC.downedMechBoss2)
            {
                name1 += "双子魔眼 ";
            }
            if (!NPC.downedMechBoss3)
            {
                name1 += "机械骷髅王 ";
            }
            CheckANPC(NPC.downedMechBossAny, name1);

            CheckANPC(NPC.downedEmpressOfLight, "光之女皇");

            string name2 = "";
            if (!NPC.downedTowerNebula)
                name2 += "星云柱 ";
            if (!NPC.downedTowerSolar)
                name2 += "日耀柱 ";
            if (!NPC.downedTowerStardust)
                name2 += "星尘柱 ";
            if (!NPC.downedTowerVortex)
                name2 += "星旋柱 ";
            CheckANPC(NPC.downedTowerVortex || NPC.downedTowerNebula || NPC.downedTowerSolar || NPC.downedTowerStardust, name2);
             
            CheckANPC(NPC.downedChristmasIceQueen, "冰霜女王");
            CheckANPC(NPC.downedChristmasSantank, "圣诞坦克");
            CheckANPC(NPC.downedChristmasTree, "常绿尖叫怪");
            CheckANPC(NPC.downedFrost, "雪人军团");
            CheckANPC(NPC.downedHalloweenKing, "南瓜王");
            CheckANPC(NPC.downedHalloweenTree, "哀木");
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
                    getKeyBoss()
                }
            };
        }

        public static JObject GetHttp(string uri)//未使用
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 0, 3);
                string res = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                return JObject.Parse(res);
            }
        }
    }
}
