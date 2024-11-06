using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;

namespace Image_Preview.Controls
{
    public class PickedEventArgs : EventArgs
    {
        public string ImagePath { get; set; }
        public Image Thumbnail { get; set; }

        public PickedEventArgs(string imagePath, Image thumbnail)
        {
            ImagePath = imagePath;
            Thumbnail = thumbnail;
        }
    }
}
