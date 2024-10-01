using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
public class ABEditor : MonoBehaviour
{
    public static string rootPath = Application.dataPath + "/GAssets";

    /// <summary>
    /// bundle 文件输出路径
    /// </summary>
    public static string abOutputPath = Application.streamingAssetsPath;

    public static List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild>();

    /// <summary>
    /// 记录哪个asset资源属于哪个bundle文件
    /// </summary>
    public static Dictionary<string, string> asset2bundle = new Dictionary<string, string>();

    /// <summary>
    /// 记录每个asset资源依赖的bundle文件列表
    /// </summary>
    public static Dictionary<string, List<string>> asset2Dependencies = new Dictionary<string, List<string>>();

    [MenuItem("ABEditor/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        Debug.Log("BuildAssetBundle!!");
        
        assetBundleBuildList.Clear();
        asset2bundle.Clear();
        asset2Dependencies.Clear();

        // 开始遍历Assets/GAssets文件夹，查找所有的文件
        ScanChildDireations(new DirectoryInfo(rootPath));

        //计算每个资源的依赖
        CalculateDependencies();
        
        /*
        foreach (AssetBundleBuild build in assetBundleBuildList)
        {
            Debug.Log("AB包名字：" + build.assetBundleName);
        }*/

        if (Directory.Exists(abOutputPath) == true)
        {
            Directory.Delete(abOutputPath, true);
        }

        Directory.CreateDirectory(abOutputPath);

        // 参数一: bundle文件列表的输出路径
        // 参数二：生成bundle文件列表所需要的AssetBundleBuild对象数组（用来指导Unity生成哪些bundle文件，每个文件的名字以及文件里包含哪些资源）
        // 参数三：压缩选项BuildAssetBundleOptions.None默认是LZMA算法压缩
        // 参数四：生成哪个平台的bundle文件，即目标平台
        BuildPipeline.BuildAssetBundles(abOutputPath, assetBundleBuildList.ToArray(), BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget);
        // 压缩选项详解
        // BuildAssetBundleOptions.None：使用LZMA算法压缩，压缩的包更小，但是加载时间更长。使用之前需要整体解压。一旦被解压，这个包会使用LZ4重新压缩。使用资源的时候不需要整体解压。在下载的时候可以使用LZMA算法，一旦它被下载了之后，它会使用LZ4算法保存到本地上。
        // BuildAssetBundleOptions.UncompressedAssetBundle：不压缩，包大，加载快
        // BuildAssetBundleOptions.ChunkBasedCompression：使用LZ4压缩，压缩率没有LZMA高，但是我们可以加载指定资源而不用解压全部

        SaveByBuilds();
        
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// 根据指定的文件夹
    /// 1. 将这个文件夹下的所有一级子文件打成一个AssetBundle
    /// 2. 并且递归遍历这个文件夹下的所有子文件夹
    /// </summary>
    /// <param name="directoryInfo"></param>
    public static void ScanChildDireations(DirectoryInfo directoryInfo)
    {
        #region [收集当前路径下的所有文件]

        List<string> assetNames = new List<string>();
        FileInfo[] fileInfoList = directoryInfo.GetFiles();
        foreach (FileInfo fileInfo in fileInfoList)
        {
            if (fileInfo.FullName.EndsWith(".meta"))
            {
                continue;
            }

            // 格式类似 "Assets/GAssets/Prefabs/Sphere.prefab"
            string assetName = fileInfo.FullName.Substring(Application.dataPath.Length - "Assets".Length)
                .Replace('\\', '/');
            assetNames.Add(assetName);
        }

        if (assetNames.Count > 0)
        {
            // 格式类似 gassets_prefabs
            string assetbundleName = directoryInfo.FullName.Substring(Application.dataPath.Length + 1).Replace('/', '_')
                .ToLower();
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = assetbundleName;
            build.assetNames = new string[assetNames.Count];
            for (int i = 0; i < assetNames.Count; i++)
            {
                build.assetNames[i] = assetNames[i];
                asset2bundle.Add(assetNames[i], assetbundleName);
            }

            assetBundleBuildList.Add(build);
        }

        #endregion

        #region [递归遍历当前文件夹下的子文件夹]

        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        foreach (DirectoryInfo info in dirs)
        {
            ScanChildDireations(info);
        }

        #endregion
    }
    
    /// <summary>
    /// 计算每个资源所依赖的ab包文件列表
    /// </summary>
    public static void CalculateDependencies()
    {
        foreach (string asset in asset2bundle.Keys)
        {
            // 这个资源自己所在的bundle
            string assetBundle = asset2bundle[asset];

            string[] dependencies = AssetDatabase.GetDependencies(asset);
            List<string> assetList = new List<string>();
            if (dependencies != null && dependencies.Length > 0)
            {
                foreach (string oneAsset in dependencies)
                {
                    if (oneAsset == asset || oneAsset.EndsWith(".cs"))
                    {
                        continue;
                    }

                    assetList.Add(oneAsset);
                }
            }

            if (assetList.Count > 0)
            {
                List<string> abList = new List<string>();
                foreach (string oneAsset in assetList)
                {
                    bool result = asset2bundle.TryGetValue(oneAsset, out string bundle);
                    if (result == true)
                    {
                        if (bundle != assetBundle)
                        {
                            abList.Add(bundle);
                        }
                    }
                }

                asset2Dependencies.Add(asset, abList);
            }
        }
    }
    
      /// <summary>
    /// 将资源依赖关系数据保存成json格式的文件
    /// </summary>
    private static void SaveByBuilds()
    {
        BundleInfoSet bundleInfoSet = new BundleInfoSet(assetBundleBuildList.Count, asset2bundle.Count);

        // 记录AB包信息
        int id = 0;
        foreach (AssetBundleBuild build in assetBundleBuildList)
        {
            BundleInfo bundleInfo = new BundleInfo();
            bundleInfo.bundle_name = build.assetBundleName;

            bundleInfo.assets = new List<string>();
            foreach (string asset in build.assetNames)
            {
                bundleInfo.assets.Add(asset);
            }

            bundleInfo.bundle_id = id;

            bundleInfoSet.AddBundle(id, bundleInfo);

            id++;
        }

        // 记录每个资源的依赖关系
        int assetIndex = 0;
        foreach (var item in asset2bundle)
        {
            AssetInfo assetInfo = new AssetInfo();
            assetInfo.asset_path = item.Key;
            assetInfo.bundle_id = GetBundleID(bundleInfoSet, item.Value);
            assetInfo.dependencies = new List<int>();

            bool result = asset2Dependencies.TryGetValue(item.Key, out List<string> dependencies);
            if (result == true)
            {
                for (int i = 0; i < dependencies.Count; i++)
                {
                    assetInfo.dependencies.Add(GetBundleID(bundleInfoSet, dependencies[i]));
                }
            }

            bundleInfoSet.AddAsset(assetIndex, assetInfo);

            assetIndex++;
        }

        string jsonPath = abOutputPath + "/asset_bundle_config.json";
        if (File.Exists(jsonPath) == true)
        {
            File.Delete(jsonPath);
        }
        File.Create(jsonPath).Dispose();

        // string jsonData = JsonMapper.ToJson(bundleInfoSet);
        // jsonData = ConvertJsonString(jsonData);
        string jsonData = JsonConvert.SerializeObject(bundleInfoSet, Formatting.Indented);
        File.WriteAllText(jsonPath, jsonData);
    }

    /// <summary>
    /// 根据一个bundle的名字，返回其bundle_id
    /// </summary>
    /// <param name="bundleInfoSet"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private static int GetBundleID(BundleInfoSet bundleInfoSet, string bundleName)
    {
        foreach (BundleInfo bundleInfo in bundleInfoSet.bundles)
        {
            if (bundleName == bundleInfo.bundle_name)
            {
                return bundleInfo.bundle_id;
            }
        }

        return -1;
    }
    
    /// <summary>
    /// 格式化json
    /// </summary>
    /// <param name="str">输入json字符串</param>
    /// <returns>返回格式化后的字符串</returns>
    private static string ConvertJsonString(string str)
    {
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }
    
}