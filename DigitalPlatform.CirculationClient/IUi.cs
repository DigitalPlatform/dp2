using DigitalPlatform.LibraryClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 图形用户界面基本视觉接口
    /// </summary>
    public interface IUi
    {
        void SetProgress(long current, long total);
        void ShowMessage(string text);
        MessagePromptEventHandler Loader_Prompt { get; set; }
    }
}
