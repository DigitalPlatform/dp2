using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace DigitalPlatform
{
    /// <summary>
    /// 没有被用过?
    /// </summary>
    public class InstanceControl : Component
    {
        private string m_strProcName = null;

        /*constructor which holds the application name*/
        public InstanceControl(string strProcName)
        {
            m_strProcName = strProcName;
        }
        public bool IsAnyInstanceExist()
        {
            /*process class GetProcessesByName() checks for particular
      process is currently running and returns array of processes
      with that name*/

            Process[] processes = Process.GetProcessesByName(m_strProcName);

            if (processes.Length != 1)
                return false; /*false no instance exist*/
            else
                return true; /*true mean instance exist*/
        }
    }
}
