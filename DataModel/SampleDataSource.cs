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

        /********************************************************************************************************************/
        private const string garageImage = "/Images/GarageBackground.jpg";

        public SampleDataSource() {
            
            //Elementos del menú de inicio
            var menuOptions = new SampleDataCollection(
                "MenuOptions",
                "Menu",
                "Options",
                garageImage,
                "Menu Group");

            menuOptions.Items.Add(
                new SampleDataItem(
                "Menu-PlayNow",
                "Play Now",
                "Play a default drum kit",
                garageImage,
                "Start playing whith a default drum kit.",
                menuOptions,
                typeof(Play)));
            menuOptions.Items.Add(
                new SampleDataItem(
                "Menu-Customize",
                "Customize",
                "Customize your drum kit",
                garageImage,
                "Create your own drum kit and play it.",
                menuOptions,
                typeof(Customize)));
            menuOptions.Items.Add(
                new SampleDataItem(
                "Menu-Help",
                "Help",
                "Lost? Get some help",
                garageImage,
                "Maybe I can lend you a hand.",
                menuOptions,
                typeof(Help)));
            menuOptions.Items.Add(
                new SampleDataItem(
                "Menu-Exit",
                "Exit",
                "",
                garageImage,
                "Exit Kinect Drums.",
                menuOptions));
            
            this.AllGroups.Add(menuOptions);

            //Elementos del modo customize
            var drumPieces = new SampleDataCollection(
                "DrumPieces",
                "Drum",
                "Pieces",
                garageImage,
                "Drum Pieces");

            drumPieces.Items.Add(
                new SampleDataItem(
                "bass",
                "Bass",
                "",
                "/Images/Bass.png",
                "",
                drumPieces));

            drumPieces.Items.Add(
                new SampleDataItem(
                "snare",
                "Snare",
                "",
                "/Images/Snare.png",
                "",
                drumPieces));

            drumPieces.Items.Add(
                new SampleDataItem(
                "middleTom",
                "Middle Tom",
                "",
                "/Images/MiddleTom.png",
                "",
                drumPieces));

            drumPieces.Items.Add(
                new SampleDataItem(
                "floorTom",
                "Floor Tom",
                "",
                "/Images/FloorTom.png",
                "",
                drumPieces));

            drumPieces.Items.Add(
                new SampleDataItem(
                "crash",
                "Crash",
                "",
                "/Images/MiddleTom.png",
                "",
                drumPieces));

            drumPieces.Items.Add(
                new SampleDataItem(
                "hihat",
                "Hihat",
                "",
                "/Images/Hihat.png",
                "",
                drumPieces));

            this.AllGroups.Add(drumPieces);

        }

        /********************************************************************************************************************/

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

            set { this.SetProperty(ref this.image, value); }
        }

        public override string ToString() {

            return this.Title;
        }
    }

    // Generic item data model.
    public class SampleDataItem : SampleDataCommon {

        private SampleDataCollection group;
        private Type navigationPage;

        public SampleDataItem(string uniqueId, string title, string subtitle, string image, string description, SampleDataCollection group)
            : base(uniqueId, title, subtitle, image, description) {

            this.group = group;
            this.navigationPage = null;
        }

        // Initializes a new instance of the <see cref="SampleDataItem" /> class.
        public SampleDataItem(string uniqueId, string title, string subtitle, string image, string description, SampleDataCollection group, Type navigationPage)
            : base(uniqueId, title, subtitle, image, description) {

            this.group = group;
            this.navigationPage = navigationPage;
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