using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyTest : MonoBehaviour
{
    

    /// <summary>
    /// 通过asset路径找到AssetRef对象
    /// </summary>
    public Dictionary<string, AssetRef> Path2AssetRef = new Dictionary<string, AssetRef>();

    private UnityWebRequest request;
    
    private void Start()
    {
        StartCoroutine(LoadAssetBundleConfig());
    }
    
    /// <summary>
    /// 1. 加载AssetBundleConfig的json文件
    /// 2. 填充Path2AssetRef容器
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadAssetBundleConfig()
    {
         string txtTest = "file://" + Application.streamingAssetsPath + "/test.txt";
// Debug.Log("开始加载 配置文件 " + txtTest);
        // 创建一个本地文件请求
        request = UnityWebRequest.Get(txtTest);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
            yield break;
        }

        // 读取下载的内容
        string allText = request.downloadHandler.text;
        
        Debug.Log("配置文件内容：" + allText); 

        // 解析 JSON 并填充 Path2AssetRef
        ParseJsonAndFillPath2AssetRef(allText);
    }

    private void ParseJsonAndFillPath2AssetRef(string json)
    {
        // 假设这里有解析 JSON 的代码，并填充 Path2AssetRef
        // 示例：Path2AssetRef = JsonUtility.FromJson<Dictionary<string, AssetRef>>(json);
    }

    private void OnDisable()
    {
        request?.Dispose();
        // 确保在组件禁用时释放资源
        
    }
}
