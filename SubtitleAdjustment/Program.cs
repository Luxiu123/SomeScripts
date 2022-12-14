using NLog;
using Shared;
using SubtitleAdjustment;

// 输入参数验证
if (!Helper.CheckArgs(args, Resource.Help)) {
    return;
}

Logger Logger = LogManager.GetCurrentClassLogger();
var Config = Helper.GetConfiguration(args);
string sourcePath = Config["sourcePath"];
string savePath = Config["savePath"];
string offsetArg = Config["offset"];

// 参数验证
if (string.IsNullOrEmpty(sourcePath)) {
    Logger.Error("参数 sourcePath 不能为空");
    return;
}
if (string.IsNullOrEmpty(savePath)) {
    Logger.Error("参数 savePath 不能为空");
    return;
}
// 解析 offset
if (!int.TryParse(offsetArg, out var offset)) {
    Logger.Error("解析 offset 失败");
    return;
}

try {
    Service.Adjust(sourcePath, savePath, offset);
} catch (Exception error) {
    Logger.Error(error);
}