namespace NPI.KinectDrums.DataModel {

    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NPI.KinectDrums.Common;
    using System.Globalization;


    // Creates a collection of groups and items with hard-coded content.
    // SampleDataSource initializes with placeholder data rather than live production
    // data so that sample data is provided at both design-time and run-time.

    public sealed class SampleDataSource {

        private static SampleDataSource sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataCollection> allGroups = new ObservableCollection<SampleDataCollection>();

        private const string garageImage = "/Images/garage.jpg";

        public SampleDataSource() {

            string itemContent = string.Format(
                CultureInfo.CurrentCulture,
                "Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                "Contenido Lorem ipsum");

            var group1 = new SampleDataCollection(
                "Group-1",
                "Group Title: 1",
                "Group Subtitle: 1",
                garageImage,
                "Group Description: Menu Group");
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-1",
                "Play Now",
                "Play a default drum kit",
                garageImage,
                "Item Description: Start playing whith a default drum kit.",
                itemContent,
                group1,
                typeof(Play)));
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-2",
                "Item Title: 2",
                "Item Subtitle: 2",
                garageImage,
                "Item Description: Inutil.",
                itemContent,
                group1));
            group1.Items.Add(
                new SampleDataItem(
                "Group-1-Item-3",
                "Item Title: 3",
                "Item Subtitle: 3",
                garageImage,
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

        // Field to store uniqueId
        private string uniqueId = string.Empty;

        // Field to store title
        private string title = string.Empty;

        // Field to store subtitle
        private string subtitle = string.Empty;

        // Field to store description
        private string description = string.Empty;

        // Field to store image path
        private string image = string.Empty;

        // Initializes a new instance of the <see cref="SampleDataCommon" /> class.
        // <param name="uniqueId">The unique id of this item.</param>
        // <param name="title">The title of this item.</param>
        // <param name="subtitle">The subtitle of this item.</param>
        // <param name="imagePath">A relative path of the image for this item.</param>
        // <param name="description">A description of this item.</param>
        protected SampleDataCommon(string uniqueId, string title, string subtitle, string image, string description) {

            this.uniqueId = uniqueId;
            this.title = title;
            this.subtitle = subtitle;
            this.description = description;
            this.image = image;
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

        public string Image {

            get { return this.image; }

            set { this.SetProperty(ref this.image, "/Images/garage.jpg")/*this.SetProperty(ref this.image, value)*/; }
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

        public SampleDataItem(string uniqueId, string title, string subtitle, string image, string description, string content, SampleDataCollection group)
            : base(uniqueId, title, subtitle, image, description) {

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
        public SampleDataItem(string uniqueId, string title, string subtitle, string image, string description, string content, SampleDataCollection group, Type navigationPage)
            : base(uniqueId, title, subtitle, image, description) {

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

        public SampleDataCollection(string uniqueId, string title, string subtitle, string image, string description)
            : base(uniqueId, title, subtitle, image, description) { }

        public ObservableCollection<SampleDataItem> Items {

            get { return this.items; }
        }

        public ObservableCollection<SampleDataItem> TopItems {

            get { return this.topItem; }
        }

        public IEnumerator GetEnumerator() {

            return this.Items.GetEnumerator();
        }
    }
}