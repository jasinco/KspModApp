using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO.Compression;

namespace ksp_mod_main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string version = "1.0.0";
            List<string> gameversion = getGameVersionOrCreate();

            Console.WriteLine("KSPMA Version: "+version);
            if (args.Length > 0) {
                switch (args[0].ToString())
                {
                    case "-q":
                        if (args.Length == 2) {
                            Search(args[1]);
                        }
                        break;
                    case "search":
                        if (args.Length == 2)
                        {
                            Search(args[1]);
                        }
                        break;
                    case "-i":
                        if (args.Length == 3)
                        {
                            switch (args[1])
                            {
                                case "--id":
                                    Install("id", args[2], gameversion);
                                    break;
                                case "--name":
                                    Install("name", args[2], gameversion);
                                    break;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid Options");
                        break;
                }
            }
            else
            {
                Console.WriteLine("No Options");
            }
        }

        public static void Install(string opt, string keyword, List<string> gameversion)
        {            
            switch (opt)
            {
                case "id":
                    StreamReader r = new StreamReader(@"./data.json");
                    string all = r.ReadToEnd();
                    JArray jsonArray = JArray.Parse(all);
                    foreach (JToken item in jsonArray)
                    {
                        if (item["id"].ToString() == keyword)
                        {
                            bool check = false;
                            while (check == false)
                            {
                                Console.Write("Are you sure to download " + item["name"] + "(yes or false)? ");
                                string ans = Console.ReadLine();
                                if (ans == "yes")
                                {
                                    check = true;
                                    break;
                                } else if(ans == "false")
                                {
                                    check = false;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("UNavaliavle");
                                }
                            }
                            if (check)
                            {
                                var setting_lowest = Convert.ToInt32(gameversion[0].Split(".")[1]);
                                var setting_highest = Convert.ToInt32(gameversion[1].Split(".")[1]);
                                var info_highest = Convert.ToInt32(item["versions"][0]["game_version"].ToString().Split('.')[1]);
                                var info_lowest = Convert.ToInt32(item["versions"][JArray.Parse(item["versions"].ToString()).Count() - 1]["game_version"].ToString().Split('.')[1]);
                                if (setting_lowest <= info_highest)
                                {
                                    IEnumerable<int> setting_allowed = Enumerable.Range(setting_lowest, setting_highest-1).Select(x => x + 1).ToList();
                                    IEnumerable<int> info_allowed = Enumerable.Range(info_lowest, info_highest-1).Select(x => x + 1).ToList();
                                    int[] finalex = info_allowed.Where(x => setting_allowed.Contains(x)).ToArray();
                                    var final = Convert.ToString(finalex[finalex.Length - 1]);
                                    foreach (var i in item["versions"])
                                    {
                                        if (i["game_version"].ToString().Split(".")[1] == final)
                                        {
                                            var url = "https://spacedock.info" + i["download_path"];
                                            var task = Install_Task(url, item["name"].ToString());
                                            task.Wait();
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    break;
                case "name":
                    StreamReader rd = new StreamReader(@"./data.json");
                    string alld = rd.ReadToEnd();
                    JArray jsonArrays = JArray.Parse(alld);
                    foreach (JToken item in jsonArrays)
                    {
                        if (item["name"].ToString() == keyword)
                        {
                            bool check = false;
                            while (check == false)
                            {
                                Console.Write("Are you sure to download " + item["name"] + "(yes or false)? ");
                                string ans = Console.ReadLine();
                                if (ans == "yes")
                                {
                                    check = true;
                                    break;
                                }
                                else if (ans == "false")
                                {
                                    check = false;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("UNavaliavle");
                                }
                            }
                            if (check)
                            {
                                var setting_lowest = Convert.ToInt32(gameversion[0].Split(".")[1]);
                                var setting_highest = Convert.ToInt32(gameversion[1].Split(".")[1]);
                                var info_highest = Convert.ToInt32(item["versions"][0]["game_version"].ToString().Split('.')[1]);
                                var info_lowest = Convert.ToInt32(item["versions"][JArray.Parse(item["versions"].ToString()).Count() - 1]["game_version"].ToString().Split('.')[1]);
                                if (setting_lowest <= info_highest)
                                {
                                    IEnumerable<int> setting_allowed = Enumerable.Range(setting_lowest, setting_highest - 1).Select(x => x + 1).ToList();
                                    IEnumerable<int> info_allowed = Enumerable.Range(info_lowest, info_highest - 1).Select(x => x + 1).ToList();
                                    int[] finalex = info_allowed.Where(x => setting_allowed.Contains(x)).ToArray();
                                    var final = Convert.ToString(finalex[finalex.Length - 1]);
                                    foreach (var i in item["versions"])
                                    {
                                        if (i["game_version"].ToString().Split(".")[1] == final)
                                        {
                                            var url = "https://spacedock.info" + i["download_path"];
                                            var task = Install_Task(url, item["name"].ToString());
                                            task.Wait();
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        public static async Task Install_Task(string url, string fname)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentType.ToString() == "application/zip")
                {
                    string fp = Path.Combine(Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), ".config", "KSPMA"), "options.json");
                    var jsonf = JObject.Parse(File.ReadAllText(fp));
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        //儲存檔案
                        var fileInfo = new FileInfo(Path.Combine(jsonf["gameDir"].ToString(), "GameData",fname+".zip"));
                        using (var fileStream = fileInfo.OpenWrite())
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                        ZipFile.ExtractToDirectory(Path.Combine(jsonf["gameDir"].ToString(), "GameData", fname + ".zip"), Path.Combine(jsonf["gameDir"].ToString(), "GameData", fname));
                        File.Delete(Path.Combine(jsonf["gameDir"].ToString(), "GameData", fname + ".zip"));
                        Console.WriteLine("Compplete !");
                    }
                }
            }
        }

        public static void Search(string search)
        {
            var baseModeUrl = String.Format("https://spacedock.info/api/search/mod?query={0}", search);
            var task = Get_Search_Data(baseModeUrl);
            task.Wait();
        }

        public static async Task Get_Search_Data(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(20);
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                JArray jsonArray = JArray.Parse(responseBody);
                int longest = 0;
                foreach (var item in jsonArray)
                {
                    if (item["name"].ToString().Length > longest)
                    {
                        longest = item["name"].ToString().Length;
                    }
                }

                int logind = 0;

                foreach (var item in jsonArray)
                {
                    if (item["id"].ToString().Length > logind)
                    {
                        logind = item["id"].ToString().Length;
                    }
                }

                Console.WriteLine('\n');
                Console.Write("Name".PadRight(longest+2));
                Console.WriteLine(" ID");
                for (int i = 0; i < (longest); i++)
                {
                    Console.Write('-');
                }
                Console.Write("   ");
                for (int i = 0; i < (logind+2); i++)
                {
                    Console.Write('-');
                }
                Console.WriteLine("");
                foreach (var item in jsonArray)
                {
                    Console.WriteLine(item["name"].ToString().PadRight(longest+3)+ item["id"]);
                }
                // add to temp
                File.WriteAllText(@"./data.json",jsonArray.ToString());
            }
        }
        public static List<string> getGameVersionOrCreate()
        {
            Directory.CreateDirectory(Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), ".config", "KSPMA"));
            string fp = Path.Combine(Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), ".config", "KSPMA"), "options.json");
            if (!File.Exists(fp))
            {
                Console.Write("Game Version: ");
                string gameversion = Console.ReadLine();
                Console.Write("Range: ");
                int range = Convert.ToInt32(Console.ReadLine());
                Console.Write("Game Directory: ");
                string gamaedir = Console.ReadLine().Replace("\\","/");
                string temp_parse = "{" + $"'gameVersion':{gameversion},"+"\n"+$"'Range':{range},"+"\n"+$"'gameDir':{gamaedir}"+"}";
                File.WriteAllText(fp, temp_parse);
                var lower = gameversion.Replace(gameversion.Split(".")[1], Convert.ToString(Convert.ToInt32(gameversion.Split(".")[1]) - range));
                var returninfo = new List<string>();
                returninfo.Add(lower);returninfo.Add(gameversion);
                return returninfo;
            }
            else
            {
                try
                {
                    var all = JObject.Parse(File.ReadAllText(fp));
                    var gameversion = all["gameVersion"].ToString();
                    var lower = gameversion.Replace(gameversion.Split(".")[1], Convert.ToString(Convert.ToInt32(gameversion.Split(".")[1]) - Convert.ToInt32(all["Range"].ToString())));
                    var returninfo = new List<string>();
                    returninfo.Add(lower); returninfo.Add(gameversion);
                    return returninfo;
                }
                catch
                {
                    Console.Write("Game Version: ");
                    string gameversion = Console.ReadLine();
                    Console.Write("Range: ");
                    int range = Convert.ToInt32(Console.ReadLine());
                    Console.Write("Game Directory: ");
                    string gamaedir = Console.ReadLine().Replace("\\", "/");
                    string temp_parse = "{" + $"\"gameVersion\":\"{gameversion}\"," + "\n" + $"\"Range\":{range}," + "\n" + $"\"gameDir\":{gamaedir}" + "}";
                    File.WriteAllText(fp, temp_parse);
                    var lower = gameversion.Replace(gameversion.Split(".")[1], Convert.ToString(Convert.ToInt32(gameversion.Split(".")[1]) - range));
                    var returninfo = new List<string>();
                    returninfo.Add(lower); returninfo.Add(gameversion);
                    return returninfo;
                }
            }
            
        }
    }
}
