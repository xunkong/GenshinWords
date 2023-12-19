using Studyzy.IMEWLConverter;
using Studyzy.IMEWLConverter.Filters;
using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.IME;
using System.Diagnostics;
using System.Reflection;

if (args.Length < 2)
{
    return;
}
var input = args[0];
var output = args[1];

Directory.CreateDirectory(output);

var assembly = typeof(MainBody).Assembly;
var exports = new List<(ComboBoxShowAttribute Attribute, IWordLibraryExport? Exporter)>();

foreach (Type type in assembly.GetTypes())
{
    if (type.Namespace != null && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME"))
    {
        var cbxa = type.GetCustomAttribute<ComboBoxShowAttribute>(false);
        if (cbxa != null)
        {
            if (type.GetInterface("IWordLibraryExport") != null)
            {
                Debug.WriteLine("Export!!!!" + type.FullName);
                exports.Add((cbxa, assembly.CreateInstance(type.FullName!) as IWordLibraryExport));
            }
        }
    }
}

exports = exports.OrderBy(x => x.Attribute.Index).ToList();

var files = Directory.GetFiles(input);

var words = new List<string>();
foreach (var file in files)
{
    var lines = File.ReadAllLines(file);
    words.AddRange(lines);
}
words = words.Distinct().ToList();

var temp = Path.GetTempFileName();
File.WriteAllText(temp, string.Join(Environment.NewLine, words));


foreach (var export in exports)
{
    try
    {
        var fileName = export.Attribute.Name;
        Console.WriteLine(fileName);
        var mainBody = new MainBody
        {
            Export = export.Exporter,
            Import = new NoPinyinWordOnly(),
            Filters = new List<ISingleFilter>(),
            SelectedWordRankGenerater = new DefaultWordRankGenerater(),
        };
        mainBody.ProcessNotice += (_) => { };

        var str = mainBody.Convert(new List<string> { temp });
        if (fileName.Contains("Gboard") || fileName.Contains("Win10"))
        {
            string place = str.Replace("词库文件在：", "").Trim();
            File.Move(place, Path.Join(output, Path.GetFileName(place.Trim())), true);
        }
        else
        {
            File.WriteAllText(Path.Join(output, fileName + ".txt"), str);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.ToString());
    }
}


File.Delete(temp);