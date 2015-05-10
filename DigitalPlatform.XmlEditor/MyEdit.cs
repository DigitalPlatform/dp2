using System;
using System.Windows.Forms;
using System.Drawing;

namespace DigitalPlatform.Xml
{
	//派生的TextBox
    public class MyEdit : TextBox
    {
        public XmlEditor XmlEditor = null;

        // 初始化小编辑控件
        // parameters:
        //      xmlEditor   XmlEditor对象
        // return:
        //      -1  出错
        //      0   成功
        public int Initial(XmlEditor xmlEditor,
            out string strError)
        {
            strError = "";
            this.XmlEditor = xmlEditor;

            this.ImeMode = ImeMode.Off;
            this.BorderStyle = BorderStyle.None;
            this.BackColor = this.XmlEditor.BackColorDefaultForEditable;
            this.Font = this.XmlEditor.FontTextDefault;
            this.Multiline = true;
            this.XmlEditor.Controls.Add(this);
            return 0;
        }


        // 接管Ctrl+各种键
        protected override bool ProcessDialogKey(
            Keys keyData)
        {

            // Ctrl + A 自动录入功能
            if ((keyData & Keys.Control) == Keys.Control
                && (keyData & (~Keys.Control)) == Keys.A)   // 2007/5/15 修改，原来的行是CTRL+C和CTRL+A都起作用，CTRL+C是副作用。
                // && (keyData & Keys.A) == Keys.A)
            {
                if (this.XmlEditor != null)
                {
                    GenerateDataEventArgs ea = new GenerateDataEventArgs();
                    this.XmlEditor.OnGenerateData(ea);
                    return true;
                }
            }

            return false;
        }


        // 鼠标卷滚时
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.XmlEditor.MyOnMouseWheel(e);
        }

        // 获得焦点
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            this.XmlEditor.Enabled = true;
        }

        // 失去焦点
        protected override void OnLostFocus(EventArgs e)
        {
            this.XmlEditor.Flush();
            base.OnLostFocus(e);
        }


        // 解决上移下移
        // 当光标移到文字顶时，再向上移则移到上一个Item
        // 当光标移到文字底时，再向下移则移到下一个Item
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        // 当光标移到文字顶时，再向上移则移到上一个Item

                        // 得到Edit的插入符位置
                        int x, y;
                        API.GetEditCurrentCaretPos(this,
                            out x,
                            out y);

                        // 如果位置为0
                        if (y == 0)
                        {
                            POINT point = new POINT();
                            point.x = 0;
                            point.y = 0;
                            bool bRet = API.GetCaretPos(ref point);  //得到插入符

                            // 得到当前的Item的上一个Item
                            Item frontItem = ItemUtil.GetNearItem(this.XmlEditor.m_selectedItem,
                                MoveMember.Front);

                            if (frontItem != null)
                            {
                                // 设为当前的Item
                                //this.m_selectedItem = frontItem;
                                this.XmlEditor.SetCurText(frontItem, null);
                                this.XmlEditor.SetActiveItem(frontItem);
                                if (this.XmlEditor.m_curText != null)
                                {
                                    // 手工单击一下，确保移动到对应的位置
                                    point.y = this.ClientSize.Height - 2;
                                    API.SendMessage(this.Handle,
                                        API.WM_LBUTTONDOWN,
                                        new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                        API.MakeLParam(point.x, point.y));

                                    API.SendMessage(this.Handle,
                                        API.WM_LBUTTONUP,
                                        new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                        API.MakeLParam(point.x, point.y));
                                }
                                e.Handled = true;
                                this.XmlEditor.Invalidate();
                            }
                        }
                    }
                    break;
                case Keys.Down:
                    {
                        // 当光标移到文字底时，再向下移则移到下一个Item

                        int x, y;
                        API.GetEditCurrentCaretPos(this,
                            out x,
                            out y);

                        // 得到当前edit的行号
                        int nTemp = API.GetEditLines(this);


                        // 不是根元素
                        if (y >= nTemp - 1 && (!(this.XmlEditor.m_selectedItem is VirtualRootItem)))
                        {
                            POINT point = new POINT();
                            point.x = 0;
                            point.y = 0;
                            bool bRet = API.GetCaretPos(ref point);

                            // 得到下一个Item
                            Item behindItem = ItemUtil.GetNearItem(this.XmlEditor.m_selectedItem,
                                MoveMember.Behind);

                            if (behindItem != null)
                            {
                                //this.m_selectedItem = behindItem; 
                                this.XmlEditor.SetCurText(behindItem, null);
                                this.XmlEditor.SetActiveItem(behindItem);



                                if (this.XmlEditor.m_curText != null)
                                {
                                    // 模拟单击一下
                                    point.y = 1;
                                    API.SendMessage(this.Handle,
                                        API.WM_LBUTTONDOWN,
                                        new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                        API.MakeLParam(point.x, point.y));

                                    API.SendMessage(this.Handle,
                                        API.WM_LBUTTONUP,
                                        new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                        API.MakeLParam(point.x, point.y));
                                }
                                e.Handled = true;

                                this.XmlEditor.Invalidate();
                            }
                        }
                    }
                    break;
                case Keys.Left:
                    break;
                case Keys.Right:
                    break;
                default:
                    break;
            }
        }


        // 把文字大于输入框时，把输入框变大
        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    break;

                default:
                    {
                        bool bChanged = false;

                        //解决Edit随着文字变大的问题
                        API.SendMessage(this.Handle,
                            API.EM_LINESCROLL,
                            0,
                            (int)1000);
                        while (true)
                        {
                            int nFirstLine = API.GetEditFirstVisibleLine(this); //得到edit可见的第一行

                            if (nFirstLine != 0) //不等于0，表明变大了
                            {
                                bChanged = true;
                                this.Size = new Size(this.Size.Width,
                                    this.Size.Height + 10);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (bChanged)
                        {
                            this.AfterEditControlHeightChanged(this.XmlEditor.m_curText);
                        }

                    }
                    break;
            }
        }



        // edit高度发生变化
        private void AfterEditControlHeightChanged(Visual visual)
        {
            if (visual == null)
                return;
            int nOldHeight = 0;
            nOldHeight = visual.Rect.Height;

            //把文字送入item
            // ??????引向未完整的数据变成xml，抛出错误
            //this.EditControlTextToVisual();

            //重新计算
            int nRetWidth, nRetHeight;
            visual.Layout(visual.Rect.X,
                visual.Rect.Y,
                visual.Rect.Width,
                this.Size.Height,// + visual.TopBlank + visual.BottomBlank + 2*visual.BorderHorzHeight ,//visual.rect .Height ,
                this.XmlEditor.nTimeStampSeed++,
                out nRetWidth,
                out nRetHeight,
                LayoutMember.Layout);


            // ???????设当前edit的大小及位置



            int nNewHeight = visual.Rect.Height;

            int nDelta = nNewHeight - nOldHeight;

            if (nDelta != 0)
            {
                //上级
                Visual containerVisual = visual.container;
                nNewHeight += containerVisual.TotalRestHeight;

                if (containerVisual != null)
                {
                    containerVisual.Layout(containerVisual.Rect.X,
                        containerVisual.Rect.Y,
                        containerVisual.Rect.Width,
                        nNewHeight,
                        this.XmlEditor.nTimeStampSeed,
                        out nRetWidth,
                        out nRetHeight,
                        LayoutMember.EnLargeHeight | LayoutMember.Up);
                }
                //Visual.ChangeSibling (visual,-1,nNewHeight);
            }

            if (nDelta != 0
                && this.XmlEditor.m_curText != null)
            {
                //这里优先，使用ScrollWindowEx
            }

            if (nDelta != 0)
            {
                this.XmlEditor.AfterDocumentChanged(ScrollBarMember.Vert);
            }

            this.XmlEditor.Invalidate();

            //也需要做优化的事情
        }

    }
}
