using ExtractWordImages;
using Microsoft.Office.Interop.Word;
using NLog;
using Shared;
using System.IO.Compression;

// 输入参数验证
if (!Helper.CheckArgs(args, Resource.Help)) {
    return;
}

Logger Logger = LogManager.GetCurrentClassLogger();

const string DocxExtension = ".docx";
const string DocExtension = ".doc";
var Config = Helper.GetConfiguration(args);
string sourcePath = Config["sourcePath"];
string saveDir = Config["saveDir"];

// 参数验证
if (string.IsNullOrEmpty(sourcePath)) {
    Logger.Error("参数 sourcePath 不能为空");
    return;
}
if (string.IsNullOrEmpty(saveDir)) {
    Logger.Error("参数 saveDir 不能为空");
    return;
}
// 验证文件/文件夹是否存在
if (!File.Exists(sourcePath)) {
    Logger.Error($"路径 '{sourcePath}' 不存在");
    return;
}
if (!Directory.Exists(saveDir)) {
    Logger.Error($"目录 '{saveDir}' 不存在");
    return;
}
// 文件后缀
string fileExtension = Path.GetExtension(sourcePath);
bool isDocxFile = fileExtension == DocxExtension;
bool isDocFile = fileExtension == DocExtension;

// 验证文件是否是 word 文件
if (!isDocxFile && !isDocFile) {
    Logger.Debug($"文件 '{sourcePath}' 不是 word 文件");
    return;
}

// 提取 docx 文件图像
var extractDocxImages = (string path) => {
    using FileStream stream = File.OpenRead(path);
    var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
    var imagesEntries = zipArchive.Entries.Where(e => e.FullName.StartsWith("word/media/"));
    // 没有图像数据
    if (!imagesEntries.Any()) {
        Logger.Info("文件无图像数据");
        return;
    }
    // 解压到指定目录
    foreach (var entry in imagesEntries) {
        entry.ExtractToFile(Path.Combine(saveDir, entry.Name), true);
        Logger.Debug($"保存 {entry.Name} 成功");
    }
};

// 提取 doc 文件图像
var extractDocImages = () => {
    Logger.Debug("转换文件中");
    #region 保存为 docx 文件
    var word = new Application();
    var document = word.Documents.Open(sourcePath);
    string newFileName = Path.GetFileName(sourcePath).Replace(".doc", ".docx");
    string newFilePath = string.Empty;
    while (File.Exists(newFilePath = Path.Combine(Environment.CurrentDirectory, $"~temp{Random.Shared.Next()}-{newFileName}")))
        ;
    document.SaveAs2(newFilePath, WdSaveFormat.wdFormatXMLDocument, CompatibilityMode: WdCompatibilityMode.wdWord2013);
    word.ActiveDocument.Close();
    word.Quit();
    #endregion
    extractDocxImages(newFilePath);
    File.Delete(newFilePath);
};

// 开始提取
try {
    if (isDocFile) {
        extractDocImages();
    } else {
        extractDocxImages(sourcePath);
    }
} catch (Exception error) {
    Logger.Error(error);
}