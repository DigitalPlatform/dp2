using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace DigitalPlatform
{
    public class FormProperty
    {
        public Size Size { get; set; }
        public Point Location { get; set; }
        public FormWindowState WindowState { get; set; }

        public static FormProperty Build(Form form)
        {
            FormProperty result = new FormProperty();

            if (form.WindowState == FormWindowState.Normal)
            {
                // save location and size if the state is normal
                result.Size = form.Size;
                result.Location = form.Location;
            }
            else
            {
                // save the RestoreBounds if the form is minimized or maximized!
                result.Size = form.RestoreBounds.Size;
                result.Location = form.RestoreBounds.Location;
            }

            result.WindowState = form.WindowState;
            return result;
        }

        public static string GetProperty(Form form)
        {
            return JsonConvert.SerializeObject(FormProperty.Build(form));
        }

        public static void SetProperty(string value,
            Form form,
            bool force_minimize = false)
        {
            FormProperty property = JsonConvert.DeserializeObject<FormProperty>(value);

            // 如果不先用 Normal 状态打开，则 taskbar 上的缩略图有问题，不便操作
            {
                if (property.WindowState == FormWindowState.Minimized)
                    property.WindowState = FormWindowState.Normal;
            }
            property.SetTo(form);

            if (force_minimize)
            {
                Task.Run(() =>
                {
                    Task.Delay(2000).Wait();
                    form.BeginInvoke((Action)(() =>
                    {
                        form.WindowState = FormWindowState.Minimized;
                    }));
                });
            }
        }

        public void SetTo(Form form)
        {
            form.Size = this.Size;
            form.Location = this.Location;
            form.WindowState = this.WindowState;
        }
    }

}
