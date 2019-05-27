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
            //global::ImageYOLOManipulation.Properties.Resources
            GetImage(Environment.CurrentDirectory + "/FishWhite.png");
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseMove += MainWindow_MouseMove;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
        }



        Point [] startPoint = new Point[0];
        Rectangle[] rectAnnotate = new  Rectangle[0];
        bool isRectStarted = false;

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isAnnotation)
            {
                Array.Resize(ref startPoint, startPoint.Length + 1);
                startPoint[startPoint.Length - 1] = e.GetPosition(canvasFish);
                Array.Resize(ref rectAnnotate, rectAnnotate.Length + 1);
                rectAnnotate[rectAnnotate.Length - 1] = new Rectangle{Stroke = Brushes.LightBlue,StrokeThickness = 2};
                Canvas.SetLeft(rectAnnotate[rectAnnotate.Length - 1], startPoint[startPoint.Length - 1].X);
                Canvas.SetTop(rectAnnotate[rectAnnotate.Length - 1], startPoint[startPoint.Length - 1].Y);
                canvasFish.Children.Add(rectAnnotate[rectAnnotate.Length - 1]);
                isRectStarted = true;
            }

        }
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
           if (isRectStarted)
            {
                Point tempPoint = e.GetPosition(canvasFish);
                rectAnnotate[rectAnnotate.Length - 1].Width =Math.Max( tempPoint.X - startPoint[startPoint.Length - 1].X,0);
                rectAnnotate[rectAnnotate.Length - 1].Height = Math.Max(tempPoint.Y - startPoint[startPoint.Length - 1].Y,0);
            }
        }
        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isRectStarted)
            {
                Point tempPoint = e.GetPosition(canvasFish);
                rectAnnotate[rectAnnotate.Length - 1].Width = Math.Max(tempPoint.X - startPoint[startPoint.Length - 1].X,0);
                rectAnnotate[rectAnnotate.Length - 1].Height = Math.Max(tempPoint.Y - startPoint[startPoint.Length - 1].Y,0);
                isRectStarted = false;
            }

        }

        string pathOfOneImage;
        string[] pathOfAllImages;
        int countOfImagesWithFish;
        int countOfEmptyImags;
        FishImage[] Fishes;
        int indexofShowingFish = 0;
        bool stopPlay = false;
        int[] listOfAnnotatedWithObjects = new int[0];
        int[] listOfAnnotatedWithoutObjects = new int[0];
        bool isAnnotation = false;
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
                {
                    countOfImagesWithFish++;
                    Array.Resize(ref listOfAnnotatedWithObjects, listOfAnnotatedWithObjects.Length + 1);
                    listOfAnnotatedWithObjects[listOfAnnotatedWithObjects.Length - 1] = index;
                }
                else
                {
                    countOfEmptyImags++;
                    Array.Resize(ref listOfAnnotatedWithoutObjects, listOfAnnotatedWithoutObjects.Length + 1);
                    listOfAnnotatedWithoutObjects[listOfAnnotatedWithoutObjects.Length - 1] = index;
                }
                index++;
            }
            countOfFishesText.Dispatcher.Invoke(new Action(() => { countOfFishesText.Text = "Count of Image with Fish : " + countOfImagesWithFish.ToString(); }));
            countOfEmptyFishesText.Dispatcher.Invoke(new Action(() => { countOfEmptyFishesText.Text = "Count of Image without Fish : " + countOfEmptyImags.ToString(); }));
            GUIShowImage();
        }
        private void GUIShowImage()
        {
            AdjustIndex();
            GetImage();
        }
        private void AdjustIndex()
        {
            bool show = false;
            bool showFishesImages = false, showEmptyImages = false;
            showEmpty.Dispatcher.Invoke(new Action(() => { showEmptyImages = (bool)showEmpty.IsChecked; }));
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
        }
        private void GetImage()
        {
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Source = Fishes[indexofShowingFish].GetImage(); }));
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Width = Fishes[indexofShowingFish].GetImageDimension()[0]; }));
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Height = Fishes[indexofShowingFish].GetImageDimension()[1]; }));
            imageFishName.Dispatcher.Invoke(new Action(() => { imageFishName.Text = "Name : " + Fishes[indexofShowingFish].ImageName(); }));
            IndexText.Dispatcher.Invoke(new Action(() => { IndexText.Text = "Image " + indexofShowingFish.ToString() + " of " + pathOfAllImages.Length.ToString(); }));
            if (Fishes[indexofShowingFish].IsAnnotated())
            {
                double[][] fishes = Fishes[indexofShowingFish].GetFishRectangles();

                canvasFish.Dispatcher.Invoke(new Action(() => { canvasFish.Width = Fishes[indexofShowingFish].GetImageDimension()[0]; }));
                canvasFish.Dispatcher.Invoke(new Action(() => { canvasFish.Height = Fishes[indexofShowingFish].GetImageDimension()[1]; }));
                canvasFish.Dispatcher.Invoke(new Action(() => { Canvas.SetLeft(fishBox, fishes[0][0]); }));
                canvasFish.Dispatcher.Invoke(new Action(() => { Canvas.SetTop(fishBox, fishes[0][1]); }));
                fishBox.Dispatcher.Invoke(new Action(() => { fishBox.Width = fishes[0][2]; }));
                fishBox.Dispatcher.Invoke(new Action(() => { fishBox.Height = fishes[0][3]; }));
            }
        }
        private void GetImage( string path)
        {
            BitmapImage Image = new BitmapImage(new Uri(path));
            
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Source = Image; }));
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Width = 800; }));
            imagePanel.Dispatcher.Invoke(new Action(() => { imagePanel.Height = 600; }));
            imageFishName.Dispatcher.Invoke(new Action(() => { imageFishName.Text = "Name : " ; }));
            IndexText.Dispatcher.Invoke(new Action(() => { IndexText.Text = "Image 0 of  0" ; }));
            canvasFish.Dispatcher.Invoke(new Action(() => { canvasFish.Width = 800; }));
            canvasFish.Dispatcher.Invoke(new Action(() => { canvasFish.Height = 600; }));

            //if (Fishes[indexofShowingFish].IsAnnotated())
            //{
            //    double[][] fishes = Fishes[indexofShowingFish].GetFishRectangles();

            //    canvasFish.Dispatcher.Invoke(new Action(() => { canvasFish.Width = Fishes[indexofShowingFish].GetImageDimension()[0]; }));
            //    canvasFish.Dispatcher.Invoke(new Action(() => { canvasFish.Height = Fishes[indexofShowingFish].GetImageDimension()[1]; }));
            //    canvasFish.Dispatcher.Invoke(new Action(() => { Canvas.SetLeft(fishBox, fishes[0][0]); }));
            //    canvasFish.Dispatcher.Invoke(new Action(() => { Canvas.SetTop(fishBox, fishes[0][1]); }));
            //    fishBox.Dispatcher.Invoke(new Action(() => { fishBox.Width = fishes[0][2]; }));
            //    fishBox.Dispatcher.Invoke(new Action(() => { fishBox.Height = fishes[0][3]; }));
            //}
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

        private void ButtonConfigureClass_Click(object sender, RoutedEventArgs e)
        {
            Thread classCalc = new Thread(classes);
            classCalc.SetApartmentState(ApartmentState.STA);
            classCalc.Start();
        }

        private void classes()
        {
            ChangeClass();
            int[] testListOfObjectsImages = GetRandomTestList(listOfAnnotatedWithObjects, 0.1);
            int[] testListOfEmptyImages = GetRandomTestList(listOfAnnotatedWithoutObjects, 0.1);
            string[] saveTrainWithObject = GetSavingList(false, listOfAnnotatedWithObjects, testListOfObjectsImages);
            string[] saveTrainEmptyObject = GetSavingList(false, listOfAnnotatedWithoutObjects, testListOfEmptyImages);
            string[] saveTestWithObject = GetSavingList(true, listOfAnnotatedWithObjects, testListOfObjectsImages);
            string[] saveTestEmptyObject = GetSavingList(true, listOfAnnotatedWithoutObjects, testListOfEmptyImages);
            string[] train = saveTrainWithObject.Concat(saveTrainEmptyObject).ToArray();
            string[] test = saveTestWithObject.Concat(saveTestEmptyObject).ToArray();
            SaveFileDialog saveTrainingDialog = new SaveFileDialog();
            saveTrainingDialog.DefaultExt = "*.txt";
            saveTrainingDialog.ShowDialog();
            File.WriteAllLines(saveTrainingDialog.FileName + "_train", train);
            File.WriteAllLines(saveTrainingDialog.FileName + "_test", test);
        }
        private void ChangeClass()
        {
            string classNumber="";
            classTextBox.Dispatcher.Invoke(new Action(() => { classNumber = classTextBox.Text; }));
                for (int i=0; i< listOfAnnotatedWithObjects.Length;i++)
            {
                string annnotationPath = Fishes[listOfAnnotatedWithObjects[i]].GetAnnotationPath();
                string[] rectangles = File.ReadAllLines(annnotationPath);
                if (rectangles.Length > 0)
                {
                    for (int ii = 0; ii < rectangles.Length; ii++)
                    {
                        string[] splittedString = rectangles[ii].Split(' ');
                        rectangles[ii] = classNumber + " " + splittedString[1] + " " + splittedString[2] + " " + splittedString[3] + " " + splittedString[4];
                    }
                }
                File.WriteAllLines(annnotationPath, rectangles);
            }
        }
        private int[] GetRandomTestList(int[] list, double testPrecentage)
        {
            Random rnd = new Random();
            int[] outputList = new int[(int)(list.Length * testPrecentage)];
            for (int h = 0; h < outputList.Length; h++)
            {
                int rndom = 0;
                while (outputList.Contains(rndom))
                {
                    rndom = rnd.Next(0, list.Length - 1);
                }
                outputList[h] = rndom;
            }
            return outputList;
        }
        private string[] GetSavingList(bool getTest, int[] list,int [] testList)
        {
            string[] savingList;
            if (getTest)
            {
                savingList = new string[testList.Length];
            }
            else
            {
                savingList = new string[list.Length - testList.Length];
            }
            int index = 0;
            for (int i = 0; i < list.Length; i++)
            {
                if (getTest)
                {
                    if (testList.Contains(i))
                    {
                        savingList[index] = Fishes[list[i]].GetImagePath();
                        index++;
                    }
                }
                else
                {
                    if (!testList.Contains(i))
                    {
                        savingList[index] = Fishes[list[i]].GetImagePath();
                        index++;
                    }
                }
            }
            return savingList;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (indexofShowingFish > 0)
            {
                SaveAnnotation();
                indexofShowingFish--;
                GUIShowImage();
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (indexofShowingFish < Fishes.Length)
            {
                SaveAnnotation();
                indexofShowingFish++;
                GUIShowImage();
            }
        }
        void SaveAnnotation()
        {
            if(rectAnnotate.Length>0)
            {
                SaveRotatedImage();
                SaveAnnotationFile();
                SaveAnnotation90DegreeFile();
                foreach (Rectangle rectangle in rectAnnotate)
                {
                    canvasFish.Children.Remove(rectangle);
                }
                startPoint = new Point[0];
                rectAnnotate = new Rectangle[0];
            }
        }
        void SaveRotatedImage()
        {
            BitmapImage imageCopy = Fishes[indexofShowingFish].GetImage();
            imageCopy.Rotation= Rotation.Rotate90;
            BitmapEncoder imageEncoder = new PngBitmapEncoder();
            imageEncoder.Frames.Add(BitmapFrame.Create(imageCopy));

            using (var fileStream = new System.IO.FileStream(Fishes[indexofShowingFish].GetImageDirectory() +"/"+ Fishes[indexofShowingFish].ImageName() + "_Rotated 90.jpeg", System.IO.FileMode.Create))
            {
                imageEncoder.Save(fileStream);
            }
        }
        void SaveAnnotationFile()
        {
            string[] rectangles = new string[rectAnnotate.Length];
            for (int ii = 0; ii < rectAnnotate.Length; ii++)
            {
                double w = 0, h = 0, x = 0, y = 0;
                w = rectAnnotate[ii].Width / Fishes[indexofShowingFish].GetImageDimension()[0];
                h = rectAnnotate[ii].Height / Fishes[indexofShowingFish].GetImageDimension()[1];
                x = ((rectAnnotate[ii].Width/2)+startPoint[ii].X) / Fishes[indexofShowingFish].GetImageDimension()[0];
                y = ((rectAnnotate[ii].Height / 2) + startPoint[ii].Y)/ Fishes[indexofShowingFish].GetImageDimension()[1];

            rectangles[ii] = "0" + " " + x.ToString() + " " + y.ToString() + " " + w.ToString() + " " + h.ToString();
            }
            File.WriteAllLines(Fishes[indexofShowingFish].GetImageDirectory() + "/" + Fishes[indexofShowingFish].ImageName()+".txt", rectangles);
        }
        void SaveAnnotation90DegreeFile()
        {
            string[] rectangles = new string[rectAnnotate.Length];
            for (int ii = 0; ii < rectAnnotate.Length; ii++)
            {
                double w = 0, h = 0, x = 0, y = 0;
                h = rectAnnotate[ii].Width / Fishes[indexofShowingFish].GetImageDimension()[0];
                w = rectAnnotate[ii].Height / Fishes[indexofShowingFish].GetImageDimension()[1];
                y = ((rectAnnotate[ii].Width / 2) + startPoint[ii].X) / Fishes[indexofShowingFish].GetImageDimension()[0]; 
                x = ((rectAnnotate[ii].Height / 2) + startPoint[ii].Y) / Fishes[indexofShowingFish].GetImageDimension()[1]; 

                rectangles[ii] = "0" + " " + x.ToString() + " " + y.ToString() + " " + w.ToString() + " " + h.ToString();
            }
            File.WriteAllLines(Fishes[indexofShowingFish].GetImageDirectory() + "/" + Fishes[indexofShowingFish].ImageName() + "_Rotated 90"+".txt", rectangles);
        }
        private void ButtonAnnotate_Click(object sender, RoutedEventArgs e)
        {
            if (isAnnotation)
            {
                isAnnotation = false;
                buttonAnnotate.Content = "Annotation";

            }
            else
            {
                isAnnotation = true;
                buttonAnnotate.Content = "Stop Annotation";
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
        public double [] GetImageDimension()
        {
            double [] dimension = new double[2];
            dimension[0] = imageWidth;
            dimension[1] = imageHeight;
            return dimension;
        }
        public string GetAnnotationPath()
        {
            return annotationPath;
        }
        public string GetImagePath()
        {
            return imagePath;
        }
        public string GetImageDirectory()
        {
            return imageDirectory;
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
