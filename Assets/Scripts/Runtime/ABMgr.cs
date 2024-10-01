using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一个Bundle数据 【用于序列化为json文件 asset_bundle_config.json】
/// </summary>
public class BundleInfo
{
    /// <summary>
    /// 这个bundle的id
    /// </summary>
    public int bundle_id;

    /// <summary>
    /// 这个bundle的名字
    /// </summary>
    public string bundle_name;

    /// <summary>
    /// 这个bundle所包含的资源的路径列表
    /// </summary>
    public List<string> assets;
}

/// <summary>
/// 一个Asset数据 【用于序列化为json文件 asset_bundle_config.json】
/// </summary>
public class AssetInfo
{
    /// <summary>
    /// 这个资源的相对路径
    /// </summary>
    public string asset_path;

    /// <summary>
    /// 这个资源所属的bundle的id
    /// </summary>
    public int bundle_id;

    /// <summary>
    /// 这个资源所依赖的bundle列表的id
    /// </summary>
    public List<int> dependencies;
}

/// <summary>
/// BundleInfoSet对象 对应 整个json文件 asset_bundle_config.json
/// </summary>
public class BundleInfoSet
{
    public BundleInfoSet(int bundleCount, int assetCount)
    {
        bundles = new BundleInfo[bundleCount];
        assetInfos = new AssetInfo[assetCount];
    }

    public BundleInfoSet() { }

    /// <summary>
    /// bundle数组，数组的index即bundle的Id
    /// </summary>
    public BundleInfo[] bundles;

    /// <summary>
    /// asset 数组
    /// </summary>
    public AssetInfo[] assetInfos;

    /// <summary>
    /// 新增一个bundle记录
    /// </summary>
    /// <param name="bundleId">bundle的id</param>
    /// <param name="bundleInfo">bundle对象</param>
    public void AddBundle(int bundleId, BundleInfo bundleInfo)
    {
        bundles[bundleId] = bundleInfo;
    }

    /// <summary>
    /// 新增一个资源记录
    /// </summary>
    /// <param name="index"></param>
    /// <param name="assetInfo"></param>
    public void AddAsset(int index, AssetInfo assetInfo)
    {
        assetInfos[index] = assetInfo;
    }
    
    
}