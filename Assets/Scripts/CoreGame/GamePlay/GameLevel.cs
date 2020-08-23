using Ballance2.Config;
using Ballance2.Interfaces;
using Ballance2.ModBase;
using Ballance2.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 关卡信息载体
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameLevel
    {
        private string _TAG = "";

        public GameLevel(string path, IModManager modManager)
        {
            FilePath = path;
            Name = GamePathManager.GetFileNameWithoutExt(path);
            _TAG = "GameLevel:" + Name;
            ModManager = modManager;
        }

        public IModManager ModManager { get; private set; }

        public string FilePath { get; private set; }

        public string Name { get; private set; }
        public string Author { get; private set; } = "未知";
        public string Introduction { get; private set; } = "暂无介绍";
        public string Logo { get; private set; } = "";
        public string Version { get; private set; } = "0.0";
        public int Difficulty { get; private set; } = 0;
        public string HelperLink { get; private set; } = "";
        public int SectorCount { get; private set; } = 0;

        public string BasePrefab { get; private set; } = "";
        public string Sky { get; private set; } = "A";
        public SkyCustomInfo SkyCustom { get { return skyCustom; } }
        public SkyLayerType SkyLayer { get; private set; } = SkyLayerType.Flat;
        public GameLightInfo Light { get { return light; } }
        public int StartLife { get; private set; } = 3;
        public int StartScore { get; private set; } = 1000;
        public int LevelScore { get; private set; } = 1000;
        public GameBgmType MusicTheme { get; private set; } = GameBgmType.Type1;
        public LevelErrorSolveType ErrorSolve { get; private set; } = LevelErrorSolveType.AskForUser;
        public List<string> MusicCustom { get; private set; } = new List<string>();

        public List<LevelGroupModul> GroupModuls { get; private set; } = new List<LevelGroupModul>();
        public List<LevelGroup> GroupFloors { get; private set; } = new List<LevelGroup>();
        public List<LevelGroup> GroupColSounds { get; private set; } = new List<LevelGroup>();
        public List<string> ResetPoints { get; private set; } = new List<string>();
        public List<LevelGroupSector> Sectors { get; private set; } = new List<LevelGroupSector>();
        public string LevelStart { get; private set; } = "";
        public string LevelEnd { get; private set; } = "";


        /// <summary>
        /// 名字标签
        /// </summary>
        public string TAG { get { return _TAG; } }
        /// <summary>
        /// 获取模组包是否直接在 Editor 加载
        /// </summary>
        public bool IsEditorPack { get; internal set; }
        /// <summary>
        /// 加载失败的错误信息
        /// </summary>
        public string LoadError { get; private set; }
        /// <summary>
        /// 加载状态
        /// </summary>
        public GameModStatus LoadStatus { get; private set; } = GameModStatus.NotInitialize;

        //自定义数据

        private Dictionary<string, object> modCustomData = new Dictionary<string, object>();

        public object AddCustomProp(string name, object data)
        {
            if (modCustomData.ContainsKey(name))
            {
                modCustomData[name] = data;
                return data;
            }
            modCustomData.Add(name, data);
            return data;
        }
        public object GetCustomProp(string name)
        {
            if (modCustomData.ContainsKey(name))
                return modCustomData[name];
            return null;
        }
        public object SetCustomProp(string name, object data)
        {
            if (modCustomData.ContainsKey(name))
            {
                object old = modCustomData[name];
                modCustomData[name] = data;
                return old;
            }
            return null;
        }
        public bool RemoveCustomProp(string name)
        {
            if (modCustomData.ContainsKey(name))
            {
                modCustomData.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 关卡定义文件
        /// </summary>
        public XmlDocument LevelDef { get { return levelDefXmlDoc; } }
        /// <summary>
        /// 关卡定义文件文本
        /// </summary>
        public string LevelDefText { get; private set; }
        /// <summary>
        /// 关卡Logo
        /// </summary>
        public Sprite LevelLogo { get; private set; }
        /// <summary>
        /// 关卡适配信息
        /// </summary>
        public GameCompatibilityInfo LevelCompatibilityInfo { get { return levelCompatibilityInfo; } }
        /// <summary>
        /// 关卡依赖信息
        /// </summary>
        public List<GameDependencyInfo> LevelDependencyInfos { get { return levelDependencyInfos; } }
        /// <summary>
        /// 关卡AssetBundle
        /// </summary>
        public AssetBundle LevelAssetBundle { get; private set; } = null;

        private SkyCustomInfo skyCustom = new SkyCustomInfo();
        private GameLightInfo light = new GameLightInfo();
        private GameCompatibilityInfo levelCompatibilityInfo = new GameCompatibilityInfo(GameConst.GameBulidVersion, GameConst.GameBulidVersion, GameConst.GameBulidVersion);
        private XmlDocument levelDefXmlDoc = new XmlDocument();
        private List<GameDependencyInfo> levelDependencyInfos = new List<GameDependencyInfo>();

        private bool inited = false;

        //初始化
        internal bool Init()
        {
            if (IsEditorPack) inited = true;
            if (inited) return true;

            if (FileUtils.TestFileIsZip(FilePath))
            {
                inited = true;
                return LoadBaseInfo();
            }

            //文件格式不支持
            GameErrorManager.LastError = GameError.BadFileType;
            GameLogger.Error(TAG, "Level package not support {0}", FilePath);
            return false;
        }
        internal void ForceLoadEditorPack(TextAsset modDef)
        {
            inited = true;
            ReadLevelDef(modDef.text);
        }

        public IEnumerator Load()
        {
            if (LevelAssetBundle == null && LoadStatus == GameModStatus.NotInitialize)
            {
                LoadStatus = GameModStatus.Loading;
                   
                Task<MemoryStream> task = LoadAssetBundleToMemoryInZipAsync();
                yield return new WaitUntil(() => task.IsCompleted);

                MemoryStream ms = task.Result;
                if (ms == null)
                {
                    LoadStatus = GameModStatus.InitializeFailed;
                    LoadError = "加载失败： AssetBundle 未找到";
                    yield break;
                }
                //加载 AssetBundle
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(ms.ToArray());

                ms.Close();
                ms.Dispose();

                yield return createRequest;

                LevelAssetBundle = createRequest.assetBundle;

                if(LevelAssetBundle ==  null)
                {
                    LoadStatus = GameModStatus.InitializeFailed;
                    LoadError = "加载失败： AssetBundle 无效";
                    yield break;
                }

                LoadStatus = GameModStatus.InitializeSuccess;
            }
        }
        //释放
        public void UnLoad()
        {
            MusicCustom.Clear();
            GroupModuls.Clear();
            GroupFloors.Clear();
            GroupColSounds.Clear();
            ResetPoints.Clear();
            Sectors.Clear();

            LevelAssetBundle.Unload(true);
            LevelAssetBundle = null;
            LoadStatus = GameModStatus.NotInitialize;
        }
        public void Destroy()
        {
            UnLoad();

            if (levelDependencyInfos != null)
            {
                levelDependencyInfos.Clear();
                levelDependencyInfos = null;
            }
            levelDefXmlDoc = null;
            light = null;
            if (modCustomData != null)
            {
                modCustomData.Clear();
                modCustomData = null;
            }
        }

        //加载完整 信息
        public void LoadFullInfo()
        {
            ReadLevelDefFull();
        }

        //加载基础信息
        private bool LoadBaseInfo()
        {
            bool hasDefFile = false;

            //在zip中加载LevelDef
            ZipInputStream zip = ZipUtils.OpenZipFile(FilePath);
            ZipEntry theEntry;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                if (theEntry.Name == "/LevelDef.xml" || theEntry.Name == "LevelDef.xml")
                {
                    hasDefFile = true;
                    LoadLevelDefInZip(zip, theEntry);
                }
                else if (theEntry.Name == "/" + Logo || theEntry.Name == Logo)
                    LoadLogoInZip(zip, theEntry);
            }
            zip.Close();
            zip.Dispose();

            if (!hasDefFile)
            {
                GameLogger.Error(TAG, "加载模组包失败，未找到 LevelDef.xml");
                GameErrorManager.LastError = GameError.InitializationFailed;

                LoadStatus = GameModStatus.InitializeFailed;
                LoadError = "未找到 ModDef.xml";

                return false;
            }

            //检查兼容性
            if (LevelCompatibilityInfo.MinVersion > GameConst.GameBulidVersion)
            {
                GameLogger.Error(TAG, "加载模组包失败，关卡与游戏版本不兼容");
                GameErrorManager.LastError = GameError.BadMod;

                LoadStatus = GameModStatus.BadMod;
                LoadError = "关卡与游戏版本不兼";

                return false;
            }

            return true;
        }
        //在zip中加载AssetBundle
        private async Task<MemoryStream> LoadAssetBundleToMemoryInZipAsync()
        {
            ZipInputStream zip = ZipUtils.OpenZipFile(FilePath);
            ZipEntry theEntry;
            while ((theEntry = zip.GetNextEntry()) != null)
            {
                if (theEntry.Name == "Level.assetbundle"
                    || theEntry.Name == "/Level.assetbundle")
                {
                    MemoryStream ms = await ZipUtils.ReadZipFileToMemoryAsync(zip);
                    zip.Close();
                    zip.Dispose();
                    return ms;
                }
            }
            zip.Close();
            zip.Dispose();
            return null;
        }

        private string LevelDefInZip = "";

        private void LoadLevelDefInZip(ZipInputStream zip, ZipEntry theEntry)
        {
            MemoryStream ms = ZipUtils.ReadZipFileToMemory(zip);
            LevelDefInZip = Encoding.UTF8.GetString(ms.ToArray());
            ReadLevelDef(LevelDefInZip);
            ms.Close();
            ms.Dispose();
        }
        private void LoadLogoInZip(ZipInputStream zip, ZipEntry theEntry)
        {
            try
            {
                Texture2D texture2D = new Texture2D(128, 128);
                MemoryStream ms = ZipUtils.ReadZipFileToMemory(zip);
                texture2D.LoadImage(ms.ToArray());
                ms.Close();
                ms.Dispose();

                LevelLogo = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), new Vector2(0.5f, 0.5f));
            }
            catch (Exception e)
            {
                LevelLogo = null;
                GameLogger.Error(TAG, "在加载关卡的 Logo {0} 失败\n错误信息：{1}", Logo, e.ToString());
            }
        }

        //读取模组定义文件
        private void ReadLevelDef(string defText)
        {
            LevelDefText = defText;
            try
            {
                levelDefXmlDoc = new XmlDocument();
                levelDefXmlDoc.LoadXml(defText);

                XmlNode nodeMod = levelDefXmlDoc.SelectSingleNode("Level");
                foreach (XmlNode node in nodeMod.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "BaseInfo": ReadLevelDefBaseInfo(node); break;
                        case "Compatibility": ReadLevelDefCompatibility(node); break;
                    }
                }
            }
            catch (Exception e)
            {
                GameLogger.Error(TAG, (object)("Failed to solve LevelDef.xml : " + e.Message));
                GameLogger.Exception(e);
            }
        }
        private void ReadLevelDefCompatibility(XmlNode nodeCompatibility)
        {
            foreach (XmlNode node in nodeCompatibility.ChildNodes)
            {
                switch (node.Name)
                {
                    case "MinVersion":
                        if (!int.TryParse(node.InnerText, out levelCompatibilityInfo.MinVersion))
                            GameLogger.Warning(TAG, "Bad MinVersion : {0}", node.InnerText);
                        break;
                    case "TargetVersion":
                        if (!int.TryParse(node.InnerText, out levelCompatibilityInfo.TargetVersion))
                            GameLogger.Warning(TAG, "Bad TargetVersion : {0}", node.InnerText);
                        break;
                    case "MaxVersion":
                        if (!int.TryParse(node.InnerText, out levelCompatibilityInfo.MaxVersion))
                            GameLogger.Warning(TAG, "Bad MaxVersion : {0}", node.InnerText);
                        break;
                }
            }
        }
        private void ReadLevelDefDependencies(XmlNode nodeBaseInfo)
        {
            levelDependencyInfos.Clear();
            foreach (XmlNode node in nodeBaseInfo.ChildNodes)
            {
                if (node.Name == "Package")
                {
                    GameDependencyInfo dependencyInfo = new GameDependencyInfo();
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        switch (attribute.Name)
                        {
                            case "name": dependencyInfo.PackageName = attribute.Value; break;
                            case "minVer": dependencyInfo.MinVersion = attribute.Value; break;
                            case "mustLoad": dependencyInfo.MustLoad = attribute.Value == "true"; break;
                        }
                    }
                    if (!string.IsNullOrEmpty(dependencyInfo.PackageName))
                        levelDependencyInfos.Add(dependencyInfo);
                }
            }
        }
        private void ReadLevelDefBaseInfo(XmlNode nodeBaseInfo)
        {
            foreach (XmlNode node in nodeBaseInfo.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Name": Name = node.InnerText; break;
                    case "Author": Author = node.InnerText; break;
                    case "Introduction": Introduction = node.InnerText; break;
                    case "Logo": Logo = node.InnerText; break;
                    case "Version": Version = node.InnerText; break;
                    case "HelperLink": HelperLink = node.InnerText; break;
                    case "Difficulty": { int i; if(int.TryParse(node.InnerText, out i)) Difficulty = i; break; }
                    case "SectorCount": { int i; if (int.TryParse(node.InnerText, out i)) SectorCount = i; break; }
                    case "Dependencies": ReadLevelDefDependencies(node); break;
                }
            }
        }
        private void ReadLevelDefFull()
        {
            try
            {
                XmlNode nodeMod = levelDefXmlDoc.SelectSingleNode("Level");
                foreach (XmlNode node in nodeMod.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Game": ReadLevelDefGame(node); break;
                        case "Groups": ReadLevelDefGroup(node); break;
                        case "Data": break;
                        default:
                            if (ModManager.LevelDefCustomPropertySolver.ContainsKey(node.Name))
                                ModManager.LevelDefCustomPropertySolver[node.Name](node, this);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                GameLogger.Error(TAG, (object)("Failed to solve LevelDef.xml : " + e.Message));
                GameLogger.Exception(e);
            }
        }
        private void ReadLevelDefGame(XmlNode nodeBaseInfo)
        {
            foreach (XmlNode node in nodeBaseInfo.ChildNodes)
            {
                switch (node.Name)
                {
                    case "BasePrefab": BasePrefab = node.InnerText; break;
                    case "Sky": Sky = node.InnerText; break;
                    case "SkyCustom":
                        {
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                switch (node2.Name)
                                {
                                    case "SkyL": skyCustom.SkyB = node2.InnerText; break;
                                    case "SkyR": skyCustom.SkyR = node2.InnerText; break;
                                    case "SkyU": skyCustom.SkyU = node2.InnerText; break;
                                    case "SkyD": skyCustom.SkyD = node2.InnerText; break;
                                    case "SkyF": skyCustom.SkyF = node2.InnerText; break;
                                    case "SkyB": skyCustom.SkyB = node2.InnerText; break;
                                }
                            }
                            break;
                        }
                    case "SkyLayer":
                        {
                            SkyLayerType skyLayerType;
                            if (Enum.TryParse(node.InnerText, out skyLayerType))
                                SkyLayer = skyLayerType;
                            break;
                        }
                    case "Light":
                        {
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                switch (node2.Name)
                                {
                                    case "L":
                                        {
                                            light.LightLColor = node2.InnerText;
                                            foreach (XmlAttribute attribute in node2.Attributes)
                                            {
                                                if(attribute.Name == "hideLight")
                                                    light.LightLHide = bool.Parse(attribute.InnerText);
                                                else if (attribute.Name == "intensity")
                                                    light.LightLIntensity = float.Parse(attribute.InnerText);
                                            }
                                            break;
                                        }
                                    case "R":
                                        {
                                            light.LightRColor = node2.InnerText;
                                            foreach (XmlAttribute attribute in node2.Attributes)
                                            {
                                                if (attribute.Name == "hideLight")
                                                    light.LightRHide = bool.Parse(attribute.InnerText);
                                                else if (attribute.Name == "intensity")
                                                    light.LightRIntensity = float.Parse(attribute.InnerText);
                                            }
                                            break;
                                        }
                                    case "Shadow":
                                        {
                                            light.LightShadowColor = node2.InnerText;
                                            foreach (XmlAttribute attribute in node2.Attributes)
                                            {
                                                if (attribute.Name == "hideLight")
                                                    light.LightShadowHide = bool.Parse(attribute.InnerText);
                                                else if (attribute.Name == "intensity")
                                                    light.LightShadowIntensity = float.Parse(attribute.InnerText);
                                            }
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    case "Energy":
                        {
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                switch (node2.Name)
                                {
                                    case "StartLife": { int i; if (int.TryParse(node2.InnerText, out i)) StartLife = i; break; }
                                    case "StartScore": { int i; if (int.TryParse(node2.InnerText, out i)) StartScore = i; break; }
                                    case "LevelScore": { int i; if (int.TryParse(node2.InnerText, out i)) LevelScore = i; break; }
                                }
                            }
                            break;
                        }
                    case "MusicCustom":
                        {
                            MusicCustom.Clear();
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                if (node2.Name == "Music")
                                    MusicCustom.Add(node2.InnerText);
                            }
                            break;
                        }
                    case "MusicTheme": { GameBgmType i; if (Enum.TryParse(node.InnerText, out i)) MusicTheme = i; break; }
                    case "ErrorSolve": { LevelErrorSolveType i; if (Enum.TryParse(node.InnerText, out i)) ErrorSolve = i; break; }
                }
            }
        }
        private void ReadLevelDefGroup(XmlNode nodeBaseInfo)
        {
            foreach (XmlNode node in nodeBaseInfo.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Moduls":
                        {
                            Sectors.Clear();
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                LevelGroupModul group = new LevelGroupModul();
                                group.ModulName = node2.Name;

                                if (node2.Attributes["packageName"] != null)
                                    group.ModulPackage = node2.Attributes["packageName"].Value;

                                foreach (XmlNode node3 in node2.ChildNodes)
                                {
                                    if (node3.Name == "Object")
                                        group.Objects.Add(node3.InnerText);
                                }

                                GroupModuls.Add(group);
                            }
                            break;
                        }
                    case "Floors":
                        {
                            Sectors.Clear();
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                LevelGroup group = new LevelGroup();
                                group.Name = node2.Name;

                                foreach (XmlNode node3 in node2.ChildNodes)
                                {
                                    if (node3.Name == "Object")
                                        group.Objects.Add(node3.InnerText);
                                }

                                GroupFloors.Add(group);
                            }
                            break;
                        }
                    case "ColSounds":
                        {
                            Sectors.Clear();
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                LevelGroup group = new LevelGroup();
                                group.Name = node2.Name;

                                foreach (XmlNode node3 in node2.ChildNodes)
                                {
                                    if (node3.Name == "Object")
                                        group.Objects.Add(node3.InnerText);
                                }

                                GroupColSounds.Add(group);
                            }
                            break;
                        }
                    case "LevelStart": LevelStart = node.InnerText; break;
                    case "LevelEnd": LevelEnd = node.InnerText; break;
                    case "ResetPoints":
                        {
                            ResetPoints.Clear();
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                if (node2.Name == "ResetPoint")
                                    ResetPoints.Add(node2.InnerText);
                            }
                            break;
                        }
                    case "Sectors":
                        {
                            Sectors.Clear();
                            foreach (XmlNode node2 in node.ChildNodes)
                            {
                                if (node2.Name == "Sector")
                                {
                                    LevelGroupSector sector = new LevelGroupSector();
                                    if (node2.Attributes["sectorStart"] != null)
                                        sector.SectorStart = node2.Attributes["sectorStart"].Value;

                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.Name == "Object")
                                            sector.Objects.Add(node3.InnerText);
                                    }

                                    Sectors.Add(sector);
                                }
                            }
                            break;
                        }
                }
            }
        }
    }

    

}
