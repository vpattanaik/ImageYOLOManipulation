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
using Microsoft.Win32;

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
        string pathOfOneImage;
        string[] pathOfAllImages;
        int countOfImagesWithFish;
        int countOfEmptyImags;
        FishImage[] Fishes;
        int indexofShowingFish = 0;
        bool stopPlay = false;
        
        private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog loadImageFiles = new OpenFileDialog();
            loadImageFiles.DefaultExt = "*.jpg";
            loadImageFiles.Title = "choose one images and will load the whole Folder";
            loadImageFiles.ShowDialog();
            pathOfOneImage = loadImageFiles.FileName;
            Thread showImage = new Thread(ShowImage);
            showImage.SetApartmentState(ApartmentState.STA);
            showImage.Start();
        }
        void ShowImage()
        {
            pathOfAllImages = Directory.GetFiles(System.IO.Path.GetDirectoryName(pathOfOneImage), "*.jpeg", SearchOption.TopDirectoryOnly);
            Fishes = new FishImage[pathOfAllImages.Length];
            int index = 0;
            foreach ( string pathOfImage in pathOfAllImages)
            {
                Fishes[index] = new FishImage(pathOfImage);
                if (Fishes[index].IsAnnotated() == true)
                    countOfImagesWithFish++;
                else
                    countOfEmptyImags++;
                index++;
            }
             
            
            countOfFishesText.Dispatcher.Invoke(new Action(() => { countOfFishesText.Text = "Count of Image with Fish : " + countOfImagesWithFish.ToString(); }));
            countOfEmptyFishesText.Dispatcher.Invoke(new Action(() => { countOfEmptyFishesText.Text = "Count of Image without Fish : " + countOfEmptyImags.ToString(); }));
            GUIShowImage();
        }
        private void GUIShowImage()
        {
            bool show = false;
            bool showFishesImages = false, showEmptyImages = false;
            showEmpty.Dispatcher.Invoke(new Action(() => { showEmptyImages =(bool) showEmpty.IsChecked; }));
            showFish.Dispatcher.Invoke(new Action(() => { showFishesImages = (bool)showFish.IsChecked; }));
            while (!show)
            {
                if (showFishesImages == true && showEmptyImages == true)
                {
                    show = true;
                }
                else if (showFishesImages == false && showEmptyImages == true)
                {
                    if (Fishes[indexofShowingFish].IsAnnotated() == true)
                        indexofShowingFish++;
                    else
                        show = true;
                }
                else if (showFishesImages == true && showEmptyImages == false)
                {
                    if (Fishes[indexofShowingFish].IsAnnotated() == false)
                        indexofShowingFish++;
                    else
                        show = true;
                }
                else
                    return;
            }
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Source = Fishes[indexofShowingFish].GetImage(); }));
            imageFishName.Dispatcher.Invoke(new Action(() => { imageFishName.Text = "Name : " + Fishes[indexofShowingFish].ImageName(); }));
            IndexText.Dispatcher.Invoke(new Action(() => { IndexText.Text = "Image " + indexofShowingFish.ToString() + " of " + pathOfAllImages.Length.ToString(); }));
            if (Fishes[indexofShowingFish].IsAnnotated())
            {
                double[][] fishes = Fishes[indexofShowingFish].GetFishRectangles();
                //canvasFish.Children.Add(fishBox);
                canvasFish.Dispatcher.Invoke(new Action(() => { Canvas.SetLeft(fishBox, fishes[0][0]); }));
                canvasFish.Dispatcher.Invoke(new Action(() => { Canvas.SetTop(fishBox, fishes[0][1]); }));
                fishBox.Dispatcher.Invoke(new Action(() => { fishBox.Width = fishes[0][2];  }));
                fishBox.Dispatcher.Invoke(new Action(() => { fishBox.Height = fishes[0][3]; }));
            }
        }
        private void Invoke(Action action)
        {
            throw new NotImplementedException();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
           
            Thread showPlayImage = new Thread(ShowPlayImage);
            showPlayImage.SetApartmentState(ApartmentState.STA);
            if (buttonPlay.Content.ToString() == "Play")
            {
                buttonPlay.Content = "Pause";
                stopPlay = false;
                showPlayImage.Start();
            }
            else
            {
                buttonPlay.Content = "Play";
                stopPlay = true;
            }
            
            
        }
       
        private void ShowPlayImage()
        {
            while (!stopPlay)
            {
                indexofShowingFish++;
                GUIShowImage();                
                double speed = 0;
                slidePlay.Dispatcher.Invoke(new Action(() => { speed = slidePlay.Value; }));
                System.Threading.Thread.Sleep((int)speed);
            }
        }
    }

    class FishImage
    {
        private readonly string imagePath = "";
        private readonly string imageName = "";
        private readonly string imageDirectory = "";
        private readonly string annotationPath = "";
        private  double imageWidth;
        private  double imageHeight;
        private  BitmapImage fishImage;
        private double[][] fishRectangles;
        private bool isImageLoaded;
        public FishImage(string path)
        {
            imagePath = path;
            DirectoryInfo directoryInfo = Directory.GetParent(path);
            imageName = System.IO.Path.GetFileNameWithoutExtension(path);
            imageDirectory = directoryInfo.ToString();
             
            
            if (File.Exists(imageDirectory + "/" + imageName + ".txt"))
            {
                annotationPath = imageDirectory + "/" + imageName + ".txt";  
            }
            else
                annotationPath = null;
        }
        public bool IsAnnotated()
        {
            if (annotationPath == null)
                return false;
            else
            {
                string[] fisheAnnotationLine = File.ReadAllLines(annotationPath);
                if (fisheAnnotationLine.Length > 0)
                    return true;
                else
                    return false;
            }
        }
        public string ImageName()
        {
            return imageName;
        }
        public BitmapImage GetImage()
        {
            fishImage = new BitmapImage(new Uri(imagePath));        
            imageWidth = fishImage.Width;
            imageHeight = fishImage.Height;
            isImageLoaded = true;
            return fishImage;
        }
        public double[] [] GetFishRectangles()
        {
            if (annotationPath != null && isImageLoaded == true)
            {
                Operations operationRect = new Operations();
                fishRectangles = operationRect.GetRectanglesOfFishes(annotationPath, imageWidth, imageHeight);
                return fishRectangles;
            }
            else
                return null;
        }
        public void UnloadImage()
        {
            fishImage = null;
        }
         ~FishImage()
        {
            isImageLoaded = false;
            fishImage = null;

        }
    }

    class Operations
    {
        public Operations()
        {

        }
        public double [][] GetRectanglesOfFishes (string annotationPath ,double width, double height)
        {
            string[] fisheAnnotationLine = File.ReadAllLines(annotationPath);
            double[][] fishRectangles = new double[fisheAnnotationLine.Length][];
            int indexer = 0;
            foreach (string rectagle in fisheAnnotationLine)
            {
                string[] splittedString = rectagle.Split(' ');
                if (splittedString.Length > 4)
                {
                    double x, y, w, h;
                    x = ((width * Convert.ToDouble(splittedString[1])) - (width * Convert.ToDouble(splittedString[3]) / 2));
                    y = ((height * Convert.ToDouble(splittedString[2])) - (height * Convert.ToDouble(splittedString[4]) / 2));
                    w = width * Convert.ToDouble(splittedString[3]);
                    h = height * Convert.ToDouble(splittedString[4]);
                    // Create rectangle.
                    fishRectangles[indexer] = new double[4];                        
                    fishRectangles[indexer][0] = Convert.ToInt32(x);
                    fishRectangles[indexer][1] = Convert.ToInt32(y);
                    fishRectangles[indexer][2] = Convert.ToInt32(w);
                    fishRectangles[indexer][3] = Convert.ToInt32(h);
                    indexer++;
                }
            }
            return fishRectangles;
        }
    }
}
