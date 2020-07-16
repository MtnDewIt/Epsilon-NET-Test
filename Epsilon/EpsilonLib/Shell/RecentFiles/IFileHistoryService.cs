using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace Shared
{
    public interface IFileHistoryService
    {
        IEnumerable<FileHistoryRecord> RecentlyOpened { get; }
        Task RecordFileOpened(Guid editorProviderId, string filePath);
    }
}
