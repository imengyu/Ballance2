using Ballance2.Utils;
using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Ballance2.Managers.CoreBridge
{
    /// <summary>
    /// 模组结构
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameMod
    {
        public GameMod(string packagePath, ModManager modManager)
        {
            ModManager = modManager;
            Uid = CommonUtils.GenNonDuplicateID();
            PackagePath = packagePath;
            PackageName = GamePathManager.GetFileNameWithoutExt(packagePath);
        }

        public int Uid { get; private set; }
        public string PackagePath { get; private set; }
        public string PackageName { get; private set; }

        private ModManager ModManager;

        /// <summary>
        /// 初始化模组包
        /// </summary>
        /// <param name="monoBehaviour">调用脚本</param>
        public void Load(MonoBehaviour monoBehaviour)
        {
            GameLogger.Log(ModManager.TAG, "Initialize mod package {0} , Full path : {1}", PackageName, PackagePath);
            monoBehaviour.StartCoroutine(LoadInternal());
        }
        /// <summary>
        /// 释放模组包
        /// </summary>
        public void Destroy()
        {
            GameLogger.Log(ModManager.TAG, "Destroy mod package {0} uid: {1}", PackageName, Uid);

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
        private IEnumerator LoadInternal()
        {
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
                    ModManager.OnModLoadFinished(this);
                    yield break;
                }

                LoadStatus = GameModStatus.InitializeSuccess;

                //读取定义文件
                TextAsset ModDef = AssetBundle.LoadAsset<TextAsset>("ModDef.xml");
                if (ModDef != null) { ReadModDef(ModDef); ModHasDefFile = true; }
                else { ModType = GameModType.AssetPack; ModHasDefFile = false; }

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
                ModManager.OnModLoadFinished(this);
                yield break;
            }
        }

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
        /// 模组是否有定义文件
        /// </summary>
        public bool ModHasDefFile { get; private set; } = false;
        /// <summary>
        /// 模组类型
        /// </summary>
        public GameModType ModType { get; private set; } = GameModType.NotSet;

        /// <summary>
        /// 模组 LUA 虚拟机壳
        /// </summary>
        public LuaServer ModLuaServer { get; private set; }
        /// <summary>
        /// 模组 LUA 虚拟机
        /// </summary>
        public LuaServer.ModState ModLuaState { get; private set; }

        private void ReadModDef(TextAsset ModDef)
        {

        }
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

        //初始化LUA虚拟机
        private void InitLuaState()
        {
            ModLuaServer = new LuaServer(PackageName);
            ModLuaServer.init(null, RunModExecutionCode);
            ModLuaState = ModLuaServer.getLuaState();
        }
        private void RunModExecutionCode()
        {

        }
        private void DestroyLuaState()
        {
            if (ModLuaState != null)
            {
                ModLuaState.Dispose();
                ModLuaState = null;
            }
        }

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

}
