using Ballance2.Config;
using Ballance2.Utils;
using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private string _TAG = "";

        public GameMod(string packagePath, ModManager modManager, string packageName = null)
        {
            ModManager = modManager;
            Uid = CommonUtils.GenNonDuplicateID();
            PackagePath = packagePath;
            if (string.IsNullOrEmpty(packageName))
            {
                PackageName = "unknow." + Uid;
                _TAG = ModManager.TAG + ":" + Uid;
            }
            else
            {
                PackageName = packageName;
                _TAG = ModManager.TAG + ":" + PackageName;
            }
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
        private bool hasMustDependenciesLoadFailed = false;

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
            if (mainLuaCodeLoaded)
                CallLuaFun("ModBeforeUnLoad", this);
            if (modDefXmlDoc != null)
                modDefXmlDoc = null;
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
        /// <summary>
        /// 开始运行模组包
        /// </summary>
        public bool Run()
        {
            return RunModExecutionCode();
        }
        /// <summary>
        /// 加载（协程）
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadInternal()
        {
            if (LoadStatus == GameModStatus.Loading
                || LoadStatus == GameModStatus.InitializeSuccess) yield break;

            LoadStatus = GameModStatus.Loading;

            //路径处理
            if (!StringUtils.IsUrl(PackagePath)) {
                //不是URL，处理路径至mod文件夹路径
                if(!PackagePath.StartsWith(Application.streamingAssetsPath) && !GamePathManager.IsAbsolutePath(PackagePath))
                    PackagePath = GamePathManager.GetResRealPath("mod", PackagePath);
            }

            GameLogger.Log(ModManager.TAG, "Initialize mod package {0} , Full path : {1}", PackageName, PackagePath);

            //当前
            ModManager.OnUpdateCurrentLoadingMod(this);

            //请求
            UnityWebRequest request = null;

            if (AssetBundle == null)
            {
                request = UnityWebRequest.Get(PackagePath);
                yield return request.SendWebRequest();
            }

            if (AssetBundle != null || (!request.isNetworkError && string.IsNullOrEmpty(request.error)))
            {
                if (AssetBundle == null)
                {
                    AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(request.downloadHandler.data);
                    yield return assetBundleCreateRequest;
                    AssetBundle = assetBundleCreateRequest.assetBundle;
                }
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
                        yield break;
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

                //加载依赖
                if (ModDependencyInfo.Count > 0)
                {
                    yield return ModManager.StartCoroutine(CheckAndLoadModDependencies());

                    if(hasMustDependenciesLoadFailed)
                    {
                        GameErrorManager.LastError = GameError.ModDependenciesLoadFailed;

                        LoadStatus = GameModStatus.InitializeFailed;
                        LoadFriendlyErrorExplain = "模组包的一个必要依赖项未能成功加载";

                        ModManager.OnModLoadFinished(this);
                        yield break;
                    }
                }
                else
                {
                    ModDependencyAllLoaded = true;
                    hasMustDependenciesLoadFailed = false;
                }

                //启动LUA 虚拟机
                if (ModType == GameModType.ModPack)
                {
                    //依赖需要全部加载
                    if(ModDependencyAllLoaded)  InitLuaState();
                    else GameLogger.Warning(TAG, "模组包 {0} 的依赖项没有全部加载，因此模组包不能运行", PackageName);
                }

                //修复 模块透明材质 Shader
                FixBundleShader();

                LoadStatus = GameModStatus.InitializeSuccess;

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

        public string TAG { get { return _TAG; } }
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
        /// <summary>
        /// 模组依赖信息
        /// </summary>
        public List<GameDependencyInfo> ModDependencyInfo { get { return modeDependencyInfos; } }
        /// <summary>
        /// 模组依赖是否已经全部加载
        /// </summary>
        public bool ModDependencyAllLoaded { get; private set; } = false;
        /// <summary>
        /// 模组启动代码自动调用时机
        /// </summary>
        public GameModEntryCodeExecutionAt ModEntryCodeExecutionAt { get; private set; } = GameModEntryCodeExecutionAt.Manual;

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
        private List<GameDependencyInfo> modeDependencyInfos = new List<GameDependencyInfo>();

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
                        {
                            GameModType type = GameModType.NotSet;
                            if (Enum.TryParse(node.InnerText, true, out type)) ModType = type;
                            else GameLogger.Warning(TAG, "Bad ModType : {0}", node.InnerText);
                            break;
                        }
                    case "EntryCode":
                        {
                            ModEntryCode = node.InnerText;
                            foreach (XmlAttribute attribute in node.Attributes)
                            {
                                switch (attribute.Name)
                                {
                                    case "executionAt":
                                        GameModEntryCodeExecutionAt executionAt = GameModEntryCodeExecutionAt.Manual;
                                        if (Enum.TryParse(attribute.Value, out executionAt))
                                            ModEntryCodeExecutionAt = executionAt;
                                        break;
                                }
                            }

                            break;
                        }
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
                        _TAG = ModManager.TAG + ":" + PackageName;
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
                    case "Dependencies": ReadModDefDependencies(node); break;
                }
            }
        }
        private void ReadModDefDependencies(XmlNode nodeBaseInfo)
        {
            modeDependencyInfos.Clear();
            foreach (XmlNode node in nodeBaseInfo.ChildNodes)
            {
                if (node.Name == "Package")
                {
                    GameDependencyInfo dependencyInfo = new GameDependencyInfo();
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        switch (attribute.Name)
                        {
                            case "name": dependencyInfo.PackageName = attribute.Value; break;
                            case "minVer": dependencyInfo.MinVersion = attribute.Value; break;
                            case "mustLoad": dependencyInfo.MustLoad = attribute.Value == "true"; break;
                        }
                    }
                    if(!string.IsNullOrEmpty(dependencyInfo.PackageName))
                        ModDependencyInfo.Add(dependencyInfo);
                }
            }
        }

        #endregion

        #region 模组操作

        /// <summary>
        /// 修复 模块透明材质 Shader
        /// </summary>
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
        /// <summary>
        /// 加载模组依赖
        /// </summary>
        private IEnumerator CheckAndLoadModDependencies()
        {
            GameLogger.Log(ModManager.TAG, "Lod mod package {0} dependencies : {1}", PackageName, ModDependencyInfo.Count);

            ModDependencyAllLoaded = true;
            hasMustDependenciesLoadFailed = false;
            int i = 0;
            foreach (GameDependencyInfo d in ModDependencyInfo)
            {
                i++;
                GameLogger.Log(ModManager.TAG, "Lod mod dependency : {0} ({1}/{2})", d.PackageName, i, ModDependencyInfo.Count);
                if (!d.Loaded)
                {
                    GameMod m = ModManager.FindGameModByName(d.PackageName);
                    if (m == null)
                    {
                        //如果没有注册，则尝试从包名直接加载
                        int uid = ModManager.LoadGameModByPackageName(d.PackageName, false);
                        m = ModManager.FindGameMod(uid);
                        if (m == null)
                        {
                            ModDependencyAllLoaded = false;
                            GameLogger.Error(TAG, "加载模组包 {0} 的依赖项 {1} 失败", PackageName, d.PackageName);

                            if (d.MustLoad)
                            {
                                hasMustDependenciesLoadFailed = true;
                                yield break;
                            }
                            continue;
                        }
                        else
                            yield return ModManager.StartCoroutine(m.LoadInternal());
                    }
                    else if (m != null && m.LoadStatus == GameModStatus.NotInitialize)//如果没有初始化则初始化
                       yield return ModManager.StartCoroutine(m.LoadInternal());

                    if(m.LoadStatus != GameModStatus.InitializeSuccess 
                        && m.LoadStatus != GameModStatus.Loading)
                    {
                        //模组依赖加载失败
                        ModDependencyAllLoaded = false;
                        GameLogger.Error(TAG, "加载模组包 {0} 的依赖项 {1} 失败", PackageName, d.PackageName);

                        if (d.MustLoad)
                        {
                            hasMustDependenciesLoadFailed = true;
                            yield break;
                        }
                    }
                    else
                    {
                        //如果设置了MinVersion，则还需要比较模组版本
                        if (!string.IsNullOrEmpty(d.MinVersion) && !string.IsNullOrEmpty(m.ModInfo.Version))
                        {
                           if( StringUtils.CompareTwoVersion(m.ModInfo.Version, d.MinVersion) < 0)
                            {
                                GameLogger.Error(TAG, "加载模组包 {0} 的依赖项 {1} 发生错误，" +
                                    "需要最低版本 {}，当前版本", PackageName, d.PackageName, d.MinVersion, m.ModInfo.Version);
                                hasMustDependenciesLoadFailed = true;
                            }
                        }
                    }

                    ModManager.OnUpdateCurrentLoadingMod(this);
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
        /// <param name="scclassNameript">目标代码类名</param>
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
        /// <summary>
        /// 获取模组启动代码是否已经执行
        /// </summary>
        /// <returns></returns>
        public bool GetModEntryCodeExecuted() { return mainLuaCodeLoaded; }

        private bool mainLuaCodeLoaded = false;
        private bool luaStateInited = false;

        //初始化LUA虚拟机
        private void InitLuaState()
        {
            ModLuaServer = new LuaServer(PackageName);
            ModLuaState = ModLuaServer.getLuaState();
            ModLuaServer.init(null, LuaStateInitFinished);
        }
        private void LuaStateInitFinished()
        {
            luaStateInited = true;
            if (ModEntryCodeExecutionAt == GameModEntryCodeExecutionAt.AfterLoaded)
                RunModExecutionCode();
        }
        /// <summary>
        /// 运行模组初始化代码
        /// </summary>
        /// <returns></returns>
        public bool RunModExecutionCode()
        {
            if(!string.IsNullOrWhiteSpace(ModEntryCode))
            {
                if(!luaStateInited)
                {
                    GameLogger.Warning(TAG, "Lua state not init, mod maybe cannot run");
                    GameErrorManager.LastError = GameError.ModCanNotRun;
                    return false;
                }

                TextAsset lua = GetTextAsset(ModEntryCode);
                if (lua == null) GameLogger.Warning(TAG, "未找到模组启动代码 {0} ", ModEntryCode);
                else if (!mainLuaCodeLoaded)
                {
                    GameLogger.Log(TAG, "Run mod ModEntryCode {0} ", ModEntryCode);

                    mainLuaCodeLoaded = true;
                    ModLuaState.doString(lua.text);
                    LuaFunction fModEntry = GetLuaFun("ModEntry");
                    if (fModEntry != null)
                    {
                        object b = fModEntry.call(this);
                        if (b is bool && !((bool)b))
                        {
                            GameLogger.Warning(TAG, "模组 {0} ModEntry 返回了错误", PackageName);
                            return (bool)b;
                        }
                        return false;
                    }
                    else
                    {
                        GameLogger.Warning(TAG, "模组 {0} 未找到 ModEntry ", PackageName);
                        GameErrorManager.LastError = GameError.AlredayRegistered;
                    }
                }
                else
                {
                    GameLogger.Warning(TAG, "无法重复运行模组启动代码 {0} {1} ", ModEntryCode, PackageName);
                    GameErrorManager.LastError = GameError.AlredayRegistered;
                }
            }
            return false;
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
            if (ModLuaState == null)
            {
                GameLogger.Warning(TAG, "GetLuaFun Failed because mod cannot run");
                GameErrorManager.LastError = GameError.ModCanNotRun;
                return null;
            }
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
            else GameLogger.Warning(TAG, "CallLuaFun Failed because function {0} not founnd", funName);
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
            else GameLogger.Warning(TAG, "CallLuaFun Failed because function {0} not founnd", funName);
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
            else GameLogger.Warning(TAG, "CallLuaFun Failed because object {0} not founnd", luaObjectName);
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
            else GameLogger.Warning(TAG, "CallLuaFun Failed because object {0} not founnd", luaObjectName);
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
        /// 正在加载
        /// </summary>
        Loading,
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
    /// 模组启动代码执行时机
    /// </summary>
    public enum GameModEntryCodeExecutionAt
    {
        /// <summary>
        /// 手动
        /// </summary>
        Manual,
        /// <summary>
        /// 模组加载完毕后
        /// </summary>
        AfterLoaded,
        /// <summary>
        /// 游戏加载完毕时
        /// </summary>
        AtStart,
        /// <summary>
        /// 关卡加载时
        /// </summary>
        AtLevelLoading,
        /// <summary>
        /// 关卡加载完成时
        /// </summary>
        AfterLevelLoad,
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
    /// <summary>
    /// 模组依赖信息
    /// </summary>
    public struct GameDependencyInfo
    {
        public GameDependencyInfo(string name, string mi)
        {
            MinVersion = mi;
            PackageName = name;
            Loaded = false;
            MustLoad = false;
        }

        public string PackageName;
        public string MinVersion;
        public bool Loaded;
        public bool MustLoad;
    }
}
