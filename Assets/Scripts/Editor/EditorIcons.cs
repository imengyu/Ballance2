using System;
using UnityEditor;
using UnityEngine;

namespace Ballance2.Editor
{
    class EditorIcons : EditorWindow
    {
        static string[] text;

        public EditorIcons()
        {
            titleContent = new GUIContent("Unity 系统内置图标查看");
        }

        [MenuItem("Tools/GUI/Editor Icon")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(EditorIcons));
            text = @"TreeEditor.AddLeaves
TreeEditor.AddBranches
TreeEditor.Trash
TreeEditor.Duplicate
TreeEditor.Refresh
editicon.sml
tree_icon_branch_frond
tree_icon_branch
tree_icon_frond
tree_icon_leaf
tree_icon
animationvisibilitytoggleon
animationvisibilitytoggleoff
MonoLogo
AgeiaLogo
AboutWindow.MainHeader
Animation.AddEvent
lightMeter/greenLight
lightMeter/lightRim
lightMeter/orangeLight
lightMeter/redLight
Animation.PrevKey
Animation.NextKey
Animation.AddKeyframe
Animation.EventMarker
Animation.Play
Animation.Record
AS Badge Delete
AS Badge Move
AS Badge New
WelcomeScreen.AssetStoreLogo
preAudioAutoPlayOff
preAudioAutoPlayOn
preAudioPlayOff
preAudioPlayOn
preAudioLoopOff
preAudioLoopOn
AvatarInspector/BodySilhouette
AvatarInspector/HeadZoomSilhouette
AvatarInspector/LeftHandZoomSilhouette
AvatarInspector/RightHandZoomSilhouette
AvatarInspector/Torso
AvatarInspector/Head
AvatarInspector/LeftArm
AvatarInspector/LeftFingers
AvatarInspector/RightArm
AvatarInspector/RightFingers
AvatarInspector/LeftLeg
AvatarInspector/RightLeg
AvatarInspector/HeadZoom
AvatarInspector/LeftHandZoom
AvatarInspector/RightHandZoom
AvatarInspector/DotFill
AvatarInspector/DotFrame
AvatarInspector/DotFrameDotted
AvatarInspector/DotSelection
SpeedScale
AvatarPivot
Avatar Icon
Mirror
AvatarInspector/BodySIlhouette
AvatarInspector/BodyPartPicker
AvatarInspector/MaskEditor_Root
AvatarInspector/LeftFeetIk
AvatarInspector/RightFeetIk
AvatarInspector/LeftFingersIk
AvatarInspector/RightFingersIk
BuildSettings.SelectedIcon
SocialNetworks.UDNLogo
SocialNetworks.LinkedInShare
SocialNetworks.FacebookShare
SocialNetworks.Tweet
SocialNetworks.UDNOpen
Clipboard
Toolbar Minus
ClothInspector.PaintValue
EditCollider
EyeDropper.Large
ColorPicker.CycleColor
ColorPicker.CycleSlider
PreTextureMipMapLow
PreTextureMipMapHigh
PreTextureAlpha
PreTextureRGB
Icon Dropdown
UnityLogo
Profiler.PrevFrame
Profiler.NextFrame
GameObject Icon
Prefab Icon
PrefabNormal Icon
PrefabModel Icon
ScriptableObject Icon
sv_icon_none
PreMatLight0
PreMatLight1
Toolbar Plus
Camera Icon
PreMatSphere
PreMatCube
PreMatCylinder
PreMatTorus
PlayButton
PauseButton
HorizontalSplit
VerticalSplit
BuildSettings.Web.Small
js Script Icon
cs Script Icon
boo Script Icon
Shader Icon
TextAsset Icon
AnimatorController Icon
AudioMixerController Icon
RectTransformRaw
RectTransformBlueprint
MoveTool
MeshRenderer Icon
Terrain Icon
SceneviewLighting
SceneviewFx
SceneviewAudio
SettingsIcon
TerrainInspector.TerrainToolRaise
TerrainInspector.TerrainToolSetHeight
TerrainInspector.TerrainToolSmoothHeight
TerrainInspector.TerrainToolSplat
TerrainInspector.TerrainToolTrees
TerrainInspector.TerrainToolPlants
TerrainInspector.TerrainToolSettings
RotateTool
ScaleTool
RectTool
MoveTool On
RotateTool On
ScaleTool On
RectTool On
ViewToolOrbit
ViewToolMove
ViewToolZoom
ViewToolOrbit On
ViewToolMove On
ViewToolZoom On
StepButton
PlayButtonProfile
PlayButton On
PauseButton On
StepButton On
PlayButtonProfile On
PlayButton Anim
PauseButton Anim
StepButton Anim
PlayButtonProfile Anim
WelcomeScreen.MainHeader
WelcomeScreen.VideoTutLogo
WelcomeScreen.UnityBasicsLogo
WelcomeScreen.UnityForumLogo
WelcomeScreen.UnityAnswersLogo
Toolbar Plus More".Split("\n"[0]);
        }

        public Vector2 scrollPosition;

        private GUIStyle window = null;

        private void OnEnable()
        {
            
        }
        private void OnGUI()
        {
            if(window == null)
                window = GUI.skin.FindStyle("window");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(window, GUILayout.Width(120));//鼠标放在按钮上的样式
            GUILayout.Label("鼠标放在按钮上的样式");

            foreach (MouseCursor item in Enum.GetValues(typeof(MouseCursor)))
            {
                GUILayout.Button(Enum.GetName(typeof(MouseCursor), item));
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), item);
                GUILayout.Space(10);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(window);   //内置图标
            GUILayout.Label("内置图标");

            for (int i = 0; i < text.Length; i += 8)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 8; j++)
                {
                    int index = i + j;
                    if (index < text.Length)
                        GUILayout.Button(EditorGUIUtility.IconContent(text[index]), GUILayout.Width(50), GUILayout.Height(30));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }
    }
}
