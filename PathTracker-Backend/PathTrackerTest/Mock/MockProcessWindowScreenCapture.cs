using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathTracker_Backend;

namespace PathTrackerTest {
    public class MockProcessWindowScreenCapture : IProcessWindowScreenshotCapture {

        private Queue<Tuple<Graphics, Bitmap>> QueueValues = new Queue<Tuple<Graphics, Bitmap>>();

        public void AddNewToQueue(Graphics graph, Bitmap bmp) {
            QueueValues.Enqueue(new Tuple<Graphics, Bitmap>(graph, bmp));
        }

        public (Graphics, Bitmap) GetProcessScreenshot(string name) {
            Tuple<Graphics, Bitmap> tup = QueueValues.Dequeue();

            return (tup.Item1, tup.Item2);
        }
    }
}
