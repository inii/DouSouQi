using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Startup : MonoBehaviour
{
    public static ILRuntime.Runtime.Enviorment.AppDomain appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();

    void Start()
    {
        StartCoroutine(LoadILRuntime());
    }
    
    IEnumerator LoadILRuntime()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(StreamingAssetsPath("HotFix.dll.txt"));
        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            yield break;
        }
        byte[] dll = webRequest.downloadHandler.data;
        webRequest.Dispose();
    
        webRequest = UnityWebRequest.Get(StreamingAssetsPath("HotFix.pdb.txt"));
        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            yield break;
        }
        byte[] pdb = webRequest.downloadHandler.data;
        webRequest.Dispose();
    
        appdomain.LoadAssembly(new MemoryStream(dll), new MemoryStream(pdb), new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
    
        OnILRuntimeInitialized();
    }
    
    void OnILRuntimeInitialized()
    {
        appdomain.Invoke("HotFix.AppMain", "Start", null, null);
    }
    
    public string StreamingAssetsPath(string fileName)
    {
        string path = Application.streamingAssetsPath + "/" + fileName;
        
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        path = "file://" + path;
#endif
        
        return path;
    }

}