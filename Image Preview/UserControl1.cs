using Image_Preview.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;
using Autodesk.Max;
using CSharpUtilities;
using MaxCustomControls;
using System.Globalization;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Image_Preview.Properties;
using Path = System.IO.Path;
using System.Windows.Media;
using System.Runtime.Serialization.Formatters.Binary;
namespace Image_Preview
{
    public partial class UserControl1 : UserControl
    {


        public static string extensions = ".jpg|.png";
        public static string saveThumbImages = @"C:\Newfolder";
        private ThumbNailSize _currentThumbSize = ThumbNailSize.Large;
        private ContextMenu menu;
        private const string PlaceholderText = "Search images...";
        
        public string[] CurrentImagePaths = null;
        public string currentSearcFolderDirectory = null;
        public string[] CurrentSearcImagePaths = null;
        private bool sortByRatingisTrue = false;
        private readonly string cacheDirectory = @"C:\ImageCache";
        

        public UserControl1()
        {

            InitializeComponent();

            ContextMenuStrip contextMenuStripView = new ContextMenuStrip();
            ContextMenuStrip contextMenuStripSort = new ContextMenuStrip();
            

            ToolStripMenuItem tinyMenuItem = new ToolStripMenuItem("Tiny", Resources.tiny, (sender, e) => ChangeThumbSize(ThumbNailSize.Tiny));
            ToolStripMenuItem mediumMenuItem = new ToolStripMenuItem("Medium", Resources.meduim, (sender, e) => ChangeThumbSize(ThumbNailSize.Medium));
            ToolStripMenuItem largeMenuItem = new ToolStripMenuItem("Large", Resources.large, (sender, e) => ChangeThumbSize(ThumbNailSize.Large));

         



            contextMenuStripView.Items.AddRange(new ToolStripMenuItem[] { tinyMenuItem, mediumMenuItem, largeMenuItem });
            ToolStripMenuItem sortByRating = new ToolStripMenuItem("Rating", Resources.yellow_star,  (sender, e) =>  ImageSortByRating());
            contextMenuStripSort.Items.AddRange(new ToolStripMenuItem[] { sortByRating });
            this.iconButton1.ContextMenuStrip = contextMenuStripView;
            this.iconButton3.ContextMenuStrip = contextMenuStripSort;


            this.iconButton1.MouseDown += new MouseEventHandler(this.iconButton1_MouseDown);
            this.iconButton3.MouseDown += new MouseEventHandler(this.iconButton2_MouseDown);





            
        }
        
        public delegate void ThumbPickedEventHandler(object sender, PickedEventArgs e);
        private void iconButton1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {

                this.iconButton1.ContextMenuStrip.Show(this.iconButton1, e.Location);
            }
        }
        public void RemoveText(object sender, EventArgs e)
        {

        }

        public void AddText(object sender, EventArgs e)
        {

        }
        private void iconButton2_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {

                this.iconButton3.ContextMenuStrip.Show(this.iconButton3, e.Location);
            }
        }
        private void ChangeThumbSize(ThumbNailSize size)
        {
            _currentThumbSize = size;
            Reload();  
        }

        private  void ImageSortByRating()
        {

            sortByRatingisTrue  = true;
            Reload();
            sortByRatingisTrue = false;

        }
      
        public async void Reload()
        {
            if (CurrentImagePaths != null && CurrentImagePaths.Length != 0 )
            {
                await Populate(CurrentImagePaths);
            }
            if (CurrentFolderDirectory != null )
            {
                await Populate(CurrentFolderDirectory);
            }
            


        }


        public async void SearchLoad(string searchText)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
              
                if (CurrentImagePaths != null && CurrentImagePaths.Length != 0)
                {
                    string[] searchImagePaths = CurrentImagePaths;
                    var filteredPaths = searchImagePaths
                        .Where(path => Path.GetFileNameWithoutExtension(path).ToLower().Contains(searchText))
                        .ToArray();
                    await Populate(filteredPaths);
                }
                // Filter by file name for files in CurrentFolderDirectory
                else if (!string.IsNullOrEmpty(CurrentFolderDirectory))
                {
                    var currentSearcDirectory = CurrentFolderDirectory;
                    var directoryInfo = new DirectoryInfo(currentSearcDirectory);
                    var filteredFiles = directoryInfo.GetFiles()
                        .Where(file => Path.GetFileNameWithoutExtension(file.FullName).ToLower().Contains(searchText))
                        .Select(file => file.FullName)
                        .ToArray();
                    await Populate(filteredFiles);
                }
            }
            if (string.IsNullOrEmpty(searchText))
            {
                Reload();
            }
        }






        public  string CurrentFolderDirectory;
        public string CurrentFolderDirectoryForReadonly;
        private bool isFirstCall = true;
        public void FirstCall(bool val,string path)
        {
            CurrentFolderDirectoryForReadonly = path;
        }
        public async Task Populate(string path)
        {
            flowLayoutPanel1.Controls.Clear();
            
            if (isFirstCall)
            {
                CurrentFolderDirectory = path;
                FirstCall(true, path);
                isFirstCall = false;
                
            }


             DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles();
          
            
            CurrentImagePaths = null;


            var sortedFiles = files.OrderBy(f => f.FullName).ToArray();

            if (sortByRatingisTrue == true)
            {
                 sortedFiles = files.Where(f =>
                {
                    if (Path.GetExtension(f.FullName).Equals(".png", StringComparison.OrdinalIgnoreCase))
                        return false;
                    ShellFile shellFile = ShellFile.FromFilePath(f.FullName);
                    int? rating = (int?)shellFile.Properties.System.Rating.Value;
                    int stars = (rating.HasValue) ? rating.Value / 20 + 1 : 0;
                    return stars >= 0;
                })
          .OrderByDescending(f =>
          {
              ShellFile shellFile = ShellFile.FromFilePath(f.FullName);
              int? rating = (int?)shellFile.Properties.System.Rating.Value;
              return (rating.HasValue) ? rating.Value : 0;
          })
          .ThenBy(f => f.FullName)
          .ToArray();
            }

            string[] extsn = extensions.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var file in sortedFiles)
            {
                if (extsn.Any(ext => file.Extension.ToLower().Contains(ext.ToLower())))
                {

                    Button customBtn = new Button();
                    Image thumbnail = await GetThumbnailAsync(file.FullName, _currentThumbSize);
                    customBtn.BackgroundImage = thumbnail;
                    customBtn.Size = new System.Drawing.Size((int)_currentThumbSize, (int)_currentThumbSize);
                    customBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                    customBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                    customBtn.Click += (sender, e) => pickeditem(file.FullName, thumbnail);
                    ContextMenuStrip contextMenuStripRightClick = new ContextMenuStrip();
                    ToolStripMenuItem setRating = new ToolStripMenuItem("Rating", Resources.yellow_star);
                    ToolStripMenuItem fiveStar = new ToolStripMenuItem("5 Star", Resources.yellow_star, (sender, e) => SetImageRating(5, file.FullName));
                    ToolStripMenuItem fourStar = new ToolStripMenuItem("4 Star", Resources.yellow_star, (sender, e) => SetImageRating( 4,file.FullName));
                    ToolStripMenuItem threeStar = new ToolStripMenuItem("3 Star", Resources.yellow_star, (sender, e) => SetImageRating(3, file.FullName));
                    setRating.DropDownItems.Add(fiveStar);
                    setRating.DropDownItems.Add(fourStar);
                    setRating.DropDownItems.Add(threeStar);
                    ToolStripMenuItem item2 = new ToolStripMenuItem("Option 2", null);
                    contextMenuStripRightClick.Items.AddRange(new ToolStripItem[] { setRating, item2 });
                    customBtn.ContextMenuStrip = contextMenuStripRightClick;

                    flowLayoutPanel1.Controls.Add(customBtn);

                }
            }
        }
        

        public async Task Populate(string[] imagePaths)
        {

            Array.Sort(imagePaths);

            this.flowLayoutPanel1.Controls.Clear();
            CurrentImagePaths = imagePaths;
            //CurrentFolderDirectory = null;
            var sortedFiles = imagePaths.OrderBy(path => path).ToArray();
            if (sortByRatingisTrue == true)
            {
                sortedFiles = imagePaths.Where(path =>
            {
                if (Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase))
                    return false;
                ShellFile shellFile = ShellFile.FromFilePath(path);
                int? rating = (int?)shellFile.Properties.System.Rating.Value;
                int stars = (rating.HasValue) ? rating.Value / 20 + 1 : 0;
                return stars >= 0;
            })
      .OrderByDescending(path =>
      {
          ShellFile shellFile = ShellFile.FromFilePath(path);
          int? rating = (int?)shellFile.Properties.System.Rating.Value;
          return (rating.HasValue) ? rating.Value : 0;
      })
      .ThenBy(path => path)
      .ToArray();

            }



            foreach (var imagePath in sortedFiles)
            {
                if (!File.Exists(imagePath))
                {

                    continue;
                }
             

                string[] extsn = extensions.Split('|', (char)StringSplitOptions.RemoveEmptyEntries);
                if (!extsn.Any(ext => imagePath.ToLower().EndsWith(ext.ToLower())))
                {

                    continue;
                }

                Button customBtn = new Button();
                Image thumbnail = await GetThumbnailAsync(imagePath, _currentThumbSize);
                customBtn.BackgroundImage = thumbnail;
                customBtn.Size = new System.Drawing.Size((int)_currentThumbSize, (int)_currentThumbSize);
                customBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                customBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                customBtn.Click += (sender, e) => pickeditem(imagePath, thumbnail);
                ContextMenuStrip contextMenuStripRightClick = new ContextMenuStrip();
                ToolStripMenuItem setRating = new ToolStripMenuItem("Rating", Resources.yellow_star);
                ToolStripMenuItem fiveStar = new ToolStripMenuItem("5 Star", Resources.yellow_star, (sender, e) => SetImageRating(5, imagePath));
                ToolStripMenuItem fourStar = new ToolStripMenuItem("4 Star", Resources.yellow_star, (sender, e) => SetImageRating(4, imagePath));
                ToolStripMenuItem threeStar = new ToolStripMenuItem("3 Star", Resources.yellow_star, (sender, e) => SetImageRating(3, imagePath));
                setRating.DropDownItems.Add(fiveStar);
                setRating.DropDownItems.Add(fourStar);
                setRating.DropDownItems.Add(threeStar);
                ToolStripMenuItem item2 = new ToolStripMenuItem("Option 2", null);
                contextMenuStripRightClick.Items.AddRange(new ToolStripItem[] { setRating, item2 });
                customBtn.ContextMenuStrip = contextMenuStripRightClick;
                flowLayoutPanel1.Controls.Add(customBtn);
            }
        }


        public event EventHandler Clicked;
        public string CurrentItem;
        public Image CurrentImage;
        public event ThumbPickedEventHandler ThumbPicked;

       

        public void pickeditem(string path, Image thumbnail)
        {

            CurrentItem = path;
            CurrentImage = thumbnail;
            ThumbPicked?.Invoke(this, new PickedEventArgs(path, thumbnail));

            MessageBox.Show($"Failed to set rating:{path} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }




        public static string smallThumbImagesDirectory = @"C:\SmallThumb";
        public async Task<Image> GetThumbnailAsync(string imagePath, ThumbNailSize size)
        {
            int targetThumbSize = (int)size;

            // Try to load the cached image first
            Image cachedImage = await LoadImageFromCache(imagePath);
            if (cachedImage != null)
                return cachedImage;

            // Ensure the thumbnail directory exists
            if (!Directory.Exists(smallThumbImagesDirectory))
                Directory.CreateDirectory(smallThumbImagesDirectory);

            // Create the thumbnail in a Task
            return await Task.Run(async () =>
            {
                try
                {
                    using (var img = Image.FromFile(imagePath))
                    {
                        int originalWidth = img.Width;
                        int originalHeight = img.Height;
                        double scalingFactor = Math.Min((double)targetThumbSize / originalWidth, (double)targetThumbSize / originalHeight);
                        int newWidth = (int)(originalWidth * scalingFactor);
                        int newHeight = (int)(originalHeight * scalingFactor);

                        // Generate the thumbnail
                        using (var thumbnail = img.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                        {
                            // Create a thumbnail for saving
                            using (var thumbnailForSave = img.GetThumbnailImage(175, 175, null, IntPtr.Zero))
                            {
                                string fileName = Path.GetFileNameWithoutExtension(imagePath) + "_thumbnail.jpg";
                                string thumbPath = Path.Combine(smallThumbImagesDirectory, fileName);
                                thumbnailForSave.Save(thumbPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                await SaveImageToCache(imagePath, thumbnailForSave);
                            }

                            return thumbnail; // Clone to return a copy of the thumbnail
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions as needed (log or rethrow)
                    throw new InvalidOperationException("Error generating thumbnail", ex);
                }
            });
        }



        public async Task SaveImageToCache(string filePath, Image image)
        {
            string cachePath = GetCacheFilePath(filePath);
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(cachePath, FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, image);
                }
            });
        }


        public async Task<Image> LoadImageFromCache(string filePath)
        {
            string cachePath = GetCacheFilePath(filePath);
            if (!File.Exists(cachePath))
                return null;

            return await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (Image)formatter.Deserialize(fs);
                }
            });
        }
        private string GetCacheFilePath(string originalFilePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(originalFilePath);
            return Path.Combine(cacheDirectory, fileName + ".imgcache");
        }





        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }





        private void button1_Click_1(object sender, EventArgs e) { }

        private void iconButton1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {

        }

        private void iconButton3_Click(object sender, EventArgs e)
        {

        }
        public enum ThumbNailSize
        {
            Tiny = 70,
            Medium = 100,
            Large = 175,
        }
        public static void SetImageRating(int rating, string path)
        {
            try
            {
                ShellFile shellFile = ShellFile.FromFilePath(path);
                int rate = (rating) * 20-1;
                shellFile.Properties.System.Rating.Value = (uint?)rate;
            }
           
            catch (Exception ex)
    {
                MessageBox.Show($"Failed to set rating: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }




      
       

        private async void button1_Click_3(object sender, EventArgs e)
        {
            //await Populate(@"C:\Users\mdsai\OneDrive\Pictures\Screenshots");
            //string[] paths = {@"C:\Users\mdsai\OneDrive\Pictures\Screenshots\Screenshot (1).png", @"C:\Users\mdsai\OneDrive\Pictures\Screenshots\Screenshot (2).png" };
            //await Populate(paths);
            await Populate(@"C:\Users\mdsai\OneDrive\Desktop");
        }

        private void textBox1_Enter_1(object sender, EventArgs e)
        {

        }

        //private void textBox1_KeyDown(object sender, KeyEventArgs e)
        //{
            
            
        //        // Print details of the key pressed
        //        Console.WriteLine($"Key pressed: {e.KeyCode}");          // Prints the key code (e.g., "A", "Enter")
        //        Console.WriteLine($"Key data: {e.KeyData}");              // Prints key data, including modifiers (e.g., "Control, A")
        //        Console.WriteLine($"Key value (int): {e.KeyValue}");      // Prints the integer value of the key (e.g., 65 for 'A')
            
        //}
    }
}