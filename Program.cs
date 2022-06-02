using System.IO.Compression;
using Newtonsoft.Json.Linq;


namespace kspma
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string helpstring = "1. (-h help) for help options\n2. (-l list) to list downloaded mods\n3. (-i install) to install files\n4. (-s search) To search files";
            var versions = getgameversions();
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine(helpstring);
                    break;
                case 1:
                    if (args[0] == "list" || args[0] == "-l")
                    {
                        var pathd = Directory.GetDirectories(Path.Combine(versions[0], "GameData"));
                        Console.WriteLine("Downloaded: ");
                        foreach (var i in pathd)
                        {
                            var filename = Path.GetFileName(i);
                            if (filename != "Squad")
                            {
                                Console.WriteLine("\t- " + filename);
                            }
                        }
                    }
                    else if (args[1] == "help" || args[1] == "-h")
                        Console.WriteLine(helpstring);
                    break;
                case 2:
                    if (args[0] == "-s" || args[0] == "search")
                    {
                        search(args[1]);
                    }
                    if (args[0] == "-i" || args[0] == "install")
                    {
                        install(args[1], "name", versions);
                    }
                    break;
                case 3:
                    if (args[0] == "-i" || args[0] == "install")
                    {
                        if (args[1] == "--name")
                        {
                            install(args[2], "name", versions);
                        }
                        else if (args[1] == "--id")
                        {
                            install(args[2], "id", versions);
                        }
                    }
                    break;
            }
        }

        public static List<string> getgameversions()
        {
            var returnstring = new List<string>();
            var optionspath = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), ".config", "KSPMA");
            Directory.CreateDirectory(optionspath);
            if (!File.Exists(Path.Combine(optionspath, "options.json")))
            {
                Console.Write("GameVersion: ");
                var gameversion = Console.ReadLine();
                Console.Write("Range: ");
                var range = Console.ReadLine();
                Console.Write("GameDir: ");
                var gamedir = Console.ReadLine().Replace('\\', '/');
                var model = "{" + $"\"gameVersion\":\"{gameversion}\"," + "\n" + $"\"Range\":{range}," + "\n" + $"\"gameDir\":{gamedir}" + "}";
                File.WriteAllText(Path.Combine(optionspath, "options.json"), model);
                returnstring.Add(gamedir);
                returnstring.Add(gameversion.Replace(gameversion.Split(".")[1], Convert.ToString(Convert.ToInt32(gameversion.Split(".")[1]) - Convert.ToInt32(range))));
                returnstring.Add(gameversion);
            }
            else
            {
                var fp = File.ReadAllText(Path.Combine(optionspath, "options.json"));
                var jsonread = JObject.Parse(fp);
                try
                {
                    var gameversion = jsonread.GetValue("gameVersion").ToString();
                    var range = jsonread.GetValue("Range").ToString();
                    var gamedir = jsonread.GetValue("gameDir").ToString();
                    returnstring.Add(gamedir);
                    returnstring.Add(gameversion.Replace(gameversion.Split(".")[1], Convert.ToString(Convert.ToInt32(gameversion.Split(".")[1]) - Convert.ToInt32(range))));
                    returnstring.Add(gameversion);
                }
                catch
                {
                    Console.Write("GameVersion: ");
                    var gameversion = Console.ReadLine();
                    Console.Write("Range: ");
                    var range = Console.ReadLine();
                    Console.Write("GameDir: ");
                    var gamedir = Console.ReadLine().Replace('\\', '/');
                    var model = "{" + $"\"gameVersion\":\"{gameversion}\"," + "\n" + $"\"Range\":{range}," + "\n" + $"\"gameDir\":{gamedir}" + "}";
                    File.WriteAllText(Path.Combine(optionspath, "options.json"), model);
                    returnstring.Add(gamedir);
                    returnstring.Add(gameversion.Replace(gameversion.Split(".")[1], Convert.ToString(Convert.ToInt32(gameversion.Split(".")[1]) - Convert.ToInt32(range))));
                    returnstring.Add(gameversion);
                }
            }
            return returnstring;
        }

        public static void search(string keyboard)
        {
            string url = String.Format("https://spacedock.info/api/search/mod?query={0}", keyboard);
            var task = searching(url);
            task.Wait();
        }

        public static async Task searching(string url)
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                var response = await resp.Content.ReadAsStringAsync();
                var name = new List<string>();
                var jsonresp = JArray.Parse(response);
                int longest = 0;
                int longestid = 0;
                foreach (var i in jsonresp)
                {
                    name.Add(i["name"].ToString());
                    if (i["name"].ToString().Length > longest)
                    {
                        longest = i["name"].ToString().Length;
                    }
                    if (i["id"].ToString().Length > longestid)
                    {
                        longestid = i["id"].ToString().Length;
                    }
                }
                if (longest != 0)
                {
                    Console.WriteLine("Name".PadRight(longest + 3) + "ID");
                    for (int i = 0; i < longest + 1; i++)
                    {
                        Console.Write("-");
                    }
                    Console.Write("  ");
                    for (int i = 0; i < longestid; i++)
                    {
                        Console.Write("-");
                    }
                    Console.Write("\n");
                    foreach (var i in jsonresp)
                    {
                        Console.WriteLine(i["name"].ToString().PadRight(longest + 3) + i["id"].ToString());
                    }
                    string path = Path.Combine(Path.GetTempPath(), "KSPMA", "data.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, jsonresp.ToString());
                }
                else
                {
                    Console.WriteLine("Nothing Found");
                }
            }
        }

        public static void install(string keyword, string type, List<string> gamever)
        {
            var datajsonfileexist = File.Exists(Path.Combine(Path.GetTempPath(), "KSPMA", "data.json"));
            
            static void processTheVersionCompareAndData(string keyword, string type, JArray jsonfile, List<string> gamever)
            {
                var answ = jsonfile.ToList().Where(x => x[type].ToString() == keyword).FirstOrDefault();
                if (answ != null)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        Console.Write(String.Format("Do you want to dwonlaod {0}? ", answ["name"]));
                        var ans = Console.ReadLine();
                        if (ans == "yes")
                        {
                            var highest = Convert.ToDouble(gamever[2].ToString().Replace("1.", ""));
                            var lowest = Convert.ToDouble(gamever[1].ToString().Replace("1.", ""));
                            var versionchk = answ["versions"].ToList().Where(x => Convert.ToDouble(x["game_version"].ToString().Replace("1.", "")) <= highest && Convert.ToDouble(x["game_version"].ToString().Replace("1.", "")) >= lowest).FirstOrDefault();
                            if (versionchk != null)
                            {
                                string urldownload = "https://spacedock.info" + versionchk["download_path"].ToString();
                                string dest = Path.Combine(Path.GetTempPath(), "KSPMA", answ["name"] + ".zip");
                                var task = installing(urldownload, dest, gamever[0]);
                                task.Wait();
                            }
                            break;
                        }
                        else if (ans == "no")
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Answer is not correct");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nothing found");
                }
            }

            if (datajsonfileexist)
            {
                processTheVersionCompareAndData(keyword,type,JArray.Parse(File.ReadAllText(Path.Combine(Path.GetTempPath(), "KSPMA", "data.json"))), gamever);
            }
            else
            {
                var temp = data_async(String.Format("https://spacedock.info/api/search/mod?query={0}", keyword).ToString());
                temp.Wait();
                var jsondata = JArray.Parse(temp.Result);
                processTheVersionCompareAndData(keyword, type, jsondata, gamever);
            }
        }



        public static async Task<string> data_async(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task installing(string url, string filepath, string save)
        {
            Console.WriteLine(url);
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    if (File.Exists(filepath)) { File.Delete(filepath); }
                    //儲存檔案
                    var fileInfo = new FileInfo(filepath);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    using (ZipArchive i = ZipFile.Open(filepath, ZipArchiveMode.Read))
                    {
                        //get game dir
                        var extracttopath = Path.GetFullPath(JObject.Parse(File.ReadAllText(Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), ".config", "KSPMA", "options.json")))["gameDir"].ToString());
                        var gamdatapathcotain = i.Entries.Where(x => x.ToString().Contains("GameData/")).FirstOrDefault()!=null;
                        foreach (ZipArchiveEntry entry in i.Entries)
                        {
                            var index = entry.FullName.ToString();
                            if (index.Contains('/') && index.Contains('.'))
                            {
                                if (gamdatapathcotain)
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(extracttopath, index))));
                                    if (File.Exists(Path.GetFullPath(Path.Combine(extracttopath, index)))) { File.Delete(Path.GetFullPath(Path.Combine(extracttopath, index))); }
                                    entry.ExtractToFile(Path.GetFullPath(Path.Combine(extracttopath, index)));
                                }
                                else
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(extracttopath, "GameData",index))));
                                    if (File.Exists(Path.GetFullPath(Path.Combine(extracttopath, "GameData", index)))) { File.Delete(Path.GetFullPath(Path.Combine(extracttopath, "GameData", index))); }
                                    entry.ExtractToFile(Path.GetFullPath(Path.Combine(extracttopath, "GameData",index)));
                                }
                            }
                        }
                    }
                    File.Delete(filepath);
                    Console.WriteLine("Complete");
                }
            }
        }

    }
}
