namespace NPI.KinectDrums.DataModel {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using NPI.KinectDrums.Common;
    using System.Globalization;


    // Creates a collection of groups and items with hard-coded content.
    // SampleDataSource initializes with placeholder data rather than live production
    // data so that sample data is provided at both design-time and run-time.

    public sealed class SampleDataSource {

        private static SampleDataSource sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataCollection> allGroups = new ObservableCollection<SampleDataCollection>();

        private static Uri darkGrayImage = new Uri("Assets/DarkGray.png", UriKind.Relative);
        private static Uri mediumGrayImage = new Uri("assets/mediumGray.png", UriKind.Relative);
        private static Uri lightGrayImage = new Uri("assets/lightGray.png", UriKind.Relative);

        public SampleDataSource() {

            string itemContent = string.Format(
                CultureInfo.CurrentCulture,
                "Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                "Contenido Lorem ipsum");

            var group1 = new SampleDataCollection(
                "Group-1",
                "Group Title: 3",
                "Group Subtitle: 3",
                SampleDataSource.mediumGrayImage,
                "Group Description: Menu Group");
            /*group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-1",
                "Buttons",
                string.Empty,
                SampleDataSource.darkGrayImage,
                "Several types of buttons with custom styles",
                itemContent,
                group1,
                typeof(ButtonSample)));*/
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-1",
                "Play Now",
                "Play a default drums kit",
                SampleDataSource.darkGrayImage,
                "Item Description: Start playing whith a default drums kit.",
                itemContent,
                group1,
                typeof(Play)));
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-2",
                "Item Title: 2",
                "Item Subtitle: 2",
                SampleDataSource.mediumGrayImage,
                "Item Description: Inutil.",
                itemContent,
                group1));
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-3",
                "Item Title: 3",
                "Item Subtitle: 3",
                SampleDataSource.mediumGrayImage,
                "Item Description: Inutil.",
                itemContent,
                group1));
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-4",
                "Item Title: 4",
                "Item Subtitle: 4",
                SampleDataSource.lightGrayImage,
                "Item Description: Inutil.",
                itemContent,
                group1));
            
            this.AllGroups.Add(group1);
        }

        public ObservableCollection<SampleDataCollection> AllGroups {

            get { return this.allGroups; }
        }

        public static SampleDataCollection GetGroup(string uniqueId) {

            // Simple linear search is acceptable for small data sets
            var matches = sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) {

                return matches.First();
            }

            return null;
        }

        public static SampleDataItem GetItem(string uniqueId) {

            // Simple linear search is acceptable for small data sets
            var matches = sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) {

                return matches.First();
            }

            return null;
        }
    }


    public abstract class SampleDataCommon : BindableBase {

        // baseUri for image loading purposes
        private static Uri baseUri = new Uri("pack://application:,,,/");

        // Field to store uniqueId
        private string uniqueId = string.Empty;

        // Field to store title
        private string title = string.Empty;

        // Field to store subtitle
        private string subtitle = string.Empty;

        // Field to store description
        private string description = string.Empty;

        // Field to store image
        private ImageSource image = null;

        // Field to store image path
        private Uri imagePath = null;

        // Initializes a new instance of the <see cref="SampleDataCommon" /> class.
        // <param name="uniqueId">The unique id of this item.</param>
        // <param name="title">The title of this item.</param>
        // <param name="subtitle">The subtitle of this item.</param>
        // <param name="imagePath">A relative path of the image for this item.</param>
        // <param name="description">A description of this item.</param>
        protected SampleDataCommon(string uniqueId, string title, string subtitle, Uri imagePath, string description) {

            this.uniqueId = uniqueId;
            this.title = title;
            this.subtitle = subtitle;
            this.description = description;
            this.imagePath = imagePath;
        }

        // Gets or sets UniqueId.
        public string UniqueId {

            get { return this.uniqueId; }
            set { this.SetProperty(ref this.uniqueId, value); }
        }

        public string Title {

            get { return this.title; }
            set { this.SetProperty(ref this.title, value); }
        }

        public string Subtitle {

            get { return this.subtitle; }
            set { this.SetProperty(ref this.subtitle, value); }
        }

        public string Description {

            get { return this.description; }
            set { this.SetProperty(ref this.description, value); }
        }

        public ImageSource Image {

            get {

                if (this.image == null && this.imagePath != null) {

                    this.image = new BitmapImage(new Uri(SampleDataCommon.baseUri, this.imagePath));
                }

                return this.image;
            }

            set {

                this.imagePath = null;
                this.SetProperty(ref this.image, value);
            }
        }

        public void SetImage(Uri path) {

            this.image = null;
            this.imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString() {

            return this.Title;
        }
    }

    // Generic item data model.
    public class SampleDataItem : SampleDataCommon {

        private string content = string.Empty;
        private SampleDataCollection group;
        private Type navigationPage;

        public SampleDataItem(string uniqueId, string title, string subtitle, Uri imagePath, string description, string content, SampleDataCollection group)
            : base(uniqueId, title, subtitle, imagePath, description) {

            this.content = content;
            this.group = group;
            this.navigationPage = null;
        }

        // Initializes a new instance of the <see cref="SampleDataItem" /> class.
        // <param name="uniqueId">The unique id of this item.</param>
        // <param name="title">The title of this item.</param>
        // <param name="subtitle">The subtitle of this item.</param>
        // <param name="imagePath">A relative path of the image for this item.</param>
        // <param name="description">A description of this item.</param>
        // <param name="content">The content of this item.</param>
        // <param name="group">The group of this item.</param>
        // <param name="navigationPage">What page should launch when clicking this item.</param>
        public SampleDataItem(string uniqueId, string title, string subtitle, Uri imagePath, string description, string content, SampleDataCollection group, Type navigationPage)
            : base(uniqueId, title, subtitle, imagePath, description) {

            this.content = content;
            this.group = group;
            this.navigationPage = navigationPage;
        }

        public string Content {

            get { return this.content; }
            set { this.SetProperty(ref this.content, value); }
        }

        public SampleDataCollection Group {

            get { return this.group; }
            set { this.SetProperty(ref this.group, value); }
        }

        public Type NavigationPage {

            get { return this.navigationPage; }
            set { this.SetProperty(ref this.navigationPage, value); }
        }
    }

    // Generic group data model.
    public class SampleDataCollection : SampleDataCommon, IEnumerable {

        private ObservableCollection<SampleDataItem> items = new ObservableCollection<SampleDataItem>();
        private ObservableCollection<SampleDataItem> topItem = new ObservableCollection<SampleDataItem>();

        public SampleDataCollection(string uniqueId, string title, string subtitle, Uri imagePath, string description)
            : base(uniqueId, title, subtitle, imagePath, description) {

            this.Items.CollectionChanged += this.ItemsCollectionChanged;
        }

        public ObservableCollection<SampleDataItem> Items {

            get { return this.items; }
        }

        public ObservableCollection<SampleDataItem> TopItems {

            get { return this.topItem; }
        }

        public IEnumerator GetEnumerator() {

            return this.Items.GetEnumerator();
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed
            switch (e.Action) {

                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12) {

                        this.TopItems.Insert(e.NewStartingIndex, this.Items[e.NewStartingIndex]);
                        if (this.TopItems.Count > 12) {

                            this.TopItems.RemoveAt(12);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12) {

                        this.TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12) {

                        this.TopItems.RemoveAt(e.OldStartingIndex);
                        this.TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12) {

                        this.TopItems.Insert(e.NewStartingIndex, this.Items[e.NewStartingIndex]);
                        this.TopItems.RemoveAt(12);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12) {

                        this.TopItems.RemoveAt(e.OldStartingIndex);
                        if (this.Items.Count >= 12) {

                            this.TopItems.Add(this.Items[11]);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12) {

                        this.TopItems[e.OldStartingIndex] = this.Items[e.OldStartingIndex];
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.TopItems.Clear();
                    while (this.TopItems.Count < this.Items.Count && this.TopItems.Count < 12) {

                        this.TopItems.Add(this.Items[this.TopItems.Count]);
                    }

                    break;
            }
        }
    }
}