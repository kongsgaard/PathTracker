using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace PathTracker_Backend
{
    public interface IProcessWindowScreenshotCapture
    {
        (Graphics,Bitmap) GetProcessScreenshot(string name);
    }
}
