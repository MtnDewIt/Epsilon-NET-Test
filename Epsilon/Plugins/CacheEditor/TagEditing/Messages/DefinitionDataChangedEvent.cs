namespace CacheEditor.TagEditing.Messages
{
    public class DefinitionDataChangedEvent
    {
        public object NewData { get; }
        public string Message { get; }
        public bool HasMessage => !string.IsNullOrWhiteSpace(Message);
        public DefinitionDataChangedEventType ChangeType { get; set; } = DefinitionDataChangedEventType.UnspecifiedChange;
        public bool DefinitionEditorSaveRequested { get; set; } = false;

		public DefinitionDataChangedEvent(object data, string message = null)
        {
            NewData = data;
			Message = message;
		}
    }
}
