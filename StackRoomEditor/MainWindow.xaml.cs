using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Deployment.Application;

using System.Windows.Media.Media3D;
using _3DTools;
using Petzold.Media3D;

//using System.Drawing;
//using System.Drawing.Drawing2D;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
//using DigitalPlatform.Drawing;

namespace StackRoomEditor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string DataDir = "";

        string m_strTextPanelDef = "";

        ShelfInfoWindow m_propertyDlg = null;

        // 书架模板
        public List<BookShelf> m_shelfModels = new List<BookShelf>();

        public Floor Floor = null;

        // 全部书架
        public List<BookShelf> m_shelfs = new List<BookShelf>();

        System.Windows.Point startPosition;
        // Visual3D modelHit = null;

        // Visual3D m_currentModel = null;
        List<BookShelf> m_selectedShelfs = new List<BookShelf>();

        // 拖动时临时指定物体。MouseUp以后就清除了
        List<BookShelf> m_dragShelfs = new List<BookShelf>();

        // 延迟ClearSelection要求。数组中的是*不要*清除的对象
        List<BookShelf> m_clearSelections = new List<BookShelf>();

        bool m_bChanged = false;
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
                if (value == true)
                {
                    this.MenuItem_save.IsEnabled = true;
                }
                else
                {
                    this.MenuItem_save.IsEnabled = false;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            // viewport.Children.Add(CreateFloor());


            this.Changed = false;

            viewport.MouseLeftButtonDown += (sender, e) =>
            {
                this.m_clearSelections.Clear();
                m_bDraged = false;
                viewport.CaptureMouse();

                startPosition = e.GetPosition(viewport);
                HitTestResult result = VisualTreeHelper.HitTest(viewport, startPosition);
                ModelVisual3D visual3D = result.VisualHit as ModelVisual3D; // Visual3D
                if (result != null && result.VisualHit != null && visual3D != null)
                {
                    // modelHit = visual3D;

                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        // 复选。此时不允许拖动
                        this.Select(visual3D, "toggle");
                    }
                    else
                    {
                        // 如果已经选择了多于一个，并且当前物体已经在选择列表中，则可以拖动
                        if (this.m_selectedShelfs != null
                            && this.m_selectedShelfs.Count > 1)
                        {
                            if (this.GetSelectedModel(visual3D) != null)
                            {
                                this.m_dragShelfs.AddRange(this.m_selectedShelfs);
                            }
                        }

                        // 延迟ClearSelection()
                        BookShelf temp = this.Select(visual3D, "select");
                        if (temp != null)
                            this.m_clearSelections.Add(temp);

                        if (this.m_dragShelfs.Count == 0)
                        {
                            this.m_dragShelfs.AddRange(this.m_clearSelections);

                            this.ClearSelection(this.m_clearSelections);  // 单选的时候，立即兑现清除，不延迟
                            this.m_clearSelections.Clear();
                        }
                    }

                }
            };

            viewport.MouseMove += (sender, e) =>
            {
                if (this.m_dragShelfs != null && this.m_dragShelfs.Count > 0
                    && e.LeftButton == MouseButtonState.Pressed)
                {
                    Point endPosition = e.GetPosition(viewport);


                    foreach (BookShelf shelf in this.m_dragShelfs)
                    {
                        Visual3D modelHit = shelf.Model;

                        try
                        {
                            Vector3D vector3D = GetTranslationVector3D(modelHit, startPosition, endPosition);


#if NO
                            Matrix3D matrix3D = modelHit.Transform.Value;
                            vector3D += new Vector3D(matrix3D.OffsetX, matrix3D.OffsetY, matrix3D.OffsetZ);
                            matrix3D.OffsetX = vector3D.X;
                            matrix3D.OffsetY = vector3D.Y;
                            matrix3D.OffsetZ = vector3D.Z;
                            modelHit.Transform = new MatrixTransform3D(matrix3D);
#endif

                            shelf.LocationX += vector3D.X;
                            shelf.LocationZ += vector3D.Z;
                            shelf.MoveToPosition();
                            shelf.RotateToDirection();

                        }
                        catch
                        {
                            return;
                        }

                        // shelf.SavePosition();
                        m_bDraged = true;
                    }
                    startPosition = endPosition;
                    this.Changed = true;

                }
            };

            viewport.MouseUp += (sender, e) =>
            {
                /*
                BookShelf shelf = this.GetBookShelf(modelHit);
                if (shelf != null)
                    shelf.SavePosition();
                modelHit = null;
                 * */
                foreach (BookShelf shelf in this.m_dragShelfs)
                {
                    shelf.SavePosition();

                    // 如果一个书架正好被属性窗口监视
                    if (this.m_propertyDlg != null
                        && shelf == this.m_propertyDlg.BookShelf)
                        this.m_propertyDlg.PutInfo(shelf);
                }
                this.m_dragShelfs.Clear();
                viewport.ReleaseMouseCapture();

                if (m_clearSelections.Count > 0)
                {
                    if (m_bDraged == false)
                    {
                        // 延迟清除
                        this.ClearSelection(this.m_clearSelections);
                    }
                    this.m_clearSelections.Clear();
                }
            };

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

#if NO
            GeometryModel3D label = Model.CreateTextLabel3D(
    "text",
    new SolidColorBrush(Colors.Black),
    false,
    10 / 2,
    10,
    new Point3D(0, 10, 0),
    true,
    new Vector3D(1, 0, 0),
    new Vector3D(0, 0, -1));
            ModelVisual3D model = new ModelVisual3D();
            model.Content = label;

            viewport.Children.Add(model);
#endif

#if NO
            CommandBinding binding = new CommandBinding(
                ApplicationCommands.Paste,
                PasteCmdExecuted,
                PasteCmdCanExecute);
            this.CommandBindings.Add(binding);
            viewport.CommandBindings.Add(binding);
#endif

        }

        void CreatePropertyDialog()
        {
            this.m_propertyDlg = new ShelfInfoWindow();
            this.m_propertyDlg.Owner = this;

            this.m_propertyDlg.ShelfInfoChanged -= new ShelfInfoChanged(m_propertyDlg_ShelfInfoChanged);
            this.m_propertyDlg.ShelfInfoChanged += new ShelfInfoChanged(m_propertyDlg_ShelfInfoChanged);

            this.m_propertyDlg.Closed -= new EventHandler(m_propertyDlg_Closed);
            this.m_propertyDlg.Closed += new EventHandler(m_propertyDlg_Closed);
        }

        void m_propertyDlg_Closed(object sender, EventArgs e)
        {
            this.m_propertyDlg = null;
        }

        // 面板中的信息发生变化,要兑现到显示
        void m_propertyDlg_ShelfInfoChanged(object sender, ShelfInfoChangedEventArgs e)
        {
            /*
            // 刷新显示
            e.Shelf.DisplayText();
            e.Shelf.MoveToPosition();
            e.Shelf.RotateToDirection();
             * */
            e.Shelf.CreateShelf(this.viewport);

            this.Changed = true;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (this.m_selectedShelfs.Count == 0)
                return;

            BookShelf shelf = this.m_selectedShelfs[0];

            GeneralTransform3DTo2D gt = shelf.Model.TransformToAncestor(viewport);
            // If null, the transform isn't possible at all
            if (gt != null)
            {
                Rect3D box = VisualTreeHelper.GetDescendantBounds(shelf.Model);
                // 取得顶部的中点
                Point3D point = new Point3D(box.X + (box.SizeX/2),
                    Math.Max(box.Y, box.Y + box.SizeY),
                    box.Z + (box.SizeZ/2));

                Point result;
                if (gt.TryTransform(point, out result) == false)
                    return;

                rect.Width = 5;
                rect.Height = 5;
                Canvas.SetLeft(rect, result.X - rect.Width / 2);
                Canvas.SetTop(rect, result.Y - rect.Height / 2);

                /*
                Rect bounds = gt.TransformBounds(VisualTreeHelper.GetDescendantBounds(shelf.Model));
                // If empty, visual3d's specific bounds couldn't be transformed
                if (!bounds.IsEmpty)
                {
                    rect.Width = bounds.Width;
                    rect.Height = bounds.Height;
                    Canvas.SetLeft(rect, bounds.Left);
                    Canvas.SetTop(rect, bounds.Top);
                }
                 * */
            }
        }

        bool m_bDraged = false;

        // 装载模型定义文件
        public int LoadModels(
            string strModelDefFilename,
            out string strError)
        {
            strError = "";

            this.m_shelfModels.Clear();

            string strDefaultFile = this.DataDir + "\\default_models.xml";
            // 装入缺省的模型
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strDefaultFile);
            }
            catch (FileNotFoundException ex)
            {
                /*
                      <shelf 
    no="建筑科学研究院书架" 
    width="0.95" 
    height="2.10" 
    depth="0.24" 
    thick="0.025" 
    level="6" />
                */
                // 加入一个惟一的模型对象
                BookShelf shelf = new BookShelf();
                shelf.No = "<default>";
                shelf.Width = 1.0;
                shelf.Height = 2.0;
                shelf.Depth = 0.24;
                shelf.Thick = 0.02;
                shelf.Level = 6;
                shelf.ColorString = "#E0E0FF";
                this.m_shelfModels.Add(shelf);

                goto SKIP1;   // 文件不存在
            }
            catch (Exception ex)
            {
                strError = "将文件 " + strDefaultFile + " 装入XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlNode node in nodes)
            {
                BookShelf shelf = new BookShelf();
                int nRet = shelf.SetValueFromXmlNode(node, out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.m_shelfModels.Add(shelf);
            }

            SKIP1:
            dom = new XmlDocument();
            try
            {
                dom.Load(strModelDefFilename);
            }
            catch (FileNotFoundException ex)
            {
                return 0;   // 文件不存在
            }
            catch (Exception ex)
            {
                strError = "将文件 " + strModelDefFilename + " 装入XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlNode node in nodes)
            {
                BookShelf shelf = new BookShelf();
                int nRet = shelf.SetValueFromXmlNode(node, out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.m_shelfModels.Add(shelf);
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 打开一个已经存在的XML书库文件
        private void MenuItem_open_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";

            // public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

            if (this.Changed == true)
            {
                MessageBoxResult result = MessageBox.Show(this,
        "当前有修改尚未保存。如果此时打开文件装入新的内容，现有内容将丢失。\r\n\r\n确实要打开文件? ",
        "StackRoomEditor",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question,
        MessageBoxResult.No,
        MessageBoxOptions.None);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            ClearAllShelf();

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

            Nullable<bool> dlg_result = dlg.ShowDialog();
            if (dlg_result == false)
                return;

            if (dlg.FileName.IndexOf(".png.") != -1)
            {
                MessageBox.Show(this, "您打开的文件 '" + dlg.FileName + "' 是一个图像文件的描述文件，它容易被后面创建图像文件的操作自动覆盖，因此建议不要直接打开这样的文件进行编辑");
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "将文件 " + dlg.FileName + " 装入XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            // floor
            XmlNode nodeFloor = dom.DocumentElement.SelectSingleNode("floor");
            if (nodeFloor != null)
            {
                if (this.Floor != null
    && this.Floor.Model != null)
                {
                    viewport.Children.Remove(this.Floor.Model);
                    this.Floor.Model = null;
                }

                this.Floor = new Floor();
                int nRet = this.Floor.SetValueFromXmlNode(nodeFloor, out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.Floor.Model = CreateFloor(this.Floor.X,
                this.Floor.Z,
                this.Floor.Width,
                this.Floor.Height);
                viewport.Children.Add(this.Floor.Model);
            }
            else
            {
                CreateDefaultFloor();
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlNode node in nodes)
            {
#if NO
                string strLocation = DomUtil.GetAttr(node, "location");

                Hashtable table = StringUtil.ParseParameters(strLocation);

                BookShelf shelf = new BookShelf();
                {
                    double v = 0;
                    double.TryParse((string)table["x"], out v);
                    shelf.LocationX = v;
                }
                {
                    double v = 0;
                    double.TryParse((string)table["z"], out v);
                    shelf.LocationZ = v;
                }
#endif
                BookShelf shelf = new BookShelf();
                int nRet = shelf.SetValueFromXmlNode(node, out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                TextBlock textblock = null;

                Model3DGroup group = CreateShelf(shelf.Width,
                    shelf.Height,
                    shelf.Depth,
                    shelf.Thick,
                    shelf.Level,
                    shelf.Color,
                    out textblock);
                ModelVisual3D model = new ModelVisual3D();
                model.Content = group;
                viewport.Children.Add(model);

                textblock.Text = shelf.No;

                shelf.Model = model;
                shelf.TextBlock = textblock;
                this.m_shelfs.Add(shelf);
                shelf.MoveToPosition();
                shelf.RotateToDirection();
#endif
                shelf.CreateShelf(this.viewport);
                this.m_shelfs.Add(shelf);
            }

            // 文字部分
            {
                XmlNode node_text = dom.DocumentElement.SelectSingleNode("textPanel");
                if (node_text != null)
                    this.m_strTextPanelDef = node_text.OuterXml;
                else
                    this.m_strTextPanelDef = "";

                int nRet = TextPanelWindow.CreateTextPanel(this.canvas_text,
                    this.m_strTextPanelDef,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (string.IsNullOrEmpty(this.m_strTextPanelDef) == false)
                    this.MenuItem_displayText.IsEnabled = true;
                else
                    this.MenuItem_displayText.IsEnabled = false;
            }

            this.m_strCurrentXmlFilename = dlg.FileName;
            this.Changed = false;
            this.Title = "书库编辑器 " + dlg.FileName;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void CreateDefaultFloor()
        {
            if (this.Floor != null
                && this.Floor.Model != null)
            {
                viewport.Children.Remove(this.Floor.Model);
                this.Floor.Model = null;
            }

            this.Floor = new Floor();
            this.Floor.X = 0;
            this.Floor.Z = 0;
            this.Floor.Width = 20;
            this.Floor.Height = 20;

            this.Floor.Model = CreateFloor(this.Floor.X,
                this.Floor.Z,
                this.Floor.Width,
                this.Floor.Height);
            viewport.Children.Add(this.Floor.Model);
        }

        // 新建内容
        private void MenuItem_new_Click(object sender, RoutedEventArgs e)
        {
            // 如果有尚未保存的要警告
            if (this.Changed == true)
            {
                MessageBoxResult result = MessageBox.Show(this,
        "当前有修改尚未保存。如果此时新建内容，现有内容将丢失。\r\n\r\n确实要继续新建? ",
        "StackRoomEditor",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question,
        MessageBoxResult.No,
        MessageBoxOptions.None);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            ClearAllShelf();

            CreateDefaultFloor();

            // TODO: viewport居中，1.0 scale ?

            this.m_strCurrentXmlFilename = "";
            this.Changed = false;
        }

        void ClearAllShelf()
        {
            foreach (BookShelf shelf in this.m_shelfs)
            {
                if (shelf.Model != null)
                    viewport.Children.Remove(shelf.Model);
                if (shelf.Frame != null)
                    viewport.Children.Remove(shelf.Frame);
            }

            this.m_shelfs.Clear();
            this.m_selectedShelfs.Clear();
            OnSelectionChanged();
        }

        // 添加一个书架
        private void MenuItem_newShelf_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 是否出现对话框询问书架样式?
            ShelfTypeWindow dlg = new ShelfTypeWindow(this.m_shelfModels);

            // dlg.ShelfModels = this.m_shelfModels;
            dlg.ShowDialog();

            if (dlg.BookShelf == null)
                return;

            BookShelf shelf = new BookShelf();
            shelf.Width = dlg.BookShelf.Width;  // 5
            shelf.Height = dlg.BookShelf.Height; // 6;
            shelf.Depth = dlg.BookShelf.Depth; // 1;
            shelf.Thick = dlg.BookShelf.Thick; // 0.1;
            shelf.Level = dlg.BookShelf.Level; // 5;
            shelf.ColorString = dlg.BookShelf.ColorString;

#if NO
            TextBlock textblock = null;
            Model3DGroup group = CreateShelf(shelf.Width,
                    shelf.Height,
                    shelf.Depth,
                    shelf.Thick,
                    shelf.Level,
                    shelf.Color,
                    out textblock);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = group;
            viewport.Children.Add(model);

            shelf.Model = model;
            shelf.TextBlock = textblock;
            shelf.SavePosition();
#endif
            shelf.CreateShelf(this.viewport);
            shelf.SavePosition();
            this.m_shelfs.Add(shelf);

            this.Changed = true;
        }

        // 添加一对书架，互相背靠
        void NewPair()
        {
            // 出现对话框询问书架样式
            ShelfTypeWindow dlg = new ShelfTypeWindow(this.m_shelfModels);

            dlg.ShowDialog();

            if (dlg.BookShelf == null)
                return;

            // 确保对话框中的信息都兑现到内存对象中
            if (this.m_propertyDlg != null)
                this.m_propertyDlg.RefreshInfo();

            this.ClearSelection(null);

            double x = 0;
            double z = 0;
            // 
            {
                BookShelf shelf = new BookShelf();
                shelf.Width = dlg.BookShelf.Width;  // 5
                shelf.Height = dlg.BookShelf.Height; // 6;
                shelf.Depth = dlg.BookShelf.Depth; // 1;
                shelf.Thick = dlg.BookShelf.Thick; // 0.1;
                shelf.Level = dlg.BookShelf.Level; // 5;
                shelf.ColorString = dlg.BookShelf.ColorString;

#if NO
                TextBlock textblock = null;
                Model3DGroup group = CreateShelf(shelf.Width,
                        shelf.Height,
                        shelf.Depth,
                        shelf.Thick,
                        shelf.Level,
                        shelf.Color,
                        out textblock);
                ModelVisual3D model = new ModelVisual3D();
                model.Content = group;
                viewport.Children.Add(model);

                shelf.Model = model;
                shelf.TextBlock = textblock;
                shelf.SavePosition();
#endif
                shelf.CreateShelf(this.viewport);
                shelf.SavePosition();
                this.m_shelfs.Add(shelf);


                shelf.LocationX = x;
                shelf.LocationZ = z;
                shelf.Direction = 0;

                // 刷新显示
                shelf.DisplayText();
                shelf.MoveToPosition();
                shelf.RotateToDirection();

                Select(shelf, "select");
            }

            {
                BookShelf shelf = new BookShelf();
                shelf.Width = dlg.BookShelf.Width;  // 5
                shelf.Height = dlg.BookShelf.Height; // 6;
                shelf.Depth = dlg.BookShelf.Depth; // 1;
                shelf.Thick = dlg.BookShelf.Thick; // 0.1;
                shelf.Level = dlg.BookShelf.Level; // 5;
                shelf.ColorString = dlg.BookShelf.ColorString;
#if NO
                TextBlock textblock = null;
                Model3DGroup group = CreateShelf(shelf.Width,
                        shelf.Height,
                        shelf.Depth,
                        shelf.Thick,
                        shelf.Level,
                        shelf.Color,
                        out textblock);
                ModelVisual3D model = new ModelVisual3D();
                model.Content = group;
                viewport.Children.Add(model);

                shelf.Model = model;
                shelf.TextBlock = textblock;
                shelf.SavePosition();
#endif
                shelf.CreateShelf(this.viewport);
                shelf.SavePosition();
                this.m_shelfs.Add(shelf);

                shelf.LocationX = x;
                shelf.LocationZ = z - shelf.Depth - 0.01;
                shelf.Direction = 180;

                // 刷新显示
                shelf.DisplayText();
                shelf.MoveToPosition();
                shelf.RotateToDirection();
                Select(shelf, "select");

            }

            this.Changed = true;
        }

        // 添加多个书架
        void NewMulti()
        {
            int nCount = 5;

            CreateMultiShelfWindow count_dlg = new CreateMultiShelfWindow();
            count_dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            count_dlg.Owner = Window.GetWindow(this);
            if (count_dlg.ShowDialog() == false)
                return;

            nCount = count_dlg.Count;

            // 出现对话框询问书架样式
            ShelfTypeWindow dlg = new ShelfTypeWindow(this.m_shelfModels);

            dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();

            if (dlg.BookShelf == null)
                return;

            // 确保对话框中的信息都兑现到内存对象中
            if (this.m_propertyDlg != null)
                this.m_propertyDlg.RefreshInfo();

            this.ClearSelection(null);

            double x = 0;
            double z = 0;
            // 
            
            for(int i=0;i<nCount;i++)
            {
                BookShelf shelf = new BookShelf();
                shelf.Width = dlg.BookShelf.Width;  // 5
                shelf.Height = dlg.BookShelf.Height; // 6;
                shelf.Depth = dlg.BookShelf.Depth; // 1;
                shelf.Thick = dlg.BookShelf.Thick; // 0.1;
                shelf.Level = dlg.BookShelf.Level; // 5;
                shelf.ColorString = dlg.BookShelf.ColorString;

#if NO
                TextBlock textblock = null;
                Model3DGroup group = CreateShelf(shelf.Width,
                        shelf.Height,
                        shelf.Depth,
                        shelf.Thick,
                        shelf.Level,
                        shelf.Color,
                        out textblock);
                ModelVisual3D model = new ModelVisual3D();
                model.Content = group;
                viewport.Children.Add(model);

                shelf.Model = model;
                shelf.TextBlock = textblock;
                shelf.SavePosition();
#endif
                shelf.CreateShelf(this.viewport);
                shelf.SavePosition();
                this.m_shelfs.Add(shelf);


                shelf.LocationX = x;
                shelf.LocationZ = z;
                shelf.Direction = 0;

                // 刷新显示
                shelf.DisplayText();
                shelf.MoveToPosition();
                shelf.RotateToDirection();

                Select(shelf, "select");

                x += shelf.Width + 0.01;
            }

            this.Changed = true;
        }

        // 获得一个对象的矩形外框
        Rect GetModelBounds(ModelVisual3D model)
        {
            GeneralTransform3DTo2D gt = model.TransformToAncestor(viewport);
            // If null, the transform isn't possible at all
            if (gt == null)
                return Rect.Empty;
            
            return gt.TransformBounds(VisualTreeHelper.GetDescendantBounds(model));
        }

        ModelVisual3D CreateFloor(
            double x,
            double z,
            double width,
            double height)
        {
            double v_w = 10F / (width / 10F);
            double v_h = 10F / (height / 10F);

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri("ground_grid.bmp", UriKind.RelativeOrAbsolute));  // ground_cell.bmp
            brush.TileMode = TileMode.Tile;
            brush.Stretch = Stretch.Fill;
            brush.Viewport = new Rect(0, 0, v_w, v_h);  // 0 0 2 2
            brush.ViewportUnits = BrushMappingMode.Absolute;

            GeometryModel3D obj = CreateCube(
                width,  // 100,
                0.1,
                height, // 100,
    Colors.LightGray,
    brush.ImageSource.Width,
    brush.ImageSource.Height);

            obj.Transform = new TranslateTransform3D(x, -0.11, z);


            MaterialGroup g = new MaterialGroup();
            g.Children.Add(new DiffuseMaterial(brush));
            // g.Children.Add(new SpecularMaterial(Brushes.LightYellow, 100));
            obj.Material = g;
            obj.BackMaterial = new DiffuseMaterial(Brushes.Black);
          

            ModelVisual3D model = new ModelVisual3D();
            model.Content = obj;

            return model;
        }

        public static Model3DGroup CreateShelf(double width, 
            double height,
            double depth,
            double thick, // 板子厚度
            int nLevel,   // 层数目
            Color color,
            out TextBlock textblock)
        {
            textblock = null;

            double level_height = (height - thick) / (double)nLevel;

            Model3DGroup group = new Model3DGroup();

            // Color color = Colors.Yellow;

            /*
            ScreenSpaceLines3D frame = CreateWiredCube(width+thick, height+thick, depth+thick,
Colors.Red);
            frame.Transform = new TranslateTransform3D(-thick/2, -thick/2, -depth / 2);
            viewport.Children.Add(frame);
             * */

            // 背板
            GeometryModel3D back = CreateCube(width, height, thick, color);
            // 向-z方向移动depth
            back.Transform = new TranslateTransform3D(0, 0, -(depth-thick) / 2);
            group.Children.Add(back);

            // 左侧板
            GeometryModel3D left = CreateCube(thick, height, depth, color);
            left.Transform = new TranslateTransform3D(-((width / 2) - (thick / 2)), 0, 0);
            group.Children.Add(left);

            // 右侧板
            GeometryModel3D right = CreateCube(thick, height, depth, color);
            // 向x方向移动width
            right.Transform = new TranslateTransform3D((width / 2) - (thick / 2), 0, 0);
            group.Children.Add(right);


            // 每层隔板
            for (int i = 0; i <= nLevel; i++)
            {
                double cur_width = width;
                double x = 0;
                double depth_delta = depth / 10;    // 水平隔板缩进去一点

                if (i == 0 || i == nLevel)
                {
                    cur_width = width;  // +thick;
                    x = 0;
                    depth_delta = 0;
                }
                else
                {
                    /*
                    cur_width = width - thick * 2;
                    x = thick;
                     * */
                }

                // 
                GeometryModel3D horz = CreateCube(cur_width, thick, depth - depth_delta, color);
                // 向y方向移动
                horz.Transform = new TranslateTransform3D(x, (i * level_height), -depth_delta/2);
                group.Children.Add(horz);
            }


            GeometryModel3D label = Model.CreateTextLabel3D(
                "",
                new SolidColorBrush(Colors.Black),
                new SolidColorBrush(Colors.White),
                false,
                depth-thick,
                depth-thick,
                new Point3D(0+thick-width/2, height + 0.01, 0+depth/2 - thick / 2),
                false,
                new Vector3D(1, 0, 0),
    new Vector3D(0, 0, -1),
    out textblock);
            group.Children.Add(label);

            /*
            ScreenSpaceLines3D frame = new ScreenSpaceLines3D();
            frame.MakeWireframe(group);

            viewport.Children.Add(frame);
             * */
            return group;
        }

        // 创建立方体，带有纹理
        static GeometryModel3D CreateCube(double width, double height, double depth,
            Color color,
            double nBitmapWidth,
            double nBitmapHeight)
        {
            double w = width / 2;
            double h1 = height;
            double d = depth / 2;

            Point3DCollection points = new Point3DCollection(20);
            Point3D point;

            //top of the floor
            point = new Point3D(-w, h1, d);// Floor Index - 0
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 1
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 2
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 3
            points.Add(point);
            //front side
            point = new Point3D(-w, 0, d);// Floor Index - 4
            points.Add(point);
            point = new Point3D(-w, h1, d);// Floor Index - 5
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 6
            points.Add(point);
            point = new Point3D(w, 0, d);// Floor Index - 7
            points.Add(point);
            //right side
            point = new Point3D(w, 0, d);// Floor Index - 8
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 9
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 10
            points.Add(point);
            point = new Point3D(w, 0, -d);// Floor Index - 11
            points.Add(point);
            //back side
            point = new Point3D(w, 0, -d);// Floor Index - 12
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 13
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 14
            points.Add(point);
            point = new Point3D(-w, 0, -d);// Floor Index - 15
            points.Add(point);
            //left side
            point = new Point3D(-w, 0, -d);// Floor Index - 16
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 17
            points.Add(point);
            point = new Point3D(-w, h1, d);// Floor Index - 18
            points.Add(point);
            point = new Point3D(-w, 0, d);// Floor Index - 19
            points.Add(point);

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = points;

            // int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13, 14, 15, 16, 17, 19, 17, 18, 19 };
            int[] indices = new int[] { 0, 1, 2, 0, 2, 3,
                   4, 7, 5, 5, 7, 6,
                   8, 11, 9, 9, 11, 10, 
                   12, 15, 13, 13, 15, 14,
                   16, 19, 17, 17, 19, 18
                   };
            mesh.TriangleIndices = new Int32Collection(indices);

            mesh.TextureCoordinates.Add(new Point(0, nBitmapHeight));
            mesh.TextureCoordinates.Add(new Point(nBitmapWidth, nBitmapHeight));
            mesh.TextureCoordinates.Add(new Point(nBitmapWidth, 0));

            /*
            mesh.TextureCoordinates.Add(new Point(0,48));
            mesh.TextureCoordinates.Add(new Point(48, 0));
            mesh.TextureCoordinates.Add(new Point(0, 0));
             * */


            GeometryModel3D obj = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Yellow));

            SolidColorBrush brush = new SolidColorBrush(color);
            MaterialGroup g = new MaterialGroup();
            g.Children.Add(new DiffuseMaterial(brush)); // Brushes.LightGray
            g.Children.Add(new SpecularMaterial(brush, 100));
            obj.Material = g;
            obj.BackMaterial = new DiffuseMaterial(Brushes.Black);

            return obj;
        }

        // 创建立方体 不带有纹理
        static GeometryModel3D CreateCube(double width, double height, double depth,
            Color color)
        {
            double w = width / 2;
            double h1 = height;
            double d = depth / 2;

            Point3DCollection points = new Point3DCollection(20);
            Point3D point;

            //top of the floor
            point = new Point3D(-w, h1, d);// Floor Index - 0
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 1
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 2
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 3
            points.Add(point);
            //front side
            point = new Point3D(-w, 0, d);// Floor Index - 4
            points.Add(point);
            point = new Point3D(-w, h1, d);// Floor Index - 5
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 6
            points.Add(point);
            point = new Point3D(w, 0, d);// Floor Index - 7
            points.Add(point);
            //right side
            point = new Point3D(w, 0, d);// Floor Index - 8
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 9
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 10
            points.Add(point);
            point = new Point3D(w, 0, -d);// Floor Index - 11
            points.Add(point);
            //back side
            point = new Point3D(w, 0, -d);// Floor Index - 12
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 13
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 14
            points.Add(point);
            point = new Point3D(-w, 0, -d);// Floor Index - 15
            points.Add(point);
            //left side
            point = new Point3D(-w, 0, -d);// Floor Index - 16
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 17
            points.Add(point);
            point = new Point3D(-w, h1, d);// Floor Index - 18
            points.Add(point);
            point = new Point3D(-w, 0, d);// Floor Index - 19
            points.Add(point);

            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Positions = points;

            // int[] indices = new int[] { 0, 1, 2, 0, 2, 3, 4, 5, 7, 5, 6, 7, 8, 9, 11, 9, 10, 11, 12, 13, 15, 13, 14, 15, 16, 17, 19, 17, 18, 19 };
            int[] indices = new int[] { 0, 1, 2, 0, 2, 3,
                   4, 7, 5, 5, 7, 6,
                   8, 11, 9, 9, 11, 10, 
                   12, 15, 13, 13, 15, 14,
                   16, 19, 17, 17, 19, 18
                   };
            mesh.TriangleIndices = new Int32Collection(indices);

            /*
            mesh.TextureCoordinates.Add(new Point(0,48));
            mesh.TextureCoordinates.Add(new Point(48, 48));
            mesh.TextureCoordinates.Add(new Point(48,0));

            mesh.TextureCoordinates.Add(new Point(0,48));
            mesh.TextureCoordinates.Add(new Point(48, 0));
            mesh.TextureCoordinates.Add(new Point(0, 0));
             * */


            GeometryModel3D obj = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Yellow));

            SolidColorBrush brush = new SolidColorBrush(color);
            MaterialGroup g = new MaterialGroup();
            g.Children.Add(new DiffuseMaterial(brush)); // Brushes.LightGray
            g.Children.Add(new SpecularMaterial(brush, 100));
            obj.Material = g;
            obj.BackMaterial = new DiffuseMaterial(Brushes.Black);

            return obj;
        }

        public static ScreenSpaceLines3D CreateWiredCube(double width, double height, double depth,
    Color color)
        {
            double w = width / 2;
            double h1 = height;
            double d = depth / 2;

            ScreenSpaceLines3D line = new ScreenSpaceLines3D();
            line.Thickness = 1.6;
            line.Color = color;



            Point3DCollection points = new Point3DCollection(20);
            Point3D point;

            //top of the floor
            point = new Point3D(-w, h1, d);// Floor Index - 0
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 1
            points.Add(point);

            point = new Point3D(w, h1, d);// Floor Index - 1
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 2
            points.Add(point);


            point = new Point3D(w, h1, -d);// Floor Index - 2
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 3
            points.Add(point);

            point = new Point3D(-w, h1, -d);// Floor Index - 3
            points.Add(point);

            point = new Point3D(-w, h1, d);// Floor Index - 0
            points.Add(point);


            //front side
            point = new Point3D(-w, 0, d);// Floor Index - 4
            points.Add(point);
            point = new Point3D(-w, h1, d);// Floor Index - 5
            points.Add(point);
            point = new Point3D(w, h1, d);// Floor Index - 6
            points.Add(point);
            point = new Point3D(w, 0, d);// Floor Index - 7
            points.Add(point);


            //back side
            point = new Point3D(w, 0, -d);// Floor Index - 12
            points.Add(point);
            point = new Point3D(w, h1, -d);// Floor Index - 13
            points.Add(point);
            point = new Point3D(-w, h1, -d);// Floor Index - 14
            points.Add(point);
            point = new Point3D(-w, 0, -d);// Floor Index - 15
            points.Add(point);

            // bottom
            point = new Point3D(-w, 0, d);// Floor Index - 0
            points.Add(point);
            point = new Point3D(w, 0, d);// Floor Index - 1
            points.Add(point);

            point = new Point3D(w, 0, d);// Floor Index - 1
            points.Add(point);
            point = new Point3D(w, 0, -d);// Floor Index - 2
            points.Add(point);


            point = new Point3D(w, 0, -d);// Floor Index - 2
            points.Add(point);
            point = new Point3D(-w, 0, -d);// Floor Index - 3
            points.Add(point);

            point = new Point3D(-w, 0, -d);// Floor Index - 3
            points.Add(point);

            point = new Point3D(-w, 0, d);// Floor Index - 0
            points.Add(point);



            line.Points = points;

            return line;
        }

        // 把三维的矢量投影到地板上
        static Vector3D Move(Vector3D v0)
        {
            double pow_x_z = Math.Pow(v0.X, 2) + Math.Pow(v0.Z, 2);
            double ratio = Math.Sqrt(pow_x_z + Math.Pow(v0.Y, 2)) / Math.Sqrt(pow_x_z);
            // double ratio = (pow_x_z + Math.Pow(v0.Y, 2)) / pow_x_z;

            Vector3D v1 = new Vector3D(v0.X, 0, v0.Z);

            v1 *= ratio;
            return v1;
        }

        private Vector3D GetTranslationVector3D(DependencyObject modelHit, Point startPosition, Point endPosition)
        {
#if NO
            Transform3D save = ((ModelVisual3D)modelHit).Transform.Clone();

            RotateTransform3D rotate = BookShelf.ClearRotate((ModelVisual3D)modelHit);
#endif
            try
            {
                Vector3D translationVector3D = new Vector3D();

                Point3D startPoint3D = new Point3D(startPosition.X, startPosition.Y, 0);
                Point3D endPoint3D = new Point3D(endPosition.X, endPosition.Y, 0);
                Vector3D vector3D = endPoint3D - startPoint3D;

                if (ViewportInfo.Point2DtoPoint3D(this.viewport,
                    vector3D,
                    out translationVector3D) == true)
                {
                    translationVector3D = Move(translationVector3D);

                    PerspectiveCamera c = viewport.Camera as PerspectiveCamera;

                    Rect3D box = VisualTreeHelper.GetDescendantBounds((Visual3D)modelHit);
                    // 取得物体的中点
                    Point3D origin = new Point3D(box.X + (box.SizeX / 2),
                        Math.Max(box.Y, box.Y + box.SizeY / 2),
                        box.Z + (box.SizeZ / 2));

                    // Point3D origin = new Point3D(0, 0, 0);
                    Vector3D c_v = c.Position - origin;
                    translationVector3D *= c_v.Length;


                    return translationVector3D;
                }

                return translationVector3D;

                {

#if NO
                    Vector3D direction = this.camera1.LookDirection;
                    direction /= direction.Length;  // 长度为一个单位的向量
                    direction.Y = 0;
                    direction *= c_v.Length;
#endif


                    Debug.WriteLine("vector3D 转换前:" + vector3D.ToString() + " translationVector3D:" + translationVector3D.ToString());
                    // translationVector3D.Y = 0;

                    // TODO: 限制到地板边沿?
                    translationVector3D = Move(translationVector3D);
                    Debug.WriteLine("vector3D 后:" + vector3D.ToString() + " translationVector3D:" + translationVector3D.ToString());

                    /*
                    direction *= translationVector3D.Y * 5;
                    translationVector3D += direction;
                    */
                }

                return translationVector3D;
            }
            finally
            {
                // ((ModelVisual3D)modelHit).Transform = save;
            }
        }

        public static readonly Matrix3D ZeroMatrix = new Matrix3D(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        public static Matrix3D TryTransformTo2DAncestor(DependencyObject visual, out Viewport3DVisual viewport, out bool success)
        {
            Matrix3D to2D = GetWorldTransformationMatrix(visual, out viewport);

            // 2012/6/6
            if (viewport == null)
            {
                success = false;
                return ZeroMatrix;
            }

            Debug.Assert(viewport != null, "");

            to2D.Append(MathUtils.TryWorldToViewportTransform(viewport, out success));

            if (!success)
            {
                return ZeroMatrix;
            }

            return to2D;
        }

        private static Matrix3D GetWorldTransformationMatrix(DependencyObject visual, out Viewport3DVisual viewport)
        {
            Matrix3D worldTransform = Matrix3D.Identity;
            viewport = null;

            if (!(visual is Visual3D))
            {
                throw new ArgumentException("Must be of type Visual3D.", "visual");
            }

            bool bFirst = true;

            while (visual != null)
            {
                if (!(visual is ModelVisual3D))
                {
                    break;
                }

                Transform3D transform = (Transform3D)visual.GetValue(ModelVisual3D.TransformProperty);

                if (transform != null)
                {
                    if (bFirst == false)
                        worldTransform.Append(transform.Value);
                    else
                    {
                        Transform3D temp = BookShelf.FindTranslate((ModelVisual3D)visual);
                        if (temp == null)
                            temp = transform;
                        worldTransform.Append(temp.Value);
                    }
                }

                visual = VisualTreeHelper.GetParent(visual);
                bFirst = false;
            }

            viewport = visual as Viewport3DVisual;

            if (viewport == null)
            {
                if (visual != null)
                {
                    // In WPF 3D v1 the only possible configuration is a chain of
                    // ModelVisual3Ds leading up to a Viewport3DVisual.

                    throw new ApplicationException(
                        String.Format("Unsupported type: '{0}'.  Expected tree of ModelVisual3Ds leading up to a Viewport3DVisual.",
                        visual.GetType().FullName));
                }

                return ZeroMatrix;
            }

            return worldTransform;
        }

        BookShelf GetBookShelf(Visual3D model)
        {
            foreach (BookShelf current in this.m_shelfs)
            {
                if (current.Model == model)
                    return current;
            }

            return null;
        }

        // 保存到文件
        private void MenuItem_save_Click(object sender, RoutedEventArgs e)
        {
            Save(false,
                ref this.m_strCurrentXmlFilename);

            this.Changed = false;
        }

        // 另存
        private void MenuItem_saveAs_Click(object sender, RoutedEventArgs e)
        {
            Save(true,
                ref this.m_strCurrentXmlFilename);

            this.Changed = false;
        }

        string m_strCurrentXmlFilename = "";
        string m_strCurrentPngFilename = "";

        void Save(bool bSaveAs,
            ref string strFileName,
            bool bCreateTopPoint = true)
        {
            if (string.IsNullOrEmpty(strFileName) == true
                || bSaveAs == true)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = this.m_strCurrentXmlFilename;
                dlg.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

                Nullable<bool> dlg_result = dlg.ShowDialog();
                if (dlg_result == false)
                    return;

                strFileName = dlg.FileName;
            }

#if NO
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            if (bCreateTopPoint == true)
            {
                // 写入视口的尺寸
                DomUtil.SetAttr(dom.DocumentElement, 
                    "viewportSize",
                    viewport.ActualWidth.ToString() + "," + viewport.ActualHeight.ToString());
            }

            foreach (BookShelf shelf in this.m_shelfs)
            {
                XmlElement node = dom.CreateElement("shelf");
                dom.DocumentElement.AppendChild(node);

                if (bCreateTopPoint == true)
                    shelf.TopPoint = GetTopPoint(shelf);
                else
                    shelf.TopPoint = null;

                // DomUtil.SetAttr(node, "location", "x=" + shelf.LocationX.ToString() + ",z=" + shelf.LocationZ.ToString());
                shelf.BuildXmlNode(node);
            }

            dom.Save(this.m_strCurrentFilename);
#endif

            string strError = "";
            int nRet = Save(strFileName,
                    bCreateTopPoint,
                    null,
                    out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(strError);
        }

        public int Save(string strFilename,
            bool bCreateTopPoint,
            GlobalInfo global_info,
            out string strError)
        {
            strError = "";

            // 确保对话框中的信息都兑现到内存对象中
            if (this.m_propertyDlg != null)
                this.m_propertyDlg.RefreshInfo();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            if (bCreateTopPoint == true)
            {
                // 写入视口的尺寸
                DomUtil.SetAttr(dom.DocumentElement,
                    "viewportSize",
                    viewport.ActualWidth.ToString() + "," + viewport.ActualHeight.ToString());
            }

            if (global_info != null)
            {
                if (global_info.ImagePixelSize != null)
                {
                    // 写入图像文件像素尺寸
                    DomUtil.SetAttr(dom.DocumentElement,
                        "imagePixelSize",
                        global_info.ImagePixelSize.Value.Width.ToString() + "," + global_info.ImagePixelSize.Value.Height.ToString());
                }
            }

            // textPanel
            if (string.IsNullOrEmpty(this.m_strTextPanelDef) == false)
            {
                XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = this.m_strTextPanelDef;
                }
                catch (Exception ex)
                {
                    strError = "创建<textPanel> fragment时出错: " + ex.Message;
                    return -1;
                }
                dom.DocumentElement.AppendChild(fragment);
            }

            // floor
            if (this.Floor != null)
            {
                XmlElement node = dom.CreateElement("floor");
                dom.DocumentElement.AppendChild(node);
                this.Floor.BuildXmlNode(node);
            }

            foreach (BookShelf shelf in this.m_shelfs)
            {
                XmlElement node = dom.CreateElement("shelf");
                dom.DocumentElement.AppendChild(node);

                if (bCreateTopPoint == true)
                    shelf.TopPoint = GetTopPoint(shelf);
                else
                    shelf.TopPoint = null;

                // DomUtil.SetAttr(node, "location", "x=" + shelf.LocationX.ToString() + ",z=" + shelf.LocationZ.ToString());
                shelf.BuildXmlNode(node);
            }

            dom.Save(strFilename);

            return 0;
        }

        Point? GetTopPoint(BookShelf shelf)
        {
            Point? result = null;

            GeneralTransform3DTo2D gt = shelf.Model.TransformToAncestor(viewport);
            // If null, the transform isn't possible at all
            if (gt == null)
                return result;
            Rect3D box = VisualTreeHelper.GetDescendantBounds(shelf.Model);
            // 取得顶部的中点
            Point3D point = new Point3D(box.X + (box.SizeX / 2),
                Math.Max(box.Y, box.Y + box.SizeY),
                box.Z + (box.SizeZ / 2));

            Point result_point;
            if (gt.TryTransform(point, out result_point) == false)
                return result;

            return result_point;
        }

        private void MenuItem_viewport_deleteObject_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";

            if (this.m_selectedShelfs == null
                || this.m_selectedShelfs.Count == 0)
            {
                strError = "尚未选定要删除的对象";
                goto ERROR1;
            }

#if NO
            foreach (BookShelf shelf in this.m_selectedShelfs)
            {
                viewport.Children.Remove(shelf.Model);
                if (shelf.Frame != null)
                    viewport.Children.Remove(shelf.Frame);

                this.m_shelfs.Remove(shelf);

                this.Changed = true;
            }
            this.m_selectedShelfs.Clear();
            OnSelectionChanged();
#endif
            DeleteAllSelected();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void SelectAll()
        {
            foreach (BookShelf shelf in this.m_shelfs)
            {
                Select(shelf, "select");
            }
        }

        void DeleteAllSelected()
        {
            foreach (BookShelf shelf in this.m_selectedShelfs)
            {
                viewport.Children.Remove(shelf.Model);
                if (shelf.Frame != null)
                    viewport.Children.Remove(shelf.Frame);

                this.m_shelfs.Remove(shelf);

                this.Changed = true;
            }
            this.m_selectedShelfs.Clear();
            OnSelectionChanged();
        }

        private void MenuItem_exit_Click(object sender, RoutedEventArgs e)
        {
            if (this.Changed == true)
            {
                MessageBoxResult result = MessageBox.Show(this,
        "当前有修改尚未保存。如果此时退出，现有内容将丢失。\r\n\r\n是否要保存并退出? ",
        "StackRoomEditor",
        MessageBoxButton.YesNoCancel,
        MessageBoxImage.Question,
        MessageBoxResult.No,
        MessageBoxOptions.None);

                if (result == MessageBoxResult.Yes)
                {
                    Save(false,
                ref this.m_strCurrentXmlFilename);
                    goto EXIT;
                }

                if (result == MessageBoxResult.Cancel)
                    return;
            }

            // exit
            EXIT:
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Changed == true)
            {
                MessageBoxResult result = MessageBox.Show(this,
        "当前有修改尚未保存。如果此时退出，现有内容将丢失。\r\n\r\n是否要保存并退出? ",
        "StackRoomEditor",
        MessageBoxButton.YesNoCancel,
        MessageBoxImage.Question,
        MessageBoxResult.No,
        MessageBoxOptions.None);

                if (result == MessageBoxResult.Yes)
                {
                    Save(false,
                        ref this.m_strCurrentXmlFilename);
                    goto EXIT;
                }

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

        EXIT:
            if (this.m_propertyDlg != null)
                this.m_propertyDlg.Close();
        }

        BookShelf GetSelectedModel(ModelVisual3D model)
        {
            foreach (BookShelf s in this.m_selectedShelfs)
            {
                if (s.Model == model)
                    return s;
                if (s.Frame == model)
                    return s;
            }

            return null;
        }

        // 选定一个物体
        BookShelf Select(BookShelf shelf,
            string strAction)
        {
            BookShelf s = this.m_selectedShelfs.IndexOf(shelf) != -1 ? shelf : null;

            if (strAction == "toggle")
            {
                if (s == null)
                    strAction = "select";
                else
                    strAction = "unselect";
            }

            if (strAction == "select")
            {
                if (s != null)
                    return s; // 已经选定

                s = this.m_shelfs.IndexOf(shelf) != -1 ? shelf : null;
                if (s == null)
                {
                    // Debug.Assert(false, "");
                    return null;
                }

                s.CreateFrame(viewport);

                Debug.Assert(s != null, "");

                this.m_selectedShelfs.Add(s);
                if (this.m_propertyDlg != null)
                {
                    OnSelectionChanged();
                }
                return s;
            }
            else if (strAction == "unselect")
            {
                if (s != null)
                {
                    viewport.Children.Remove(s.Frame);
                    s.Frame = null;

                    this.m_selectedShelfs.Remove(s);
                    if (this.m_propertyDlg != null)
                    {
                        // this.m_propertyDlg.Clear();
                        OnSelectionChanged();
                    }
                }
            }

            return s;
        }


        // 选定一个物体
        BookShelf Select(ModelVisual3D model,
            string strAction)
        {
            if (model == null)
                return null;

            BookShelf s = GetSelectedModel(model);

            if (strAction == "toggle")
            {
                if (s == null)
                    strAction = "select";
                else
                    strAction = "unselect";
            }

            if (strAction == "select")
            {
                if (s != null)
                    return s; // 已经选定

                s = GetBookShelf(model);
                if (s == null)
                {
                    // Debug.Assert(false, "");
                    return null;
                }

                s.CreateFrame(viewport);

                Debug.Assert(s != null, "");

                this.m_selectedShelfs.Add(s);
                if (this.m_propertyDlg != null)
                {
#if NO
                    if (this.m_selectedShelfs.Count == 1)
                    {
                        if (this.m_propertyDlg.IsEnabled == false)
                            this.m_propertyDlg.IsEnabled = true;

                        this.m_propertyDlg.PutInfo(s);
                    }
                    else
                        this.m_propertyDlg.IsEnabled = false;
#endif
                    OnSelectionChanged();
                }
                return s;
            }
            else if (strAction == "unselect")
            {
                if (s != null)
                {
                    viewport.Children.Remove(s.Frame);
                    s.Frame = null;

                    this.m_selectedShelfs.Remove(s);
                    if (this.m_propertyDlg != null)
                    {
                        // this.m_propertyDlg.Clear();
                        OnSelectionChanged();
                    }
                }
            }

            return s;
        }

        void OnSelectionChanged()
        {
            if (this.m_propertyDlg != null)
            {
                if (this.m_selectedShelfs.Count == 1)
                {
                    if (this.m_propertyDlg.IsEnabled == false)
                        this.m_propertyDlg.IsEnabled = true;

                    BookShelf shelf = this.m_selectedShelfs[0];
                    this.m_propertyDlg.PutInfo(shelf);
                }
                else
                {
                    this.m_propertyDlg.RefreshInfo();
                    this.m_propertyDlg.Clear();
                    this.m_propertyDlg.IsEnabled = false;
                }
            }
        }

        // 清除全部选择
        // parameters:
        //      excludes    要留下不清除的对象。如果为空，则表示全部清除
        void ClearSelection(List<BookShelf> excludes)
        {
            Debug.Assert(this.m_selectedShelfs != null, "");

            List<BookShelf> remains = new List<BookShelf>();
            foreach (BookShelf shelf in this.m_selectedShelfs)
            {
                Debug.Assert(shelf != null, "");

                if (excludes != null && excludes.IndexOf(shelf) != -1)
                {
                    remains.Add(shelf);
                    continue;
                }

                if (shelf.Frame != null)
                {
                    viewport.Children.Remove(shelf.Frame);
                    shelf.Frame = null;
                }
            }
            this.m_selectedShelfs.Clear();
            if (remains.Count > 0)
                this.m_selectedShelfs.AddRange(remains);

            OnSelectionChanged();
        }

        public static BitmapFrame Resize(
            ImageSource photo,
            int width,
            int height,
            BitmapScalingMode scalingMode)
        {
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(
                group, scalingMode);
            group.Children.Add(
                new ImageDrawing(photo,
                    new Rect(0, 0, width, height)));
            var targetVisual = new DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            var target = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);
            var targetFrame = BitmapFrame.Create(target);
            return targetFrame;
        }

        // 创建图像文件
        private void MenuItem_createImageFile_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            /*
            if (m_strCurrentXmlFilename.IndexOf(".png.") != -1)
            {
                MessageBox.Show(this, "您打开的文件 '" + m_strCurrentXmlFilename + "' 是一个图像文件的描述文件，用再创建图像文件，容易覆盖掉自身，建议不要这样操作");
            }
             * */

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = this.m_strCurrentPngFilename;
            dlg.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";

            Nullable<bool> dlg_result = dlg.ShowDialog();
            if (dlg_result == false)
                return;

            FrameworkElement v = this.layout;  // this.viewport;


            // 清除当前的选定，以便获得一个干净的图像
            this.ClearSelection(null);

            this.m_strCurrentPngFilename = dlg.FileName;

            double scale = 300 / 96;   // 4开始就不正常了

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(scale * (v.ActualWidth+1)),
                (int)(scale * (v.ActualHeight+1)),
                scale * 96,
                scale * 96,
                PixelFormats.Default);  // PixelFormats.Pbgra32
            bmp.Render(v);

#if NO
            ScaleTransform st = new ScaleTransform(1 / scale, 1 / scale);
            TransformedBitmap bmp1 = new TransformedBitmap(bmp, st);
#endif
            BitmapFrame bmp1 = Resize(
                bmp,
                (int)(bmp.PixelWidth / scale),
                (int)(bmp.PixelHeight / scale),
                BitmapScalingMode.HighQuality);

            PngBitmapEncoder png = new PngBitmapEncoder();
            // png.Frames.Add(BitmapFrame.Create(bmp1));
            png.Frames.Add(bmp1);
            using (Stream s = File.Create(dlg.FileName))
            {
                png.Save(s);
            }

#if NO
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            Stream s = new MemoryStream();
            encoder.Save(s);

            System.Drawing.Image source = System.Drawing.Bitmap.FromStream(s);


                    // 缩小图像
        // parameters:
        //		nNewWidth0	宽度(0表示不变化)
        //		nNewHeight0	高度
        //      bRatio  是否保持纵横比例
        // return:
        //      -1  出错
        //      0   没有必要缩放(objBitmap未处理)
        //      1   已经缩放
            int nRet = GraphicsUtil.ShrinkPic(ref source,
                (int)(bmp.PixelWidth / scale),
                0,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            source.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
            source.Dispose();
#endif

            GlobalInfo info = new GlobalInfo();
            // info.ImagePixelSize = new Size(bmp.PixelWidth, bmp.PixelHeight);
            info.ImagePixelSize = new Size((int)(bmp.PixelWidth / scale), (int)(bmp.PixelHeight / scale));

            // 紧接着保存一个图像说明文件
            nRet = Save(dlg.FileName + ".xml",
                    true,
                    info,
                    out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(strError);
        }

        // 书架属性
        private void MenuItem_viewport_property_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";

            if (this.m_selectedShelfs == null
                || this.m_selectedShelfs.Count == 0)
            {
                strError = "尚未选定要查看属性的书架";
                goto ERROR1;
            }

            BookShelf shelf = this.m_selectedShelfs[0];

            if (this.m_propertyDlg == null)
            {
                CreatePropertyDialog();
            }

            // this.m_propertyDlg.PutInfo(shelf);
            if (this.m_propertyDlg.IsVisible == false)
            {
                this.m_propertyDlg.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                this.m_propertyDlg.Top = Mouse.GetPosition(this).Y;
                this.m_propertyDlg.Left = Mouse.GetPosition(this).X;
                this.m_propertyDlg.Show();
            }

            OnSelectionChanged();
            // this.m_propertyDlg.PutInfo(shelf);
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                // DataDir = Application.LocalUserAppDataPath;
                // DataDir = Environment.CurrentDirectory;
                // DataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                // DataDir = ApplicationDeployment.CurrentDeployment.DataDirectory;
                DataDir = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            else
            {
                // MessageBox.Show(this, "no network");
                DataDir = Environment.CurrentDirectory;
            }

#if NO
            try
            {
                MessageBox.Show(this, "IsNetworkDeployed - '" + ApplicationDeployment.IsNetworkDeployed.ToString() + "'");
                

                // DataDir = ApplicationDeployment.CurrentDeployment.DataDirectory;
                DataDir = System.Windows.Forms.Application.UserAppDataPath;
                MessageBox.Show(this, "1 - '" + DataDir + "'");
            }
            catch
            {
                DataDir = Environment.CurrentDirectory;
                MessageBox.Show(this, "2 - '" + DataDir + "'");
            }
#endif

            CreatePropertyDialog();

            string strError = "";
            // 装载模型定义文件
            int nRet = LoadModels(
                this.DataDir + "\\models.xml",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MenuItem_new_Click(this, null);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_testWindow_Click(object sender, RoutedEventArgs e)
        {
            TestWindow window = new TestWindow();
            window.Show();
        }

        private void panningControl1_ButtonClick(object sender, ButtonClickEventArgs e)
        {
            if (e.ButtonName == "center")
            {
                AdjustCenter(1);
                AdjustCenter(0.1);
                AdjustCenter(0.01);
                return;
            }
            Panning(e.ButtonName, 1);
        }

        void Panning(string strButtonName,
            double step)
        {
            if (this.MenuItem_view_45degree.IsChecked == true)
            {
                Vector3D direction = this.camera1.LookDirection;
                direction /= direction.Length;  // 长度为一个单位的向量

                direction *= step;

                // Matrix3D matrix3D = this.camera1.Transform.Value;

                Vector3D vector3D = new Vector3D(0, 0, 0);
                if (strButtonName == "left")
                {
                    vector3D.X += direction.X;
                    vector3D.Z -= direction.Z;
                }
                else if (strButtonName == "right")
                {
                    vector3D.X -= direction.X;
                    vector3D.Z += direction.Z;
                }
                else if (strButtonName == "top")
                {
                    vector3D.X += direction.X;
                    vector3D.Z += direction.Z;
                }
                else if (strButtonName == "bottom")
                {
                    vector3D.X -= direction.X;
                    vector3D.Z -= direction.Z;
                }

                /*
                vector3D += new Vector3D(matrix3D.OffsetX, matrix3D.OffsetY, matrix3D.OffsetZ);

                matrix3D.OffsetX = vector3D.X;
                matrix3D.OffsetY = vector3D.Y;
                matrix3D.OffsetZ = vector3D.Z;
                this.camera1.Transform = new MatrixTransform3D(matrix3D);
                 * 
                 * */

                this.cameraTranslate.OffsetX += vector3D.X;
                this.cameraTranslate.OffsetY += vector3D.Y;
                this.cameraTranslate.OffsetZ += vector3D.Z;
            }
            else if (this.MenuItem_view_top.IsChecked == true)
            {
                // double step = 4;

                // Matrix3D matrix3D = this.camera1.Transform.Value;

                Vector3D vector3D = new Vector3D(0, 0, 0);
                if (strButtonName == "left")
                {
                    vector3D.X -= step;
                }
                else if (strButtonName == "right")
                {
                    vector3D.X += step;
                }
                else if (strButtonName == "top")
                {
                    vector3D.Z -= step;
                }
                else if (strButtonName == "bottom")
                {
                    vector3D.Z += step;
                }

                /*
                vector3D += new Vector3D(matrix3D.OffsetX, matrix3D.OffsetY, matrix3D.OffsetZ);

                matrix3D.OffsetX = vector3D.X;
                matrix3D.OffsetY = vector3D.Y;
                matrix3D.OffsetZ = vector3D.Z;
                this.camera1.Transform = new MatrixTransform3D(matrix3D);
                */
                this.cameraTranslate.OffsetX += vector3D.X;
                this.cameraTranslate.OffsetY += vector3D.Y;
                this.cameraTranslate.OffsetZ += vector3D.Z;

            }

            // this.camera1.Transform = new TranslateTransform3D();
        }


        // 顶部视图
        private void MenuItem_view_top_Click(object sender, RoutedEventArgs e)
        {
            this.camera1.LookDirection = new Vector3D(0, -1, 0);
            this.camera1.Position = new Point3D(0, 70, 0);
            this.camera1.UpDirection = new Vector3D(0, 0, -1);

            this.MenuItem_view_top.IsChecked = true;
            this.MenuItem_view_45degree.IsChecked = false;
        }

        // 斜向45度视图
        private void MenuItem_view_45degree_Click(object sender, RoutedEventArgs e)
        {
            this.camera1.LookDirection = new Vector3D(-14, -14, -14);
            this.camera1.Position = new Point3D(14, 14, 14);
            this.camera1.UpDirection = new Vector3D(0, 1, 0);

            this.MenuItem_view_top.IsChecked = false;
            this.MenuItem_view_45degree.IsChecked = true;

        }

        // 将选定的书架绕纵轴翻转180度
        private void MenuItem_viewport_rotate180_Click(object sender, RoutedEventArgs e)
        {
            foreach (BookShelf shelf in this.m_selectedShelfs)
            {
                Visual3D modelHit = shelf.Model;

#if NO
                try
                {
                    Matrix3D matrix3D = modelHit.Transform.Value;

                    matrix3D.RotateAt(new Quaternion(new Vector3D(0,1,0), 180),
                        new Point3D(matrix3D.OffsetX, matrix3D.OffsetY, matrix3D.OffsetZ));
                    modelHit.Transform = new MatrixTransform3D(matrix3D);

                }
                catch
                {
                    return;
                }
#endif

                // 修改内存中的度数
                shelf.Direction += 180;
                if (shelf.Direction >= 360)
                    shelf.Direction %= 360;
                shelf.RotateToDirection();

                // 如果一个书架正好被属性窗口监视
                if (this.m_propertyDlg != null
                    && shelf == this.m_propertyDlg.BookShelf)
                    this.m_propertyDlg.PutInfo(shelf);

            }
            this.Changed = true;

        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
#if NO
            double step = 5;
            double value = e.NewValue;  // *step;

            Matrix3D matrix3D = this.camera1.Transform.Value;
            matrix3D.Scale(new Vector3D(value, value, value));    // new Vector3D(0,1,0)
            this.camera1.Transform = new MatrixTransform3D(matrix3D);
#endif

            cameraScale.ScaleX = e.NewValue;
            cameraScale.ScaleY = e.NewValue;
            cameraScale.ScaleZ = e.NewValue;

            /*
            double step = 5;
            this.camera1.Position = new Point3D(this.camera1.Position.X,
                this.camera1.Position.Y + (e.NewValue - e.OldValue) * step,
                camera1.Position.Z);
             * */
        }

        // 修改地板属性
        private void MenuItem_floorProperty_Click(object sender, RoutedEventArgs e)
        {
            FloorInfoWindow dlg = new FloorInfoWindow();

            dlg.PutInfo(this.Floor);
            if (dlg.ShowDialog() == false)
                return;

            dlg.GetInfo(this.Floor);

            // 重建地板
            if (this.Floor != null
&& this.Floor.Model != null)
            {
                viewport.Children.Remove(this.Floor.Model);
                this.Floor.Model = null;
            }

            this.Floor.Model = CreateFloor(this.Floor.X,
            this.Floor.Z,
            this.Floor.Width,
            this.Floor.Height);
            viewport.Children.Add(this.Floor.Model);

            this.Changed = true;
        }

        private void MenuItem_batchChangeShelfProperties_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";

            if (this.m_selectedShelfs.Count == 0)
            {
                strError = "尚未选择要修改属性的书架";
                goto ERROR1;
            }

            ShelfInfoWindow dlg = new ShelfInfoWindow();

            dlg.Title = "批修改书架属性 ("+this.m_selectedShelfs.Count.ToString()+" 个)";
            dlg.SetModelState();
            if (dlg.ShowDialog() == false)
                return;

            try
            {
                // 确保对话框中的信息都兑现到内存对象中
                if (this.m_propertyDlg != null)
                    this.m_propertyDlg.RefreshInfo();

                // TODO: 需要警告同时修改X和Z值的情形，这样会把书架叠加在一起
                foreach (BookShelf shelf in this.m_selectedShelfs)
                {
                    dlg.PartialGetInfo(shelf);

#if NO
                    // 刷新显示
                    shelf.DisplayText();
                    shelf.MoveToPosition();
                    shelf.RotateToDirection();
#endif
                    shelf.CreateShelf(this.viewport);

                    // 如果一个书架正好被属性窗口监视
                    if (this.m_propertyDlg != null
                        && shelf == this.m_propertyDlg.BookShelf)
                        this.m_propertyDlg.PutInfo(shelf);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 将选定的书架复制到剪贴板
        private void MenuItem_viewport_copy_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";
            if (this.m_selectedShelfs.Count == 0)
            {
                strError = "尚未选定要复制的书架";
                goto ERROR1;
            }

            BookShelfCollection items = new BookShelfCollection();
            items.AddRange(this.m_selectedShelfs);

            Clipboard.SetDataObject(items, false);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从剪贴板粘贴书架
        private void MenuItem_viewport_paste_Click(object sender, RoutedEventArgs e)
        {
            string strError = "";
            BookShelfCollection items = null;
            IDataObject obj1 = Clipboard.GetDataObject();
            if (obj1.GetDataPresent(typeof(BookShelfCollection)))
            {
                items = (BookShelfCollection)obj1.GetData(typeof(BookShelfCollection));
            }
            else
            {
                strError = "当前Windows剪贴板中没有书架对象";
                goto ERROR1;
            }

            // 确保对话框中的信息都兑现到内存对象中
            if (this.m_propertyDlg != null)
                this.m_propertyDlg.RefreshInfo(); 
            
            this.ClearSelection(null);

            foreach (BookShelf item in items)
            {
                BookShelf shelf = new BookShelf();
                shelf.Width = item.Width;  // 5
                shelf.Height = item.Height; // 6;
                shelf.Depth = item.Depth; // 1;
                shelf.Thick = item.Thick; // 0.1;
                shelf.Level = item.Level; // 5;
                shelf.ColorString = item.ColorString;

#if NO
                TextBlock textblock = null;
                Model3DGroup group = CreateShelf(shelf.Width,
                        shelf.Height,
                        shelf.Depth,
                        shelf.Thick,
                        shelf.Level,
                        shelf.Color,
                        out textblock);
                ModelVisual3D model = new ModelVisual3D();
                model.Content = group;
                viewport.Children.Add(model);

                shelf.Model = model;
                shelf.TextBlock = textblock;
                shelf.SavePosition();
#endif
                shelf.CreateShelf(this.viewport);
                shelf.SavePosition();
                this.m_shelfs.Add(shelf);

                shelf.RoomName = item.RoomName;
                shelf.No = item.No;
                shelf.AccessNoRange = item.AccessNoRange;

                shelf.LocationX = item.LocationX + item.Width;
                shelf.LocationZ = item.LocationZ + item.Depth;
                shelf.Direction = item.Direction;

                // 刷新显示
                shelf.DisplayText();
                shelf.MoveToPosition();
                shelf.RotateToDirection();

                Select(shelf, "select");
                this.Changed = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #region 命令处理

        static double Radian(double degree)
        {
            return Math.PI * degree / 180.0;
        }

        // 将两个书架背靠在一起。第一个书架的位置和方向不动
        void BackTouch()
        {
            string strError = "";

            if (this.m_selectedShelfs.Count != 2)
            {
                strError = "请选择两个要背靠的书架，然后重新操作";
                goto ERROR1;
            }

            BookShelf shelf1 = this.m_selectedShelfs[0];
            BookShelf shelf2 = this.m_selectedShelfs[1];

            double center_x1 = shelf1.LocationX;
            double center_z1 = shelf1.LocationZ;

            double delta_x = -1 * Math.Sin(Radian(shelf1.Direction)) * (shelf1.Depth + 0.01);
            double delta_z = -1 * Math.Cos(Radian(shelf1.Direction)) * (shelf1.Depth + 0.01);

            // 计算第二个书架的中心位置
            double center_x2 = center_x1 + delta_x;
            double center_z2 = center_z1 + delta_z;

            shelf2.LocationX = center_x2;
            shelf2.LocationZ = center_z2;
            shelf2.Direction = shelf1.Direction + 180;
            if (shelf2.Direction >= 360)
                shelf2.Direction %= 360;

            shelf2.RotateToDirection();
            shelf2.MoveToPosition();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 将若干个书架平行靠在一起。第一个书架的位置和方向不动
        void RightTouch()
        {
            string strError = "";

            if (this.m_selectedShelfs.Count < 2)
            {
                strError = "请选择两个以上要右靠的书架，然后重新操作";
                goto ERROR1;
            }

            BookShelf shelf1 = this.m_selectedShelfs[0];
            double distance = 0;
            for (int i = 1; i < this.m_selectedShelfs.Count; i++)
            {
                BookShelf shelf2 = this.m_selectedShelfs[i];

                distance += shelf2.Width + 0.01;

                double center_x1 = shelf1.LocationX;
                double center_z1 = shelf1.LocationZ;

                double delta_z = -1 * Math.Sin(Radian(shelf1.Direction)) * distance;
                double delta_x = Math.Cos(Radian(shelf1.Direction)) * distance;

                // 计算第二个书架的中心位置
                double center_x2 = center_x1 + delta_x;
                double center_z2 = center_z1 + delta_z;

                shelf2.LocationX = center_x2;
                shelf2.LocationZ = center_z2;
                shelf2.Direction = shelf1.Direction;

                shelf2.RotateToDirection();
                shelf2.MoveToPosition();
            }


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }



        #endregion

        void EditCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            string strName = ((RoutedCommand)e.Command).Name;
            string strTarget = ((FrameworkElement)target).Name;

            if (strName == "Paste")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    MenuItem_viewport_paste_Click(null, null);
                }
            }
            else if (strName == "Copy")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    MenuItem_viewport_copy_Click(null, null);
                }
            }
            else if (strName == "Cut")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    MenuItem_viewport_copy_Click(null, null);
                    DeleteAllSelected();
                }
            }
            else if (strName == "SelectAll")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    SelectAll();
                }
            }
            else if (strName == "Delete")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    DeleteAllSelected();
                }
            }
            else if (strName == "Properties")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    MenuItem_viewport_property_Click(null, null);
                }
            }
            else if (strName == "BatchChange")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    MenuItem_batchChangeShelfProperties_Click(null, null);
                }
            }
            else if (strName == "Rotate180")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    MenuItem_viewport_rotate180_Click(null, null);
                }
            }
            else if (strName == "AddPair")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    NewPair();
                }
            }
            else if (strName == "AddMulti")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    NewMulti();
                }
            }
            else if (strName == "BackTouch")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    BackTouch();
                }
            }
            else if (strName == "RightTouch")
            {
                if (strTarget == "viewport"
                    || target == this)
                {
                    RightTouch();
                }
            }
        }

        void EditCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            string strName = ((RoutedCommand)e.Command).Name;

            if (strName == "Paste")
            {
                IDataObject obj1 = Clipboard.GetDataObject();
                if (obj1.GetDataPresent(typeof(BookShelfCollection)))
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "Copy")
            {
                if (this.m_selectedShelfs.Count > 0)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "Cut")
            {
                if (this.m_selectedShelfs.Count > 0)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "SelectAll")
            {
                if (this.m_selectedShelfs.Count < this.m_shelfs.Count)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "Delete")
            {
                if (this.m_selectedShelfs.Count > 0)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "Properties")
            {
                if (this.m_selectedShelfs.Count == 1)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "BatchChange")
            {
                if (this.m_selectedShelfs.Count > 0)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "Rotate180")
            {
                if (this.m_selectedShelfs.Count > 0)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "AddPair")
            {
                e.CanExecute = true;
            }
            else if (strName == "AddMulti")
            {
                e.CanExecute = true;
            }
            else if (strName == "BackTouch")
            {
                if (this.m_selectedShelfs.Count == 2)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "RightTouch")
            {
                if (this.m_selectedShelfs.Count > 1)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
        }

        private void MenuItem_help_operDataDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        private void MenuItem_help_operProgramDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        private void MenuItem_center_Click(object sender, RoutedEventArgs e)
        {
            AdjustCenter(1);
            AdjustCenter(0.1);
            AdjustCenter(0.01);
        }

        void AdjustCenter(double step)
        {
            double center_x = viewport.ActualWidth / 2;
            double center_y = viewport.ActualHeight / 2;

            for (; ; )
            {
                Rect rect = GetModelBounds(this.Floor.Model);
                double x0 = rect.X + rect.Width / 2;
                double y0 = rect.Y + rect.Height / 2;

                if (x0 > center_x)
                    Panning("right", step);
                else
                    break;
            }

            for (; ; )
            {
                Rect rect = GetModelBounds(this.Floor.Model);
                double x0 = rect.X + rect.Width / 2;
                double y0 = rect.Y + rect.Height / 2;

                if (y0 > center_y)
                    Panning("bottom", step);
                else
                    break;
            }

            // 

            for (; ; )
            {
                Rect rect = GetModelBounds(this.Floor.Model);
                double x0 = rect.X + rect.Width / 2;
                double y0 = rect.Y + rect.Height / 2;

                if (x0 < center_x)
                    Panning("left", step);
                else
                    break;
            }

            for (; ; )
            {
                Rect rect = GetModelBounds(this.Floor.Model);
                double x0 = rect.X + rect.Width / 2;
                double y0 = rect.Y + rect.Height / 2;

                if (y0 < center_y)
                    Panning("top", step);
                else
                    break;
            }
        }

        private void MenuItem_textProperty_Click(object sender, RoutedEventArgs e)
        {
            TextPanelWindow dlg = new TextPanelWindow();
            dlg.Xml = this.m_strTextPanelDef;
            dlg.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() == false)
                return;

            string strError = "";
            int nRet = TextPanelWindow.CreateTextPanel(this.canvas_text,
                dlg.Xml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            this.m_strTextPanelDef = dlg.Xml;

            if (string.IsNullOrEmpty(this.m_strTextPanelDef) == false)
                this.MenuItem_displayText.IsEnabled = true;
            else
                this.MenuItem_displayText.IsEnabled = false;

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(strError);
        }

        private void text_wrapper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MenuItem_textProperty_Click(this, new RoutedEventArgs());
        }

        private void MenuItem_displayText_Click(object sender, RoutedEventArgs e)
        {
            if (this.canvas_text.Visibility == System.Windows.Visibility.Visible)
            {
                this.canvas_text.Visibility = System.Windows.Visibility.Hidden;
                this.MenuItem_displayText.IsChecked = false;
            }
            else
            {
                this.canvas_text.Visibility = System.Windows.Visibility.Visible;
                this.MenuItem_displayText.IsChecked = true;
            }
        }
    }

    // 一个书架
    [Serializable()]
    public class BookShelf
    {
        public string Caption
        {
            get
            {
                return this.No;
            }
        }

        public string RoomName = "";    // 馆藏地点名。例如阅览室名，书库名
        public string No = "";  // 架号 一般从1开始编号
        public string AccessNoRange = "";   // 索取号范围
        public string ColorString = "";    // 颜色

        public Color Color
        {
            get
            {
                if (string.IsNullOrEmpty(this.ColorString) == false)
                    return (Color)ColorConverter.ConvertFromString(this.ColorString);
                return Colors.White;
            }
        }

        public double Width = 0;    // 宽度
        public double Height = 0;   // 高度
        public double Depth = 0;    // 深度

        public double Thick = 0.1;  // 板子厚度

        public int Level = 5;   // 书架层数

        // 书架的中点位置
        public double LocationX = 0;
        public double LocationZ = 0;

        public double Direction = 0;    // 朝向 度数

        [NonSerialized()]
        public ModelVisual3D Model = null;
        [NonSerialized()]
        public TextBlock TextBlock = null;
        // 外框
        [NonSerialized()]
        public ModelVisual3D Frame = null;

        [NonSerialized()]
        public Point? TopPoint = null;  // bitmap中，书架顶板中点像素位置

        public void SavePosition()
        {
            Matrix3D matrix3D = this.Model.Transform.Value;

            this.LocationX = matrix3D.OffsetX;
            this.LocationZ = matrix3D.OffsetZ;

            // 同步框子的位置
            if (this.Frame != null)
                this.Frame.Transform = this.Model.Transform.Clone();
        }

        public static TranslateTransform3D FindTranslate(ModelVisual3D model)
        {
            if (!(model.Transform is Transform3DGroup))
                return null;

            Transform3DGroup group = (Transform3DGroup)model.Transform;

            foreach (Transform3D trans in group.Children)
            {
                if (trans is TranslateTransform3D)
                {
                    return (TranslateTransform3D)trans;
                }
            }

            return null;
        }

        public static RotateTransform3D ClearRotate(ModelVisual3D model)
        {
            if (!(model.Transform is Transform3DGroup))
                return null;

            Transform3DGroup group = (Transform3DGroup)model.Transform;

            RotateTransform3D rotate = null;
            foreach (Transform3D trans in group.Children)
            {
                if (trans is RotateTransform3D)
                {
                    rotate = (RotateTransform3D)trans;
                    break;
                }
            }

            group.Children.Remove(rotate);

            return rotate;
        }

        public static RotateTransform3D FindRotate(ModelVisual3D model)
        {
            if (!(model.Transform is Transform3DGroup))
                return null;

            Transform3DGroup group = (Transform3DGroup)model.Transform;

            foreach (Transform3D trans in group.Children)
            {
                if (trans is RotateTransform3D)
                {
                    return (RotateTransform3D)trans;
                }
            }

            return null;
        }

        // 翻转到  this.Direction 表示的角度
        public void RotateToDirection()
        {
            TranslateTransform3D move = FindTranslate(this.Model);
            if (move == null)
            {
                Transform3DGroup group = null;
                if (!(this.Model.Transform is Transform3DGroup))
                {
                    group = new Transform3DGroup();
                    this.Model.Transform = group;
                }
                else
                    group = (Transform3DGroup)this.Model.Transform;

                move = new TranslateTransform3D();
                move.OffsetX = this.LocationX;
                move.OffsetZ = this.LocationZ;

                group.Children.Insert(0, move);
            }
            else
            {
                move.OffsetX = this.LocationX;
                move.OffsetZ = this.LocationZ;
            }

            RotateTransform3D rotate = FindRotate(this.Model);
            if (rotate == null)
            {
                Transform3DGroup group = null;
                if (!(this.Model.Transform is Transform3DGroup))
                {
                    group = new Transform3DGroup();
                    this.Model.Transform = group;
                }
                else
                    group = (Transform3DGroup)this.Model.Transform;

                rotate = new RotateTransform3D();
                /*
                rotate.CenterX = 0;
                rotate.CenterY = 0;
                rotate.CenterZ = 0;
                 * */
                AxisAngleRotation3D a = new AxisAngleRotation3D();
                a.Angle = this.Direction;
                a.Axis = new Vector3D(0, 1, 0);
                rotate.Rotation = a;
                rotate.CenterX = move.OffsetX;
                rotate.CenterY = move.OffsetY;
                rotate.CenterZ = move.OffsetZ;

                group.Children.Add(rotate);
            }
            else
            {
                AxisAngleRotation3D a = new AxisAngleRotation3D();
                a.Angle = this.Direction;
                a.Axis = new Vector3D(0, 1, 0);
                rotate.Rotation = a;
                rotate.CenterX = move.OffsetX;
                rotate.CenterY = move.OffsetY;
                rotate.CenterZ = move.OffsetZ;

            }

            if (this.Frame != null)
                this.Frame.Transform = this.Model.Transform.Clone();
        }

        // 移动到LocationX 和 LocationZ 表示的位置
        public void MoveToPosition()
        {
            TranslateTransform3D move = FindTranslate(this.Model);
            if (move == null)
            {
                Transform3DGroup group = null;
                if (!(this.Model.Transform is Transform3DGroup))
                {
                    group = new Transform3DGroup();
                    this.Model.Transform = group;
                }
                else
                    group = (Transform3DGroup)this.Model.Transform;

                move = new TranslateTransform3D();
                move.OffsetX = this.LocationX;
                move.OffsetZ = this.LocationZ;

                group.Children.Insert(0, move);
            }
            else
            {
                move.OffsetX = this.LocationX;
                move.OffsetZ = this.LocationZ;
            }

            if (this.Frame != null)
                this.Frame.Transform = this.Model.Transform.Clone();
#if NO
            Matrix3D matrix3D = this.Model.Transform.Value;

            matrix3D.OffsetX = this.LocationX;
            matrix3D.OffsetZ = this.LocationZ;
            this.Model.Transform = new MatrixTransform3D(matrix3D);

            if (this.Frame != null)
                this.Frame.Transform = this.Model.Transform.Clone();
#endif
        }

        public static double GetDouble(string strValue,
            string strWarningPrefix,
            ref string strError)
        {
            double v = 0;

            if (string.IsNullOrEmpty(strValue) == true)
                return v;

            if (double.TryParse(strValue, out v) == false)
            {
                strError += strWarningPrefix + " '" + strValue + "' 格式不正确";
                return 0;
            }

            return v;
        }

        public static int GetInt(string strValue,
    string strWarningPrefix,
    ref string strError)
        {
            int v = 0;
            if (int.TryParse(strValue, out v) == false)
            {
                strError += strWarningPrefix + " '" + strValue + "' 格式不正确";
                return 0;
            }

            return v;
        }

        public int SetValueFromXmlNode(XmlNode node,
            out string strError)
        {
            strError = "";

            string strLocation = DomUtil.GetAttr(node, "location");

            Hashtable table = StringUtil.ParseParameters(strLocation);

            this.LocationX = GetDouble((string)table["x"],
                "location属性中x=部分",
                ref strError);

            this.LocationZ = GetDouble((string)table["z"],
                "location属性中z=部分",
                ref strError);

            string strWidth = DomUtil.GetAttr(node, "width");
            this.Width = GetDouble(strWidth, "width属性", ref strError);

            string strHeight = DomUtil.GetAttr(node, "height");
            this.Height = GetDouble(strHeight, "height属性", ref strError);

            string strDepth = DomUtil.GetAttr(node, "depth");
            this.Depth = GetDouble(strDepth, "depth属性", ref strError);

            string strDirection = DomUtil.GetAttr(node, "direction");
            this.Direction = GetDouble(strDirection, "direction属性", ref strError);

            string strThick = DomUtil.GetAttr(node, "thick");
            this.Thick = GetDouble(strThick, "thick属性", ref strError);

            string strLevel = DomUtil.GetAttr(node, "level");
            this.Level = GetInt(strLevel, "level属性", ref strError);

            this.RoomName = DomUtil.GetAttr(node, "roomName");
            this.No = DomUtil.GetAttr(node, "no");
            this.AccessNoRange = DomUtil.GetAttr(node, "accessNoRange");
            this.ColorString = DomUtil.GetAttr(node, "color");

            if (string.IsNullOrEmpty(strError) == false)
                return -1;
            return 0;
        }

        public void BuildXmlNode(XmlNode node)
        {
            DomUtil.SetAttr(node, "roomName", this.RoomName);
            DomUtil.SetAttr(node, "no", this.No);
            DomUtil.SetAttr(node, "accessNoRange", this.AccessNoRange);
            DomUtil.SetAttr(node, "color", this.ColorString);

            DomUtil.SetAttr(node, "location", "x=" + this.LocationX.ToString() + ",z=" + this.LocationZ.ToString());

            DomUtil.SetAttr(node, "width", this.Width.ToString());
            DomUtil.SetAttr(node, "height", this.Height.ToString());
            DomUtil.SetAttr(node, "depth", this.Depth.ToString());
            DomUtil.SetAttr(node, "direction", this.Direction.ToString());
            DomUtil.SetAttr(node, "thick", this.Thick.ToString());
            DomUtil.SetAttr(node, "level", this.Level.ToString());

            if (this.TopPoint != null)
                DomUtil.SetAttr(node, "topPoint", this.TopPoint.Value.X.ToString() + "," + this.TopPoint.Value.Y.ToString());
        }

        static Color Reverse(Color color)
        {
            return Color.FromArgb(
                color.A,
                (byte)(byte.MaxValue - color.R),
                (byte)(byte.MaxValue - color.G),
                (byte)(byte.MaxValue - color.B));
        }

        // 创建外框
        public void CreateFrame(Viewport3D viewport)
        {
            Debug.Assert(viewport != null, "");

            if (this.Frame == null)
            {
                double delta = 0.01;    // 0.02

                this.Frame = MainWindow.CreateWiredCube(this.Width + delta,
                    this.Height + delta,
                    this.Depth + delta,
                    Reverse(this.Color));
            // if (viewport.Children.IndexOf(this.Frame) == -1)
                viewport.Children.Add(this.Frame);
            }

            this.Frame.Transform = this.Model.Transform.Clone();
        }

        public void CreateShelf(Viewport3D viewport)
        {
            // 清除以前残留的Model
            if (this.Model != null)
            {
                viewport.Children.Remove(this.Model);
                this.Model = null;
            }

            TextBlock textblock = null;

            Model3DGroup group = MainWindow.CreateShelf(this.Width,
                this.Height,
                this.Depth,
                this.Thick,
                this.Level,
                this.Color,
                out textblock);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = group;
            viewport.Children.Add(model);

            textblock.Text = this.No;

            this.Model = model;
            this.TextBlock = textblock;
            this.MoveToPosition();
            this.RotateToDirection();
        }

        // 兑现显示顶部标签
        public void DisplayText()
        {
            if (this.TextBlock == null)
            {
                Debug.Assert(false, "");
                return;
            }

            this.TextBlock.Text = this.No;
        }
    }

    [Serializable()]
    public class BookShelfCollection : List<BookShelf>
    {
    }

    // 希望写入文件的全剧信息
    public class GlobalInfo
    {
        // 图像的像素尺寸
        public Size? ImagePixelSize = null; 
    }

    public class Floor
    {
        public double X = 0;
        public double Z = 0;
        public double Width = 0;
        public double Height = 0;
        public ModelVisual3D Model = null;

        public void BuildXmlNode(XmlNode node)
        {
            DomUtil.SetAttr(node, "center", "x=" + this.X.ToString() + ",z=" + this.Z.ToString());

            DomUtil.SetAttr(node, "width", this.Width.ToString());
            DomUtil.SetAttr(node, "height", this.Height.ToString());
        }

        public int SetValueFromXmlNode(XmlNode node,
    out string strError)
        {
            strError = "";

            string strLocation = DomUtil.GetAttr(node, "center");

            Hashtable table = StringUtil.ParseParameters(strLocation);

            this.X = BookShelf.GetDouble((string)table["x"],
                "center属性中x=部分",
                ref strError);


            this.Z = BookShelf.GetDouble((string)table["z"],
                "center属性中z=部分",
                ref strError);

            string strWidth = DomUtil.GetAttr(node, "width");
            this.Width = BookShelf.GetDouble(strWidth, "width属性", ref strError);

            string strHeight = DomUtil.GetAttr(node, "height");
            this.Height = BookShelf.GetDouble(strHeight, "height属性", ref strError);

            if (string.IsNullOrEmpty(strError) == false)
                return -1;
            return 0;
        }

    }

    public static class MyCommand
    {
        public static readonly RoutedUICommand BatchChange = new RoutedUICommand("批修改属性", "BatchChange", typeof(MainWindow));
        public static readonly RoutedUICommand Rotate180 = new RoutedUICommand("旋转180度", "Rotate180", typeof(MainWindow));
        public static readonly RoutedUICommand AddPair = new RoutedUICommand("添加一对书架", "AddPair", typeof(MainWindow));
        public static readonly RoutedUICommand AddMulti = new RoutedUICommand("添加多个书架", "AddMulti", typeof(MainWindow));
        public static readonly RoutedUICommand BackTouch = new RoutedUICommand("背靠", "BackTouch", typeof(MainWindow));
        public static readonly RoutedUICommand RightTouch = new RoutedUICommand("右靠", "RightTouch", typeof(MainWindow));
        // public static readonly RoutedUICommand ToggleTextVisible = new RoutedUICommand("显示说明文字", "ToggleTextVisible", typeof(MainWindow));
    }
}
