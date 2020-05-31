using Ballance2.Config;
using Ballance2.Managers.CoreBridge;
using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using SLua;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.Managers
{
    [CustomLuaClass]
    public class DebugManager : BaseManagerBindable
    {
        public const string TAG = "DebugManager";

        public DebugManager() : base(TAG, "Singleton")
        {
        }

        public override bool InitManager()
        {
            InitCommands();
            InitDebugWindow();
            return true;
        }
        public override bool ReleaseManager()
        {
            OnDisable();
            DestroyCommands();
            DestroyDebugWindow();
            return true;
        }

        #region 调试窗口

        private UIWindow debugWindow;
        private RectTransform DebugCmdContent;
        private InputField DebugInputCommand;
        private GameObject UIDebugTextItem;
        private GameObject UIDebugToolBar;
        private Text DebugTextFPS;
        private Text DebugTextErrors;
        private Text DebugTextWarnings;
        private Text DebugTextInfos;
        private Text DebugItemContent;
        private Toggle DebugToggleInfo;
        private Toggle DebugToggleWarning;
        private Toggle DebugToggleError;

        private RectTransform DebugCmdScrollView;
        private RectTransform DebugDetailsScrollView;
        private GameObject DebugToolsItem;
        private RectTransform DebugToolsItemHost;

        private FPSManager fPSManager;

        private Sprite ico_warning;
        private Sprite ico_info;
        private Sprite ico_error;
        private Sprite ico_success;
        private Sprite box_round_light;
        private Sprite background_transparent;

        private void InitDebugWindow()
        {
            debugWindow = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIDebugWindow"), GameManager.UIManager.UIRoot.transform).GetComponent<UIWindow>();
            UIDebugToolBar = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIDebugToolBar"), GameManager.UIManager.UIRoot.transform, "GameUIDebugToolBar");
            debugWindow.CloseAsHide = true;
            debugWindow.Hide();

            ico_warning = GameManager.FindStaticAssets<Sprite>("ico_warning");
            ico_info = GameManager.FindStaticAssets<Sprite>("ico_info");
            ico_error = GameManager.FindStaticAssets<Sprite>("ico_error");
            ico_success = GameManager.FindStaticAssets<Sprite>("ico_success");
            box_round_light = GameManager.FindStaticAssets<Sprite>("box_round_grey");
            background_transparent = GameManager.FindStaticAssets<Sprite>("background_transparent");

            GameManager.UIManager.RegisterWindow(debugWindow);

            UIDebugToolBar.SetActive(true);
            DebugTextFPS = UIDebugToolBar.transform.Find("DebugTextFPS").GetComponent<Text>();
            DebugTextErrors = debugWindow.UIWindowClientArea.transform.Find("DebugToolErrors/Text").GetComponent<Text>();
            DebugTextWarnings = debugWindow.UIWindowClientArea.transform.Find("DebugToolWarnings/Text").GetComponent<Text>();
            DebugTextInfos = debugWindow.UIWindowClientArea.transform.Find("DebugToolInfos/Text").GetComponent<Text>();
            DebugCmdContent = debugWindow.UIWindowClientArea.transform.Find("DebugCmdScrollView/Viewport/DebugCmdContent").GetComponent<RectTransform>();
            DebugInputCommand = debugWindow.UIWindowClientArea.transform.Find("DebugInputCommand").GetComponent<InputField>();
            DebugToggleInfo = debugWindow.UIWindowClientArea.transform.Find("DebugToggleInfo").GetComponent<Toggle>();
            DebugToggleWarning = debugWindow.UIWindowClientArea.transform.Find("DebugToggleWarning").GetComponent<Toggle>();
            DebugToggleError = debugWindow.UIWindowClientArea.transform.Find("DebugToggleError").GetComponent<Toggle>();
            DebugItemContent = debugWindow.UIWindowClientArea.transform.Find("DebugDetailsScrollView/Viewport/DebugItemContent").GetComponent<Text>();
            Toggle DebugToggleStackTrace = debugWindow.UIWindowClientArea.transform.Find("DebugToggleStackTrace").GetComponent<Toggle>();
            DebugCmdScrollView = debugWindow.UIWindowClientArea.transform.Find("DebugCmdScrollView").GetComponent<RectTransform>();
            DebugDetailsScrollView = debugWindow.UIWindowClientArea.transform.Find("DebugDetailsScrollView").GetComponent<RectTransform>();
            DebugToolsItem = UIDebugToolBar.transform.Find("DebugToolsItem").gameObject;
            DebugToolsItemHost = UIDebugToolBar.transform.Find("DebugToolsItem/Viewport/DebugToolsItemHost").GetComponent<RectTransform>();

            UIDebugTextItem = GameManager.FindStaticPrefabs("UIDebugTextItem");

            fPSManager = GameCloneUtils.CreateEmptyObjectWithParent(GameManager.GameRoot.transform, "FPSManager").AddComponent<FPSManager>();
            fPSManager.FpsText = DebugTextFPS;

            EventTriggerListener.Get(UIDebugToolBar.transform.Find("DebugToolCmd").gameObject).onClick = (g) => {
                if (debugWindow.GetVisible())
                    debugWindow.Hide();
                else
                {
                    debugWindow.Show();
                    ForceReloadLogList();
                }
            };
            EventTriggerListener.Get(UIDebugToolBar.transform.Find("DebugTools").gameObject).onClick = (g) => { DebugToolsItem.SetActive(!DebugToolsItem.activeSelf); };

            EventTriggerListener.Get(debugWindow.UIWindowClientArea.transform.Find("DebugButtonRun").gameObject).onClick = (g) =>
            {
                if (RunCommand(DebugInputCommand.text))
                    DebugInputCommand.text = "";
            };
            EventTriggerListener.Get(debugWindow.UIWindowClientArea.transform.Find("DebugButtonClear").gameObject).onClick = (g) =>  { ClearLogs(); };

            DebugInputCommand.onEndEdit.AddListener((s) =>
            {
                if (RunCommand(s))
                    DebugInputCommand.text = "";
            });
            DebugToggleError.onValueChanged.AddListener((b) =>
            {
                SetShowLogTypes(GameLogger.LogType.Error, b);
                SetShowLogTypes(GameLogger.LogType.Assert, b);
                ForceReloadLogList();
            });
            DebugToggleWarning.onValueChanged.AddListener((b) =>
            {
                SetShowLogTypes(GameLogger.LogType.Warning, b);
                ForceReloadLogList();
            });
            DebugToggleInfo.onValueChanged.AddListener((b) =>
            {
                SetShowLogTypes(GameLogger.LogType.Text, b);
                SetShowLogTypes(GameLogger.LogType.Info, b);
                ForceReloadLogList();
            });
            DebugToggleStackTrace.onValueChanged.AddListener((b) =>
            {
                DebugDetailsScrollView.gameObject.SetActive(b);
                if (!b)
                    UIAnchorPosUtils.SetUILeftBottom(DebugCmdScrollView, UIAnchorPosUtils.GetUILeft(DebugCmdScrollView), 30);
                else
                    UIAnchorPosUtils.SetUILeftBottom(DebugCmdScrollView, UIAnchorPosUtils.GetUILeft(DebugCmdScrollView), 100);
            });

            GameLogger.RegisterLogCallback(HandleLog);
            ForceReloadLogList();
        }
        private void DestroyDebugWindow()
        {
            OnDisable();
            GameLogger.UnRegisterLogCallback();
            ClearLogs();
            debugWindow.Close();
        }

        private int customDebugToolItemY = 0;

        public void AddCustomDebugToolItem(string text, GameHandler callbackHandler)
        {
            GameObject newGo = GameCloneUtils.CreateEmptyUIObjectWithParent(DebugToolsItemHost.transform, "DebugToolItem");
            RectTransform rectTransform = newGo.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            UIAnchorPosUtils.SetUIAnchor(rectTransform, UIAnchor.Stretch, UIAnchor.Top);

            CustomData customData = newGo.AddComponent<CustomData > ();
            customData.customData = callbackHandler;
            Text newText = newGo.AddComponent<Text>();
            newText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            newText.text = text;
            newText.color = Color.white;
            newText.fontSize = 11;
            newText.alignment = TextAnchor.MiddleCenter;
            ContentSizeFitter contentSizeFitter = newGo.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            EventTriggerListener.Get(newGo).onClick = OnCustomDebugToolItemClick;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -customDebugToolItemY);
            customDebugToolItemY += 20;
            DebugToolsItemHost.sizeDelta = new Vector2(DebugToolsItemHost.sizeDelta.x, customDebugToolItemY);
        }
        private void OnCustomDebugToolItemClick(GameObject go)
        {
            DebugToolsItem.SetActive(false);
            CustomData customData = go.GetComponent<CustomData>();
            GameHandler callbackHandler = (GameHandler)customData.customData;
            callbackHandler.Call("OnCustomDebugToolItemClick");
        }

        #region 日志截取

        private float currentLogY = 0;
        private float currentLogX = 0;
        private bool[] showLogTypes = new bool[(int)GameLogger.LogType.Max] {
            true, true, true, true, true, true
        };

        private void SetShowLogTypes(GameLogger.LogType type, bool show)
        {
            showLogTypes[(int)type] = show;
        }
        private void ClearLogs()
        {
            for (int i = DebugCmdContent.transform.childCount - 1; i >= 0; i--)
                Destroy(DebugCmdContent.transform.GetChild(i).gameObject);
            currentLogY = 0;
            currentLogX = 0;
            DebugCmdContent.sizeDelta = new Vector2(100, 30);
            GameLogger.ClearAllLogs();
            UpdateLogCount();
        }
        private void HandleLog(GameLogger.LogData data)
        {
            if (showLogTypes[(int)data.Type])
            {
                AddLogItem(data);
                DeleteExcessLogs();
                UpdateLogCount();
            }
        }
        private void AddLogItem(GameLogger.LogData data)
        {
            GameObject newGo = GameCloneUtils.CloneNewObjectWithParent(UIDebugTextItem, DebugCmdContent.transform, "Text");
            GameObject newT = newGo.transform.Find("Text").gameObject;
            Image newI = newGo.transform.Find("Image").gameObject.GetComponent<Image>();
            Text newText = newT.GetComponent<Text>();
            CustomData customData = newGo.GetComponent<CustomData>();
            customData.customData = data;

            switch (data.Type)
            {
                case GameLogger.LogType.Text:
                    newText.text = data.Data + "        ";
                    newI.sprite = ico_success;
                    break;
                case GameLogger.LogType.Error:
                    newText.text = "<color=#FF2400>" + data.Data + "</color>       ";
                    newI.sprite = ico_error;
                    break;
                case GameLogger.LogType.Assert:
                    newText.text = "<color=#FF0000>" + data.Data + "</color>       ";
                    newI.sprite = ico_error;
                    break;
                case GameLogger.LogType.Info:
                    newText.text = "<color=#70DBDB>" + data.Data + "</color>       ";
                    newI.sprite = ico_info;
                    break;
                case GameLogger.LogType.Warning:
                    newText.text = "<color=#FF7F00>" + data.Data + "</color>       ";
                    newI.sprite = ico_warning;
                    break;
            }

            RectTransform newGoRectTransform = newGo.GetComponent<RectTransform>();
            RectTransform newTextRectTransform = newT.GetComponent<RectTransform>();

            newGoRectTransform.anchoredPosition = new Vector2(newGoRectTransform.anchoredPosition.x, -currentLogY);

            Vector2 textSize = UIContentSizeUtils.GetContentSizeFitterPreferredSize(newTextRectTransform,
               newT.GetComponent<ContentSizeFitter>());

            currentLogY += textSize.y;
            if (textSize.x + 2 > currentLogX)
                currentLogX = textSize.x + 2;
            DebugCmdContent.sizeDelta = new Vector2(currentLogX, currentLogY + 6);

            EventTriggerListener.Get(newT).onClick = SetCurrentActiveLogItem;
        }
        private GameObject lastActiveLogItem = null;
        private void SetCurrentActiveLogItem(GameObject logItem)
        {
            Image image = null;
            if (lastActiveLogItem != null)
            {
                image = lastActiveLogItem.transform.parent.gameObject.GetComponent<Image>();
                image.overrideSprite = background_transparent;
            }

            if (lastActiveLogItem != logItem)
            {
                lastActiveLogItem = logItem;
                image = lastActiveLogItem.transform.parent.gameObject.GetComponent<Image>();
                image.overrideSprite = box_round_light;
                CustomData customData = lastActiveLogItem.transform.parent.gameObject.GetComponent<CustomData>();
                GameLogger.LogData data = (GameLogger.LogData)customData.customData;
                DebugItemContent.text = data.Data + "\nStackTrace:\n" + data.StackTrace;
            }
            else
            {
                lastActiveLogItem = null;
                DebugItemContent.text = "Click a item to show details";
            }
        }
        private void ForceReloadLogList()
        {
            for (int i = DebugCmdContent.transform.childCount - 1; i >= 0; i--)
                Destroy(DebugCmdContent.transform.GetChild(i).gameObject);
            currentLogY = 0;
            currentLogX = 0;
            DebugCmdContent.sizeDelta = new Vector2(100, 30);

            List<GameLogger.LogData> datas = GameLogger.GetLogData();
            foreach (GameLogger.LogData data in datas)
            {
                if (showLogTypes[(int)data.Type])
                    AddLogItem(data);
            }

            UpdateLogCount();
        }
        private void DeleteExcessLogs()
        {
            var amountToRemove = Mathf.Max(DebugCmdContent.transform.childCount - 
                GameConst.GameLoggerBufferMax, 0);
            if (amountToRemove == 0)
                return;

            float yLess = 0;
            GameObject goT = null;
            RectTransform goR = null;
            for (int i = amountToRemove - 1; i >= 0; i--)
            {
                goT = DebugCmdContent.transform.GetChild(i).gameObject;
                goR = goT.GetComponent<RectTransform>();
                yLess  +=  goR.sizeDelta.y;
                Destroy(goT);
            }
            for (int i = DebugCmdContent.transform.childCount - 1; i >= 0; i--)
            {
                goT = DebugCmdContent.transform.GetChild(i).gameObject;
                goR = goT.GetComponent<RectTransform>();
                goR.sizeDelta = new Vector2(goR.sizeDelta.x, goR.sizeDelta.y - yLess);
            }
            currentLogY -= yLess;
            DebugCmdContent.sizeDelta = new Vector2(currentLogX, currentLogY);
        }
        private void UpdateLogCount()
        {
            DebugTextErrors.text = GameLogger.GetLogCount(GameLogger.LogType.Error).ToString();
            DebugTextWarnings.text = GameLogger.GetLogCount(GameLogger.LogType.Warning).ToString();
            DebugTextInfos.text = GameLogger.GetLogCount(GameLogger.LogType.Info).ToString();
        }

        void OnEnable()
        {
#if UNITY_4
            Application.RegisterLogCallback(HandleLog);
#else
            Application.logMessageReceived += HandleUnityLog;
#endif
        }
        void OnDisable()
        {
#if UNITY_4
            Application.RegisterLogCallback(null);
#else
            Application.logMessageReceived -= HandleUnityLog;
#endif
        }

        void HandleUnityLog(string message, string stackTrace, LogType type)
        {
            GameLogger.WriteLog(type == LogType.Exception ?
                GameLogger.LogType.Error : (GameLogger.LogType)type, "", message + "\n" + stackTrace);
            UpdateLogCount();
        }

        #endregion

        #endregion

        #region 调试命令控制

        public delegate bool CommandDelegate(string keyword, string fullCmd, string[] args);

        private List<CmdItem> commands = null;
        private class CmdItem
        {
            public string Keyword;
            public int LimitArgCount;
            public string HelpText;
            public string Handler;
            public GameLuaHandler HandlerInternal;
            public CommandDelegate KernelCallback;
        }

        private void InitCommands()
        {
            commands = new List<CmdItem>();

            //注册基础内置命令
            RegisterCommand("quit", (keyword, fullCmd, args) =>
            {
                GameManager.QuitGame();
                return true;
            }, 0, "退出游戏");
            RegisterCommand("echo", (keyword, fullCmd, args) =>
            {
                GameLogger.Log(TAG, "[echo] " + fullCmd.Substring(3));
                return true;
            }, 1, "[any] 测试");
            RegisterCommand("lua", (keyword, fullCmd, args) =>
            {
                LuaSvr.mainState.doString(fullCmd.Substring(3));
                return true;
            }, 1, "[any] 运行 LUA 命令");
            RegisterCommand("fps", (keyword, fullCmd, args) =>
            {
                int fpsVal = 0;
                if(args.Length >= 1)
                {
                    if (int.TryParse(args[0], out fpsVal) && fpsVal > 0 && fpsVal < 120)
                        fPSManager.ForceSetFps(fpsVal);
                    else GameLogger.Error(TAG, "错误的参数：{0}", args[0]);
                }
                GameLogger.Error(TAG, "Application.targetFrameRate = {0}", Application.targetFrameRate);
                return true;
            }, 0, "[targetFps:int] 获取或设置 targetFrameRate");
            RegisterCommand("help", OnCommandHelp, 1, "显示命令帮助");
        }
        private void DestroyCommands()
        {
            if(commands != null)
            {
                foreach(CmdItem c in commands)
                {
                    c.HandlerInternal = null;
                    c.KernelCallback = null;
                }
                commands.Clear();
                commands = null;
            }
        }
        private bool OnCommandHelp(string keyword, string fullCmd, string[] args)
        {
            string helpText = "命令帮助：\n";
            foreach (CmdItem cmdItem in commands)
                helpText += cmdItem.Keyword + " <color=#adadad>" + cmdItem.HelpText + "</color>\n";
            GameLogger.Log(TAG, helpText);
            return true;
        }

        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="cmd">命令字符串</param>
        /// <returns>返回是否成功</returns>
        public bool RunCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return false;

            StringSpliter sp = new StringSpliter(cmd, ' ', true);
            if (sp.Count >= 1)
            {
                foreach (CmdItem cmdItem in commands)
                {
                    if (cmdItem.Keyword == sp.Result[0])
                    {
                        //arg
                        if (cmdItem.LimitArgCount > 0 && sp.Count < cmdItem.LimitArgCount - 1)
                        {
                            GameLogger.Log(TAG, "命令 {0} 至少需要 {1} 个参数", sp.Result[0], cmdItem.LimitArgCount);
                            return false;
                        }

                        List<string> arglist = new List<string>(sp.Result);
                        arglist.RemoveAt(0);

                        //Kernel hander
                        if (cmdItem.KernelCallback != null)
                            return cmdItem.KernelCallback(sp.Result[0], cmd, arglist.ToArray());
                        //Modul handler
                        if (!string.IsNullOrEmpty(cmdItem.Handler) && cmdItem.HandlerInternal != null)
                            return cmdItem.HandlerInternal.RunCommandHandler(sp.Result[0], sp.Count, cmd, arglist.ToArray());
                    }
                }
                GameLogger.Warning(TAG, "未找到命令 {0}", sp.Result[0]);
            }
            return false;
        }

        /*
         * 注册命令说明
         * 
         * 支持在LUA端直接注册控制台指令，你可以用于模块调试
         * eg: 
         *     local DebugManager = GameManager:GetManager("DebugManager")
	     *     DebugManager:RegisterCommand("test", "TestModul:Modul:CmdTestHandler", 1, "测试指令帮助文字")
         * 命令处理器 回调格式
         *    func(keyword, argCount, stringList)
         *         keyword 用户输入的命令单词
         *         argCount 用户输入的参数个数（您也可以通过RegisterCommand中的limitArgCount参数指定最低参数个数）
         *         stringList 参数数组，string[] 类型
         * 
         */

        /// <summary>
        /// 注册命令 (lua使用)
        /// </summary>
        /// <param name="keyword">命令单词</param>
        /// <param name="handler">命令处理器。命令处理器格式与通用格式相同。LUA接受函数须满足定义：func(keyword, argCount, stringList)</param>
        /// <param name="limitArgCount">命令最低参数，默认 0 表示无参数或不限制</param>
        /// <param name="helpText">命令帮助文字</param>
        /// <returns>成功返回命令ID，不成功返回-1</returns>
        public int RegisterCommand(string keyword, string handler, int limitArgCount, string helpText)
        {
            if (!IsCommandRegistered(keyword))
            {
                CmdItem item = new CmdItem();
                item.Keyword = keyword;
                item.LimitArgCount = limitArgCount;
                item.HelpText = helpText;
                item.Handler = handler;
                item.KernelCallback = null;
                item.HandlerInternal = new GameLuaHandler(handler);

                commands.Add(item);
                return commands.Count - 1;
            }
            return -1;
        }
        /// <summary>
        /// 注册命令（内部使用）
        /// </summary>
        /// <param name="keyword">命令单词</param>
        /// <param name="kernelCallback">命令回调</param>
        /// <param name="limitArgCount">命令最低参数，默认 0 表示无参数或不限制</param>
        /// <param name="helpText">命令帮助文字</param>
        /// <returns>成功返回命令ID，不成功返回-1</returns>
        /// <returns></returns>
        public int RegisterCommand(string keyword, CommandDelegate kernelCallback, int limitArgCount, string helpText)
        {
            if (!IsCommandRegistered(keyword))
            {
                CmdItem item = new CmdItem();
                item.Keyword = keyword;
                item.LimitArgCount = limitArgCount;
                item.HelpText = helpText;
                item.Handler = "";
                item.KernelCallback = kernelCallback;

                commands.Add(item);
                return commands.Count - 1;
            }
            return -1;
        }
        /// <summary>
        /// 取消注册命令
        /// </summary>
        /// <param name="cmdId">命令ID</param>
        public void UnRegisterCommand(string keyword)
        {
            CmdItem removeItem = null;
            foreach (CmdItem cmdItem in commands)
            {
                if (cmdItem.Keyword == keyword)
                {
                    removeItem = cmdItem;
                    break;
                }
            }
            if (removeItem != null)
                commands.Remove(removeItem);
        }
        /// <summary>
        /// 获取命令是否注册
        /// </summary>
        /// <param name="keyword">命令单词</param>
        /// <returns></returns>
        public bool IsCommandRegistered(string keyword)
        {
            foreach (CmdItem cmdItem in commands)
            {
                if (cmdItem.Keyword == keyword)
                    return true;
            }
            return false;
        }

        #endregion

    }
}
