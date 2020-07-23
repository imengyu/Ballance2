using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.Interfaces;
using Ballance2.Managers;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.Managers
{
    /// <summary>
    /// 球管理器
    /// </summary>
    public class BallManager : BaseManager
    {
        public const string TAG = "BallManager";

        public BallManager() : base(GamePartName.BallManager, TAG, "Singleton")
        {

        }

        protected override void InitPre()
        {
            InitActions();
            InitShareDataStores();
            base.InitPre();
        }
        protected override bool InitStore(Store store)
        {
            InitGlobaShareAndStore(store);
            return base.InitStore(store);
        }
        public override bool InitManager()
        {
            keyListener = gameObject.AddComponent<KeyListener>();

            InitSettings();
            InitMisc();
            InitBalls();
            return true;
        }
        public override bool ReleaseManager()
        {
            UnInitActions();
            ClearActiveBall();
            if (ballTypes != null)
            {
                ballTypes.Clear();
                ballTypes = null;
            }
            return true;
        }

        public GameObject Ball_LightningSphere;
        public GameObject Ball_LightningSphere2;
        public GameObject Ball_Lightning;
        public ParticleSystem Ball_Smoke;
        public GameBall BallWood;
        public GameBall BallStone;
        public GameBall BallPaper;

        private List<GameBall> ballTypes = new List<GameBall>();

        #region 设置变量

        private bool debug = false;
        private KeyCode keyFront = KeyCode.UpArrow;
        private KeyCode keyBack = KeyCode.DownArrow;
        private KeyCode keyLeft = KeyCode.LeftArrow;
        private KeyCode keyRight = KeyCode.RightArrow;
        private KeyCode keyUpCamera = KeyCode.Space;
        private KeyCode keyRoateCamera = KeyCode.LeftShift;
        private KeyCode keyFront2 = KeyCode.W;
        private KeyCode keyBack2 = KeyCode.S;
        private KeyCode keyLeft2 = KeyCode.A;
        private KeyCode keyRight2 = KeyCode.D;
        private KeyCode keyRoateCamera2 = KeyCode.RightShift;
        private KeyCode keyUp = KeyCode.Q;
        private KeyCode keyDown = KeyCode.E;
        private bool reverseControl = false;

        private GameSettingsActuator GameSettings;

        #endregion

        #region 声音变量

        private AudioSource Misc_Lightning;

        #endregion

        private ISoundManager SoundManager;
        private IICManager ICManager;

        private void InitSettings()
        {
            GameSettings = GameSettingsManager.GetSettings("core");
#if UNITY_EDITOR
            debug = true;
#else
            debug = GameSettings.GetBool("debug");
#endif
            GameSettings.RegisterSettingsUpdateCallback("control", new GameHandler(TAG, OnControlSettingsChanged));
            GameSettings.RequireSettingsLoad("control");
        }
        private void InitBalls()
        {
            ICManager = (IICManager)GameManager.GetManager("ICManager");

            pushType = BallPushType.None;

            RegisterBall("BallWood", BallWood);
            RegisterBall("BallStone", BallStone);
            RegisterBall("BallPaper", BallPaper);

            Ball_LightningSphere.SetActive(false);
            Ball_LightningSphere2.SetActive(false);
            Ball_Lightning.SetActive(false);

            Ball_Light = Ball_Lightning.GetComponent<Light>();
        }
        private void InitKeyEvents()
        {
            //添加按键事件
            keyListener.ClearKeyListen();
            keyListener.AddKeyListen(keyFront, keyFront2, new KeyListener.KeyDelegate(UpArrow_Key));
            keyListener.AddKeyListen(keyBack, keyBack2, new KeyListener.KeyDelegate(DownArrow_Key));
            keyListener.AddKeyListen(keyLeft, keyLeft2, new KeyListener.KeyDelegate(LeftArrow_Key));
            keyListener.AddKeyListen(keyRight, keyRight2, new KeyListener.KeyDelegate(RightArrow_Key));
            keyListener.AddKeyListen(keyUp, new KeyListener.KeyDelegate(Up_Key));
            keyListener.AddKeyListen(keyDown, new KeyListener.KeyDelegate(Down_Key));
            keyListener.AddKeyListen(keyUpCamera, new KeyListener.KeyDelegate(Space_Key));
            keyListener.AddKeyListen(keyRoateCamera, keyRoateCamera2, new KeyListener.KeyDelegate(Shift_Key));
        }
        private void InitMisc()
        {
            SoundManager = (ISoundManager)GameManager.GetManager("SoundManager");
            Misc_Lightning = SoundManager.RegisterSoundPlayer(GameSoundType.BallEffect, "core.assets.sounds:Misc_Lightning.wav");
        }
        private void InitActions()
        {
            GameManager.GameMediator.RegisterActions(GameActionNames.BallManager, 
                TAG, new GameActionHandlerDelegate[] {
                    (param) =>
                    {
                        StartControll();
              
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        EndControll();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        PlaySmoke();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        PlayLighting((bool)param[0], (bool)param[1]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        RemoveBallSpeed((GameBall)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        AddBallPush((BallPushType)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        RemoveBallPush((BallPushType)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        RecoverSetPos((Vector3)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        RecoverBallDef();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        RecoverBallAtPos((Vector3)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        ActiveBall((string)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        ActiveBallDef();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        ClearActiveBall();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        SmoothMoveBallToPos((Vector3)param[0]);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        if (param[0] is string)
                            ThrowPieces(param[0] as string);
                        else if (param[0] is GameBall)
                             ThrowPieces(param[0] as GameBall);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        if (param[0] is string)
                            RecoverPieces(param[0] as string);
                        else if (param[0] is GameBall)
                             RecoverPieces(param[0] as GameBall);
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) =>
                    {
                        return GameActionCallResult.CreateActionCallResult(
                            RegisterBall((string)param[0], (GameBall)param[1], (GameObject)param[2])
                        );
                    },
                    (param) =>
                    {
                        return GameActionCallResult.CreateActionCallResult(UnRegisterBall((string)param[0]));
                    },
                    (param) =>
                    {
                        return GameActionCallResult.CreateActionCallResult(
                            true,
                            new object[] {
                                GetRegisteredBall((string)param[0])
                            }
                        );
                    },
                },
                new string[][]
                {
                    null,
                    null,
                    null,
                    new string[] { "System.Boolean", "System.Boolean" },
                    new string[] { "Ballance2.CoreGame.GamePlay.GameBall" },
                    new string[] { "Ballance2.CoreGame.GamePlay.BallPushType" },
                    new string[] { "Ballance2.CoreGame.GamePlay.BallPushType" },
                    new string[] { "UnityEngine.Vector3" },
                    null,
                    new string[] { "UnityEngine.Vector3" },
                    new string[] { "System.String" },
                    null,
                    null,
                    new string[] { "UnityEngine.Vector3" },
                    new string[] { "System.String/Ballance2.CoreGame.GamePlay.BallPushType" },
                    new string[] { "System.String/Ballance2.CoreGame.GamePlay.BallPushType"  },
                    new string[] { "System.String", "Ballance2.CoreGame.GamePlay.BallPushType", "UnityEngine.GameObject/null"   },
                    new string[] { "System.String" },
                    new string[] { "System.String" },
                }
            );
        }
        private void UnInitActions()
        {
            GameManager.GameMediator.UnRegisterActions(GameActionNames.BallManager);
        }

        #region 全局数据共享

        //私有控制数据
        private StoreData CurrentBall = null;//[GamBall] 获取当前的球
        private StoreData CurrentBallName = null;//[string] 获取当前球的名称
        private StoreData IsControlling = null;//[bool] 获取或设置是否启用球控制
        private StoreData PushType = null;//[BallPushType] 获取当前球推力的方向
        private StoreData IsBallSmoothMove = null;//[bool] 获取当前球是否是正在平滑移动
        private StoreData IsBallDebug = null;//[bool] 获取球是否调试
        private StoreData IsReverseControl = null;//[bool] 获取是否控制反转

        //其他模块全局共享数据
        private StoreData IsLookingBall = StoreData.Empty;
        private StoreData IsFollowCam = StoreData.Empty;
        private StoreData CamFollowTarget = StoreData.Empty;
        //他模块全局共享操作
        private GameAction CamRoteRight = GameAction.Empty;
        private GameAction CamRoteLeft = GameAction.Empty;
        private GameAction CamRoteSpace = GameAction.Empty;
        private GameAction CamRoteSpaceBack = GameAction.Empty;

        private void InitGlobaShareAndStore(Store store)
        {
            //初始化数据桥
            CurrentBall = store.AddParameter("CurrentBall", StoreDataAccess.Get, StoreDataType.Custom);
            CurrentBallName = store.AddParameter("CurrentBallName", StoreDataAccess.Get, StoreDataType.String);
            IsControlling = store.AddParameter("IsControlling", StoreDataAccess.GetAndSet,  StoreDataType.Boolean);
            PushType = store.AddParameter("PushType", StoreDataAccess.Get, StoreDataType.Custom);
            IsBallSmoothMove = store.AddParameter("IsBallSmoothMove", StoreDataAccess.Get,  StoreDataType.Boolean);
            IsBallDebug = store.AddParameter("IsBallDebug", StoreDataAccess.Get,  StoreDataType.Boolean);
            IsReverseControl = store.AddParameter("IsReverseControl", StoreDataAccess.Get,  StoreDataType.Boolean);

            //Get
            CurrentBall.SetDataProvider(currentContext, () => currentBallType);
            CurrentBallName.SetDataProvider(currentContext, () => currentBallType.TypeName);
            IsControlling.SetDataProvider(currentContext, () => isBallControl);
            PushType.SetDataProvider(currentContext, () => pushType);
            IsBallSmoothMove.SetDataProvider(currentContext, () => isBallSmoothMove);
            IsBallDebug.SetDataProvider(currentContext, () => debug);
            IsReverseControl.SetDataProvider(currentContext, () => reverseControl);

            //Set
            IsControlling.RegisterDataObserver((storeData, oldV, newV) =>
            {
                bool value = (bool)newV;
                if (value != isBallControl)
                {
                    isBallControl = value;
                    keyListener.IsListenKey = value;
                    if (value)
                    {
                        IsLookingBall.SetData(currentContext, true);
                        IsFollowCam.SetData(currentContext, true);
                        if (currentBallType != null) currentBallType.StartControll();
                    }
                    else
                    {
                        IsFollowCam.SetData(currentContext, false);
                        IsLookingBall.SetData(currentContext, false);
                        if (currentBallType != null) currentBallType.EndControll(true);
                    }
                }
            });
        }
        private void InitShareDataStores()
        {
            GameManager.RegisterManagerRedayCallback("CamManager", (self, store, manager) =>
            {
                IsFollowCam = store.GetParameter("IsFollowCam");
                IsLookingBall = store.GetParameter("IsLookingBall");
                CamFollowTarget = store.GetParameter("CamFollowTarget");

                CamRoteRight = GameManager.GameMediator.GetRegisteredAction(GameActionNames.CamManager["CamRoteRight"]);
                CamRoteLeft = GameManager.GameMediator.GetRegisteredAction(GameActionNames.CamManager["CamRoteLeft"]);
                CamRoteSpace = GameManager.GameMediator.GetRegisteredAction(GameActionNames.CamManager["CamRoteSpace"]);
                CamRoteSpaceBack = GameManager.GameMediator.GetRegisteredAction(GameActionNames.CamManager["CamRoteSpaceBack"]);

            });
        }

        #endregion

        //设置加载
        private bool OnControlSettingsChanged(string evtName, params object[] param)
        {
            keyFront = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.front", "UpArrow"));
            keyFront2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.front2", "W"));
            keyUp = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.up", "Q"));
            keyDown = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.down", "E"));
            keyBack = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.back", "DownArrow"));
            keyBack2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.back2", "S"));
            keyLeft = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.left", "LeftArrow"));
            keyLeft2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.left2", "A"));
            keyRight = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.right", "RightArrow"));
            keyRight2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.right2", "D"));
            keyRoateCamera = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.roate", "LeftShift"));
            keyRoateCamera2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.roate2", "RightShift"));
            keyUpCamera = (KeyCode)System.Enum.Parse(typeof(KeyCode), GameSettings.GetString("control.key.up_cam", "Space"));

            reverseControl = GameSettings.GetBool("control.reverse", false);

            InitKeyEvents();
            return true;
        }

        protected void Update()
        {
#if UNITY_EDITOR
            //调试所用代码
            if (Input.GetKeyDown(KeyCode.F1))
                IsControlling.SetData(currentContext, !IsControlling.BoolData());
            if (Input.GetKeyDown(KeyCode.F2))
                ActiveBall("BallWood");
            if (Input.GetKeyDown(KeyCode.F3))
                ActiveBall("BallStone");
            if (Input.GetKeyDown(KeyCode.F4))
                ActiveBall("BallPaper");
            if (Input.GetKeyDown(KeyCode.F5))
                PlayLighting(true, true);
            if (Input.GetKeyDown(KeyCode.F6))
                ThrowPieces("BallWood");
            if (Input.GetKeyDown(KeyCode.F7))
                RecoverPieces("BallWood");
            if (Input.GetKeyDown(KeyCode.F8))
                ThrowPieces("BallPaper");
#endif

            //闪电球
            if (lighing)
            {
                if (Ball_LightningSphere.transform.localEulerAngles.z > 360f)
                    Ball_LightningSphere.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (Ball_LightningSphere2.transform.localEulerAngles.z > 360f)
                    Ball_LightningSphere2.transform.localEulerAngles = new Vector3(0, 0, 0);
                Ball_LightningSphere.transform.localEulerAngles = new Vector3(-90, Ball_LightningSphere.transform.localEulerAngles.y - ballLightingRoateSpeed1 * Time.deltaTime, 0);
                Ball_LightningSphere2.transform.localEulerAngles = new Vector3(-90, Ball_LightningSphere2.transform.localEulerAngles.y + ballLightingRoateSpeed2 * Time.deltaTime, 0);
                
                //更换闪电球贴图
                if (secxx < 0.1f) secxx += Time.deltaTime;
                else
                {
                    if (currentBallLightningSphereTexture >= 3)
                        currentBallLightningSphereTexture = 1;
                    else currentBallLightningSphereTexture++;
                    switch (currentBallLightningSphereTexture)
                    {
                        case 1:
                            ballLightningSphereMaterial.mainTexture = ballLightningSphere1;
                            ballLightningSphereMaterial2.mainTexture = ballLightningSphere2;
                            break;
                        case 2:
                            ballLightningSphereMaterial.mainTexture = ballLightningSphere2;
                            ballLightningSphereMaterial2.mainTexture = ballLightningSphere3;
                            break;
                        case 3:
                            ballLightningSphereMaterial.mainTexture = ballLightningSphere3;
                            ballLightningSphereMaterial2.mainTexture = ballLightningSphere1;
                            break;
                    }
                    secxx = 0;
                }
            }
            //闪电球 放大
            if (lighingBig)
            {
                if(lighingLightBigTick < ballLightBallBigSec)
                {
                    lighingLightBigTick += Time.deltaTime;

                    float v = ballLightningBallBigCurve.Evaluate(lighingLightBigTick / ballLightBallBigSec);

                    Ball_LightningSphere.transform.localScale = new Vector3(v, v, v);
                    Ball_LightningSphere2.transform.localScale = new Vector3(v, v, v);
                }
                else
                {
                    Ball_LightningSphere.transform.localScale = new Vector3(1f, 1f, 1f);
                    Ball_LightningSphere2.transform.localScale = new Vector3(1f, 1f, 1f);
                    lighingBig = false;
                }
            }
            //闪电light
            if (lighingLight)
            {
                lighingLightTick += Time.deltaTime;
                Ball_Light.color = new Color(Ball_Light.color.r, Ball_Light.color.g,
                    ballLightningCurve.Evaluate(lighingLightTick / ballLightSec));
                if (lighingLightTick > ballLightSec)
                {
                    lighingLightEndTick = 0;
                    lighingLightEnd = true;
                    lighingLight = false;
                }
            }
            //闪电light
            if (lighingLightEnd)
            {
                float v = ballLightningCurveEnd.Evaluate(lighingLightEndTick / ballLightEndSec);
                lighingLightEndTick += Time.deltaTime;
                Ball_Light.color = new Color(v, v, v);
                if (lighingLightEndTick  > ballLightEndSec)
                {
                    Ball_Lightning.SetActive(false);
                    lighingLightEnd = false;
                }
            }
            //平滑移动
            if (isBallSmoothMove)
            {
                if (currentBall != null)
                {
                    currentBall.transform.position = Vector3.SmoothDamp(currentBall.transform.position, ballSmoothMoveTarget, ref ballSmoothMoveVelocityTarget, ballSmoothMoveTime);
                    if (currentBall.transform.position == ballSmoothMoveTarget)
                        isBallSmoothMove = false;
                }
                else isBallSmoothMove = false;
            }
            //球碎片回收
            if(piecesThrowedBalls.Count > 0)
            {
                CollectPiecesTick();
            }
        }
        protected void FixedUpdate()
        {
            if (isBallControl)
                BallPush();
        }

        private Rect line = new Rect(10, 35, 300, 20);

        protected void OnGUI()
        {
            if (debug && currentBallType != null)
            {
                line.y = 35;
                GUI.Label(line, "CurrentBall : " + currentBallType.TypeName); line.y += 16;
                GUI.Label(line, "Pos : " + currentBall.transform.position); line.y += 16;
                GUI.Label(line, "Rotation : " + currentBall.transform.eulerAngles); line.y += 16;
                GUI.Label(line, "Velocity : " + currentBallType.Rigidbody.velocity); line.y += 16;
                GUI.Label(line, "[OnFloor]: " + currentBallType.IsOnFloor); line.y += 16;
                GUI.Label(new Rect(10, line.y, 300, 32), "[ColObj]: [" + currentBallType.CurrentColObjectLayout + "] "
                    + currentBallType.CurrentColObject); line.y += 36;
                GUI.Label(line, "FinalPushForce : " + pushType + " Force: " + currentBallType.PushForce); line.y += 16;
                GUI.Label(line, "FallForce :" + currentBallType.FallForce); line.y += 16;
                GUI.Label(line, "FinalPushForceVectorFB :" + currentBallType.FinalPushForceVectorFB); line.y += 16;
                GUI.Label(line, "FinalPushForceVectorLR :" + currentBallType.FinalPushForceVectorLR); line.y += 16;
            }
        }

        //球管理
        //============================

        /// <summary>
        /// 注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        /// <param name="ball">附加了GameBall组件的球实例</param>
        private bool RegisterBall(string name, GameBall ball)
        {
            if (ball == null)
            {
                GameLogger.Warning(TAG, "要注册的球 {0} 为空", name);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return false;
            }
            return RegisterBall(name, ball, ball.Pieces);
        }
        /// <summary>
        /// 注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        /// <param name="ball">附加了GameBall组件的球实例</param>
        /// <param name="pieces">球碎片组</param>
        private bool RegisterBall(string name, GameBall ball, GameObject pieces)
        {
            if (ball == null)
            {
                GameLogger.Warning(TAG, "要注册的球 {0} 为空", name);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return false;
            }
            if (GetRegisteredBall(name) != null)
            {
                GameLogger.Log(TAG, "球 {0} 已经注册", name);
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return false;
            }

            ball.TypeName = name;
            if (pieces != null)
                ball.Pieces = pieces;
            ballTypes.Add(ball);

            if (pieces != null)
            {
                GameBallPiecesControl ballPiecesControl = pieces.GetComponent<GameBallPiecesControl>();
                if (ballPiecesControl != null)
                {
                    ballPiecesControl.Ball = ball;
                    ball.BallPiecesControl = ballPiecesControl;
                }
                if (pieces.activeSelf) pieces.SetActive(false);
            }

            if (ball.gameObject.activeSelf) ball.gameObject.SetActive(false);

            return ball.Init();
        }
        /// <summary>
        /// 取消注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        private bool UnRegisterBall(string name)
        {
            GameBall targetBall = GetRegisteredBall(name);
            if (targetBall != null)
            {
                ballTypes.Remove(targetBall);
                targetBall.Destroy();
                return true;
            }
            else
            {
                GameLogger.Warning(TAG, "无法取消注册球 {0} 因为它没有注册", name);
                GameErrorManager.LastError = GameError.NotRegister;
                return false;
            }
        }
        /// <summary>
        /// 获取已注册的球
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GameBall GetRegisteredBall(string name)
        {
            foreach (GameBall b in ballTypes)
            {
                if (b.TypeName == name)
                    return b;
            }
            return null;
        }

        //球控制
        //============================

        /// <summary>
        /// 开始控制球
        /// </summary>
        private void StartControll()
        {
            IsControlling.SetData(currentContext, true);
        }
        /// <summary>
        /// 停止控制球
        /// </summary>
        private void EndControll()
        {
            IsControlling.SetData(currentContext, false);
        }

        //球推检测
        private void BallPush()
        {
            if (currentBallType != null)
                currentBallType.BallPush();
        }

        #region Key Events

        //按键侦听器
        private KeyListener keyListener = null;
        private bool shiftPressed = false;

        //一些按键事件
        private void Shift_Key(KeyCode key, bool down)
        {
            shiftPressed = down;
        }
        private void Space_Key(KeyCode key, bool down)
        {
            if (down)
                GameManager.GameMediator.CallAction(CamRoteSpace);
            else
                GameManager.GameMediator.CallAction(CamRoteSpaceBack);
        }
        private void RightArrow_Key(KeyCode key, bool down)
        {
            if (down)
            {
                if (shiftPressed)
                {
                    if ((pushType & BallPushType.Right) == BallPushType.Right)
                    {
                        pushType ^= BallPushType.Right;
                    }
                    GameManager.GameMediator.CallAction(CamRoteRight);
                }
                else if (isBallControl)
                {
                    pushType |= BallPushType.Right;
                }
            }
            else if (isBallControl && (pushType & BallPushType.Right) == BallPushType.Right)
            {
                pushType ^= BallPushType.Right;
            }
        }
        private void LeftArrow_Key(KeyCode key, bool down)
        {
            if (down)
            {
                if (shiftPressed)
                {
                    if ((pushType & BallPushType.Left) == BallPushType.Left)
                    {
                        pushType ^= BallPushType.Left;
                    }
                    GameManager.GameMediator.CallAction(CamRoteLeft);
                }
                else if (isBallControl)
                {
                    pushType |= BallPushType.Left;
                }
            }
            else if (isBallControl && (pushType & BallPushType.Left) == BallPushType.Left)
            {
                pushType ^= BallPushType.Left;
            }
        }
        private void DownArrow_Key(KeyCode key, bool down)
        {
            if (down)
            {
                pushType |= BallPushType.Back;
            }
            else if ((pushType & BallPushType.Back) == BallPushType.Back)
            {
                pushType ^= BallPushType.Back;
            }
        }
        private void UpArrow_Key(KeyCode key, bool down)
        {
            if (down)
            {
                pushType |= BallPushType.Forward;
            }
            else if ((pushType & BallPushType.Forward) == BallPushType.Forward)
            {
                pushType ^= BallPushType.Forward;
            }
        }
        private void Up_Key(KeyCode key, bool down)
        {
            if (down)
            {
                pushType |= BallPushType.Up;
            }
            else
            {
                pushType ^= BallPushType.Up;
            }
        }
        private void Down_Key(KeyCode key, bool down)
        {
            if (down)
            {
                pushType |= BallPushType.Down;
            }
            else
            {
                pushType ^= BallPushType.Down;
            }
        }

        #endregion

        #region Lighting

        private const float pushVector = 0.05f;
        private Material ballLightningSphereMaterial;
        private Material ballLightningSphereMaterial2;
        private bool lighing = false;
        private bool lighingLight = false;
        private bool lighingLightEnd = false;
        private float lighingLightBigTick = 0;
        private float lighingLightTick = 0;
        private float lighingLightEndTick = 0;
        private bool lighingBig = false;
        private Light Ball_Light;

        public float ballLightingRoateSpeed1 = 80f;
        public float ballLightingRoateSpeed2 = 80f;

        //闪电球变大
        public AnimationCurve ballLightningBallBigCurve;
        public float ballLightBallBigSec = 1.5f;

        //闪电球灯 闪光曲线
        public AnimationCurve ballLightningCurve;
        public float ballLightSec = 2.5f;

        //闪电球灯 最后一闪
        public AnimationCurve ballLightningCurveEnd;
        public float ballLightEndSec = 1.5f;

        public Texture ballLightningSphere1,
            ballLightningSphere2,
            ballLightningSphere3;

        private int currentBallLightningSphereTexture = 1;
        private float secxx = 0;

        /// <summary>
        /// 播放球 闪电动画
        /// </summary>
        /// <param name="smallToBig">是否由小变大</param>
        /// <param name="lightAnim">是否播放相对应的 Light 灯光</param>
        public virtual void PlayLighting(bool smallToBig, bool lightAnim)
        {
            //播放闪电声音
            if (Misc_Lightning != null)
                Misc_Lightning.Play();

            lighing = true;

            ballLightningSphereMaterial2 = Ball_LightningSphere2.GetComponent<MeshRenderer>().material;
            ballLightningSphereMaterial = Ball_LightningSphere.GetComponent<MeshRenderer>().material;
            
            Ball_LightningSphere.SetActive(true);
            Ball_LightningSphere2.SetActive(true);
            Ball_LightningSphere.transform.position = nextRecoverBallPos;
            Ball_LightningSphere2.transform.position = nextRecoverBallPos;
            if (smallToBig)
            {
                Ball_LightningSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                Ball_LightningSphere2.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                lighingLightBigTick = 0;
                lighingBig = true;
            }
            if (lightAnim)
            {
                lighingLight = true;
                lighingLightTick = 0;
                Ball_Lightning.SetActive(true);
            }
            else
                Ball_Lightning.SetActive(false);

            StartCoroutine(PlayLightingWait(lightAnim));
        }
        private IEnumerator PlayLightingWait(bool lightAnim)
        {
            yield return new WaitForSeconds(ballLightSec);

            ballLightningSphereMaterial.mainTexture = ballLightningSphere1;
            Ball_LightningSphere.transform.localScale = new Vector3(1f, 1f, 1f);
            Ball_LightningSphere.SetActive(false);
            Ball_LightningSphere2.transform.localScale = new Vector3(1f, 1f, 1f);
            Ball_LightningSphere2.SetActive(false);
            Ball_Smoke.Play();

            if (lightAnim)
            {
                yield return new WaitForSeconds(ballLightEndSec);
                Ball_Lightning.SetActive(false);
            }
            
            lighing = false;
            yield break;
        }

        #endregion

        #region 球控制

        //当前球
        private GameObject currentBall;
        private GameBall currentBallType;

        //下一次恢复球的位置
        private Vector3 nextRecoverBallPos = Vector3.zero;
        public bool isBallControl = false;
        public bool isBallSmoothMove = false;
        private Vector3 ballSmoothMoveTarget;
        private Vector3 ballSmoothMoveVelocityTarget;
        private float ballSmoothMoveTime = 0.2f;
        [SerializeField, SetProperty("PushType")]
        private BallPushType pushType = BallPushType.None;

        /// <summary>
        /// 指定球速度清零。
        /// </summary>
        /// <param name="ball">指定球</param>
        private void RemoveBallSpeed(GameBall ball)
        {
            if (ball != null)
                ball.RemoveSpeed();
        }
        /// <summary>
        /// 添加球推动方向
        /// </summary>
        /// <param name="t"></param>
        private void AddBallPush(BallPushType t)
        {
            if ((pushType & t) != t)
            {
                pushType |= t;
            }
        }
        /// <summary>
        /// 去除球推动方向
        /// </summary>
        /// <param name="t"></param>
        private void RemoveBallPush(BallPushType t)
        {
            if ((pushType & t) == t)
            {
                pushType ^= t;
            }
        }
        /// <summary>
        /// 设置球下次激活的位置。
        /// </summary>
        /// <param name="pos">下次激活的位置</param>
        private void RecoverSetPos(Vector3 pos)
        {
            nextRecoverBallPos = pos;
        }
        /// <summary>
        /// 重新设置默认球位置并激活
        /// </summary>
        private void RecoverBallDef()
        {
            RecoverBallAtPos(nextRecoverBallPos);
        }
        /// <summary>
        /// 重新设置指定球位置并激活
        /// </summary>
        /// <param name="pos">球名字</param>
        private void RecoverBallAtPos(Vector3 pos)
        {
            if (currentBallType != null)
            {
                currentBallType.Recover(pos);
                CamFollowTarget.TransformData().position = pos;
            }
        }
        /// <summary>
        /// 激活默认球
        /// </summary>
        private void ActiveBallDef()
        {
            if (currentBallType != null)
                ActiveBall(currentBallType.TypeName);
            else ActiveBall("BallWood");
        }
        /// <summary>
        /// 激活指定的球
        /// </summary>
        /// <param name="type">球名字</param>
        private void ActiveBall(string type)
        {
            RecoverBallDef();
            GameBall ball = GetRegisteredBall(type);
            if (ball != null)
            {
                currentBallType = ball;
                currentBallType.Active(nextRecoverBallPos);
                currentBall = ball.gameObject;

                CamFollowTarget.SetData(currentContext, currentBall.transform);
                IsFollowCam.SetData(currentContext, true);
                IsLookingBall.SetData(currentContext, true);
            }
        }
        /// <summary>
        /// 清除已激活的球
        /// </summary>
        private void ClearActiveBall()
        {
            IsControlling.SetData(currentContext, false);
            if (currentBallType != null)
                currentBallType.Deactive();
            if (currentBall != null)
            {
                currentBall.SetActive(false);
                currentBall = null;
            }
        }

        #region 碎片控制

        private List<GameBall> piecesThrowedBalls = new List<GameBall>();
        private float fCollectPiecesTick = 0;

        private void CollectPiecesTick()//回收碎片tick
        {
            if (fCollectPiecesTick < 1.0f) fCollectPiecesTick += Time.deltaTime;
            else {
                fCollectPiecesTick = 0;
                GameBall ball = null;
                for (int i = piecesThrowedBalls.Count - 1; i >= 0; i--)
                {
                    ball = piecesThrowedBalls[i];
                    if (ball.CollectPiecesSecTick > 0) ball.CollectPiecesSecTick--;
                    else
                    {
                        piecesThrowedBalls.Remove(ball);
                        RecoverPieces(ball);
                    }
                }
            }
        }

        /// <summary>
        /// 抛出指定球碎片
        /// </summary>
        /// <param name="type">球类型</param>
        private void ThrowPieces(string type)
        {
            ThrowPieces(GetRegisteredBall(type));
        }
        /// <summary>
        /// 抛出指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        private void ThrowPieces(GameBall ball)
        {
            if (ball != null)
            {
                if (ball.BallPiecesControl != null && ball.BallPiecesControl.ThrowPieces())
                    return;
                if (ball.Pieces != null && ball.PiecesRigidbody != null)
                {
                    ball.Pieces.SetActive(true);

                    foreach (Rigidbody r in ball.PiecesRigidbody)
                    {
                        ICManager.ResetIC(r.gameObject);

                        r.gameObject.SetActive(true);
                        r.AddExplosionForce(ball.ThrowPiecesForce, transform.position, 6f);
                    }

                    ball.CollectPiecesSecTick = ball.CollectPiecesSec;
                    if(!piecesThrowedBalls.Contains(ball))
                        piecesThrowedBalls.Add(ball);
                }
            }
        }
        /// <summary>
        /// 恢复指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        private void RecoverPieces(string type)
        {
            RecoverPieces(GetRegisteredBall(type));
        }
        /// <summary>
        /// 恢复指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        private void RecoverPieces(GameBall ball)
        {
            if (ball != null)
            {
                if (ball.BallPiecesControl != null && ball.BallPiecesControl.RecoverPieces())
                    return;
                if (piecesThrowedBalls.Contains(ball))
                    piecesThrowedBalls.Remove(ball);
                if (ball.Pieces != null && ball.PiecesMaterial != null && ball.PiecesRigidbody != null)
                {
                    foreach (MeshRenderer m in ball.PiecesMaterial)
                    {
                        if (m.materials.Length > 0)
                            GameManager.UIManager.UIFadeManager.AddFadeOut(m.gameObject, 2.0f, true, m.materials);
                    }
                }

                StartCoroutine(RecoverPiecesDelay(ball));
            }
        }

        private IEnumerator RecoverPiecesDelay(GameBall ball)
        {
            yield return new WaitForSeconds(2.0f);

            if (ball.Pieces != null && ball.PiecesMaterial != null && ball.PiecesRigidbody != null)
            {
                foreach (Rigidbody r in ball.PiecesRigidbody)
                {
                    ICManager.ResetIC(r.gameObject);
                    r.gameObject.SetActive(true);
                }
                foreach (MeshRenderer m in ball.PiecesMaterial)
                {
                    if (m.materials.Length > 0)
                        foreach(Material ma in m.materials)
                            ma.color = new Color(ma.color.r, ma.color.g, ma.color.b, 1.0f);
                }

                ball.Pieces.SetActive(false);
                ball.CollectPiecesSecTick = ball.CollectPiecesSec;
            }
        }

        #endregion

        /// <summary>
        /// 平滑移动球到指定位置。
        /// </summary>
        /// <param name="pos">指定位置。</param>
        /// <param name="off">动画平滑时间</param>
        private void SmoothMoveBallToPos(Vector3 pos, float off = 2f)
        {
            if (CurrentBall != null)
            {
                if (IsControlling.BoolData())
                    IsControlling.SetData(currentContext, false);
                RemoveBallSpeed(CurrentBall.Data<GameBall>());
                ballSmoothMoveTarget = pos;
                ballSmoothMoveTime = off;

                isBallSmoothMove = true;
            }
        }

        /// <summary>
        /// 播放烟雾
        /// </summary>
        private void PlaySmoke()
        {
            Ball_Smoke.Play();
        }

        #endregion

    }
}
