using System.Text.Json;
using Windows.Storage;
using AnEoT.Tools.VolumeCreator.Models;

namespace AnEoT.Tools.VolumeCreator.Helpers;

/// <summary>
/// 为生成「泰拉广告」提供帮助方法的类
/// </summary>
public static class FakeAdHelper
{
    private static volatile Dictionary<string, FakeAdConfiguration>? fakeAdConfigurations = null;

    /// <summary>
    /// 随机获取一个「泰拉广告」
    /// </summary>
    public static async Task<FakeAdInfo> RollFakeAdAsync()
    {
        fakeAdConfigurations ??= await GetConfigurations();

        FakeAdInfo fakeAdInfo = new();

        float cumulativeProbability = 0;
        Random random = new();
        foreach (KeyValuePair<string, FakeAdConfiguration> item in fakeAdConfigurations)
        {
            cumulativeProbability += item.Value.Probability;

            if (random.NextSingle()  < cumulativeProbability)
            {
                FakeAdConfiguration selectedAd = item.Value;
                int index = (int)Math.Floor(random.NextDouble() * selectedAd.Files!.Length);
                string fileName = selectedAd.Files[index];
                StorageFile adInfoFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/fake-ads/{fileName}.txt"));

                fakeAdInfo = fakeAdInfo with { AdImageLink = $"{fileName}.webp" };

                IList<string> lines = await FileIO.ReadLinesAsync(adInfoFile);

                int i = 0;
                foreach (string line in lines)
                {
                    fakeAdInfo = i switch
                    {
                        0 => fakeAdInfo with { AdText = line },
                        1 => fakeAdInfo with { AdAbout = line },
                        2 => fakeAdInfo with { AboutLink = line },
                        3 => fakeAdInfo with { AdLink = line },
                        _ => throw new NotImplementedException(),
                    };
                    i++;
                }
                return fakeAdInfo;
            }
        }

        throw new NotImplementedException();
    }

    private static async Task<Dictionary<string, FakeAdConfiguration>> GetConfigurations()
    {
        StorageFile adJsonFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/fake-ads/ads.json"));
        using Stream utf8Json = await adJsonFile.OpenStreamForReadAsync();
        Dictionary<string, FakeAdConfiguration> adConfig = await JsonSerializer.DeserializeAsync(utf8Json, StringFakeAdConfigContext.Default.DictionaryStringFakeAdConfiguration)
            ?? throw new FileNotFoundException("未能找到配置「泰拉广告」的 JSON 文件。");

        return adConfig;
    }
}