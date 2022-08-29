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

        public string _备注="true表示Boss已锁定";

        public Dictionary<string, bool> Boss锁定状态 = new Dictionary<string, bool>();

        
        /// <summary>
        /// 确认配置文件存在，不存在则创建并填入默认值
        /// </summary>
        public static void EnsureFile()
        {
            if (!File.Exists(ConfigPath))
            {

                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config()));
         
            }
        }
        public static Config ReadConfig()
        {
            //读取ConfigFile
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
        }

        public Config()
        {
            foreach (var item in new Boss().BossNameList)
            {
                Boss锁定状态[item] = false;
            }
        }
        public static void WriteConfig(Config PMConfig)
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(PMConfig));
        }
    }
}
