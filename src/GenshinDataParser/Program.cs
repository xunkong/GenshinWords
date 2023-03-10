using System.Text.Json;
using System.Text.Json.Nodes;

var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"));

if (settings is null)
{
    Console.WriteLine($"Setting file '{Path.GetFullPath("settings.json")}' cannot be found.");
    File.WriteAllText("settings.json", JsonSerializer.Serialize(new Settings()));
    return;
}

Console.WriteLine($"[Setting] GenshinDataPath: {settings.GenshinDataPath}");
Console.WriteLine($"[Setting] OutputPath: {settings.OutputPath}");

Console.WriteLine("Parse text map.");
var textmapstr = File.ReadAllText(Path.Join(settings.GenshinDataPath, "TextMap\\TextMapCHS.json"));

var textmap = JsonNode.Parse(textmapstr);

ArgumentNullException.ThrowIfNull(textmap);

if (string.IsNullOrWhiteSpace(settings.OutputPath))
{
    settings.OutputPath = "./output";
}

Directory.CreateDirectory(settings.OutputPath);
Console.WriteLine($"OutputPath: {Path.GetFullPath(settings.OutputPath)}");

var filePaths = Directory.GetFiles(Path.Join(settings.GenshinDataPath, "ExcelBinOutput"));


foreach (var filePath in filePaths)
{
    var fileName = Path.GetFileName(filePath);
    if (!settings.ConverterAll && !settings.ExcelBinOutputs.Contains(fileName))
    {
        continue;
    }
    Console.WriteLine(fileName);
    var baseNode = JsonNode.Parse(File.ReadAllText(filePath));
    var words = new List<string>();
    foreach (var node in (baseNode as JsonArray)!)
    {
        foreach (var kv in (node as JsonObject)!)
        {
            if (kv.Key.Contains("TextMapHash") && !kv.Key.StartsWith("cv") && !kv.Key.Contains("content", StringComparison.OrdinalIgnoreCase) && !kv.Key.Contains("desc", StringComparison.OrdinalIgnoreCase))
            {
                var hash = kv.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(hash))
                {
                    var word = textmap[hash]?.ToString();
                    if (!string.IsNullOrWhiteSpace(word) && !word.Contains("test") && !word.Contains("废弃") && !word.Contains("测试") && !word.Contains("$") && !word.Contains("%") && !word.Contains("/") && !word.Contains("#") && !word.Contains("{") && !word.Contains("\n") && !word.Contains("，") && !word.Contains("。"))
                    {
                        words.Add(word);
                    }
                }
            }
        }
    }
    words = words.Distinct().ToList();
    Console.WriteLine($"{words.Count} words");
    if (words.Any())
    {
        File.WriteAllText(Path.Join(settings.OutputPath, Path.ChangeExtension(fileName, ".txt")), string.Join(Environment.NewLine, words));
    }
}



#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。


class Settings
{

    public string GenshinDataPath { get; set; }


    public string OutputPath { get; set; }


    public bool ConverterAll { get; set; }

    public List<string> ExcelBinOutputs { get; set; }
}

