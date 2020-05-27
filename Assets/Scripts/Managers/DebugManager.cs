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
        private FPSManager fPSManager;

        private void InitDebugWindow()
        {
            debugWindow = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIDebugWindow"), GameManager.UIManager.UIRoot.transform).GetComponent<UIWindow>();
            UIDebugToolBar = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIDebugToolBar"), GameManager.UIManager.UIRoot.transform, "GameUIDebugToolBar");
            debugWindow.CloseAsHide = true;

            GameManager.UIManager.RegisterWindow(debugWindow);

            UIDebugToolBar.SetActive(true);
            DebugTextFPS = UIDebugToolBar.transform.Find("DebugTextFPS").GetComponent<Text>();
            DebugTextErrors = UIDebugToolBar.transform.Find("DebugToolErrors/Text").GetComponent<Text>();
            DebugTextWarnings = UIDebugToolBar.transform.Find("DebugToolWarnings/Text").GetComponent<Text>();
            DebugTextInfos = UIDebugToolBar.transform.Find("DebugToolInfos/Text").GetComponent<Text>();
            DebugCmdContent = debugWindow.UIWindowClientArea.transform.Find("DebugCmdScrollView/Viewport/DebugCmdContent").GetComponent<RectTransform>();
            DebugInputCommand = debugWindow.UIWindowClientArea.transform.Find("DebugInputCommand").GetComponent<InputField>();

            UIDebugTextItem = GameManager.FindStaticPrefabs("UIDebugTextItem");

            fPSManager = GameCloneUtils.CreateEmptyObjectWithParent(GameManager.GameRoot.transform).AddComponent<FPSManager>();
            fPSManager.FpsText = DebugTextFPS;

            EventTriggerListener.Get(UIDebugToolBar.gameObject).onClick = (g) => { debugWindow.Show();  };
            EventTriggerListener.Get(debugWindow.UIWindowClientArea.transform.Find("DebugButtonRun").gameObject).onClick = (g) =>
            {
                if (RunCommand(DebugInputCommand.text))
                    DebugInputCommand.text = "";
            };
            EventTriggerListener.Get(debugWindow.UIWindowClientArea.transform.Find("DebugButtonClear").gameObject).onClick = (g) =>  { ClearLogs(); };

            GameLogger.RegisterLogCallback(HandleLog);
            GameLogger.ReSendNotReceivedLogs();
        }
        private void DestroyDebugWindow()
        {
            ClearLogs();
            debugWindow.Close();
        }

        #region 日志截取

        private float currentLogY = 0;
        private float currentLogX = 0;

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
        private void HandleLog(GameLogger.LogType type, string content)
        {
            AddLogItem(type, content);
            DeleteExcessLogs();
            UpdateLogCount();
        }
        private void AddLogItem(GameLogger.LogType type, string content)
        {
            GameObject newT = GameCloneUtils.CloneNewObjectWithParent(UIDebugTextItem, DebugCmdContent.transform, "Text");
            Text newText = newT.GetComponent<Text>();

            switch (type)
            {
                case GameLogger.LogType.Text:
                    newText.text = content;
                    break;
                case GameLogger.LogType.Error:
                    newText.text = "<color=#FF2400>" + content + "</color>";
                    break;
                case GameLogger.LogType.Assert:
                    newText.text = "<color=#FF0000>" + content + "</color>";
                    break;
                case GameLogger.LogType.Info:
                    newText.text = "<color=#70DBDB>" + content + "</color>";
                    break;
                case GameLogger.LogType.Warning:
                    newText.text = "<color=#FF7F00>" + content + "</color>";
                    break;
            }

            RectTransform newTextRectTransform = newT.GetComponent<RectTransform>();
            newTextRectTransform.anchoredPosition = new Vector2(newTextRectTransform.anchoredPosition.x, -currentLogY);

            Vector2 textSize = UIContentSizeUtils.GetContentSizeFitterPreferredSize(newTextRectTransform,
               newT.GetComponent<ContentSizeFitter>());

            currentLogY += textSize.y;
            if (textSize.x > currentLogX)
                currentLogX = textSize.x;
            DebugCmdContent.sizeDelta = new Vector2(currentLogX, currentLogY);
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

        public delegate bool CommandDelegate(string keyword, string[] args);

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

        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="cmd">命令字符串</param>
        /// <returns>返回是否成功</returns>
        public bool RunCommand(string cmd)
        {
            if(string.IsNullOrEmpty(cmd))
            {
                GameLogger.Warning(TAG, "请输入命令！");
                return false;
            }
            if (cmd.StartsWith("say "))
            {
                GameLogger.Log(TAG, "[say] " + cmd.Substring(3));
                return true;
            }
            if (cmd.StartsWith("lua "))
            {
                LuaSvr.mainState.doString(cmd.Substring(3));
                return true;
            }
            if (cmd.StartsWith("help"))
            {
                string helpText = "命令帮助：\n";
                helpText += "help <color=#adadad>显示本帮助</color>\n";
                helpText += "lua <color=#adadad>运行较短的lua脚本</color>\n";
                foreach (CmdItem cmdItem in commands)
                    helpText += cmdItem.Keyword + " <color=#adadad>" + cmdItem.HelpText + "</color>\n";
                GameLogger.Log(TAG, helpText);
                return true;
            }
            else
            {
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
                                GameLogger.Log(TAG, "命令 {0} 至少需要 {1} 个参数", sp.Result[0] , cmdItem.LimitArgCount);
                                return false;
                            }
                            //Kernel hander
                            if (cmdItem.KernelCallback != null)
                            {
                                if (sp.Count > 1)
                                {
                                    string[] newsp = new string[sp.Count - 1];
                                    for (int i = 1; i < sp.Count - 1; i++)
                                        newsp[i - 1] = sp.Result[i];
                                    return cmdItem.KernelCallback(sp.Result[0], newsp);
                                }
                                else return cmdItem.KernelCallback(sp.Result[0], null);
                            }
                            //Modul handler
                            if (!string.IsNullOrEmpty(cmdItem.Handler) && cmdItem.HandlerInternal != null)
                            {
                                List<string> arglist = new List<string>(sp.Result);
                                arglist.RemoveAt(0);
                                cmdItem.HandlerInternal.RunCommandHandler(sp.Result[0], sp.Count, arglist.ToArray());
                            }
                        }
                    }
                    GameLogger.Warning(TAG, "未找到命令 {0}", sp.Result[0]);
                }
                return false;
            }
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
