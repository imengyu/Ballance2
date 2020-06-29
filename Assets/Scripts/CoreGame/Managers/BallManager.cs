using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.Managers;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.Managers
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 球管理器
    /// </summary>
    public class BallManager : BaseManagerBindable
    {
        public const string TAG = "BallManager";

        public BallManager() : base(TAG, "Singleton")
        {

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
            ClearBall();
            if (ballTypes != null)
            {
                ballTypes.Clear();
                ballTypes = null;
            }
            return true;
        }

        public GameObject Ball_Wood_piece;
        public GameObject Ball_Stone_piece;
        public GameObject Ball_Paper_piece;
        public GameObject Ball_LightningSphere;
        public GameObject Ball_LightningSphere2;
        public GameObject Ball_Lightning;
        public ParticleSystem Ball_Smoke;
        public GameBall BallWood;
        public GameBall BallStone;
        public GameBall BallPaper;

        private List<GameBall> ballTypes = new List<GameBall>();

        /// <summary>
        /// 所有球类型
        /// </summary>
        public List<GameBall> BallTypes { get { return ballTypes; } }
        /// <summary>
        /// 当前球的名称
        /// </summary>
        public string CurrentBallName { get; private set; }
        /// <summary>
        /// 当前球
        /// </summary>
        public GameBall CurrentBall { get; private set; }
        /// <summary>
        /// 获取球是否调试
        /// </summary>
        public bool IsBallDebug { get { return debug; } }
        /// <summary>
        /// 获取是否控制反转
        /// </summary>
        public bool IsReverseControl { get { return reverseControl; } }

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

        private SoundManager SoundManager;
        private CamManager CamManager;

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
            CamManager = (CamManager)GameManager.GetManager(CamManager.TAG);

            pushType = BallPushType.None;

            RegisterBall("BallWood", BallWood, Ball_Wood_piece);
            RegisterBall("BallStone", BallStone, Ball_Stone_piece);
            RegisterBall("BallPaper", BallPaper, Ball_Paper_piece);

            Ball_LightningSphere.SetActive(false);
            Ball_LightningSphere2.SetActive(false);
            Ball_Lightning.SetActive(false);
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
            SoundManager = (SoundManager)GameManager.GetManager(SoundManager.TAG);

            Misc_Lightning = SoundManager.RegisterSoundPlayer(GameSoundType.BallEffect, "core.assets.sounds:Misc_Lightning.wav");

        }

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

        private void Update()
        {
#if UNITY_EDITOR
            //调试所用代码
            if (Input.GetKeyDown(KeyCode.F1))
                IsControlling = !IsControlling;
            if (Input.GetKeyDown(KeyCode.F2))
                ActiveBall("BallWood");
            if (Input.GetKeyDown(KeyCode.F3))
                ActiveBall("BallStone");
            if (Input.GetKeyDown(KeyCode.F4))
                ActiveBall("BallPaper");
            if (Input.GetKeyDown(KeyCode.F5))
                PlayLighting(true);
#endif

            //闪电球
            if (lighing)
            {
                if (Ball_LightningSphere.transform.localEulerAngles.z > 360f)
                    Ball_LightningSphere.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (Ball_LightningSphere2.transform.localEulerAngles.z > 360f)
                    Ball_LightningSphere2.transform.localEulerAngles = new Vector3(0, 0, 0);
                Ball_LightningSphere.transform.localEulerAngles = new Vector3(0, Ball_LightningSphere.transform.localEulerAngles.y + 60f * Time.deltaTime, 0);
                Ball_LightningSphere2.transform.localEulerAngles = new Vector3(0, Ball_LightningSphere2.transform.localEulerAngles.y + 60f * Time.deltaTime, 0);
                if (secxx < 0.2)
                    secxx += Time.deltaTime;
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
                if (Ball_LightningSphere.transform.localScale.x < 1f)
                {
                    Ball_LightningSphere.transform.localScale = new Vector3(Ball_LightningSphere.transform.localScale.x + 0.8f * Time.deltaTime,
                    Ball_LightningSphere.transform.localScale.y + 0.8f * Time.deltaTime,
                    Ball_LightningSphere.transform.localScale.z + 0.8f * Time.deltaTime);
                    Ball_LightningSphere2.transform.localScale = new Vector3(Ball_LightningSphere.transform.localScale.x + 0.8f * Time.deltaTime,
                    Ball_LightningSphere2.transform.localScale.y + 0.8f * Time.deltaTime,
                    Ball_LightningSphere2.transform.localScale.z + 0.8f * Time.deltaTime);
                }
                else
                {
                    Ball_LightningSphere.transform.localScale = new Vector3(1f, 1f, 1f);
                    Ball_LightningSphere2.transform.localScale = new Vector3(1f, 1f, 1f);
                    lighingBig = false;
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
        }
        private void FixedUpdate()
        {
            if (IsControlling)
                BallPush();
        }

        private Rect line = new Rect(10, 35, 300, 20);

        private void OnGUI()
        {
            if (debug && CurrentBall != null)
            {
                line.y = 35;
                GUI.Label(line, "CurrentBall : " + CurrentBallName); line.y += 16;
                GUI.Label(line, "Pos : " + CurrentBall.transform.position); line.y += 16;
                GUI.Label(line, "Rotation : " + CurrentBall.transform.eulerAngles); line.y += 16;
                GUI.Label(line, "Velocity : " + CurrentBall.Rigidbody.velocity); line.y += 16;
                GUI.Label(line, "[OnFloor]: " + CurrentBall.IsOnFloor); line.y += 16;
                GUI.Label(new Rect(10, line.y, 300, 32), "[ColObj]: [" + CurrentBall.CurrentColObjectLayout + "] "
                    + CurrentBall.CurrentColObject); line.y += 36;
                GUI.Label(line, "FinalPushForce : " + pushType + " Force: " + CurrentBall.PushForce); line.y += 16;
                GUI.Label(line, "FinalFallForce : " + CurrentBall.FinalFallForce); line.y += 16;
            }
        }

        //球管理
        //============================

        /// <summary>
        /// 注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        /// <param name="ball">附加了GameBall组件的球实例</param>
        /// <param name="pieces">球碎片组</param>
        public bool RegisterBall(string name, GameBall ball, GameObject pieces)
        {
            if (ball == null)
            {
                GameLogger.Log(TAG, "要注册的球 {0} 为空", name);
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
            ball.Pieces = pieces;
            ball.BallManager = this;
            ball.CamManager = CamManager;
            BallTypes.Add(ball);

            if (ball.gameObject.activeSelf) ball.gameObject.SetActive(false);
            if (pieces.activeSelf) pieces.SetActive(false);

            return true;
        }
        /// <summary>
        /// 取消注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        public void UnRegisterBall(string name)
        {
            GameBall targetBall = GetRegisteredBall(name);
            if (targetBall != null) BallTypes.Remove(targetBall);
            else
            {
                GameLogger.Log(TAG, "无法取消注册球 {0} 因为它没有注册", name);
                GameErrorManager.LastError = GameError.NotRegistered;
            }
        }
        /// <summary>
        /// 获取已注册的球
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameBall GetRegisteredBall(string name)
        {
            foreach (GameBall b in BallTypes)
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
        public void StartControll()
        {
            IsControlling = true;
        }
        /// <summary>
        /// 停止控制球
        /// </summary>
        public void EndControll()
        {
            IsControlling = false;
        }

        //球推检测
        private void BallPush()
        {
            if (CurrentBall != null)
                CurrentBall.BallPush();
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
                CamManager.CamRoteSpace();
            else
                CamManager.CamRoteSpaceBack();
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
                    CamManager.CamRoteRight();
                }
                else if (IsControlling)
                {
                    pushType |= BallPushType.Right;
                }
            }
            else if (IsControlling && (pushType & BallPushType.Right) == BallPushType.Right)
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
                    CamManager.CamRoteLeft();
                }
                else if (IsControlling)
                {
                    pushType |= BallPushType.Left;
                }
            }
            else if (IsControlling && (pushType & BallPushType.Left) == BallPushType.Left)
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
        private bool lighingBig = false;

        public Texture ballLightningSphere1,
            ballLightningSphere2,
            ballLightningSphere3;

        private int currentBallLightningSphereTexture = 1;
        private float secxx = 0;

        /// <summary>
        /// 播放球 闪电动画
        /// </summary>
        /// <param name="smallToBig">是否由小变大。</param>
        /// <param name="lightEnd">是否在之后闪一下。</param>
        public void PlayLighting(bool smallToBig = false, bool lightEnd = false)
        {
            //播放闪电声音
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
                Ball_LightningSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                Ball_LightningSphere2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                lighingBig = true;
            }

            StartCoroutine(PlayLightingWait(lightEnd));
        }
        private IEnumerator PlayLightingWait(bool lightEnd)
        {
            yield return new WaitForSeconds(2);
            ballLightningSphereMaterial.mainTexture = ballLightningSphere1;
            Ball_LightningSphere.transform.localScale = new Vector3(1f, 1f, 1f);
            Ball_LightningSphere.SetActive(false);
            Ball_LightningSphere2.transform.localScale = new Vector3(1f, 1f, 1f);
            Ball_LightningSphere2.SetActive(false);
            if (lightEnd)
            {
                Ball_Lightning.SetActive(true);
                yield return new WaitForSeconds(0.35f);
                Ball_Lightning.SetActive(false);
            }
            lighing = false;
            yield break;
        }

        #endregion

        #region 球控制

        //当前球
        private GameObject currentBall;
        private Rigidbody rigidbodyCurrent;

        //下一次恢复球的位置
        private Vector3 nextRecoverBallPos = Vector3.zero;
        private bool isBallControl = false;
        private bool isBallSmoothMove = false;
        private Vector3 ballSmoothMoveTarget;
        private Vector3 ballSmoothMoveVelocityTarget;
        private float ballSmoothMoveTime = 0.2f;
        [SerializeField, SetProperty("PushType")]
        private BallPushType pushType = BallPushType.None;

        public bool IsSmoothMove() { return isBallSmoothMove; }

        /// <summary>
        /// 获取设置是否可以控制球
        /// </summary>
        public bool IsControlling
        {
            get { return isBallControl; }
            set
            {
                if (isBallControl != value)
                {
                    isBallControl = value;
                    keyListener.IsListenKey = value;
                    if (value)
                    {
                        CamManager.IsLookingBall = true;
                        CamManager.IsFollowCam = true;
                        if (CurrentBall != null)
                            CurrentBall.StartControll();
                    }
                    else
                    {
                        CamManager.IsFollowCam = false;
                        CamManager.IsLookingBall = false;
                        if (CurrentBall != null)
                            CurrentBall.EndControll(true);
                    }
                }
            }
        }
        /// <summary>
        /// 获取当前球推动方向
        /// </summary>
        public BallPushType PushType { get { return pushType; } }

        /// <summary>
        /// 指定球速度清零。
        /// </summary>
        /// <param name="ball">指定球</param>
        public void RemoveBallSpeed(GameBall ball)
        {
            if (ball != null)
                ball.RemoveSpeed();
        }
        /// <summary>
        /// 添加球推动方向
        /// </summary>
        /// <param name="t"></param>
        public void AddBallPush(BallPushType t)
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
        public void RemoveBallPush(BallPushType t)
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
        public void RecoverSetPos(Vector3 pos)
        {
            nextRecoverBallPos = pos;
        }
        /// <summary>
        /// 重新设置默认球位置并激活
        /// </summary>
        public void RecoverBallDef()
        {
            RecoverBall(nextRecoverBallPos);
        }
        /// <summary>
        /// 重新设置指定球位置并激活
        /// </summary>
        /// <param name="pos">球名字</param>
        public void RecoverBall(Vector3 pos)
        {
            if (CurrentBall != null)
            {
                CurrentBall.Recover(pos);
                CamManager.CamFollowTarget.position = pos;
            }
        }
        /// <summary>
        /// 激活默认球
        /// </summary>
        public void ActiveBallDef()
        {
            if (CurrentBall != null)
                ActiveBall(CurrentBall.TypeName);
            else ActiveBall("BallWood");
        }
        /// <summary>
        /// 激活指定的球
        /// </summary>
        /// <param name="type">球名字</param>
        public void ActiveBall(string type)
        {
            RecoverBallDef();
            GameBall ball = GetRegisteredBall(type);
            if (ball != null)
            {
                CurrentBall = ball;
                CurrentBall.Active(nextRecoverBallPos);
                currentBall = CurrentBall.gameObject;
                CurrentBallName = type;
                rigidbodyCurrent = currentBall.GetComponent<Rigidbody>();
                CamManager.CamFollowTarget = currentBall.GetComponent<Transform>();
                CamManager.IsFollowCam = true;
                CamManager.IsLookingBall = true;
            }
        }
        /// <summary>
        /// 清除已激活的球
        /// </summary>
        public void ClearBall()
        {
            IsControlling = false;
            if (CurrentBall != null)
            {
                CurrentBall.Deactive();
            }
            if (currentBall != null)
            {
                currentBall.SetActive(false);
                currentBall = null;
            }
        }

        /// <summary>
        /// 抛出指定球碎片
        /// </summary>
        /// <param name="type">球类型</param>
        public void ThrowPieces(string type)
        {
            GameBall ball = GetRegisteredBall(type);
            if (ball != null) ball.ThrowPieces();
        }
        /// <summary>
        /// 抛出指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        public void ThrowPieces(GameBall ball)
        {
            if (ball != null) ball.ThrowPieces();
        }
        /// <summary>
        /// 恢复指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        public void RecoverPieces(GameBall ball)
        {
            if (ball != null) ball.RecoverPieces();
        }
        /// <summary>
        /// 平滑移动球到指定位置。
        /// </summary>
        /// <param name="pos">指定位置。</param>
        /// <param name="off">动画平滑时间</param>
        public void SmoothMoveBallToPos(Vector3 pos, float off = 2f)
        {
            if (CurrentBall != null)
            {
                if (IsControlling)
                    IsControlling = false;
                RemoveBallSpeed(CurrentBall);
                ballSmoothMoveTarget = pos;
                ballSmoothMoveTime = off;

                isBallSmoothMove = true;
            }
        }

        /// <summary>
        /// 根据 GameObject 获取球的类型（通常在lua中调用）
        /// </summary>
        /// <param name="ball"></param>
        /// <returns></returns>
        public string GetBallType(GameObject ball)
        {
            GameBall b = ball.GetComponent<GameBall>();
            if (b != null) return b.TypeName;
            return "";
        }

        /// <summary>
        /// 播放烟雾
        /// </summary>
        public void PlaySmoke()
        {
            Ball_Smoke.Play();
        }

        #endregion

    }
}
