using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TShockAPI;

namespace BossManager
{
    public class Config
    {
        //路径
        public static string ConfigPath = $"{TShock.SavePath}/BossManager.json";

        public string _备注 = "true表示Boss已锁定";

        public Dictionary<string, bool> NPC锁定状态 = new Dictionary<string, bool>();


        /// <summary>
        /// 确认配置文件存在，不存在则创建并填入默认值
        /// </summary>
        public static void EnsureFile()
        {
            if (!File.Exists(ConfigPath))
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
            }
        }
        public static Config ReadConfig()
        {
            //读取ConfigFile
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
        }

        public Config()
        {
            foreach (string name in new Boss().BossNameList)
            {
                NPC锁定状态[name] = false;
            }
        }
        public static void WriteConfig(Config PMConfig)
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(PMConfig, Formatting.Indented));
        }
    }
}
