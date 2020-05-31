using Ballance2.Managers.CoreBridge;
using Ballance2.UI;
using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.Managers
{
    /// <summary>
    /// 模组管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public class ModManager : BaseManagerBindable
    {
        public const string TAG = "ModManager";

        public ModManager() : base(TAG, "Singleton")
        {
        }

        public override bool InitManager()
        {
            gameMods = new List<GameMod>();
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_LOAD_FAILED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_LOAD_SUCCESS);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_REGISTERED);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_MOD_UNLOAD);
            InitModDebug();
            return true;
        }
        public override bool ReleaseManager()
        {
            if (gameMods != null)
            {
                foreach (GameMod gameMod in gameMods)
                    gameMod.Destroy();
                gameMods.Clear();
                gameMods = null;
            }
            return true;
        }

        #region 模组包管理

        // 所有模组包
        private List<GameMod> gameMods = null;

        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="packagePath">模组包路径</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包UID</returns>
        public int LoadGameMod(string packagePath, bool initialize = true)
        {
            GameMod mod = FindGameModByPath(packagePath);
            if (mod != null)
            {
                GameLogger.Warning(TAG, "Mod \"{0}\" already registered, skip", packagePath);
                return mod.Uid;
            }

            mod = new GameMod(packagePath, this);
            if(!gameMods.Contains(mod))
                gameMods.Add(mod);

            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_REGISTERED, "*", mod.Uid, mod);
            GameLogger.Log(TAG, "Register mod \"{0}\"", packagePath);

            if (initialize)
                mod.Load(this);

            return mod.Uid;
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
        public GameMod FindGameModByName(string packageName)
        {
            foreach (GameMod m in gameMods)
                if (m.PackageName == packageName)
                    return m;
            return null;
        }
        /// <summary>
        /// 通过包名查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        public GameMod[] FindAllGameModByName(string packageName)
        {
            List<GameMod> list = new List<GameMod>();
            foreach (GameMod m in gameMods)
                if (m.PackageName == packageName)
                    list.Add(m);
            return list.ToArray() ;
        }
        /// <summary>
        /// 通过UID查找模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns></returns>
        public GameMod FindGameMod(int modUid)
        {
            foreach (GameMod m in gameMods)
                if (m.Uid == modUid)
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

            int modUid = 0;
            if(int.TryParse(modStrIndef, out modUid))
                return FindGameMod(modUid);
             
            if(Regex.IsMatch(modStrIndef, "^([a-zA-Z]+[.][a-zA-Z]+)[.]*.*"))
                return FindGameModByName(modStrIndef);

            return FindGameModByPath(modStrIndef);
        }
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        public bool UnLoadGameMod(int modUid)
        {
            GameMod mod = FindGameMod(modUid);
            if (mod == null)
            {
                GameLogger.Warning(TAG, "无法卸载模组 (UID: {0}) ，因为没有加载", modUid);
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }

            mod.Destroy();
            gameMods.Remove(mod);
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_UNLOAD, "*", mod.Uid, mod);

            return true;
        }
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="mod">模组包实例</param>
        /// <returns>返回操作是否成功</returns>
        public bool UnLoadGameMod(GameMod mod)
        {
            mod.Destroy();
            gameMods.Remove(mod);
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_UNLOAD, "*", mod.Uid, mod);
            return true;
        }
        /// <summary>
         /// 初始化模组包
         /// </summary>
         /// <param name="modUid">模组包UID</param>
         /// <returns>返回操作是否成功</returns>
        public bool InitializeLoadGameMod(int modUid)
        {
            GameMod m = FindGameMod(modUid);
            if (m == null)
            {
                GameLogger.Warning(TAG, "无法初始化模组包 (UID: {0}) ，因为没有加载", modUid);
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }

            if (m.LoadStatus != GameModStatus.InitializeSuccess)
                m.Load(this);
            return true;
        }

        internal void OnModLoadFinished(GameMod m)
        {
            if (m.LoadStatus == GameModStatus.InitializeSuccess)
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_LOAD_SUCCESS, "*", m.Uid, m);
            else
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_LOAD_FAILED, "*", m.Uid, m, m.LoadFriendlyErrorExplain);
        }

        #endregion

        #region 模组包管理调试

        private UIManager UIManager;
        private DebugManager DebugManager;
        private UIWindow modManageWindow;
        private RectTransform modManagerView;
        private UICommonList modList;
        private Text TextModCount;

        private Sprite mod_icon_not_load;
        private Sprite mod_icon_default;
        private Sprite mod_icon_failed;
        private Sprite mod_icon_bad;

        private void InitModDebug()
        {
            UIManager = (UIManager)GameManager.GetManager(UIManager.TAG);
            DebugManager = (DebugManager)GameManager.GetManager(DebugManager.TAG);
            DebugManager.RegisterCommand("loadmod", OnCommandLoadMod, 1, "[packagePath:string] [initialize:true/false] 加载模组 [模组完整路径] [是否立即初始化]");
            DebugManager.RegisterCommand("unloadmod", OnCommandUnLoadMod, 1, "[packageUid:int]  加载模组 [模组UID]");
            DebugManager.RegisterCommand("initmod", OnCommandInitializeMod, 1, "[packageUid:int]  初始化模组 [模组UID]");
            DebugManager.RegisterCommand("modasset", OnCommandShowModAssets, 1, "[packageUid:int]  显示模组包内所有资源 [模组UID]");
            DebugManager.RegisterCommand("mods", OnCommandShowAllMod, 0, "显示所有模组");
            DebugManager.AddCustomDebugToolItem("模组管理器", new GameHandler(TAG, OnDebugToolItemClick));

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
            modManageWindow.CloseAsHide = true;
            modManageWindow.Hide();
            modList = modManagerView.transform.Find("UIScrollView/Viewport/Content").gameObject.GetComponent<UICommonList>();
            TextModCount = modManagerView.transform.Find("TextModCount").GetComponent<Text>();

            GameManager.GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_MOD_REGISTERED, "ModDebug", (evtName, param) =>
            {
                OnModAdded((GameMod)param[1]);
                return false;
            });
            GameManager.GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_MOD_UNLOAD, "ModDebug", (evtName, param) =>
            {
                OnModRemoved((GameMod)param[1]);
                return false;
            });
            GameManager.GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_MOD_LOAD_FAILED, "ModDebug", (evtName, param) =>
            {
                UpdateModListItemInfos(modList.GetItemById((int)param[0]), (GameMod)param[1]);
                return false;
            });
            GameManager.GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_MOD_LOAD_SUCCESS, "ModDebug", (evtName, param) =>
            {
                UpdateModListItemInfos(modList.GetItemById((int)param[0]), (GameMod)param[1]);
                return false;
            });
            GameManager.GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "ModDebug", (evtName, param) =>
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
        private void OnModAdded(GameMod mod)
        {
            UICommonList.CommonListItem newItem = modList.AddItem();
            newItem.id = mod.Uid;
            newItem.itemObject.name = mod.Uid.ToString();

            EventTriggerListener.Get(newItem.itemObject.transform.Find("Loaded").gameObject).onClick = (b) => OnLoadModStatus(mod);
            EventTriggerListener.Get(newItem.itemObject.transform.Find("Unload").gameObject).onClick = (b) => OnUnloadMod(mod);

            UpdateModListItemInfos(newItem, mod);
            UpdateModListCountInfos();
        }
        private void UpdateModListItemInfos(UICommonList.CommonListItem newItem, GameMod mod)
        {
            Text title = newItem.itemObject.transform.Find("Title").GetComponent<Text>();
            Text text = newItem.itemObject.transform.Find("Text").GetComponent<Text>();
            Text TextFailed = newItem.itemObject.transform.Find("TextFailed").GetComponent<Text>();
            Button Unload = newItem.itemObject.transform.Find("Unload").GetComponent<Button>();
            Image image = newItem.itemObject.transform.Find("Image").GetComponent<Image>();
            Toggle toggleOn = newItem.itemObject.transform.Find("Loaded").GetComponent<Toggle>();
            toggleOn.isOn = mod.LoadStatus == GameModStatus.InitializeSuccess;

            switch (mod.LoadStatus)
            {
                case GameModStatus.InitializeFailed:
                    image.sprite = mod_icon_failed;
                    toggleOn.gameObject.SetActive(false);
                    TextFailed.gameObject.SetActive(true);
                    Unload.gameObject.SetActive(true);
                    TextFailed.text = "初始化失败" ;
                    text.text = "错误信息：" + mod.LoadFriendlyErrorExplain;
                    title.text = mod.PackageName + "\n " + mod.PackagePath + "\n(" + mod.Uid + ")";
                    break;
                case GameModStatus.BadMod:
                    image.sprite = mod_icon_bad;
                    TextFailed.gameObject.SetActive(true);
                    Unload.gameObject.SetActive(true);
                    toggleOn.gameObject.SetActive(false);
                    TextFailed.text = "此模组与当前游戏版本不兼容";
                    text.text = "";
                    title.text = mod.ModInfo.Name + "\n( " + mod.PackageName + "/" + mod.Uid + "/" + mod.ModType + ")";
                    break;
                case GameModStatus.InitializeSuccess:
                    toggleOn.gameObject.SetActive(true);
                    TextFailed.gameObject.SetActive(false);
                    Unload.gameObject.SetActive(true);
                    if (mod.ModLogo != null) image.sprite = mod.ModLogo;
                    else image.sprite = mod_icon_default;
                    text.text = mod.ModInfo.Introduction + "\n作者：" + mod.ModInfo.Author + "   版本：" +
                        mod.ModInfo.Version;
                    title.text = mod.ModInfo.Name + "\n( " + mod.PackageName + "/" + mod.Uid + "/" + mod.ModType + ")";
                    break;
                case GameModStatus.NotInitialize:
                    image.sprite = mod_icon_not_load;
                    text.text = "模组还未初始化";
                    toggleOn.gameObject.SetActive(true);
                    TextFailed.gameObject.SetActive(false);
                    Unload.gameObject.SetActive(true);
                    title.text = mod.PackageName + "\n " + mod.PackagePath + "\n(" + mod.Uid + ")";
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
                loadedCount + " 个，" + failedCount + " 个加载失败，不兼容模组 " + badCount + " 个";
        }

        private int unloadModConfirmUid = 0;
        private int initModConfirmUid = 0;
        private GameMod currentModConfirm = null;

        private void OnModRemoved(GameMod mod)
        {
            UICommonList.CommonListItem oldItem = modList.GetItemById(mod.Uid);
            if(oldItem != null)
                modList.RemoveItem(oldItem);
            UpdateModListCountInfos();
        }
        private void OnUnloadMod(GameMod mod)
        {
            currentModConfirm = mod;
            unloadModConfirmUid = GameManager.UIManager.GlobalConfirm("您真的要卸载模组 " + mod.PackageName +
                " 吗？\n如果模块正在使用，强制卸载会导致资源丢失！", "警告", "确定卸载");
        }
        private void OnLoadModStatus(GameMod mod)
        {
            if (mod.LoadStatus == GameModStatus.NotInitialize)
            {
                currentModConfirm = mod;
                initModConfirmUid = GameManager.UIManager.GlobalConfirm("您是否要立即初始化模组 " + mod.PackageName +
                    " ?", "提示", "确定初始化");
            }
        }

        private bool OnDebugToolItemClick(string evn, params object [] param)
        {
            modManageWindow.Show();
            return false;
        }
        private bool OnCommandLoadMod(string keyword, string fullCmd, string[] args)
        {
            int newId = LoadGameMod(args[0], args.Length >= 2 ? args[1] == "true" : false);
            GameLogger.Log(TAG, "模组 UID : {0}", newId.ToString());
            return true;
        }
        private bool OnCommandUnLoadMod(string keyword, string fullCmd, string[] args)
        {
            int id = 0;
            if (!int.TryParse(args[0], out id))
            {
                GameLogger.Error(TAG, "Bad param 0 : {0}", args[0]);
                return false;
            }
            return UnLoadGameMod(id);
        }
        private bool OnCommandInitializeMod(string keyword, string fullCmd, string[] args)
        {
            int id = 0;
            if(!int.TryParse(args[0], out id))
            {
                GameLogger.Error(TAG, "Bad param 0 : {0}", args[0]);
                return false;
            }
            return InitializeLoadGameMod(id);
        }
        private bool OnCommandShowModAssets(string keyword, string fullCmd, string[] args)
        {
            int id = 0;
            if (!int.TryParse(args[0], out id))
            {
                GameLogger.Error(TAG, "Bad param 0 : {0}", args[0]);
                return false;
            }

            GameMod m = FindGameMod(id);
            if (m == null)
            {
                GameLogger.Error(TAG, "未找到模组 : {0}", id);
                return true;
            }

            if (m.LoadStatus == GameModStatus.InitializeSuccess && m.AssetBundle != null)
            {
                StringBuilder sb = new StringBuilder();
                string[] paths = m.AssetBundle.GetAllAssetNames();
                string[] scensePaths = m.AssetBundle.GetAllScenePaths();
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
                sb.Append("(");
                sb.Append(m.Uid);
                sb.Append(") ");
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
