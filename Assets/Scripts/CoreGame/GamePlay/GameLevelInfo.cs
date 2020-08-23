using System.Collections.Generic;

namespace Ballance2.CoreGame.GamePlay
{
    [SLua.CustomLuaClass]
    public struct SkyCustomInfo
    {
        public string SkyL;
        public string SkyR;
        public string SkyF;
        public string SkyB;
        public string SkyU;
        public string SkyD;
    }
    [SLua.CustomLuaClass]
    public class GameLightInfo
    {
        public bool LightLHide = false;
        public bool LightRHide = false;
        public bool LightShadowHide = false;

        public string LightLColor = "#FFF";
        public string LightRColor = "#FFF";
        public string LightShadowColor = "#FFF";

        public float LightLIntensity = 0.3f;
        public float LightRIntensity = 0.3f;
        public float LightShadowIntensity = 0.4f;
    }
    [SLua.CustomLuaClass]
    public enum SkyLayerType
    {
        Flat,
        Votex,
        Custom,
    }
    [SLua.CustomLuaClass]
    public enum GameBgmType
    {
        None,
        Type1 = 1,
        Type2,
        Type3,
        Type4,
        Type5,
        Custom = 10,
    }
    [SLua.CustomLuaClass]
    public enum LevelErrorSolveType
    {
        Continue,
        Break,
        AskForUser,
    }
    [SLua.CustomLuaClass]
    public class LevelGroupModul
    {
        public string ModulName = "";
        public string ModulPackage = "";
        public List<string> Objects = new List<string>();

        public void Destroy()
        {
            Objects.Clear();
            Objects = null;
        }
    }
    [SLua.CustomLuaClass]
    public class LevelGroup
    {
        public string Name = "";

        public List<string> Objects = new List<string>();

        public void Destroy()
        {
            Objects.Clear();
            Objects = null;
        }
    }
    [SLua.CustomLuaClass]
    public class LevelGroupSector
    {
        public string SectorStart = "";

        public List<string> Objects = new List<string>();

        public void Destroy()
        {
            Objects.Clear();
            Objects = null;
        }
    }
}
