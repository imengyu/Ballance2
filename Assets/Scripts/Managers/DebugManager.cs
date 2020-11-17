using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.Interfaces;
using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using SLua;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * DebugManager.cs
 * 用途：
 * 用于提供调试管理控制台，和日志记录器
 * 
 * 作者：
 * mengyu
 * 
 * 更改历史：
 * 2020-1-1 创建
 *
 */

namespace Ballance2.Managers
{
    /// <summary>
    /// 调试管理器
    /// </summary>
    public class DebugManager : BaseManager, IDebugManager
    {
        public const string TAG = "DebugManager";

        public DebugManager() : base("core.debugmgr", TAG, "Singleton") {}

        protected override void InitPre()
        {
            InitCommands();
            base.InitPre();
        }
        public override bool InitManager()
        {
            InitDebugWindow();
            //日志回调
#if UNITY_4
            Application.RegisterLogCallback(HandleLog);
#else
            Application.logMessageReceived += HandleUnityLog;
#endif
            return true;
        }
        public override bool ReleaseManager()
        {
            logDestroyed = true;
            //日志回调
#if UNITY_4
            Application.RegisterLogCallback(null);
#else
            Application.logMessageReceived -= HandleUnityLog;
#endif
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
        private ScrollRect DebugCmdScrollViewScrollRect;
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

        private Sprite ico_warning_big;
        private Sprite ico_warning2_big;
        private Sprite ico_info_big;
        private Sprite ico_error_big;
        private Sprite ico_success_big;

        private void InitDebugWindow()
        {
            RectTransform debugRectTransform = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIDebugWindow"), GameManager.UIManager.UIRoot.transform).GetComponent<RectTransform>();
            debugWindow = GameManager.UIManager.CreateWindow("Debug console window", debugRectTransform);
            UIDebugToolBar = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIDebugToolBar"), GameManager.UIManager.UIRoot.transform, "GameUIDebugToolBar");
            debugWindow.CloseAsHide = true;
            debugWindow.SetSize(450, 330);
            debugWindow.SetMinSize(420, 270);
            debugWindow.MoveToCenter();
            debugWindow.Hide();

            ico_warning2_big = GameManager.FindStaticAssets<Sprite>("ico_warning2_big");
            ico_warning_big = GameManager.FindStaticAssets<Sprite>("ico_warning_big");
            ico_info_big = GameManager.FindStaticAssets<Sprite>("ico_info_big");
            ico_error_big = GameManager.FindStaticAssets<Sprite>("ico_error_big");
            ico_success_big = GameManager.FindStaticAssets<Sprite>("ico_success_big");
            ico_warning = GameManager.FindStaticAssets<Sprite>("ico_warning");
            ico_info = GameManager.FindStaticAssets<Sprite>("ico_info");
            ico_error = GameManager.FindStaticAssets<Sprite>("ico_error");
            ico_success = GameManager.FindStaticAssets<Sprite>("ico_success");
            box_round_light = GameManager.FindStaticAssets<Sprite>("box_round_grey");
            background_transparent = GameManager.FindStaticAssets<Sprite>("background_transparent");

            GameManager.UIManager.RegisterWindow(debugWindow);

            UIDebugToolBar.SetActive(true);
            DebugTextFPS = UIDebugToolBar.transform.Find("DebugTextFPS").GetComponent<Text>();
            DebugTextErrors = debugRectTransform.transform.Find("DebugToolErrors/Text").GetComponent<Text>();
            DebugTextWarnings = debugRectTransform.transform.Find("DebugToolWarnings/Text").GetComponent<Text>();
            DebugTextInfos = debugRectTransform.transform.Find("DebugToolInfos/Text").GetComponent<Text>();
            DebugCmdContent = debugRectTransform.transform.Find("DebugCmdScrollView/Viewport/DebugCmdContent").GetComponent<RectTransform>();
            DebugInputCommand = debugRectTransform.transform.Find("DebugInputCommand").GetComponent<InputField>();
            DebugToggleInfo = debugRectTransform.transform.Find("DebugToggleInfo").GetComponent<Toggle>();
            DebugToggleWarning = debugRectTransform.transform.Find("DebugToggleWarning").GetComponent<Toggle>();
            DebugToggleError = debugRectTransform.transform.Find("DebugToggleError").GetComponent<Toggle>();
            DebugItemContent = debugRectTransform.transform.Find("DebugDetailsScrollView/Viewport/DebugItemContent").GetComponent<Text>();
            Toggle DebugToggleStackTrace = debugRectTransform.transform.Find("DebugToggleStackTrace").GetComponent<Toggle>();
            DebugCmdScrollView = debugRectTransform.transform.Find("DebugCmdScrollView").GetComponent<RectTransform>();
            DebugCmdScrollViewScrollRect = DebugCmdScrollView.GetComponent<ScrollRect>();
            DebugDetailsScrollView = debugRectTransform.transform.Find("DebugDetailsScrollView").GetComponent<RectTransform>();
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

            EventTriggerListener.Get(debugRectTransform.transform.Find("DebugButtonRun").gameObject).onClick = (g) =>
            {
                if (RunCommand(DebugInputCommand.text))
                    DebugInputCommand.text = "";
            };
            EventTriggerListener.Get(debugRectTransform.transform.Find("DebugButtonClear").gameObject).onClick = (g) =>  { ClearLogs(); };

            DebugInputCommand.onEndEdit.AddListener((s) =>
            {
                ClearCurrentActiveLogItem();
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
            GameLogger.UnRegisterLogCallback();
            if (!thisDestroyed)
            {
                ClearLogs();
                debugWindow.Close();
            }
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
            callbackHandler.CallEventHandler("OnCustomDebugToolItemClick");
        }

        private List<int> showedExceptionDialogs = new List<int>();

        public void ShowExceptionDialog(string title, string message, LogType type)
        {
            if(showedExceptionDialogs.Count >= 10)
            {
                GameManager.UIManager.GlobalToast(title + "\n发生的错误过多，请打开控制台查看\n\n" +
                    (message.Length > 50 ? (message.Substring(0, 40) + "\n... (" + (message.Length - 50) + " more)") : message));
            }

            RectTransform debugRectTransform = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIErrorAlertDialog"), GameManager.UIManager.UIRoot.transform).GetComponent<RectTransform>();
            UIWindow errWindow = GameManager.UIManager.CreateWindow(title, debugRectTransform);
            errWindow.CanClose = true;
            errWindow.CanResize = true;
            errWindow.CanDrag = true;
            errWindow.SetMinSize(300, 200);
            errWindow.Show();
            errWindow.onClose = (id) => showedExceptionDialogs.Remove(id);

            showedExceptionDialogs.Add(errWindow.GetWindowId());

            debugRectTransform.Find("UIButtonCopy").GetComponent<Button>().onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = message;
                GameManager.UIManager.GlobalToast("错误信息已复制到剪贴板!");
            });
            debugRectTransform.Find("UIButtonClose").GetComponent<Button>().onClick.AddListener(() => errWindow.Close());

            Image ico = debugRectTransform.Find("UIErrorImage").GetComponent<Image>();
            Text text = debugRectTransform.Find("UIScrollView/Viewport/Content").GetComponent<Text>();

            text.text = message;

            switch (type)
            {
                case LogType.Error: ico.sprite = ico_error_big; break;
                case LogType.Exception: ico.sprite = ico_error_big; break;
                case LogType.Assert: ico.sprite = ico_warning2_big; break;
                case LogType.Warning: ico.sprite = ico_warning_big; break;
                case LogType.Log: ico.sprite = ico_info_big; break;
            }
        }

        #region 日志截取

        private void OnDestroy()
        {
#if UNITY_4
            Application.RegisterLogCallback(null);
#else
            Application.logMessageReceived -= HandleUnityLog;
#endif
            logDestroyed = true;
            thisDestroyed = true;
        }

        private bool thisDestroyed = false;
        private bool logDestroyed = false;
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
            if (!logDestroyed && showLogTypes[(int)data.Type])
            {
                AddLogItem(data);
                DeleteExcessLogs();
                UpdateLogCount();
            }
        }
        private void AddLogItem(GameLogger.LogData data)
        {
            if (logDestroyed)
                return;

            GameObject newGo = GameCloneUtils.CloneNewObjectWithParent(UIDebugTextItem, DebugCmdContent.transform, "Text");
            GameObject newT = newGo.transform.Find("Text").gameObject;
            Image newI = newGo.transform.Find("Image").gameObject.GetComponent<Image>();
            Text newText = newT.GetComponent<Text>();
            CustomData customData = newGo.GetComponent<CustomData>();
            customData.customData = data;

            if (data.Data.Length > 32767)//字符过长Text无法显示
                data.Data = data.Data.Substring(0, 32766);

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

            currentLogY += 12;
            if (textSize.x + 2 > currentLogX)
                currentLogX = textSize.x + 2;
            DebugCmdContent.sizeDelta = new Vector2(currentLogX, currentLogY + 6);

            EventTriggerListener.Get(newT).onClick = SetCurrentActiveLogItem;

            if (lastActiveLogItem == null || lastActiveLogItem == lastLogItem)
            {
                SetCurrentActiveLogItem(newT);
                DebugCmdScrollViewScrollRect.verticalNormalizedPosition = 0;
            }

            lastLogItem = newT;
        }
        private GameObject lastActiveLogItem = null;
        private GameObject lastLogItem = null;
        private void SetCurrentActiveLogItem(GameObject logItem)
        {
            Image image = null;
            ClearCurrentActiveLogItem();

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
        private void ClearCurrentActiveLogItem()
        {
            Image image = null;
            if (lastActiveLogItem != null)
            {
                image = lastActiveLogItem.transform.parent.gameObject.GetComponent<Image>();
                image.overrideSprite = background_transparent;
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

        void HandleUnityLog(string message, string stackTrace, LogType type)
        {
            if (!logDestroyed && !GameLogger.GetUnityLogLock()) { 
                GameLogger.WriteLog(type == LogType.Exception ?
                    GameLogger.LogType.Error : (GameLogger.LogType)type, "Unity", message + "\n" + stackTrace);
                UpdateLogCount();

                if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
                    ShowExceptionDialog("发生错误", message + "\n" + stackTrace, type);
            }
        }

        #endregion

        #endregion

        #region 调试命令控制

        private List<CmdItem> commands = null;
        private class CmdItem
        {
            public string Keyword;
            public int LimitArgCount;
            public string HelpText;
            public string Handler;
            public GameLuaHandler HandlerInternal;
            public int Id;
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
                item.Id = CommonUtils.GenNonDuplicateID();
                commands.Add(item);
                return item.Id;
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
                item.Id = CommonUtils.GenNonDuplicateID() ;

                commands.Add(item);
                return item.Id;
            }
            return -1;
        }
        /// <summary>
        /// 取消注册命令
        /// </summary>
        /// <param name="cmdId">命令ID</param>
        public void UnRegisterCommand(int cmdId)
        {
            CmdItem removeItem = null;
            foreach (CmdItem cmdItem in commands)
            {
                if (cmdItem.Id == cmdId)
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
