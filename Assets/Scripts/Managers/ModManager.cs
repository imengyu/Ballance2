using Ballance2.Managers.CoreBridge;
using Ballance2.Utils;
using System.Collections.Generic;

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
            InitModDebug();
            return true;
        }
        public override bool ReleaseManager()
        {
            if(gameMods != null)
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
            gameMods.Add(mod);

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
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        public bool UnLoadGameMod(int modUid)
        {
            GameMod m = FindGameMod(modUid);
            if(m == null)
            {
                GameLogger.Warning(TAG, "无法卸载模组 (UID: {0}) ，因为没有加载", modUid);
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }

            m.Destroy();
            gameMods.Remove(m);
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
            if(m.LoadStatus == GameModStatus.InitializeSuccess)
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_LOAD_SUCCESS, "*", m.Uid, m);
            else
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_MOD_LOAD_FAILED, "*", m.Uid, m, m.LoadFriendlyErrorExplain);
        }

        #endregion

        #region 模组包管理调试

        private DebugManager DebugManager;

        private void InitModDebug()
        {
            DebugManager = (DebugManager)GameManager.GetManager(DebugManager.TAG);
            DebugManager.RegisterCommand("loadmod", OnCommandLoadMod, 1, "[packagePath:string] [initialize:true/false] 加载模组 [模组完整路径] [是否立即初始化]");
            DebugManager.RegisterCommand("unloadmod", OnCommandUnLoadMod, 1, "[packageUid:int]  加载模组 [模组UID]");
            DebugManager.RegisterCommand("initmod", OnCommandInitializeMod, 1, "[packageUid:int]  初始化模组 [模组UID]");
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

        #endregion
    }
}
