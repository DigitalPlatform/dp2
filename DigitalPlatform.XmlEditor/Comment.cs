using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
    public class Comment : TextVisual
    {
        public override ItemRegion GetRegionName()
        {
            return ItemRegion.Comment;
        }
    }
}
