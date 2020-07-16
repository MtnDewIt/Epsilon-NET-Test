using System.ComponentModel;

namespace TagStructEditor.Fields
{
    public interface IField : INotifyPropertyChanged
    {
        /// <summary>
        /// The Parent <see cref="IField" /> this field
        /// </summary>
        IField Parent { get; set; }

        /// <summary>
        /// Populates the field recursively
        /// </summary>
        /// <param name="owner">The owner of used to get the field's value</param>
        /// <param name="value">The value to set. If null it will get the value from the Owner</param>
        void Populate(object owner, object value = null);

        /// <summary>
        /// Visitor Accept function - Allows functionality to be implemented without having to modify the tag field interface.
        /// </summary>
        /// <param name="visitor">The visitor</param>
        void Accept(IFieldVisitor visitor);
    }
}
