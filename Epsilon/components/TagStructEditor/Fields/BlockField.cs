using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls;
using TagStructEditor.Common;
using TagStructEditor.Helpers;

namespace TagStructEditor.Fields
{
    public class BlockField : ValueField, IExpandable
    {
        private bool _isExpanded = true;

        public BlockField(Type elementType, ValueFieldInfo info) : base(info)
        {
            ElementType = elementType;

            Block = new ObservableNonGenericCollection();
            Block.CollectionChanged += Block_CollectionChanged;
            AddCommand = new DelegateCommand(Add, () => !IsFixedSize);
            InsertCommand = new DelegateCommand(Insert, () => CurrentElementValid && !IsFixedSize);
            DeleteCommand = new DelegateCommand(Delete, () => CurrentElementValid && !IsFixedSize);
            DeleteAllCommand = new DelegateCommand(DeleteAll, () => CurrentElementValid && !IsFixedSize);
            DuplicateCommand = new DelegateCommand(Duplicate, () => CurrentElementValid && !IsFixedSize);
            ShiftUpCommand = new DelegateCommand(() => Shift(-1), () => CurrentElementValid && !IsFixedSize && CurrentIndex > 0);
            ShiftDownCommand = new DelegateCommand(() => Shift(1), () => CurrentElementValid && !IsFixedSize && CurrentIndex < Block.Count - 1);
            ExpandAllCommand = new DelegateCommand(ExpandChildren, () => CurrentElementValid);
            CollapseAllCommand = new DelegateCommand(CollapseChildren, () => CurrentElementValid);
        }

        public Type ElementType { get; set; }
        public IField Template { get; set; }
        public int CurrentIndex { get; set; } = -1;
        public ObservableNonGenericCollection Block { get; } = new ObservableNonGenericCollection();
        public bool IsFixedSize { get; set; }

        public DelegateCommand AddCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand DeleteAllCommand { get; set; }
        public DelegateCommand InsertCommand { get; set; }
        public DelegateCommand DuplicateCommand { get; set; }
        public DelegateCommand ShiftDownCommand { get; set; }
        public DelegateCommand ShiftUpCommand { get; set; }
        public DelegateCommand ExpandAllCommand { get; set; }
        public DelegateCommand CollapseAllCommand { get; set; }

        bool CurrentElementValid => Block != null && CurrentIndex != -1;

        public bool IsExpanded
        {
            // only show as expanded if there is any element to display
            get => _isExpanded && CurrentElementValid;
            set
            {
                _isExpanded = value;
            }
        }

        public override void Accept(IFieldVisitor visitor)
        {
             visitor.Visit(this);
        }

        /// <summary>
        /// Called when the field should be populated with a value
        /// </summary>
        /// <param name="value">Value to populate with</param>
        protected override void OnPopulate(object value)
        {
            CurrentIndex = -1;
            // reset the current index so that the change handler gets invoked even if the value has not changed
            Block.BaseCollection = (IList)value;
        }

        private void Block_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // update the current index
            CurrentIndex = DetermineNextCurrentIndex(e);
            // commit the changes to the underlying field storage
            SetActualValue(Block.BaseCollection);

            NotifyCommandAvailability();
        }

        private int DetermineNextCurrentIndex(NotifyCollectionChangedEventArgs e)
        {
            // if the block is empty after the change, set the current index to -1
            if (Block.Count < 1) return -1;

            if (e.Action == NotifyCollectionChangedAction.Reset)
                return 0;
            else if (e.Action == NotifyCollectionChangedAction.Add)
                // set the current index to the index of the new element
                return e.NewStartingIndex;
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                // set the current index to the previous element if any, otherwise leave it unchanged
                return CurrentIndex > 0 ? (CurrentIndex - 1) : CurrentIndex;
            else if (e.Action == NotifyCollectionChangedAction.Move)
                // set the current index to the shifted element index
                return CurrentIndex = e.NewStartingIndex;
            else
                throw new NotSupportedException();
        }

        private void NotifyCommandAvailability()
        {
            AddCommand.RaiseCanExecuteChanged();
            InsertCommand.RaiseCanExecuteChanged();
            DuplicateCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            DeleteAllCommand.RaiseCanExecuteChanged();
            ShiftDownCommand.RaiseCanExecuteChanged();
            ShiftUpCommand.RaiseCanExecuteChanged();
        }

        public void OnCurrentIndexChanged()
        {
            if (CurrentIndex < 0 || CurrentIndex >= Block.Count)
                return;

            // Load the page
            Template.Populate(Block[CurrentIndex]);
        }

        private void Add()
        {
            Block.Add(Utils.ActivateType(ElementType));
        }

        private void Delete()
        {
            Block.RemoveAt(CurrentIndex);
        }

        private void DeleteAll()
        {
            Block.Clear();
        }

        private void Insert()
        {
            Block.Insert(CurrentIndex, Utils.ActivateType(ElementType));
        }

        private void Duplicate()
        {
            Block.Add(Block[CurrentIndex].DeepCloneV2());
        }

        private void Shift(int direction)
        {
            Block.Move(CurrentIndex, (CurrentIndex + direction));
        }

        private void ExpandChildren()
        {
            FieldExpander.Expand(this, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Expand);
        }

        private void CollapseChildren()
        {
            FieldExpander.Expand(this, FieldExpander.ExpandTarget.All, FieldExpander.ExpandMode.Collapse);
        }

        protected override void OnPopulateContextMenu(ContextMenu menu)
        {
            if (menu.Items.Count > 0)
                menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem() { Header = "Add", ToolTip="Add a new element", Command = AddCommand });
            menu.Items.Add(new MenuItem() { Header = "Insert", ToolTip = "Insert a new element at the current index", Command = InsertCommand });
            menu.Items.Add(new MenuItem() { Header = "Delete", ToolTip = "Delete the element at the current index", Command = DeleteCommand });
            menu.Items.Add(new MenuItem() { Header = "Duplicate", ToolTip = "Duplicate the element at the current index", Command = DuplicateCommand });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem() { Header = "Shift Up", ToolTip = "Shift the current element up one", Command = ShiftUpCommand });
            menu.Items.Add(new MenuItem() { Header = "Shift Down", ToolTip = "Shift the current element down one", Command = ShiftDownCommand });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem() { Header = "Delete All", ToolTip = "Delete all elements", Command = DeleteAllCommand });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem() { Header = "Collapse All", ToolTip = "Collapse all children", Command = CollapseAllCommand });
            menu.Items.Add(new MenuItem() { Header = "Expand All", ToolTip = "Expand all children", Command = ExpandAllCommand });
        }
    }
}
