namespace NPI.KinectDrums.Common{

    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    // Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    public abstract class BindableBase : INotifyPropertyChanged {

        // Multicast event for property change notifications.
        public event PropertyChangedEventHandler PropertyChanged;

        // Checks if a property already matches a desired value.  Sets the property and
        // notifies listeners only when necessary.
        // <typeparam name="T">Type of the property.</typeparam>
        // <param name="storage">Reference to a property with both getter and setter.</param>
        // <param name="value">Desired value for the property.</param>
        // <param name="propertyName">Name of the property used to notify listeners.  This
        // value is optional and can be provided automatically when invoked from compilers that
        // support CallerMemberName.</param>
        // <returns>True if the value was changed, false if the existing value matched the
        // desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {

            if (object.Equals(storage, value)) {

                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    
        // Notifies listeners that a property value has changed.
        // <param name="propertyName">Name of the property used to notify listeners.  This
        // value is optional and can be provided automatically when invoked from compilers
        // that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {

            var eventHandler = this.PropertyChanged;
            if (eventHandler != null) {

                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
