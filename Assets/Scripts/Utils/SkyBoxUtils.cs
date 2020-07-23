using Ballance2.Interfaces;
using Ballance2.ModBase;
using UnityEngine;

namespace Ballance2.Utils
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 天空盒生成器
    /// </summary>
    public static class SkyBoxUtils
    {
        private const string TAG = "SkyBoxUtils";

        private static GameMod skyAssetPack = null;
        private static IModManager ModManager = null;

        /// <summary>
        /// 创建预制的天空盒
        /// </summary>
        /// <param name="s">天空盒名字，（必须是 A~K ，对应原版游戏11个天空）</param>
        /// <returns>返回创建好的天空盒材质</returns>
        public static Material MakeSkyBox(string s)
        { 
            if (ModManager == null) ModManager = (IModManager)GameManager.GetManager("ModManager");
            if (skyAssetPack == null) skyAssetPack = ModManager.FindGameMod("core.assets.skys");
            if (skyAssetPack == null)
            {
                GameLogger.Error(TAG, "MakeSkyBox failed because skybase pack core.assets.skys not load !");
                return null;
            }

            Texture SkyLeft = skyAssetPack.GetAsset<Texture>("Sky_"+s+"_Left.BMP");
            Texture SkyRight = skyAssetPack.GetAsset<Texture>("Sky_" + s + "_Right.BMP");
            Texture SkyFront = skyAssetPack.GetAsset<Texture>("Sky_" + s + "_Front.BMP");
            Texture SkyBack = skyAssetPack.GetAsset<Texture>("Sky_" + s + "_Back.BMP");
            Texture SkyDown = skyAssetPack.GetAsset<Texture>("Sky_" + s + "_Down.BMP");

            return MakeCustomSkyBox(SkyLeft, SkyRight, SkyFront, SkyBack, SkyDown, null);
        }
        /// <summary>
        /// 创建自定义天空盒
        /// </summary>
        /// <param name="SkyLeft">左边的图像</param>
        /// <param name="SkyRight">右边的图像</param>
        /// <param name="SkyFront">前边的图像</param>
        /// <param name="SkyBack">后边的图像</param>
        /// <param name="SkyDown">下边的图像</param>
        /// <returns>返回创建好的天空盒材质</returns>
        public static Material MakeCustomSkyBox(Texture SkyLeft, Texture SkyRight, Texture SkyFront, Texture SkyBack, Texture SkyDown, Texture SkyTop)
        {
            Material m = new Material(Shader.Find("Skybox/6 Sided"));
            m.SetTexture("_FrontTex", SkyFront);
            m.SetTexture("_BackTex", SkyBack);
            m.SetTexture("_LeftTex", SkyRight);
            m.SetTexture("_RightTex", SkyLeft);
            m.SetTexture("_DownTex", SkyDown);
            m.SetTexture("_TopTex", SkyTop);
            return m;
        }
    }
}
