using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.Managers
{
    /// <summary>
    /// UI 管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIManager : BaseManagerSingleton
    {
        public const string TAG = "UIManager";

        public UIManager() : base(TAG)
        {

        }

        public override bool InitManager()
        {
            UIRoot = GameManager.GameCanvas;
            UIFadeManager = UIRoot.AddComponent<UIFadeManager>();

            GlobalFadeMaskWhite = GameObject.Find("GlobalFadeMaskWhite").GetComponent<Image>();
            GlobalFadeMaskBlack = GameObject.Find("GlobalFadeMaskBlack").GetComponent<Image>();

            return base.InitManager();
        }
        public override bool ReleaseManager()
        {
            return base.ReleaseManager();
        }
        public override void ReloadData()
        {
            base.ReloadData();
        }

        /// <summary>
        /// UI 根
        /// </summary>
        public GameObject UIRoot;
        /// <summary>
        /// 渐变管理器
        /// </summary>
        public UIFadeManager UIFadeManager;



        #region 全局渐变遮罩

        private Image GlobalFadeMaskWhite;
        private Image GlobalFadeMaskBlack;

        /// <summary>
        /// 全局黑色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskBlackFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskBlack, second);
        }
        /// <summary>
        /// 全局白色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskWhiteFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskBlack, second);
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskBlackFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskBlack, second, true);
        }
        /// <summary>
        /// 全局白色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskWhiteFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskBlack, second, true);
        }

        #endregion



    }
}
