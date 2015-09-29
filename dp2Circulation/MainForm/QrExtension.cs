using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关 QR 识别的功能
    /// </summary>
    public partial class MainForm
    {
        #region QR 识别

        // 当前所允许的输入类型
        InputType m_inputType = InputType.None;

        // 当输入焦点进入 读者标识 编辑区 的时候触发
        internal void EnterPatronIdEdit(InputType inputtype)
        {
            m_inputType = inputtype;
            if (this.qrRecognitionControl1 != null)
                this.qrRecognitionControl1.StartCatch();
        }

        // 当输入焦点离开 读者标识 编辑区 的时候触发
        internal void LeavePatronIdEdit()
        {
            if (this.qrRecognitionControl1 != null)
                this.qrRecognitionControl1.EndCatch();
            // m_bDisableCamera = false;
        }

        // 清除防止重复的缓存条码号
        public void ClearQrLastText()
        {
            if (this.qrRecognitionControl1 != null)
                this.qrRecognitionControl1.LastText = "";
        }

        bool m_bDisableCamera = false;
        // string _cameraName = "";

        /// <summary>
        /// 摄像头禁止捕获
        /// </summary>
        public void DisableCamera()
        {
            //    _cameraName = this.qrRecognitionControl1.CurrentCamera;
            if (this.qrRecognitionControl1 != null
                && this.qrRecognitionControl1.InCatch == true)
            {
                this.qrRecognitionControl1.EndCatch();
                this.m_bDisableCamera = true;

                // this.qrRecognitionControl1.CurrentCamera = "";
            }
        }

        /// <summary>
        /// 摄像头恢复捕获
        /// </summary>
        public void EnableCamera()
        {
            //    this.qrRecognitionControl1.CurrentCamera = _cameraName;
            if (this.qrRecognitionControl1 != null
                && m_bDisableCamera == true)
            {
                this.qrRecognitionControl1.StartCatch();
                this.m_bDisableCamera = false;
            }
        }

        void qrRecognitionControl1_Catched(object sender, DigitalPlatform.Drawing.CatchedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text) == true)
                return;

            int nHitCount = 0;  // 匹配的次数
            if ((this.m_inputType & InputType.QR) == InputType.QR)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.QR_CODE) != 0)
                    nHitCount++;
            }
            // 检查是否属于 PQR 二维码
            if ((this.m_inputType & InputType.PQR) == InputType.PQR)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.QR_CODE) != 0
                    && StringUtil.HasHead(e.Text, "PQR:") == true)
                    nHitCount++;
            }
            // 检查是否属于 ISBN 一维码
            if ((this.m_inputType & InputType.EAN_BARCODE) == InputType.EAN_BARCODE)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.EAN_13) != 0
                    /* && IsbnSplitter.IsIsbn13(e.Text) == true */)
                    nHitCount++;
            }
            // 检查是否属于普通一维码
            if ((this.m_inputType & InputType.NORMAL_BARCODE) == InputType.NORMAL_BARCODE)
            {
                if ((e.BarcodeFormat & ZXing.BarcodeFormat.All_1D) > 0)
                    nHitCount++;
            }

            if (nHitCount > 0)
            {
                // SendKeys.Send(e.Text + "\r");
                Invoke(new Action<string>(SendKey), e.Text + "\r");
            }
            else
            {
                // TODO: 警告
            }
        }

        private void SendKey(string strText)
        {
            SendKeys.Send(strText);
        }

        #endregion

    }
}
