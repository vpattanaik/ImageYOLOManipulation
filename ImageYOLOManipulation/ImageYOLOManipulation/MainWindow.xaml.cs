using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

namespace ImageYOLOManipulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
        {
            Thread showImage = new Thread(ShowImage);
            showImage.Start();
        }
        void ShowImage()
        {

        }

    }

    class FishImage
    {
        private readonly string imagePath = "";
        private readonly string imageName = "";
        private readonly string imageDirectory = "";
        private readonly string annotationPath = "";
        private readonly int imageWidth = 0;
        private readonly int imageHeight = 0;
        private readonly Image fishImage;
        private readonly Rectangle [] fishRectangles;
        public FishImage(string path)
        {
            imagePath = path;
            DirectoryInfo directoryInfo = Directory.GetParent(path);
            imageName = System.IO.Path.GetFileNameWithoutExtension(path);
            imageDirectory = directoryInfo.ToString();
            if (File.Exists(imageDirectory+"/"+ imageName+".txt"))
            {
                annotationPath = imageDirectory + "/" + imageName + ".txt";

            }
           
        }
    }
}
