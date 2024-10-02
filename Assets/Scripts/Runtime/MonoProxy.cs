using ILRuntime.CLR.TypeSystem;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 在GameObject上挂载的真正的脚本对象MonoProxy
/// 运行时，会把逻辑执行转交给绑定的对应热更脚本对象ScriptObject
/// </summary>
public class MonoProxy : MonoBehaviour
{
    /// <summary>
    /// 当前这个MonoProxy对象映射的热更脚本的类型字符串
    /// </summary>
    public string ScriptName;

    /// <summary>
    /// 映射的热更脚本的类型的对象
    /// </summary>
    public object ScriptObject;

    /// <summary>
    /// 将本MonoProxy对象和一个热更脚本绑定在一起
    /// </summary>
    /// <param name="scriptName"></param>
    public void Bind(string scriptName)
    {
        ScriptName = "HotFix." + scriptName;

        ScriptObject = Startup.appdomain.Instantiate(ScriptName);

        IType scriptIType = Startup.appdomain.LoadedTypes[ScriptName];
        FieldInfo goField = scriptIType.ReflectionType.GetField("gameObject");
        goField.SetValue(ScriptObject, gameObject);

        Startup.appdomain.Invoke(ScriptName, "Awake", ScriptObject, null);
    }
    
    void Start()
    {
        Startup.appdomain.Invoke(ScriptName, "Start", ScriptObject, null);
    }

    void Update()
    {
        Startup.appdomain.Invoke(ScriptName, "Update", ScriptObject, null);
    }

    private void OnDestroy()
    {
        Startup.appdomain.Invoke(ScriptName, "OnDestroy", ScriptObject, null);
    }
}