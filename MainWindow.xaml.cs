using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using Path = System.IO.Path;

namespace DevAssessmentWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<FileInformation> FileInfo = new List<FileInformation>();
        public Dictionary<string, int> VersionDict = new Dictionary<string, int>();
        public List<string> ExcludedPaths = new List<string>();

        public class FileInformation
        {
            public string FileName { get; set; }
            public string VersionNumber { get; set; }
            public bool IsAverageVersion { get; set; }
            public bool IsOutlier { get; set; }
            public string FullPath { get; set; }

        }
        public MainWindow()
        {
            InitializeComponent();
            lbl1.Visibility = Visibility.Hidden;
            lbl2.Visibility = Visibility.Hidden;
            lbl3.Visibility = Visibility.Hidden;
            StatGroupbox.Visibility = Visibility.Hidden;
            btnExclude.Visibility = Visibility.Hidden;
            ListViewExcluded.Visibility = Visibility.Hidden;
            lblexclude.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                DirectoryTextBox.Text = dialog.SelectedPath;
                SearchBtn.Visibility = Visibility.Visible;
                lbl1.Visibility = Visibility.Visible;
                lbl2.Visibility = Visibility.Visible;
                lbl3.Visibility = Visibility.Visible;
                StatGroupbox.Visibility = Visibility.Visible;
                btnExclude.Visibility = Visibility.Visible;
                ListViewExcluded.Visibility = Visibility.Visible;
                lblexclude.Visibility = Visibility.Visible;
            }
        }

        private void Button_Search(object sender, RoutedEventArgs e)
        {
            Search();


        }

        public void Search()
        {
            string AverageVersion = "";
            int AmountofDLLs = 0;
            int AmountofAverageDlls = 0;
            DataGridInfo.ItemsSource = null;
            this.FileInfo.Clear();
            this.VersionDict.Clear();

            if (Directory.Exists(DirectoryTextBox.Text))
            {
                // This path is a directory
                var isComplete = ProcessDirectory(DirectoryTextBox.Text);
                if (isComplete && this.FileInfo.Count > 0)
                {

                    foreach (var file in this.FileInfo)
                    {
                        if (file.VersionNumber != "None found")
                        {
                            if (this.VersionDict.ContainsKey(file.VersionNumber))
                            {
                                this.VersionDict[file.VersionNumber] += 1;
                            }
                            else
                            {
                                this.VersionDict[file.VersionNumber] = 1;
                            }
                        }
                    }
                    if (VersionDict.Count > 0)
                    {
                        AverageVersion = this.VersionDict.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                        AmountofDLLs = this.VersionDict.Sum(x => x.Value);
                        AmountofAverageDlls = 0;
                        foreach (var file in this.FileInfo)
                        {
                            if (file.VersionNumber == AverageVersion)
                            {
                                file.IsAverageVersion = true;
                                AmountofAverageDlls++;
                            }else
                            {
                                file.IsOutlier = true;
                            }
                        }
                        LblAverageVersion.Content = AverageVersion;
                        LblNumAverageVersion.Content = AmountofAverageDlls;
                        LblNumDlls.Content = AmountofDLLs;

                    }



                    DataGridInfo.ItemsSource = this.FileInfo;
                   
                    //this.dataGridView1.Sort(this.dataGridView1.Columns["Name"], ListSortDirection.Ascending);

                }
                else
                {
                    MessageBox.Show("There are no Dlls in this Directory !", "Alert");
                }
            }
        }



        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public bool ProcessDirectory(string targetDirectory)
        {
            if(!ExcludedPaths.Contains(targetDirectory))
            { 
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
            }
            return true;
        }

        // Insert logic for processing found files here.
        public void ProcessFile(string path)
        {
            if(path.Contains(".dll") || path.Contains(".exe"))
            {
                var versInfo = FileVersionInfo.GetVersionInfo(path);
                if (versInfo.FileVersion == null)
                {
                    this.FileInfo.Add(new FileInformation { FileName = Path.GetFileName(path), VersionNumber = "None found", IsOutlier = true, IsAverageVersion = false, FullPath= path }); 
                }
                else
                {
                    this.FileInfo.Add(new FileInformation{ FileName = Path.GetFileName(path), VersionNumber = versInfo.FileVersion, IsOutlier = false, IsAverageVersion = false, FullPath = path });
                    // DataGridInfo.Items.Add()

                }



            }

            
        }

        private void Exclude_Directories(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.ExcludedPaths.Add(dialog.SelectedPath);
                ListViewExcluded.Items.Add(dialog.SelectedPath);
                SearchBtn.Visibility = Visibility.Visible;

            }
        }



        private void Remove_Directory(object sender, MouseButtonEventArgs e)
        {
            ExcludedPaths.Remove(ListViewExcluded.SelectedValue.ToString());
            ListViewExcluded.Items.Remove(ListViewExcluded.SelectedItem);
            //refresh the list
            Search();
        }
    }
}
