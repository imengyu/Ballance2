using Ballance2.CoreGame.GamePlay;

namespace Ballance2.Interfaces
{
    interface ILevelLoader
    {
        void UpdateLoaderProgress(float v);
        void LoadeErrorFail(string errText);
        void LoaderReportError(string errText);
        GameLevel GetCurrentLevel();
    }
}
