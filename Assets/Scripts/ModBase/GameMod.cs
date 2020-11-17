using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;
using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using Ballance2.Interfaces;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * GameMod.cs
 * 用途：
 * 模组基础承载结构
 * 
 * 作者：
 * mengyu
 * 
 * 更改历史：
 * 2020-1-1 创建
 *
 */

namespace Ballance2.ModBase
{
    /// <summary>
    /// 模组基础承载结构
    /// </summary>
    [CustomLuaClass]
    public class GameMod
    {
        private string _TAG = "";

        public GameMod(string packagePath, IModManager modManager, string packageName = null)
        {
            ModManager = modManager;
            PackagePath = packagePath;

            if (string.IsNullOrEmpty(packageName))
            {
                string fName = Path.GetFileNameWithoutExtension(packagePath);
                if (StringUtils.IsPackageName(fName)) PackageName = fName;
                else PackageName = "com.asset." + fName;
            }
            else PackageName = packageName;

            _TAG = "GameMod:" + PackageName;

            modInfo = new GameModInfo(GamePathManager.GetFileNameWithoutExt(PackagePath));
        }

        /// <summary>
        /// 名字标签
        /// </summary>
        public string TAG { get { return _TAG; } }
        /// <summary>
        /// 唯一id，内部使用
        /// </summary>
        public int Uid { get; } = CommonUtils.GenAutoIncrementID();

        /// <summary>
        /// 路径
        /// </summary>
        public string PackagePath { get; private set; }
        /// <summary>
        /// 模组名称
        /// </summary>
        public string PackageName { get; private set; }

        private IModManager ModManager;
        private bool hasMustDependenciesLoadFailed = false;
        private bool inited = false;

        /// <summary>
        /// 返回是否加载完成。该函数可用于WaitUntil
        /// </summary>
        /// <returns></returns>
        public bool IsLoadComplete()
        { 
            return LoadStatus != GameModStatus.Loading;
        }
        /// <summary>
        /// 返回是否加载成功
        /// </summary>
        /// <returns></returns>
        public bool IsInitSuccess()
        {
            return inited;
        }

        //初始化
        internal bool Init()
        {
            if (IsEditorPack) inited = true;
            if (inited) return true;
            if (FileUtils.TestFileIsZip(PackagePath))
            {
                inited = true;
                ModFileType = GameModFileType.BallanceZipPack;
                return LoadBaseInfo(); 
            }
            else if (FileUtils.TestFileIsAssetBundle(PackagePath))
            {
                ModType = GameModType.AssetPack;
                ModFileType = GameModFileType.AssetBundle;
                inited = true;
                return true;
            }

            //文件格式不支持
            GameErrorManager.LastError = GameError.BadFileType;
            GameLogger.Error(TAG, "Mod package not support {0}", PackagePath);
            return false;
        }

        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="monoBehaviour">调用脚本</param>
        public void Load(MonoBehaviour monoBehaviour)
        {
            if (LoadStatus == GameModStatus.Loading || LoadStatus == GameModStatus.InitializeSuccess)
                return;
            monoBehaviour.StartCoroutine(LoadInternal());
        }
        /// <summary>
        /// 加载模组包（协程）
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadInternal()
        {
            if (LoadStatus == GameModStatus.Loading || LoadStatus == GameModStatus.InitializeSuccess) 
                yield break;

            LoadStatus = GameModStatus.Loading;
            ModManager.CurrentLoadingMod.Add(this);

            GameLogger.Log(TAG, "Load mod {0}", PackageName);

            if (ModFileType == GameModFileType.AssetBundle)
            {
                //直接请求 AssetBundle
                UnityWebRequest request = UnityWebRequest.Get(PackagePath);
                yield return request.SendWebRequest();
              
                if (!request.isNetworkError && string.IsNullOrEmpty(request.error))
                {
                    AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(request.downloadHandler.data);
                    yield return assetBundleCreateRequest;
                    AssetBundle = assetBundleCreateRequest.assetBundle;

                    if (AssetBundle == null)
                    {
                        LoadErrorAndFinish("无效的资源包，加载 AssetBundle 失败", GameError.BadAssetBundle);
                        yield break;
                    }
                    else
                    {
                        TextAsset modDefTextAsset = AssetBundle.LoadAsset<TextAsset>("modDef.xml");
                        if (modDefTextAsset != null) 
                            ReadModDef(modDefTextAsset.text);
                        ModType = GameModType.AssetPack;
                    }
                }
                else
                {
                    if (request.responseCode == 404)
                        LoadFriendlyErrorExplain = "未找到资源包";
                    else if (request.responseCode == 403)
                        LoadFriendlyErrorExplain = "无权限读取资源包";
                    else
                        LoadFriendlyErrorExplain = "HTTP 请求错误 " + request.responseCode;

                    LoadErrorAndFinish("加载 AssetBundle 失败：" + LoadFriendlyErrorExplain + "\n" + request.error, 
                        GameError.NetworkError);
                    yield break;
                }
            }
            else if (ModFileType == GameModFileType.BallanceZipPack)
            {
                //从zip读取AssetBundle
                Task<MemoryStream> t = LoadAssetBundleToMemoryInZipAsync();
                yield return new WaitUntil(() => t.IsCompleted);

                MemoryStream ms = t.Result;
                if(ms == null)
                {
                    LoadErrorAndFinish("加载失败： AssetBundle 未找到 (1)", GameError.BrokenMod);
                    yield break;
                }
                //加载 AssetBundle
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(ms.ToArray());
                yield return createRequest;

                AssetBundle = createRequest.assetBundle;

                ms.Close();
                ms.Dispose();

                if (AssetBundle == null)
                {
                    LoadErrorAndFinish("加载失败： AssetBundle 未找到 (2)", GameError.BrokenMod);
                    yield break;
                }
                else
                {
                    TextAsset modDefTextAsset = AssetBundle.LoadAsset<TextAsset>("ModDef.xml");
                    if (modDefTextAsset == null)
                    {
                        LoadErrorAndFinish("加载失败： ModDef.xml 未找到 (2)", GameError.BrokenMod);
                        yield break;
                    }
                    else if (string.Compare(modDefTextAsset.text, ModDefInZip, true) != 0)
                    {
                        LoadErrorAndFinish("模组校验失败，文件可能被损坏", GameError.BrokenMod);
                        yield break;
                    }
                }
            }
            else
            {
                LoadFriendlyErrorExplain = "无效的资源包 模式未设置";
                LoadErrorAndFinish("无效的资源包, 模式未设置", GameError.BrokenMod);
                yield break;
            }

            yield return ((MonoBehaviour)ModManager).StartCoroutine(InitModBase());
        }
        /// <summary>
        /// 卸载
        /// </summary>
        public void UnLoad()
        {
            RunModBeforeUnLoadCode();

            GameManager.DestroyManagersInMod(PackageName);

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

            if (ModCSharpAssembly != null)
                ModCSharpAssembly = null;

            LoadStatus = GameModStatus.NotInitialize;
        }

        /// <summary>
        /// 释放模组包
        /// </summary>
        public void Destroy()
        {
            GameLogger.Log(TAG, "Destroy mod package {0}", PackageName);
            
            UnLoad();

            if (modDefXmlDoc != null) modDefXmlDoc = null;

            LoadStatus = GameModStatus.Destroyed;
        }
        /// <summary>
        /// 开始运行模组包
        /// </summary>
        public bool Run()
        {
            return RunModExecutionCode();
        }


        //一些加载函数

        //加载基础信息
        private bool LoadBaseInfo()
        {
            //在zip中加载ModDef
            ZipInputStream zip = ZipUtils.OpenZipFile(PackagePath);
            ZipEntry theEntry;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                if (theEntry.Name == "/ModDef.xml" || theEntry.Name == "ModDef.xml")
                    LoadModDefInZip(zip, theEntry);
                else if (theEntry.Name == "/" + ModInfo.Logo || theEntry.Name == ModInfo.Logo)
                    LoadLogoInZip(zip, theEntry);
            }
            zip.Close();
            zip.Dispose();

            if(!ModHasDefFile)
            {
                GameLogger.Error(TAG, "加载模组包失败，未找到 ModDef.xml");
                GameErrorManager.LastError = GameError.InitializationFailed;

                LoadStatus = GameModStatus.InitializeFailed;
                LoadFriendlyErrorExplain = "未找到 ModDef.xml";

                return false;
            }

            //检查兼容性
            if (modCompatibilityInfo.MinVersion > GameConst.GameBulidVersion)
            {
                GameLogger.Error(TAG, "加载模组包失败，模组包版本不兼容");
                GameErrorManager.LastError = GameError.BadMod;

                LoadStatus = GameModStatus.BadMod;
                LoadFriendlyErrorExplain = "模组包版本不兼";

                return false;
            }

            //判断包名是否重复
            if (ModHasDefFile)
            {
                GameMod otherNod = ModManager.FindGameMod(PackageName);
                if (otherNod != null)
                {
                    GameLogger.Warning(TAG, "模组包 {0} ({1} )，包名与  {0} ({1}) 冲突", PackageName,
                        PackagePath, otherNod.PackagePath, otherNod.PackagePath);

                    GameErrorManager.LastError = GameError.ModConflict;

                    LoadStatus = GameModStatus.InitializeFailed;
                    LoadFriendlyErrorExplain = "模组包包名与已加载模组包冲突";

                    return false;
                }
            }

            return true;
        }
        //在zip中加载AssetBundle
        private async Task<MemoryStream> LoadAssetBundleToMemoryInZipAsync()
        {
            ZipInputStream zip = ZipUtils.OpenZipFile(PackagePath);
            ZipEntry theEntry;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                if (theEntry.Name == PackageName + ".assetbundle"
                    || theEntry.Name == "/" + PackageName + ".assetbundle")
                {
                    MemoryStream ms = await ZipUtils.ReadZipFileToMemoryAsync(zip);
                    zip.Close();
                    zip.Dispose();
                    return ms;
                }
            }

            zip.Close();
            zip.Dispose();
            return null;
        }
        //模组初始化
        private IEnumerator InitModBase()
        {
            //加载依赖
            if (ModDependencyInfo.Count > 0)
            {
                yield return ((MonoBehaviour)ModManager).StartCoroutine(CheckAndLoadModDependencies());

                if (hasMustDependenciesLoadFailed)
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

            //模组代码环境初始化
            if (ModType == GameModType.ModulePack)
            {
                if (ModCodeType == GameModCodeType.Lua)//启动LUA 虚拟机
                {
                    //依赖需要全部加载
                    if (ModDependencyAllLoaded) InitLuaState();
                    else GameLogger.Warning(TAG, "模组包的依赖项没有全部加载，因此模组包不能运行");
                }
                else if (ModCodeType == GameModCodeType.CSharp)
                {
                    //加载C#程序集
#if ENABLE_MONO || ENABLE_DOTNET
                    TextAsset dll = GetTextAsset(ModEntryCode);
                    if(dll != null)
                    {
                        try
                        {
                            ModCSharpAssembly = Assembly.Load(dll.bytes);
                        }
                        catch (Exception e)
                        {
                            GameLogger.Exception(e);
                            GameLogger.Warning(TAG, (object)("模组包 " + PackageName + " ModEntryCode " + ModEntryCode + " 加载失败 : " + e.Message));
                        }
                    }
                    else GameLogger.Warning(TAG, "模组包 ModEntryCode {0} 未找到", ModEntryCode);
#else
                    GameLogger.Error(TAG, "模组包 {0} 加载代码错误：当前使用 IL2CPP 模式，因此 C# DLL 不能加载", PackageName);
#endif
                }
            }

            //修复 模块透明材质 Shader
            FixBundleShader();

            LoadStatus = GameModStatus.InitializeSuccess;
            LoadFinish();
            yield break;
        }
        //错误返回
        private void LoadErrorAndFinish(string err, GameError gameError)
        {
            GameErrorManager.SetLastErrorAndLog(gameError, TAG, err);
            LoadFriendlyErrorExplain = err;
            LoadStatus = GameModStatus.InitializeFailed;
            LoadFinish();
        }
        private void LoadFinish()
        {
            ModManager.OnModLoadFinished(this);
            ModManager.CurrentLoadingMod.Remove(this);
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
        /// 获取模组包是否直接在 Editor 加载
        /// </summary>
        public bool IsEditorPack { get; internal set; }
        /// <summary>
        /// 获取模组是否是在 Gameinit 就加载的（内核模块）
        /// </summary>
        public bool IsModInitByGameinit { get; internal set; }

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
        /// 模组文件类型
        /// </summary>
        public GameModFileType ModFileType { get; private set; } = GameModFileType.NotSet;
        /// <summary>
        /// 模组代码类型
        /// </summary>
        public GameModCodeType ModCodeType { get; private set; } = GameModCodeType.Lua;
        /// <summary>
        /// 模组运行范围
        /// </summary>
        public GameModRunMask ModRunType { get; private set; } = GameModRunMask.GameBase;
        /// <summary>
        /// 模组共享lua虚拟机
        /// </summary>
        /// <remarks>
        /// 如果你的模组分成多个模块，你可以仅用一个模块来运行代码，其他的模块作为附属模块，
        /// 填写 ModShareLuaState 为主模块包名，这样子模块可以和主模块共享运行环境。
        /// （默认情况下每个模块相互独立不能互相访问）
        /// </remarks>
        public string ModShareLuaState { get; private set; } = "";
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

        /// <summary>
        /// 模组的 c# dll （仅Standalone 和 非AOT 模式有效）
        /// </summary>
        public Assembly ModCSharpAssembly { get; private set; }
        /// <summary>
        /// 模组的 c# dll 入口类
        /// </summary>
        public object ModCSharpModEntry { get; private set; }

        /// <summary>
        /// 模组入口代码
        /// </summary>
        public string ModEntryCode { get; private set; }
        public LuaServer ModLuaServer { get; private set; }
        /// <summary>
        /// 模组 LUA 虚拟机
        /// </summary>
        public LuaState ModLuaState { get; private set; }

        //自定义数据

        private Dictionary<string, object> modCustomData = new Dictionary<string, object>();

        public object AddCustomProp(string name, object data)
        {
            if (modCustomData.ContainsKey(name))
            {
                modCustomData[name] = data;
                return data;
            }
            modCustomData.Add(name, data);
            return data;
        }
        public object GetCustomProp(string name)
        {
            if (modCustomData.ContainsKey(name))
                return modCustomData[name];
            return null;
        }
        public object SetCustomProp(string name, object data)
        {
            if (modCustomData.ContainsKey(name))
            {
                object old =  modCustomData[name];
                modCustomData[name] = data;
                return old;
            }
            return null;
        }
        public bool RemoveCustomProp(string name)
        {
            if (modCustomData.ContainsKey(name))
            {
                modCustomData.Remove(name);
                return true;
            }
            return false;
        }

        #endregion

        #region 模组信息读取

        private string ModDefInZip = "";

        private void LoadModDefInZip(ZipInputStream zip, ZipEntry theEntry)
        {
            MemoryStream ms = ZipUtils.ReadZipFileToMemory(zip);
            ModDefInZip = Encoding.UTF8.GetString(ms.ToArray());
            ReadModDef(ModDefInZip);
            ms.Close();
            ms.Dispose();
        }
        private void LoadLogoInZip(ZipInputStream zip, ZipEntry theEntry)
        {
            try
            {
                Texture2D texture2D = new Texture2D(128, 128);
                MemoryStream ms = ZipUtils.ReadZipFileToMemory(zip);
                texture2D.LoadImage(ms.ToArray());
                ms.Close();
                ms.Dispose();

                ModLogo = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                ModLogo = null;
                GameLogger.Error(TAG, "在加载模组包的 Logo {0} 失败\n错误信息：{1}", modInfo.Logo, e.ToString());
            }
        }

        private GameModInfo modInfo;
        private GameCompatibilityInfo modCompatibilityInfo =
            new GameCompatibilityInfo(GameConst.GameBulidVersion, GameConst.GameBulidVersion, GameConst.GameBulidVersion);
        private XmlDocument modDefXmlDoc = new XmlDocument();
        private List<GameDependencyInfo> modeDependencyInfos = new List<GameDependencyInfo>();

        //读取模组定义文件
        private void ReadModDef(string ModDefText)
        {
            try
            {
                modDefXmlDoc = new XmlDocument();
                modDefXmlDoc.LoadXml(ModDefText);

                XmlNode nodeMod = modDefXmlDoc.SelectSingleNode("Mod");
                foreach (XmlNode node in nodeMod.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "BaseInfo": ReadModDefBaseInfo(node); break;
                        case "Compatibility": ReadModDefCompatibility(node); break;
                        case "ModType":
                            {
                                GameModType type = GameModType.NotSet;
                                if (Enum.TryParse(node.InnerText, true, out type)) ModType = type;
                                else GameLogger.Warning(TAG, "Bad ModType : {0}", node.InnerText);
                                break;
                            }
                        case "ModCodeType":
                            {
                                GameModCodeType type = GameModCodeType.Lua;
                                if (Enum.TryParse(node.InnerText, true, out type)) ModCodeType = type;
                                else GameLogger.Warning(TAG, "Bad ModCodeType : {0}", node.InnerText);
                                break;
                            }
                        case "ModShareLuaState":
                            {
                                if (!StringUtils.IsPackageName(node.InnerText))
                                {
                                    GameLogger.Warning(TAG, "Bad ModShareLuaState : {0}", node.InnerText);
                                    break;
                                }
                                ModShareLuaState = node.InnerText;
                                GameDependencyInfo dependencyInfo = new GameDependencyInfo();
                                dependencyInfo.PackageName = ModShareLuaState;
                                dependencyInfo.MustLoad = false;
                                ModDependencyInfo.Add(dependencyInfo);
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
                        case "ModRunType":
                            {
                                GameModRunMask type = GameModRunMask.GameBase;
                                if (Enum.TryParse(node.InnerText, true, out type)) ModRunType = type;
                                else GameLogger.Warning(TAG, "Bad ModRunType : {0}", node.InnerText);
                                break;
                            }
                        case "Define": 

                            break;
                        default:
                            if (ModManager.ModDefCustomPropertySolver.ContainsKey(node.Name))
                                ModManager.ModDefCustomPropertySolver[node.Name](node, this);
                            break;
                    }
                }

                ModHasDefFile = true;
            }
            catch(Exception e)
            {
                GameLogger.Error(TAG, (object)("Failed to solve ModDef.xml : " + e.Message));
                GameLogger.Exception(e);
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
                switch (attribute.Name)
                {
                    case "packageName":
                        PackageName = attribute.Value;
                        _TAG = "GameMod:" + PackageName;
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
                    if (!string.IsNullOrEmpty(dependencyInfo.PackageName))
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
#if UNITY_EDITOR //editor 模式下修复一下透明shader
            if (AssetBundle == null)
                return;

            var materials = AssetBundle.LoadAllAssets<Material>();
            var standardShader = Shader.Find("Standard");
            if (standardShader == null)
                return;

            int _SrcBlend = 0;
            int _DstBlend = 0;

            foreach (Material material in materials)
            {
                var shaderName = material.shader.name;
                if (shaderName == "Standard")
                {
                    material.shader = standardShader;

                    _SrcBlend = material.renderQueue == 0 ? 0 : material.GetInt("_SrcBlend");
                    _DstBlend = material.renderQueue == 0 ? 0 : material.GetInt("_DstBlend");

                    if (_SrcBlend == (int)UnityEngine.Rendering.BlendMode.SrcAlpha
                        && _DstBlend == (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha)
                    {
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
#endif
        }
        /// <summary>
        /// 加载模组依赖
        /// </summary>
        private IEnumerator CheckAndLoadModDependencies()
        {
            GameLogger.Log(TAG, "Load mod package {0} dependencies : {1}", PackageName, ModDependencyInfo.Count);

            ModDependencyAllLoaded = true;
            hasMustDependenciesLoadFailed = false;
            int i = 0;
            foreach (GameDependencyInfo d in ModDependencyInfo)
            {
                i++;
                GameLogger.Log(TAG, "Load mod dependency : {0} ({1}/{2})", d.PackageName, i, ModDependencyInfo.Count);
                if (!d.Loaded)
                {
                    GameMod m = ModManager.FindGameMod(d.PackageName);
                    if (m == null)
                    {
                        //如果没有注册，则尝试从包名直接加载
                        m = ModManager.LoadGameModByPackageName(d.PackageName, false);
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
                            yield return ((MonoBehaviour)ModManager).StartCoroutine(m.LoadInternal());
                    }
                    else if (m != null && m.LoadStatus == GameModStatus.NotInitialize)//如果没有初始化则初始化
                        yield return ((MonoBehaviour)ModManager).StartCoroutine(m.LoadInternal());

                    if (m.LoadStatus != GameModStatus.InitializeSuccess
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
                            if (StringUtils.CompareTwoVersion(m.ModInfo.Version, d.MinVersion) < 0)
                            {
                                GameLogger.Error(TAG, "加载模组包 {0} 的依赖项 {1} 发生错误，" +
                                    "需要最低版本 {}，当前版本", PackageName, d.PackageName, d.MinVersion, m.ModInfo.Version);
                                hasMustDependenciesLoadFailed = true;
                            }
                        }
                    }
                }
            }
        }

        internal void ForceLoadEditorPack(TextAsset modDef)
        {
            ReadModDef(modDef.text);
            ((MonoBehaviour)ModManager).StartCoroutine(InitModBase());
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
        /// <param name="className">目标代码类名</param>
        public void RegisterLuaObject(string name, GameObject gameObject, string className)
        {
            GameLuaObjectHost newGameLuaObjectHost = gameObject.AddComponent<GameLuaObjectHost>();
            newGameLuaObjectHost.Name = name;
            newGameLuaObjectHost.GameMod = this;
            newGameLuaObjectHost.LuaState = ModLuaState;
            newGameLuaObjectHost.LuaClassName = className;
            newGameLuaObjectHost.LuaModName = PackageName;
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
            if (luaObjects != null)
                luaObjects.Remove(o);
        }
        internal void AddeLuaObject(GameLuaObjectHost o)
        {
            if (luaObjects != null && !luaObjects.Contains(o))
                luaObjects.Add(o);
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
            mainLuaCodeLoaded = false;
            requiredLuaFiles = new List<string>();
            requiredLuaClasses = new Dictionary<string, LuaFunction>();

            if (!string.IsNullOrEmpty(ModShareLuaState))
            {
                //如果指定了core，则代码载入到全局lua虚拟机中
                if(ModShareLuaState == "core")
                {
                    ModLuaState = GameManager.GameMainLuaState;
                    LuaStateInitFinished();
                    return;
                }

                GameMod m = ModManager.FindGameMod(ModShareLuaState);

                if(m != null && m.ModLuaState != null && m.luaStateInited)
                {
                    ModLuaState = m.ModLuaState;
                    LuaStateInitFinished();
                    return;
                }
                else GameLogger.Warning(TAG, "ModShareLuaState {0} can not be load because " +
                    "the target mod lua environment is not ready or not load. now use independent LuaServer", 
                    ModShareLuaState);
            }

            ModLuaServer = new LuaServer(PackageName);
            ModLuaState = ModLuaServer.getLuaState();
            ModLuaServer.init((i) => { }, LuaStateInitFinished);
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
            if (LoadStatus != GameModStatus.InitializeSuccess)
            {
                GameLogger.Warning(TAG, "RunModExecutionCode failed, Mod not initialize");
                GameErrorManager.LastError = GameError.NotInitialize;
                return false;
            }
            if (ModCodeType == GameModCodeType.Lua)
            {
                if (!string.IsNullOrWhiteSpace(ModEntryCode))
                {
                    if (!luaStateInited)
                    {
                        GameLogger.Warning(TAG, "RunModExecutionCode failed, Lua state not init, mod maybe cannot run");
                        GameErrorManager.LastError = GameError.ModCanNotRun;
                        return false;
                    }

                    TextAsset lua = GetTextAsset(ModEntryCode);
                    if (lua == null) GameLogger.Warning(TAG, "RunModExecutionCode failed, function {0} not found", ModEntryCode);
                    else if (!mainLuaCodeLoaded)
                    {
                        GameLogger.Log(TAG, "Run mod ModEntryCode {0} ", ModEntryCode);

                        try
                        {
                            mainLuaCodeLoaded = true;
                            ModLuaState.doString(lua.text, PackageName + ":Main");
                            requiredLuaFiles.Add(ModEntryCode);
                        }
                        catch (Exception e)
                        {
                            GameLogger.Warning(TAG, (object)("模组 " + PackageName + " 运行启动代码失败! " + e.Message));
                            GameLogger.Exception(e);
                            GameErrorManager.LastError = GameError.ModExecutionCodeRunFailed;
                            return false;
                        }

                        LuaFunction fModEntry = GetLuaFun("ModEntry");
                        if (fModEntry != null)
                        {
                            object b = fModEntry.call(this);
                            if (b is bool && !((bool)b))
                            {
                                GameLogger.Warning(TAG, "模组 {0} ModEntry 返回了错误", PackageName);
                                GameErrorManager.LastError = GameError.ModExecutionCodeRunFailed;
                                return (bool)b;
                            }
                            return false;
                        }
                        else
                        {
                            GameLogger.Warning(TAG, "模组 {0} 未找到 ModEntry ", PackageName);
                            GameErrorManager.LastError = GameError.FunctionNotFound;
                        }
                    }
                    else
                    {
                        GameLogger.Warning(TAG, "无法重复运行模组启动代码 {0} {1} ", ModEntryCode, PackageName);
                        GameErrorManager.LastError = GameError.AlredayRegistered;
                    }
                }
            }
            else if (ModCodeType == GameModCodeType.CSharp)
            {
                if(ModCSharpAssembly != null)
                {
                    Type type = ModCSharpAssembly.GetType("BallanceModEntry");
                    if(type == null)
                    {
                        GameLogger.Warning(TAG, "未找到 BallanceModEntry ");
                        GameErrorManager.LastError = GameError.ClassNotFound;
                        return false;
                    }

                    ModCSharpModEntry = Activator.CreateInstance(type);
                    MethodInfo methodInfo = type.GetMethod("ModEntry");  //根据方法名获取MethodInfo对象
                    if (type == null)
                    {
                        GameLogger.Warning(TAG, "未找到 ModEntry()");
                        GameErrorManager.LastError = GameError.FunctionNotFound;
                        return false;
                    }

                    object b = methodInfo.Invoke(ModCSharpModEntry, null);
                    if (b is bool && !((bool)b))
                    {
                        GameLogger.Warning(TAG, "模组 ModEntry 返回了错误");
                        GameErrorManager.LastError = GameError.ModExecutionCodeRunFailed;
                        return (bool)b;
                    }
                    return true;
                }
            }

            return false;
        }
        private void RunModBeforeUnLoadCode()
        {
            if (ModCodeType == GameModCodeType.Lua)
            {
                if (mainLuaCodeLoaded)
                    CallLuaFun("ModBeforeUnLoad", this);
            }
            else if (ModCodeType == GameModCodeType.CSharp)
            {
                if (ModCSharpAssembly != null)
                {
                    Type type = ModCSharpAssembly.GetType("BallanceModEntry");
                    MethodInfo methodInfo = type.GetMethod("ModBeforeUnLoad");
                    methodInfo.Invoke(ModCSharpModEntry, null);
                }
            }
        }
        private void DestroyLuaState()
        {
            if (requiredLuaFiles != null)
            {
                requiredLuaFiles.Clear();
                requiredLuaFiles = null;
            }
            if (requiredLuaClasses != null)
            {
                foreach (var v in requiredLuaClasses)
                    v.Value.Dispose();
                requiredLuaClasses.Clear();
                requiredLuaClasses = null;
            }
            if (ModLuaState != null)
            {
                ModLuaState.Dispose();
                ModLuaState = null;
            }
        }

        private List<string> requiredLuaFiles = null;
        private Dictionary<string, LuaFunction> requiredLuaClasses = null;

        /// <summary>
        /// 导入 Lua 类到当前模组虚拟机中。
        /// 注意，类函数以 “class_类名” 开头，
        /// 关于 Lua 类，请参考 Docs/LuaClass 。
        /// </summary>
        /// <param name="className">类名</param>
        /// <returns>类创建函数</returns>
        /// <exception cref="MissingReferenceException">
        /// 如果没有在当前模组包中找到类文件或是类创建函数 class_* ，则抛出 MissingReferenceException 异常。
        /// </exception>
        /// <exception cref="Exception">
        /// 如果Lua执行失败，则抛出此异常。
        /// </exception>
        public LuaFunction RequireLuaClass(string className)
        {
            LuaFunction classInit;
            if (requiredLuaClasses.TryGetValue(className, out classInit))
                return classInit;

            classInit = ModLuaState.getFunction("class_" + className);
            if (classInit != null)
            {
                requiredLuaClasses.Add(className, classInit);
                return classInit;
            }

            TextAsset lua = GetTextAsset(className);
            if (lua == null) lua = GetTextAsset(className + ".lua.txt");
            if (lua == null) lua = GetTextAsset(className + ".txt");
            if (lua == null)
                throw new MissingReferenceException(PackageName + " 无法导入 Lua class : " + className + " ,未找到该文件");

            try
            {
                ModLuaState.doString(lua.text, PackageName + ":" + className);
            }
            catch (Exception e)
            {
                GameLogger.Exception(e);
                GameErrorManager.LastError = GameError.ModExecutionCodeRunFailed;

                throw new Exception(PackageName + " 无法导入 Lua class : " + e.Message);
            }

            classInit = ModLuaState.getFunction("class_" + className);
            if (classInit == null)
            {
                throw new MissingReferenceException(PackageName + " 无法导入 Lua class : " + 
                    className + ", 未找到初始类函数: class_" + className);
            }

            requiredLuaClasses.Add(className, classInit);
            return classInit;
        }
        /// <summary>
        /// 导入Lua文件到当前模组虚拟机中
        /// </summary>
        /// <param name="fileName">LUA文件名</param>
        /// <exception cref="MissingReferenceException">
        /// 如果没有在当前模组包中找到类文件或是类创建函数 class_* ，则抛出 MissingReferenceException 异常。
        /// </exception>
        /// <exception cref="Exception">
        /// 如果Lua执行失败，则抛出此异常。
        /// </exception>
        public bool RequireLuaFile(string fileName)
        {
            if (requiredLuaFiles.Contains(fileName))
                return true;

            TextAsset lua = GetTextAsset(fileName);
            if (lua == null)
                lua = GetTextAsset(fileName + ".lua.txt");
            if (lua == null)
                throw new MissingReferenceException(PackageName + " 无法导入 Lua : " + fileName + " ,未找到该文件");

            try
            {
                ModLuaState.doString(lua.text, PackageName + ":" + GamePathManager.GetFileNameWithoutExt(fileName));
                requiredLuaFiles.Add(fileName);
            }
            catch (Exception e)
            {
                GameLogger.Exception(e);
                GameErrorManager.LastError = GameError.ModExecutionCodeRunFailed;

                throw new Exception(PackageName + " 无法导入 Lua class : " + e.Message);
            }

            return true;
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
#if UNITY_EDITOR
            if (IsEditorPack)
            {
                string p = pathorname.StartsWith("Assets/") ? pathorname :
                    (PackagePath + (pathorname.StartsWith("/") ? "" : "/") + pathorname);

                GameLogger.Log(TAG, "Load asset in editor : {0}", p);
                return AssetDatabase.LoadAssetAtPath<T>(p);
            }
#endif
            if (AssetBundle == null)
            {
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }

            return AssetBundle.LoadAsset<T>(pathorname);
        }
        /// <summary>
        /// 读取 模组包 中的文字资源
        /// </summary>
        /// <param name="pathorname">资源路径</param>
        /// <returns></returns>
        public TextAsset GetTextAsset(string pathorname)
        {
            return GetAsset<TextAsset>(pathorname);
        }
        /// <summary>
        /// 读取 模组包 中的 Prefab 资源
        /// </summary>
        /// <param name="pathorname">资源路径</param>
        /// <returns></returns>
        public GameObject GetPrefabAsset(string pathorname)
        {
            return GetAsset<GameObject>(pathorname);
            
        }
        public Texture GetTextureAsset(string pathorname)  {  return GetAsset<Texture>(pathorname); }
        public Texture2D GetTexture2DAsset(string pathorname) { return GetAsset<Texture2D>(pathorname); }
        public Sprite GetSpriteAsset(string pathorname) { return GetAsset<Sprite>(pathorname); }
        public Material GetMaterialAsset(string pathorname) { return GetAsset<Material>(pathorname); }
        public PhysicMaterial GetPhysicMaterialAsset(string pathorname) { return GetAsset<PhysicMaterial>(pathorname); }

        #endregion

    }
}
