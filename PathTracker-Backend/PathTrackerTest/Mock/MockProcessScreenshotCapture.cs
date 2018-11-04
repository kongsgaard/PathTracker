using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class MockProcessScreenshotCapture : IProcessWindowScreenshotCapture {

        public (Graphics, Bitmap) GetProcessScreenshot(string name) {

            Tuple<Graphics, Bitmap> tuple = returnQueue.Dequeue();

            return (tuple.Item1, tuple.Item2);
        }

        private Queue<Tuple<Graphics, Bitmap>> returnQueue = new Queue<Tuple<Graphics, Bitmap>>();

        public void SetupImageAddToQueue(string imageFilePath) {
            Image image = Image.FromFile(imageFilePath);

            Graphics g = Graphics.FromImage(image);

            Bitmap b = new Bitmap(image);

            returnQueue.Enqueue(new Tuple<Graphics, Bitmap>(g, b));
        }

    }
}
