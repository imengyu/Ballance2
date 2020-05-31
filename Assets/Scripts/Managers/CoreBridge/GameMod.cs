using Ballance2.Config;
using Ballance2.Utils;
using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

namespace Ballance2.Managers.CoreBridge
{
    /// <summary>
    /// 模组结构
    /// </summary>
    [CustomLuaClass]
    public class GameMod
    {
        private string TAG = "";

        public GameMod(string packagePath, ModManager modManager)
        {
            ModManager = modManager;
            Uid = CommonUtils.GenNonDuplicateID();
            PackagePath = packagePath;
            PackageName = "unknow." + Uid;
            TAG = ModManager.TAG + ":" + Uid;
            modInfo = new GameModInfo(GamePathManager.GetFileNameWithoutExt(PackagePath));
        }

        /// <summary>
        /// 模组 UID
        /// </summary>
        public int Uid { get; private set; }
        /// <summary>
        /// 名字路径
        /// </summary>
        public string PackagePath { get; private set; }
        /// <summary>
        /// 模组名称
        /// </summary>
        public string PackageName { get; private set; }

        private ModManager ModManager;

        /// <summary>
        /// 初始化模组包
        /// </summary>
        /// <param name="monoBehaviour">调用脚本</param>
        public void Load(MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(LoadInternal());
        }
        /// <summary>
        /// 释放模组包
        /// </summary>
        public void Destroy()
        {
            GameLogger.Log(ModManager.TAG, "Destroy mod package {0} uid: {1}", PackageName, Uid);
            if (modDefXmlDoc != null)
                modDefXmlDoc = null;
            if (mainLuaCodeLoaded)
                CallLuaFun("ModBeforeUnLoad", this);
            if (AssetBundle != null)
            {
                AssetBundle.Unload(true);
                AssetBundle = null;
            }
            DestroyLuaState();
            if (luaObjects != null)
            {
                //释放所有LUA 虚拟机
                foreach (GameLuaObjectHost o in luaObjects)
                {
                    o.enabled = false;
                    o.StopAllCoroutines();
                    UnityEngine.Object.Destroy(o);
                }
                luaObjects.Clear();
                luaObjects = null;
            }
        }
        //加载
        public IEnumerator LoadInternal()
        {
            //路径处理
            if (!Regex.IsMatch(PackagePath, "(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]")) {
                //不是URL，处理路径至mod文件夹路径
                if(!PackagePath.StartsWith(Application.streamingAssetsPath) && !GamePathManager.IsAbsolutePath(PackagePath))
                    PackagePath = GamePathManager.GetResRealPath("mod", PackagePath);
            }

            GameLogger.Log(ModManager.TAG, "Initialize mod package {0} , Full path : {1}", PackageName, PackagePath);

            //请求
            UnityWebRequest request = UnityWebRequest.Get(PackagePath);
            yield return request.SendWebRequest();

            if (!request.isNetworkError && string.IsNullOrEmpty(request.error))
            {
                AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(request.downloadHandler.data);
                yield return assetBundleCreateRequest;
                AssetBundle = assetBundleCreateRequest.assetBundle;

                if (AssetBundle == null)
                {
                    LoadFriendlyErrorExplain = "无效的资源包";
                    GameLogger.Error(ModManager.TAG, "加载 AssetBundle {0} 失败", PackagePath);
                    GameErrorManager.LastError = GameError.BadAssetBundle;
                    LoadStatus = GameModStatus.InitializeFailed;
                    ModManager.OnModLoadFinished(this);
                    yield break;
                }

                //读取定义文件
                TextAsset ModDef = AssetBundle.LoadAsset<TextAsset>("ModDef.xml");
                if (ModDef != null) { ReadModDef(ModDef); ModHasDefFile = true; }
                else { ModType = GameModType.AssetPack; ModHasDefFile = false; }

                //检查兼容性
                if (modCompatibilityInfo.MinVersion > GameConst.GameBulidVersion)
                {
                    GameLogger.Error(ModManager.TAG, "加载模组包 {0} 失败，模组包版本不兼容", PackageName);
                    GameErrorManager.LastError = GameError.BadMod;

                    LoadStatus = GameModStatus.BadMod;
                    LoadFriendlyErrorExplain = "模组包版本不兼";

                    ModManager.OnModLoadFinished(this);
                    yield break;
                }

                //判断包名是否重复
                if(ModHasDefFile)
                {
                    GameMod otherNod = null;
                    GameMod[] allNameMod = ModManager.FindAllGameModByName(PackageName);
                    if(allNameMod.Length > 1)
                    {
                        foreach(GameMod m in allNameMod)
                            if(m != this)
                            {
                                otherNod = m;
                                break;
                            }

                        GameLogger.Warning(ModManager.TAG, "模组包 {0} ({1} )，包名与  {0} ({1}) 冲突", PackageName, 
                            Uid, otherNod.PackagePath, otherNod.Uid);

                        GameErrorManager.LastError = GameError.ModConflict;

                        LoadStatus = GameModStatus.InitializeFailed;
                        LoadFriendlyErrorExplain = "模组包包名与已加载模组包冲突";

                        ModManager.OnModLoadFinished(this);
                    }
                }

                //获取图标
                if(!string.IsNullOrEmpty(modInfo.Logo))
                {
                    try {
                        Texture2D texture2D = GetAsset<Texture2D>(modInfo.Logo);
                        ModLogo = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), new Vector2(0.5f, 0.5f));
                    }
                    catch(Exception e)
                    {
                        ModLogo = null;
                        GameLogger.Error(ModManager.TAG, "在加载模组包 {0} 的 Logo {1} 失败\n错误信息：{2}", 
                            PackageName, modInfo.Logo, e.ToString());
                    }
                }

                LoadStatus = GameModStatus.InitializeSuccess;

                //修复 模块透明材质 Shader
                FixBundleShader();

                ModManager.OnModLoadFinished(this);
                yield break;
            }
            else
            {
                if (request.responseCode == 404)
                    LoadFriendlyErrorExplain = "未找到资源包";
                else if (request.responseCode == 403)
                    LoadFriendlyErrorExplain = "无权限读取资源包";
                else
                    LoadFriendlyErrorExplain = "HTTP 请求错误 " + request.responseCode;

                GameLogger.Error(ModManager.TAG, "加载模组包 {0} 失败\n错误信息：{1}", PackageName, request.error);
                GameErrorManager.LastError = GameError.NetworkError;
                LoadStatus = GameModStatus.InitializeFailed;

                ModManager.OnModLoadFinished(this);
                yield break;
            }
        }

        #region 公共属性

        /// <summary>
        /// 加载失败的友好错误信息
        /// </summary>
        public string LoadFriendlyErrorExplain { get; private set; }
        /// <summary>
        /// 加载失败的错误信息
        /// </summary>
        public string LoadError { get; private set; }
        /// <summary>
        /// 加载状态
        /// </summary>
        public GameModStatus LoadStatus { get; private set; }
        /// <summary>
        /// 资源包
        /// </summary>
        public AssetBundle AssetBundle { get; private set; }

        /// <summary>
        /// 模组图标
        /// </summary>
        public Sprite ModLogo { get; private set; } = null;
        /// <summary>
        /// 模组基础信息
        /// </summary>
        public GameModInfo ModInfo { get { return modInfo; } }
        /// <summary>
        /// 模组是否有定义文件
        /// </summary>
        public bool ModHasDefFile { get; private set; } = false;
        /// <summary>
        /// 模组定义文件
        /// </summary>
        public XmlDocument ModDefFile { get { return modDefXmlDoc; } }
        /// <summary>
        /// 模组类型
        /// </summary>
        public GameModType ModType { get; private set; } = GameModType.NotSet;
        /// <summary>
        /// 模组适配信息
        /// </summary>
        public GameCompatibilityInfo ModCompatibilityInfo { get { return modCompatibilityInfo; } }

        public string ModEntryCode { get; private set; }
        public LuaServer ModLuaServer { get; private set; }
        /// <summary>
        /// 模组 LUA 虚拟机
        /// </summary>
        public LuaServer.ModState ModLuaState { get; private set; }

        #endregion

        #region 模组信息读取

        private GameModInfo modInfo;
        private GameCompatibilityInfo modCompatibilityInfo = 
            new GameCompatibilityInfo(GameConst.GameBulidVersion, GameConst.GameBulidVersion, GameConst.GameBulidVersion);
        private XmlDocument modDefXmlDoc = new XmlDocument();

        //读取模组定义文件
        private void ReadModDef(TextAsset ModDef)
        {
            modDefXmlDoc = new XmlDocument();
            modDefXmlDoc.LoadXml(ModDef.text);

            XmlNode nodeMod = modDefXmlDoc.SelectSingleNode("Mod");
            foreach(XmlNode node in nodeMod.ChildNodes)
            {
                switch (node.Name)
                {
                    case "BaseInfo": ReadModDefBaseInfo(node);  break;
                    case "Compatibility": ReadModDefCompatibility(node); break;
                    case "ModType":
                        GameModType type = GameModType.NotSet;
                        if (Enum.TryParse(node.InnerText, true, out type)) ModType = type;
                        else GameLogger.Warning(TAG, "Bad ModType : {0}", node.InnerText);
                        break;
                    case "EntryCode": ModEntryCode = node.InnerText; break;
                    case "Define": break;
                    case "Data": break;
                }
            }
        }
        private void ReadModDefCompatibility(XmlNode nodeCompatibility)
        {
            foreach (XmlNode node in nodeCompatibility.ChildNodes)
            {
                switch (node.Name)
                {
                    case "MinVersion":
                        if (!int.TryParse(node.InnerText, out modCompatibilityInfo.MinVersion))
                            GameLogger.Warning(TAG, "Bad MinVersion : {0}", node.InnerText);
                        break;
                    case "TargetVersion":
                        if (!int.TryParse(node.InnerText, out modCompatibilityInfo.TargetVersion))
                            GameLogger.Warning(TAG, "Bad TargetVersion : {0}", node.InnerText);
                        break;
                    case "MaxVersion":
                        if (!int.TryParse(node.InnerText, out modCompatibilityInfo.MaxVersion))
                            GameLogger.Warning(TAG, "Bad MaxVersion : {0}", node.InnerText);
                        break;
                }
            }
        }
        private void ReadModDefBaseInfo(XmlNode nodeBaseInfo)
        {
            foreach (XmlAttribute attribute in nodeBaseInfo.Attributes)
            {
                switch(attribute.Name)
                {
                    case "packageName":
                        PackageName = attribute.Value;
                        break;
                }
            }
            foreach (XmlNode node in nodeBaseInfo.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Name": modInfo.Name = node.InnerText; break;
                    case "Author": modInfo.Author = node.InnerText; break;
                    case "Introduction": modInfo.Introduction = node.InnerText; break;
                    case "Logo": modInfo.Logo = node.InnerText; break;
                    case "Version": modInfo.Version = node.InnerText; break;
                }
            }
        }

        #endregion

        #region 模组操作

        //修复 模块透明材质 Shader
        private void FixBundleShader()
        {
            var materials = AssetBundle.LoadAllAssets<Material>();
            var standardShader = Shader.Find("Standard");
            int _SrcBlend = 0;
            int _DstBlend = 0;

            foreach (Material material in materials)
            {
                var shaderName = material.shader.name;
                if (shaderName == "Standard")
                {

                    _SrcBlend = material.renderQueue == 0 ? 0 : material.GetInt("_SrcBlend");
                    _DstBlend = material.renderQueue == 0 ? 0 : material.GetInt("_DstBlend");

                    if (_SrcBlend == (int)UnityEngine.Rendering.BlendMode.SrcAlpha
                        && _DstBlend == (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha)
                    {
                        material.shader = standardShader;
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                }
            }
        }

        #endregion

        #region LUA 虚拟机操作

        //管理当前模块下的所有lua虚拟脚本，统一管理、释放
        private List<GameLuaObjectHost> luaObjects = new List<GameLuaObjectHost>();

        /// <summary>
        /// 注册lua虚拟脚本到物体上
        /// </summary>
        /// <param name="name">lua虚拟脚本的名称</param>
        /// <param name="gameObject">要附加的物体</param>
        /// <param name="script">脚本代码资源</param>
        public void RegisterLuaObject(string name, GameObject gameObject, TextAsset script, string className)
        {
            GameLuaObjectHost newGameLuaObjectHost = gameObject.AddComponent<GameLuaObjectHost>();
            newGameLuaObjectHost.Name = name;
            newGameLuaObjectHost.GameMod = this;
            newGameLuaObjectHost.LuaScript = script;
            newGameLuaObjectHost.LuaState = ModLuaState;
            newGameLuaObjectHost.LuaClassName = className;
            luaObjects.Add(newGameLuaObjectHost);
        }
        /// <summary>
        /// 查找lua虚拟脚本
        /// </summary>
        /// <param name="name">lua虚拟脚本的名称</param>
        /// <param name="gameLuaObjectHost">输出lua虚拟脚本</param>
        /// <returns>返回是否找到对应脚本</returns>
        public bool FindLuaObject(string name, out GameLuaObjectHost gameLuaObjectHost)
        {
            foreach (GameLuaObjectHost luaObjectHost in luaObjects)
            {
                if (luaObjectHost.Name == name)
                {
                    gameLuaObjectHost = luaObjectHost;
                    return true;
                }
            }
            gameLuaObjectHost = null;
            return false;
        }
        //清除已释放的lua虚拟脚本
        internal void RemoveLuaObject(GameLuaObjectHost o)
        {
            luaObjects.Remove(o);
        }

        private bool mainLuaCodeLoaded = false;

        //初始化LUA虚拟机
        private void InitLuaState()
        {
            ModLuaServer = new LuaServer(PackageName);
            ModLuaServer.init(null, RunModExecutionCode);
            ModLuaState = ModLuaServer.getLuaState();
        }
        private void RunModExecutionCode()
        {
            if(!string.IsNullOrWhiteSpace(ModEntryCode))
            {
                TextAsset lua = GetTextAsset(ModEntryCode);
                if (lua == null) GameLogger.Warning(TAG, "未找到模组启动代码 {0} ", ModEntryCode);
                else
                {
                    mainLuaCodeLoaded = true;
                    ModLuaState.doString(lua.text);
                    CallLuaFun("ModulEntry", this);
                }
            }
        }
        private void DestroyLuaState()
        {
            if (ModLuaState != null)
            {
                ModLuaState.Dispose();
                ModLuaState = null;
            }
        }

        #region LUA 函数调用

        /// <summary>
        /// 获取当前 模块主代码 的指定函数
        /// </summary>
        /// <param name="funName">函数名</param>
        /// <returns>返回函数，未找到返回null</returns>
        public LuaFunction GetLuaFun(string funName)
        {
            return ModLuaState.getFunction(funName);
        }
        /// <summary>
        /// 调用模块主代码的lua无参函数
        /// </summary>
        /// <param name="funName">lua函数名称</param>
        public void CallLuaFun(string funName)
        {
            LuaFunction f = GetLuaFun(funName);
            if (f != null) f.call();
        }
        /// <summary>
        /// 调用模块主代码的lua函数
        /// </summary>
        /// <param name="funName">lua函数名称</param>
        /// <param name="pararms">参数</param>
        public void CallLuaFun(string funName, params object[] pararms)
        {
            LuaFunction f = GetLuaFun(funName);
            if (f != null) f.call(pararms);
        }
        /// <summary>
        /// 调用指定的lua虚拟脚本中的lua无参函数
        /// </summary>
        /// <param name="luaObjectName">lua虚拟脚本名称</param>
        /// <param name="funName">lua函数名称</param>
        public void CallLuaFun(string luaObjectName, string funName)
        {
            GameLuaObjectHost targetObject = null;
            if (FindLuaObject(luaObjectName, out targetObject))
                targetObject.CallLuaFun(funName);
        }
        /// <summary>
        /// 调用指定的lua虚拟脚本中的lua函数
        /// </summary>
        /// <param name="luaObjectName">lua虚拟脚本名称</param>
        /// <param name="funName">lua函数名称</param>
        /// <param name="pararms">参数</param>
        public void CallLuaFun(string luaObjectName, string funName, params object[] pararms)
        {
            GameLuaObjectHost targetObject = null;
            if (FindLuaObject(luaObjectName, out targetObject))
                targetObject.CallLuaFun(funName, pararms);
        }

        #endregion

        #endregion

        #region 资源读取

        /// <summary>
        /// 读取 AssetBundle 中的资源
        /// </summary>
        /// <param name="pathorname">资源路径</param>
        /// <returns></returns>
        public T GetAsset<T>(string pathorname) where T : UnityEngine.Object
        {
            if (AssetBundle == null)
            {
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }
            return AssetBundle.LoadAsset<T>(pathorname);
        }
        /// <summary>
        /// 读取 AssetBundle 中的文字资源
        /// </summary>
        /// <param name="pathorname">资源路径</param>
        /// <returns></returns>
        public TextAsset GetTextAsset(string pathorname)
        {
            if (AssetBundle == null)
            {
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }
            return AssetBundle.LoadAsset<TextAsset>(pathorname);
        }
        /// <summary>
        /// 读取 AssetBundle 中的 Prefab 资源
        /// </summary>
        /// <param name="pathorname">资源路径</param>
        /// <returns></returns>
        public GameObject GetPrefabAsset(string pathorname)
        {
            if (AssetBundle == null)
            {
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }
            return AssetBundle.LoadAsset<GameObject>(pathorname);
            
        }

        #endregion

    }

    /// <summary>
    /// 模组加载状态
    /// </summary>
    public enum GameModStatus
    {
        /// <summary>
        /// 已注册但未初始化
        /// </summary>
        NotInitialize,
        /// <summary>
        /// 初始化成功
        /// </summary>
        InitializeSuccess,
        /// <summary>
        /// 初始化失败
        /// </summary>
        InitializeFailed,
        /// <summary>
        /// 版本不兼容的模组
        /// </summary>
        BadMod,
    }
    /// <summary>
    /// 模组类型
    /// </summary>
    public enum GameModType
    {
        NotSet,
        /// <summary>
        /// 仅资源包
        /// </summary>
        AssetPack,
        /// <summary>
        /// 代码模组和资源包
        /// </summary>
        ModPack,
        /// <summary>
        /// 关卡文件
        /// </summary>
        Level,
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    public struct GameModInfo
    {
        public GameModInfo(string name)
        {
            Name = name;
            Author = "未知";
            Introduction = "未填写介绍";
            Logo = "";
            Version = "未知";
        }

        public string Name;
        public string Author;
        public string Introduction;
        public string Logo;
        public string Version;
    }
    /// <summary>
    /// 模组适配信息
    /// </summary>
    public struct GameCompatibilityInfo
    {
        public GameCompatibilityInfo(int mi, int t, int ma)
        {
            MinVersion = mi;
            TargetVersion = t;
            MaxVersion = ma;
        }

        public int MinVersion;
        public int TargetVersion;
        public int MaxVersion;
    }
}
