using Ballance2.CoreBridge;
using Ballance2.ModBase;
using Ballance2.UI;
using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Ballance2.Interfaces;
using System.Xml;
using System.Collections;
using Ballance2.Config;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ballance2.Managers
{
    /// <summary>
    /// 模组管理器
    /// </summary>
    public class ModManager : BaseManager, IModManager
    {
        public const string TAG = "ModManager";

        public ModManager() : base("core.modmgr", TAG, "Singleton")
        {
            replaceable = false;
        }

        public override bool InitManager()
        {
            gameMods = new List<GameMod>();
            LoadModEnableStatusList();

            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_LOAD_FAILED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_LOAD_SUCCESS);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_REGISTERED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_UNLOAD);

            InitModDebug();
            return true;
        }
        public override bool ReleaseManager()
        {
            SaveModSettings();
            SaveModEnableStatusList();
            DestroyModEnableStatusList();
            if (gameMods != null)
            {
                foreach (GameMod gameMod in gameMods)
                    gameMod.Destroy();
                gameMods.Clear();
                gameMods = null;
            }
            return true;
        }

        private void Update()
        {
            if (flushModListTick >= 0)
            {
                flushModListTick--;
                if (flushModListTick == 0) 
                    DoFlushModList();
            }
        }

        #region 模组包管理

        // 所有模组包
        private List<GameMod> gameMods = null;
        private GameMod _CurrentLoadingMod = null;
        private bool _NoModMode = false;

        /// <summary>
        /// 获取所有已注册模组数
        /// </summary>
        /// <returns></returns>
        public int GetAllModCount() { return gameMods.Count; }
        /// <summary>
        /// 获取已经加载的模组数量
        /// </summary>
        /// <returns></returns>
        public int GetLoadedModCount()
        {
            int count = 0;
            foreach (GameMod m in gameMods)
                if (m.LoadStatus == GameModStatus.InitializeSuccess)
                    count++;
            return count;
        }

        /// <summary>
        /// 获取当前正在加载的模组
        /// </summary>
        public GameMod CurrentLoadingMod { get { return _CurrentLoadingMod; }  }

        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="packagePath">模组包路径</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包UID</returns>
        public GameMod LoadGameMod(string packagePath, bool initialize = true)
        {
            GameMod mod = FindGameModByPath(packagePath);
            if (mod != null)
            {
                GameLogger.Warning(TAG, "Mod \"{0}\" already registered, skip", packagePath);
                return mod;
            }

            //路径处理
            if (StringUtils.IsUrl(packagePath))
            {
                GameLogger.Error(TAG, "不支持从 URL 加载模组包 \"{0}\" ，请将其先下载至 streamingAssetsPath 后再加载。", packagePath);
                return null;
            }
            //处理路径至mod文件夹路径
            if (!File.Exists(packagePath) && !GamePathManager.IsAbsolutePath(packagePath))
                packagePath = GamePathManager.GetResRealPath("mod", packagePath);
            if (!File.Exists(packagePath))
            {
                GameLogger.Error(TAG, "Mod file \"{0}\" not exists", packagePath);
                return null;
            }
 
            mod = new GameMod(packagePath, this);
            if (!mod.Init()) 
                return null;
            if (!gameMods.Contains(mod))
                gameMods.Add(mod);

            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_REGISTERED, "*", mod.PackageName, mod);
            GameLogger.Log(TAG, "Register mod \"{0}\"", packagePath);

            if (initialize)
                mod.Load(this);

            return mod;
        }
        /// <summary>
        /// 通过包名加载模组包
        /// </summary>
        /// <param name="packageName">模组包包名</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包UID</returns>
        public GameMod LoadGameModByPackageName(string packageName, bool initialize = true)
        {
            GameMod mod = FindGameMod(packageName);
            if (mod != null)
            {
                GameLogger.Warning(TAG, "Mod \"{0}\" already registered, skip", packageName);
                return mod;
            }

            string pathInMods = GamePathManager.GetResRealPath("mod", packageName + ".ballance");
            string pathInCore = GamePathManager.GetResRealPath("core", packageName + ".ballance");

            string pathInEditorMods = GamePathManager.DEBUG_MOD_FOLDER + "/" + packageName;
            if (false) { }
#if UNITY_EDITOR
            //编辑器直接加载模组
            else if (DebugSettings.Instance.ModLoadInEditor && File.Exists(pathInEditorMods + "/ModDef.xml"))
                mod = LoadModInEditor(pathInEditorMods, packageName);
#endif
            else if (File.Exists(pathInMods)) mod = new GameMod(pathInMods, this, packageName);
            else if (File.Exists(pathInCore)) mod = new GameMod(pathInCore, this, packageName);
#if UNITY_EDITOR
            else if (DebugSettings.Instance.ModLoadInEditor == false && File.Exists(pathInEditorMods + "/ModDef.xml"))
                mod = LoadModInEditor(pathInEditorMods, packageName);
#endif
            else
            {
                GameLogger.Warning(TAG, "无法通过包名加载模组包 {0} ，未找到文件", packageName);
                GameErrorManager.LastError = GameError.FileNotFound;
                return null;
            }

            if (!mod.Init()) return null;
            if (!gameMods.Contains(mod))
                gameMods.Add(mod);

            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_REGISTERED, "*", mod.PackageName, mod);
            GameLogger.Log(TAG, "Register mod \"{0}\" {1}", packageName, (mod.IsEditorPack ? "(Editor Pack)" : ""));

            if (initialize) mod.Load(this);

            return mod;
        }
        /// <summary>
        /// 通过路径查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        public GameMod FindGameModByPath(string packagePath)
        {
            foreach (GameMod m in gameMods)
                if (m.PackagePath == packagePath)
                    return m;
            return null;
        }
        /// <summary>
        /// 通过包名查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        public GameMod FindGameMod(string packageName)
        {
            foreach (GameMod m in gameMods)
                if (m.PackageName == packageName)
                    return m;
            return null;
        }

        /// <summary>
        /// 通过资源定义字符串查找模组包
        /// </summary>
        /// <param name="modStrIndef">模组包UID或资源定义字符串</param>
        /// <returns></returns>
        public GameMod FindGameModByAssetStr(string modStrIndef)
        {
            if (modStrIndef.Contains(":") && !modStrIndef.StartsWith(":"))
                modStrIndef = modStrIndef.Substring(0, modStrIndef.IndexOf(':') - 1);
             
            if(StringUtils.IsPackageName(modStrIndef))
                return FindGameMod(modStrIndef);

            return FindGameModByPath(modStrIndef);
        }
        /// <summary>
        /// 移除模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        public bool UnLoadGameMod(string packageName)
        {
            GameMod mod = FindGameMod(packageName);
            if (mod == null)
            {
                GameLogger.Warning(TAG, "无法卸载模组 {0}，因为没有加载", packageName);
                GameErrorManager.LastError = GameError.NotRegister;
                return false;
            }

            mod.Destroy();
            gameMods.Remove(mod);
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_UNLOAD, "*", mod.PackageName, mod);

            return true;
        }
        /// <summary>
        /// 获取模组包是否正在加载
        /// </summary>
        /// <param name="packageName">模组包名</param>
        /// <returns>返回操作是否成功</returns>
        public bool IsGameModLoading(string packageName)
        {
            GameMod mod = FindGameMod(packageName);
            if (mod == null)
            {
                GameLogger.Warning(TAG, "无法卸载模组 (UID: {0}) ，因为没有加载", packageName);
                GameErrorManager.LastError = GameError.NotRegister;
                return false;
            }
            return mod.LoadStatus == GameModStatus.Loading;
        }
        /// <summary>
        /// 移除模组包
        /// </summary>
        /// <param name="mod">模组包实例</param>
        /// <returns>返回操作是否成功</returns>
        public bool UnLoadGameMod(GameMod mod)
        {
            mod.Destroy();
            gameMods.Remove(mod);
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_UNLOAD, "*", mod.PackageName, mod);
            return true;
        }

        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="m">模组包</param>
        /// <returns>返回操作是否成功</returns>
        public bool InitializeLoadGameMod(GameMod m)
        {
            if (m.LoadStatus != GameModStatus.InitializeSuccess)
                m.Load(this);
            return true;
        }
        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="modUid">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        public bool InitializeLoadGameMod(string modPackageName)
        {
            GameMod m = FindGameMod(modPackageName);
            if (m == null)
            {
                GameLogger.Warning(TAG, "无法初始化模组包 {0}，因为没有注册", modPackageName);
                GameErrorManager.LastError = GameError.NotRegister;
                return false;
            }
            return InitializeLoadGameMod(m);
        }
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid"></param>
        /// <returns></returns>
        public bool UnInitializeLoadGameMod(string modPackageName)
        {
            GameMod m = FindGameMod(modPackageName);
            if (m == null)
            {
                GameLogger.Warning(TAG, "无法卸载模组包 {0} ，因为没有注册", modPackageName);
                GameErrorManager.LastError = GameError.NotRegister;
                return false;
            }
            return UnInitializeLoadGameMod(m);
        }
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid"></param>
        /// <returns></returns>
        public bool UnInitializeLoadGameMod(GameMod m)
        {
            if (m.LoadStatus == GameModStatus.InitializeSuccess)
            {
                m.UnLoad();
                return true;
            }
            else
            {
                GameLogger.Warning(TAG, "无法卸载模组包 {0} ，因为没有加载", m.PackageName);
                GameErrorManager.LastError = GameError.NotInitialize;
                return false;
            }
        }

        /// <summary>
        /// 执行模组包代码
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        public bool RunGameMod(string packageName)
        {
            GameMod m = FindGameMod(packageName);
            if (m == null)
            {
                GameLogger.Warning(TAG, "无法执行化模组包 {0} ，因为没有加载", packageName);
                GameErrorManager.LastError = GameError.NotRegister;
                return false;
            }
            return RunGameMod(m);
        }
        /// <summary>
        /// 执行模组包代码
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        public bool RunGameMod(GameMod m)
        {
            if (m == null)
            {
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return false;
            }
            if (m.LoadStatus == GameModStatus.InitializeSuccess)
                return m.Run();

            GameLogger.Warning(TAG, "无法执行化模组包 {0}，因为没有初始化", m.PackageName);
            GameErrorManager.LastError = GameError.NotInitialize;
            return false;
        }

        private GameMod LoadModInEditor(string pathInEditorMods, string packageName)
        {
#if UNITY_EDITOR
            GameLogger.Warning(TAG, "LoadModInEditor : {0} ({1})", packageName, pathInEditorMods);
            GameMod mod = null;
            TextAsset modDef = AssetDatabase.LoadAssetAtPath<TextAsset>(pathInEditorMods + "/ModDef.xml");
            if (modDef == null)
            {
                GameLogger.Warning(TAG, "模组无效 {0} ，未找到 ModDef.xml ", pathInEditorMods);
                GameErrorManager.LastError = GameError.FileNotFound;
                return null;
            }

            mod = new GameMod(pathInEditorMods, this, packageName);
            mod.IsEditorPack = true;
            mod.ForceLoadEditorPack(modDef);
            return mod;
#else
            throw new System.Exception("LoadModInEditor can only use in editor mode!");
#endif
        }
        public void OnModLoadFinished(GameMod m)
        {
            if (m.LoadStatus == GameModStatus.InitializeSuccess)
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_LOAD_SUCCESS, "*", m.PackageName, m);
            else
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_LOAD_FAILED, "*", m.PackageName, m, m.LoadFriendlyErrorExplain);
        }

        public void ExecuteModEntry(GameModEntryCodeExecutionAt at)
        {
            foreach (GameMod m in gameMods)
            {
                if (m.ModType == GameModType.ModulePack
                    && m.ModEntryCodeExecutionAt == at
                    && !m.GetModEntryCodeExecuted())
                    m.RunModExecutionCode();
            }
        }

        #region 模组包启用管理

        private List<string> modEnableStatusList = null;
        private XmlDocument modEnableStatusListXml = null;
        private bool modEnableStatusListSaved = false;

        public string[] GetModEnableStatusList() { return modEnableStatusList.ToArray(); }
        public bool IsModEnabled(GameMod mod) { return IsModEnabled(mod.PackageName); }
        public bool IsModEnabled(string packageName) { return modEnableStatusList.Contains(packageName); }

        private void LoadModEnableStatusList()
        {
            modEnableStatusList = new List<string>();
            modEnableStatusListXml = new XmlDocument();
            string pathModStatus = Application.persistentDataPath + "/ModStatus.xml";
            if (File.Exists(pathModStatus))
            {
                StreamReader sr = new StreamReader(pathModStatus, Encoding.UTF8);
                modEnableStatusListXml.LoadXml(sr.ReadToEnd());
                sr.Close();
                sr.Dispose();
            }
            else//加载默认xml文档
                modEnableStatusListXml.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><ModConfig><ModList></ModList><NoModMode>False</NoModMode></ModConfig>");

            XmlNode nodeNoModMode = modEnableStatusListXml.SelectSingleNode("NoModMode");
            if (nodeNoModMode != null)
                _NoModMode = bool.Parse(nodeNoModMode.InnerText);
            XmlNode nodeModList = modEnableStatusListXml.SelectSingleNode("ModList");
            if (nodeModList != null)
            {
                foreach (XmlNode n in nodeModList)
                    modEnableStatusList.Add(n.InnerText);
            }

            modEnableStatusListSaved = true;
        }
        private void DestroyModEnableStatusList()
        {
            if (modEnableStatusListXml != null)
                modEnableStatusListXml = null;
            if (modEnableStatusList != null)
            {
                modEnableStatusList.Clear();
                modEnableStatusList = null;
            }
            modEnableStatusListSaved = true;
        }
        private void SaveModEnableStatusList()
        {
            if (!modEnableStatusListSaved)
            {
                StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/ModStatus.xml", false, Encoding.UTF8);

                XmlDocument xml = new XmlDocument();
                XmlNode nodeModConfig = xml.CreateElement("ModConfig");
                XmlNode nodeModList = xml.CreateElement("ModList");
                XmlNode nodeNoModMode = xml.CreateElement("NoModMode");

                xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));
                xml.AppendChild(nodeModConfig);
                nodeModConfig.AppendChild(nodeModList);
                nodeModConfig.AppendChild(nodeNoModMode);

                nodeNoModMode.InnerText = _NoModMode.ToString();
                foreach(string s in modEnableStatusList)
                {
                    XmlNode node = xml.CreateElement("Package"); 
                    node.InnerText = s;
                    nodeModList.AppendChild(node);
                }

                //save
                modEnableStatusListXml.Save(sw);
                sw.Close();
                sw.Dispose();

                modEnableStatusListSaved = true;
            }
        }

        //释放已经禁用的模组，加载启用的模组
        internal IEnumerator FlushModEnableStatus()
        {
            foreach(GameMod m in gameMods)
            {
                if(!m.IsModInitByGameinit)
                {
                    if (IsModEnabled(m.PackageName) && m.LoadStatus == GameModStatus.NotInitialize)
                        yield return StartCoroutine(m.LoadInternal());
                }
            }
            UnLoadNotUsedMod();
            yield break;
        }
        public void UnLoadNotUsedMod()
        {
            foreach (GameMod m in gameMods)
            {
                if (!m.IsModInitByGameinit)
                {
                    if (!IsModEnabled(m.PackageName) && m.LoadStatus == GameModStatus.InitializeSuccess)
                        m.UnLoad();
                }
            }
        }

        #endregion

        #endregion

        #region 模组包管理调试窗口

        private UIManager UIManager;
        private IDebugManager DebugManager;
        private UIWindow modManageWindow;
        private RectTransform modManagerView;
        private UICommonList modList;
        private Text TextModCount;
        private Toggle UIToggleHideCoreMod;

        private Sprite mod_icon_not_load;
        private Sprite mod_icon_default;
        private Sprite mod_icon_failed;
        private Sprite mod_icon_bad;

        private GameSettingsActuator settings;

        private bool hideCoreMod = true;
        private int flushModListTick = 0;

        private void InitModDebug()
        {
            UIManager = (UIManager)GameManager.GetManager(UIManager.TAG);
            DebugManager = (IDebugManager)GameManager.GetManager("DebugManager");
            DebugManager.RegisterCommand("loadmod", OnCommandLoadMod, 1, "[packagePath:string] [initialize:true/false] 加载模组 [模组完整路径] [是否立即初始化]");
            DebugManager.RegisterCommand("unloadmod", OnCommandUnLoadMod, 1, "[packageUid:int]  加载模组 [模组UID]");
            DebugManager.RegisterCommand("initmod", OnCommandInitializeMod, 1, "[packageUid:int]  初始化模组 [模组UID]");
            DebugManager.RegisterCommand("modasset", OnCommandShowModAssets, 1, "[packageUid:int]  显示模组包内所有资源 [模组UID]");
            DebugManager.RegisterCommand("mods", OnCommandShowAllMod, 0, "显示所有模组");
            DebugManager.AddCustomDebugToolItem("模组管理器", new GameHandler(TAG, OnDebugToolItemClick));

            settings = GameSettingsManager.GetSettings("core");
            hideCoreMod = settings.GetBool("modmgr.hideCoreMod", true);

            InitModManagementWindow();
        }
        private void InitModManagementWindow()
        {
            mod_icon_not_load = GameManager.FindStaticAssets<Sprite>("mod_icon_not_load");
            mod_icon_default = GameManager.FindStaticAssets<Sprite>("mod_icon_default");
            mod_icon_failed = GameManager.FindStaticAssets<Sprite>("mod_icon_failed");
            mod_icon_bad = GameManager.FindStaticAssets<Sprite>("mod_icon_bad");

            modManagerView = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIModManagement"), UIManager.UIRoot.transform).GetComponent<RectTransform>();
            modManageWindow = UIManager.CreateWindow("模组管理", modManagerView);
            modManageWindow.SetSize(500, 300);
            modManageWindow.SetMinSize(400, 250);
            modManageWindow.CloseAsHide = true;
            modManageWindow.Hide();
            modManageWindow.MoveToCenter();
            modManageWindow.onHide += (windowId) => { SaveModEnableStatusList(); };
            modManageWindow.onShow += (windowId) => { FlushModList(); };
            modList = modManagerView.transform.Find("UIScrollView/Viewport/Content").gameObject.GetComponent<UICommonList>();
            TextModCount = modManagerView.transform.Find("TextModCount").GetComponent<Text>();
            hideCoreMod = settings.GetBool("modmgr.hideCoreMod", true);
            UIToggleHideCoreMod = modManagerView.transform.Find("UIToggleHideCoreMod").GetComponent<Toggle>();
            UIToggleHideCoreMod.isOn = hideCoreMod;
            UIToggleHideCoreMod.onValueChanged.AddListener((b) =>
            {
                hideCoreMod = b;
                FlushModList();
            });

            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_MOD_REGISTERED, "ModDebug", (evtName, param) =>
            {
                OnModAdded((GameMod)param[1]);
                return false;
            });
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_MOD_UNLOAD, "ModDebug", (evtName, param) =>
            {
                OnModRemoved((GameMod)param[1]);
                return false;
            });
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_MOD_LOAD_FAILED, "ModDebug", (evtName, param) =>
            {
                GameMod m = (GameMod)param[1];
                UpdateModListItemInfos(modList.GetItemById(m.Uid), m);
                return false;
            });
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_MOD_LOAD_SUCCESS, "ModDebug", (evtName, param) =>
            {
                GameMod m = (GameMod)param[1];
                UpdateModListItemInfos(modList.GetItemById(m.Uid), m);
                return false;
            });
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "ModDebug", (evtName, param) =>
            {
                if (currentModConfirm != null)
                {
                    int id = (int)param[0];
                    bool confirm = (bool)param[1];
                    if (confirm)
                    {
                        if (id == unloadModConfirmUid)
                        {
                            UnLoadGameMod(currentModConfirm);
                            currentModConfirm = null;
                        }
                        else if (id == initModConfirmUid)
                        {
                            currentModConfirm.Load(this);
                            currentModConfirm = null;
                        }
                    }
                }
                return false;
            });

            InitModList();
        }
        private void InitModList()
        {
            foreach (GameMod m in gameMods)
                OnModAdded(m);
            UpdateModListCountInfos();
        }
        private void SaveModSettings()
        {
            settings.SetBool("modmgr.hideCoreMod", hideCoreMod);
        }

        private class ModListItemData
        {
            public GameMod Mod;
            public Text Title;
            public Text Text;

            public Text TextFailed;
            public Text TextStatus;

            public Toggle EnableStatus;
            public Text EnableStatusText;

            public Button Unload;
            public Image Image;
            public Image ImageSmall;
        } 

        private void UpdateModListItemInfos(UICommonList.CommonListItem newItem, GameMod mod)
        {
            ModListItemData modListItemData = (ModListItemData)newItem.data;

            modListItemData.EnableStatus.isOn = IsModEnabled(mod);
            modListItemData.Text.text = mod.ModInfo.Introduction + "\n作者：" + mod.ModInfo.Author + "   版本：" +
                        mod.ModInfo.Version;
            modListItemData.Title.text = mod.ModInfo.Name + "\n( " + mod.PackageName + "/" + mod.ModType + ")";
            modListItemData.Image.sprite = mod.ModLogo == null ? mod_icon_default : mod.ModLogo;

            if (mod.IsModInitByGameinit)
            {
                modListItemData.EnableStatus.isOn = true;
                modListItemData.EnableStatus.interactable = false;
                modListItemData.EnableStatusText.text = "基础模块";
            }

            switch (mod.LoadStatus)
            {
                case GameModStatus.InitializeFailed:
                    modListItemData.Image.sprite = mod_icon_failed;
                    modListItemData.TextFailed.gameObject.SetActive(true);
                    modListItemData.TextFailed.text = mod.LoadFriendlyErrorExplain;
                    modListItemData.TextStatus.text = "初始化失败";
                    modListItemData.ImageSmall.sprite = mod_icon_failed;
                    modListItemData.ImageSmall.gameObject.SetActive(true);
                    break;
                case GameModStatus.BadMod:
                    modListItemData.ImageSmall.sprite = mod_icon_bad;
                    modListItemData.ImageSmall.gameObject.SetActive(true);
                    modListItemData.TextFailed.gameObject.SetActive(true);
                    modListItemData.EnableStatus.gameObject.SetActive(false);
                    modListItemData.TextStatus.text = "";
                    modListItemData.TextFailed.text = "此模组与当前游戏版本不兼容";
                    break;
                case GameModStatus.InitializeSuccess:
                    modListItemData.TextFailed.gameObject.SetActive(false);
                    
                    modListItemData.ImageSmall.sprite = null;
                    modListItemData.TextStatus.text = "加载成功，已初始化";
                    if(mod.IsModInitByGameinit)
                    {
                        modListItemData.ImageSmall.gameObject.SetActive(true);
                        modListItemData.ImageSmall.sprite = mod_icon_default;
                    }
                    else modListItemData.ImageSmall.gameObject.SetActive(false);
                    break;
                case GameModStatus.NotInitialize:
                    modListItemData.ImageSmall.sprite = mod_icon_not_load;
                    modListItemData.ImageSmall.gameObject.SetActive(true);
                    modListItemData.TextStatus.text = "模组还未初始化";
                    modListItemData.TextFailed.gameObject.SetActive(false);
                    break;
                case GameModStatus.Loading:
                    modListItemData.ImageSmall.sprite = mod_icon_not_load;
                    modListItemData.ImageSmall.gameObject.SetActive(true);
                    modListItemData.TextStatus.text = "模组正在加载中";
                    modListItemData.TextFailed.gameObject.SetActive(false);
                    break;
            }
        
            UpdateModListCountInfos();
        }
        private void UpdateModListCountInfos()
        {
            int loadedCount = 0;
            int failedCount = 0;
            int badCount = 0;
            foreach (GameMod m in gameMods)
            {
                if (m.LoadStatus == GameModStatus.BadMod) badCount++;
                if (m.LoadStatus == GameModStatus.InitializeFailed) failedCount++;
                if (m.LoadStatus == GameModStatus.InitializeSuccess) loadedCount++;
            }

            TextModCount.text = "共 " + gameMods.Count + " 个模组，已加载" +
                loadedCount + " 个，" + failedCount + " 个加载失败，不兼容 " + badCount + " 个";
        }
        private void FlushModList()
        {
            flushModListTick = 5;
        }
        private void DoFlushModList()
        {
            foreach (UICommonList.CommonListItem item in modList.List)
            {
                GameMod mod = ((ModListItemData)item.data).Mod;
                if (mod.IsModInitByGameinit)
                    item.visible = !hideCoreMod;
                UpdateModListItemInfos(item, mod);
            }

            UpdateModListCountInfos();
            modList.Relayout();
        }

        private int unloadModConfirmUid = 0;
        private int initModConfirmUid = 0;
        private GameMod currentModConfirm = null;

        private void OnModAdded(GameMod mod)
        {
            ModListItemData modListItemData = new ModListItemData();

            UICommonList.CommonListItem newItem = modList.AddItem();
            newItem.id = mod.Uid;
            newItem.itemObject.name = mod.PackageName;
            newItem.data = modListItemData;

            modListItemData.Mod = mod;
            modListItemData.Title = newItem.itemObject.transform.Find("Title").GetComponent<Text>();
            modListItemData.Text = newItem.itemObject.transform.Find("Text").GetComponent<Text>();
            modListItemData.TextFailed = newItem.itemObject.transform.Find("TextFailed").GetComponent<Text>();
            modListItemData.TextStatus = newItem.itemObject.transform.Find("TextStatus").GetComponent<Text>();
            modListItemData.Unload = newItem.itemObject.transform.Find("Unload").GetComponent<Button>();
            modListItemData.Image = newItem.itemObject.transform.Find("Image").GetComponent<Image>();
            modListItemData.EnableStatus = newItem.itemObject.transform.Find("EnableStatus").GetComponent<Toggle>();
            modListItemData.ImageSmall = newItem.itemObject.transform.Find("ImageSmall").GetComponent<Image>();
            modListItemData.EnableStatusText = newItem.itemObject.transform.Find("EnableStatus/Label").GetComponent<Text>();

            //Events
            modListItemData.Unload.onClick.AddListener(() => OnUnloadModClicked(mod));
            EventTriggerListener.Get(modListItemData.EnableStatus.gameObject).onClick = (b) => OnEnableStatusClicked(mod, newItem);

            UpdateModListItemInfos(newItem, mod);
            UpdateModListCountInfos();
        }
        private void OnModRemoved(GameMod mod)
        {
            UICommonList.CommonListItem oldItem = modList.GetItemById(mod.Uid);
            if(oldItem != null)
                modList.RemoveItem(oldItem);
            oldItem.data = null;
            UpdateModListCountInfos();
        }

        private void OnUnloadModClicked(GameMod mod)
        {
            currentModConfirm = mod;
            unloadModConfirmUid = GameManager.UIManager.GlobalConfirmWindow("您真的要立即卸载模组 " + mod.PackageName +
                " 吗？\n如果模块资源正在使用，强制卸载会导致物体丢失！", "警告", "确定卸载");
        }
        private void OnEnableStatusClicked(GameMod mod, UICommonList.CommonListItem newItem)
        {
            modEnableStatusListSaved = false;
            if (((ModListItemData)newItem.data).EnableStatus.isOn) modEnableStatusList.Add(mod.PackageName);
            else modEnableStatusList.Remove(mod.PackageName);
        }

        private bool OnDebugToolItemClick(string evn, params object [] param)
        {
            modManageWindow.Show();
            return false;
        }
        private bool OnCommandLoadMod(string keyword, string fullCmd, string[] args)
        {
            GameMod mod = LoadGameMod(args[0], args.Length >= 2 ? args[1] == "true" : false);
            if(mod == null)
            {
                GameLogger.Warning(TAG, "模组 {0} 加载失败", args[0]);
                return false;
            }
            GameLogger.Log(TAG, "模组已加载 {0} ({1})", mod.PackageName, mod.Uid);
            return true;
        }
        private bool OnCommandUnLoadMod(string keyword, string fullCmd, string[] args)
        {
            GameMod mod = FindGameModByAssetStr(args[0]);
            if (mod == null)
            {
                GameLogger.Error(TAG, "未找到模组 {0} ", args[0]);
                return false;
            }
            return UnLoadGameMod(mod);
        }
        private bool OnCommandInitializeMod(string keyword, string fullCmd, string[] args)
        {
            GameMod mod = FindGameModByAssetStr(args[0]);
            if (mod == null)
            {
                GameLogger.Error(TAG, "未找到模组 {0} ", args[0]);
                return false;
            }
            return InitializeLoadGameMod(mod);
        }
        private bool OnCommandShowModAssets(string keyword, string fullCmd, string[] args)
        {
            GameMod mod = FindGameModByAssetStr(args[0]);
            if (mod == null)
            {
                GameLogger.Error(TAG, "未找到模组 {0} ", args[0]);
                return false;
            }

            if (mod.LoadStatus == GameModStatus.InitializeSuccess && mod.AssetBundle != null)
            {
                StringBuilder sb = new StringBuilder();
                string[] paths = mod.AssetBundle.GetAllAssetNames();
                string[] scensePaths = mod.AssetBundle.GetAllScenePaths();
                sb.Append("\n<size=16>Asset Names:  </size>(" + paths .Length + ")\n");
                foreach (string s in paths)
                {
                    sb.Append(s);
                    sb.Append("\n");
                }
                sb.Append("<size=16>Scene Paths:  </size>(" + scensePaths.Length + ")\n");
                foreach (string s in scensePaths)
                {
                    sb.Append(s);
                    sb.Append("\n");
                }
                GameLogger.Log(TAG, sb.ToString());
            }
            else GameLogger.Error(TAG, "指定模组未初始化或初始化失败");

            return true;
        }
        private bool OnCommandShowAllMod(string keyword, string fullCmd, string[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<size=16>All mods:</size>(" + gameMods.Count + ")\n");
            foreach (GameMod m in gameMods)
            {
                sb.Append(m.PackageName);
                sb.Append("\n<color=#808080>");
                sb.Append(m.PackagePath);
                sb.Append("</color>");
                sb.Append("\n    LoadStatus: ");
                sb.Append(m.LoadStatus);
                sb.Append("\n    LoadFriendlyErrorExplain: ");
                sb.Append(m.LoadFriendlyErrorExplain);
                sb.Append("\n    LoadError: ");
                sb.Append(m.LoadError);
                sb.Append("\n ");
            }
            GameLogger.Log(TAG, sb.ToString());
            return true;
        }

        #endregion
    }
}
