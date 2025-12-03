namespace CacheEditor.TagEditing.Messages
{
    public class DefinitionDataChangedEvent
    {
        public bool WasReloaded { get; internal set; }
        public object NewData { get; }

        public DefinitionDataChangedEvent(object data)
        {
            NewData = data;
        }
    }
}
