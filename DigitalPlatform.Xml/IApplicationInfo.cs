using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform.Xml
{
    public interface IApplicationInfo
    {
        void LinkFormState(Form form,
    string strCfgTitle);

        void UnlinkFormState(Form form);

        bool GetBoolean(string strPath,
    string strName,
    bool bDefault);

        void SetBoolean(string strPath,
    string strName,
    bool bValue);

        int GetInt(string strPath,
    string strName,
    int nDefault);

        void SetInt(string strPath,
    string strName,
    int nValue);

        string GetString(string strPath,
    string strName,
    string strDefault);

        void SetString(string strPathParam,
    string strName,
    string strValue);

        float GetFloat(string strPath,
    string strName,
    float fDefault);

        void SetFloat(string strPath,
    string strName,
    float fValue);

        void LoadFormStates(Form form,
    string strCfgTitle);

        void LoadFormStates(Form form,
    string strCfgTitle,
    FormWindowState default_state);

        void LoadFormMdiChildStates(Form form,
    string strCfgTitle);

        void LoadMdiChildFormStates(Form form,
    string strCfgTitle);

        void LoadMdiChildFormStates(Form form,
    string strCfgTitle,
    SizeStyle style);

        void LoadMdiChildFormStates(Form form,
    string strCfgTitle,
    SizeStyle style,
    int nDefaultWidth,
    int nDefaultHeight);

        void SaveMdiChildFormStates(Form form,
    string strCfgTitle);

        void SaveMdiChildFormStates(Form form,
    string strCfgTitle,
    SizeStyle style);

        void SaveFormStates(Form form,
    string strCfgTitle);
    }
}
