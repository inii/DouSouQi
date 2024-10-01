using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    
    private GameObject oneSphere;
    
    IEnumerator Start()
    {
        yield return AssetLoader.Instance.LoadAssetBundleConfig();
        
        oneSphere = AssetLoader.Instance.Clone("Assets/GAssets/Prefabs/Sphere.prefab");
        
        yield return new WaitForSeconds(5.0f);
        
        Destroy(oneSphere);
    }
    
    public void Update()
    {
        AssetLoader.Instance.Update();
    }
    
}